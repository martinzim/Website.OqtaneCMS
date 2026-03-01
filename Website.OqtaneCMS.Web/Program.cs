using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Shared;

var builder = WebApplication.CreateBuilder(args);

// Required by Oqtane.Infrastructure.DatabaseManager.CreateDatabase - reads this via AppDomain.CurrentDomain.GetData
AppDomain.CurrentDomain.SetData(Constants.DataDirectory, Path.Combine(builder.Environment.ContentRootPath, "Data"));

builder.AddServiceDefaults();

// Zamedzenie pádu aplikácie pri bežiacich Oqtane táskoch pred inštaláciou (napr. chýbajúca tabuľka 'job')
/*builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});*/

// DEBUG: Check if connection string was injected by Aspire
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"=== OQTANE DEBUG ===");
Console.WriteLine($"DefaultConnection: {(string.IsNullOrEmpty(defaultConnection) ? "NOT FOUND!" : "Found ✓")}");
Console.WriteLine($"Connection String: {defaultConnection}");
Console.WriteLine($"Database:DefaultDBType: {builder.Configuration["Database:DefaultDBType"]}");
Console.WriteLine($"Installation:HostEmail: {builder.Configuration["Installation:HostEmail"]}");
Console.WriteLine($"===================");

/*if (string.IsNullOrEmpty(defaultConnection))
{
    throw new InvalidOperationException(
        "DefaultConnection string not found! " +
        "Check Aspire AppHost: .WithReference(postgresDb, connectionName: \"DefaultConnection\")");
}*/

// Configure HttpClient with base address for Oqtane services
/*builder.Services.AddHttpContextAccessor();

// Register named HttpClient for Oqtane with proper factory pattern
builder.Services.AddHttpClient("OqtaneClient")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        UseDefaultCredentials = false,
        // Allow cookies for antiforgery tokens
        UseCookies = true
    });

// Provide HttpClient instance for Oqtane services (scoped per request)
builder.Services.AddScoped<HttpClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext;

    var client = httpClientFactory.CreateClient("OqtaneClient");

    if (httpContext != null)
    {
        var request = httpContext.Request;
        var baseAddress = $"{request.Scheme}://{request.Host}{request.PathBase}";
        client.BaseAddress = new Uri(baseAddress);
        
        // Copy cookies for antiforgery token validation
        var cookies = request.Headers.Cookie;
        if (!string.IsNullOrEmpty(cookies))
        {
            client.DefaultRequestHeaders.Add("Cookie", cookies.ToString());
        }
    }
    else
    {
        // Fallback for non-HTTP contexts (e.g., background services)
        client.BaseAddress = new Uri("https://localhost");
    }

    return client;
});*/

// Configure Antiforgery for cross-origin scenarios (if needed during Aspire dev)
/*builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax; // Allow cross-origin during development
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});*/

// Oqtane services configuration
// HttpClient must be registered BEFORE AddOqtane: Oqtane's AddHttpClients() skips registration
// if HttpClient already exists (if (!services.Any(x => x.ServiceType == typeof(HttpClient)))).
// In Interactive Blazor Server mode (SignalR circuit), IHttpContextAccessor.HttpContext is null,
// so Oqtane's own registration would leave BaseAddress unset → InvalidOperationException on every API call.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpClient>(sp =>
{
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var client = new HttpClient(new HttpClientHandler { UseCookies = false });

    if (httpContext != null)
    {
        // Static SSR phase: HttpContext available, set BaseAddress from current request
        client.BaseAddress = new Uri($"{httpContext.Request.Scheme}://{httpContext.Request.Host}");
        foreach (var cookie in httpContext.Request.Cookies)
            client.DefaultRequestHeaders.Add("Cookie", $"{cookie.Key}={System.Net.WebUtility.UrlEncode(cookie.Value)}");
    }
    else
    {
        // Interactive Blazor Server phase: HttpContext is null (SignalR circuit has no HTTP request).
        // Fall back to the app's configured HTTPS endpoint from the environment.
        var configuredUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "https://localhost:7070";
        var baseUrl = configuredUrls.Split(';')
            .FirstOrDefault(u => u.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            ?? configuredUrls.Split(';')[0];
        client.BaseAddress = new Uri(baseUrl);
    }

    return client;
});

builder.Services.AddOqtane(builder.Configuration, builder.Environment);

var app = builder.Build();

// AUTO INSTALLATION: Runs headless Oqtane setup on first start.
// Required when using Aspire: Aspire always injects the connection string, so
// Oqtane's web wizard is never reachable (TenantMiddleware crashes first).
using (var scope = app.Services.CreateScope())
{
    var dbManager = scope.ServiceProvider.GetRequiredService<Oqtane.Infrastructure.IDatabaseManager>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var installation = dbManager.IsInstalled();

    logger.LogInformation("Oqtane IsInstalled: {Success} ({Message})", installation.Success, installation.Message);

    if (!installation.Success)
    {
        logger.LogInformation("Running Oqtane headless installation...");
        var config = app.Configuration;
        var installConfig = new Oqtane.Shared.InstallConfig
        {
            ConnectionString = config.GetConnectionString("DefaultConnection") ?? "",
            DatabaseType = config["Database:DefaultDBType"] ?? "",
            Aliases = config["Installation:DefaultAlias"] ?? "localhost",
            HostUsername = config["Installation:HostUsername"] ?? "",
            HostPassword = config["Installation:HostPassword"] ?? "",
            HostEmail = config["Installation:HostEmail"] ?? "",
            HostName = config["Installation:HostName"] ?? config["Installation:HostUsername"] ?? "Host User",
            TenantName = "Master",
            IsNewTenant = true,
            SiteName = config["Installation:SiteName"] ?? "Website.OqtaneCMS",
            SiteTemplate = config["Installation:SiteTemplate"] ?? "",
            DefaultTheme = config["Installation:DefaultTheme"] ?? "",
            DefaultContainer = config["Installation:DefaultContainer"] ?? "",
            RenderMode = config["RenderMode"] ?? "Interactive",
            Runtime = config["Runtime"] ?? "Server",
            Register = true
        };

        try
        {
            var result = dbManager.Install(installConfig);
            if (result.Success)
                logger.LogInformation("Oqtane installation completed successfully.");
            else
                logger.LogError("Oqtane installation failed: {Message}", result.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Oqtane installation exception.");
        }
    }
}

app.MapDefaultEndpoints();
app.MapStaticAssets(); // Required: UseOqtane calls AddInteractiveWebAssemblyRenderMode internally

// Oqtane middleware - resolve required services from DI
var configuration = app.Services.GetRequiredService<IConfigurationRoot>();
var environment = app.Services.GetRequiredService<IWebHostEnvironment>();
var corsService = app.Services.GetRequiredService<ICorsService>();
var corsPolicyProvider = app.Services.GetRequiredService<ICorsPolicyProvider>();
var syncManager = app.Services.GetRequiredService<ISyncManager>();

app.UseOqtane(configuration, environment, corsService, corsPolicyProvider, syncManager);

await app.RunAsync();

/*using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<MasterDBContext>();
await dbContext.Database.MigrateAsync();*/
