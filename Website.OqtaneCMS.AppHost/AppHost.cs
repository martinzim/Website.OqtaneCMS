var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("artportfolio");

// PostgreSQL database for Oqtane
// Lokálne: Docker PostgreSQL container
// Azure: Azure Database for PostgreSQL Flexible Server

// Choose deployment mode from environment variable or configuration
var useAzure = builder.Configuration["UseAzurePostgreSQL"] == "true";

if (useAzure)
{
    // Azure PostgreSQL Flexible Server
    // Requires Azure subscription and provisioned PostgreSQL server
    var postgres = builder.AddAzurePostgresFlexibleServer("postgres");
    var postgresDb = postgres.AddDatabase("artportfolio");

    // Main web application with Oqtane
    // Map connection string to DefaultConnection for Oqtane compatibility
    builder.AddProject<Projects.Website_OqtaneCMS_Web>("artportfolio-web")
        .WithReference(postgresDb, connectionName: "DefaultConnection")
        .WaitFor(postgresDb);
}
else
{
    // Local Docker PostgreSQL container
    // Default credentials can be overridden in appsettings.Development.json or User Secrets
    var pgUsername = builder.AddParameter("pg-username");
    var pgPassword = builder.AddParameter("pg-password", secret: true);

    var postgres = builder.AddPostgres(name: "artportfolio-postgres", pgUsername, pgPassword, port: 54321)
        .WithHttpEndpoint(port: 54321, targetPort: 5432)
        .WithDataVolume(name: "artportfolio-postgres")
        .WithLifetime(ContainerLifetime.Persistent)
        // suppress GSSAPI/Kerberos negotiation LOG noise on Windows (not an error, connections succeed via scram-sha-256)
        //.WithArgs("-c", "log_min_messages=WARNING")
        .WithPgAdmin(configureContainer: (cc) =>
        {
            cc.WithLifetime(ContainerLifetime.Persistent);
            cc.WithHostPort(60751);
            cc.WithExplicitStart();
        });

    // Main Oqtane database
    var postgresDb = postgres.AddDatabase("artportfolio");

    // Main web application with Oqtane
    // Map connection string to DefaultConnection for Oqtane compatibility
    builder.AddProject<Projects.Website_OqtaneCMS_Web>("artportfolio-web")
        //.WithExternalHttpEndpoints()
        .WithReference(postgresDb, connectionName: "DefaultConnection")
        //.WithEnvironment("Database__DefaultDBType", "Oqtane.Database.PostgreSQL.PostgreSQLDatabase, Oqtane.Server")
        .WaitFor(postgresDb);
}

await builder.Build().RunAsync();
