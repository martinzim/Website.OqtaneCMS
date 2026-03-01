# ArtPortfolio - Copilot Development Instructions

## Project Overview

**ArtPortfolio** je .NET 10 Blazor Server aplikácia navrhnutá ako webové portfólio pre digitálneho ilustrátora. Aplikácia využíva Oqtane CMS pre správu obsahu a je nasadená do Azure Container Apps (ACA).

### Technology Stack

- **.NET 10** (C# 14.0)
- **Blazor Server** (interactive server-side rendering)
- **Oqtane CMS 10.0.3** (headless/integrated CMS)
- **Aspire 13.0** (local development orchestration)
- **PostgreSQL** (databáza - plánovaná)
- **Azure Container Apps** (production deployment)

### Solution Structure

```
ArtPortfolio/
├── ArtPortfolio.Web/           # Main Blazor Server application with Oqtane
├── ArtPortfolio.AppHost/        # Aspire orchestration host
└── ArtPortfolio.ServiceDefaults/ # Shared service defaults
```

---

## Core Development Rules

### 1. Architecture & Design

- **Oqtane-first approach**: Používaj Oqtane framework pre správu obsahu, moduly, themes a stránky
- **No direct Blazor components in ArtPortfolio.Web**: Všetky UI komponenty by mali byť implementované ako Oqtane moduly
- **Module-based architecture**: Nové funkcionality implementuj ako samostatné Oqtane moduly
- **Separation of concerns**: 
  - Client-side logika → Oqtane.Client
  - Server-side logika → Oqtane.Server  
  - Shared models → Oqtane.Shared
- **Keep it simple**: Nepridávaj zbytočné abstrakcie alebo služby

#### ❌ Architecture Anti-Patterns

**NEPOUŽÍVAJ tieto architektúry - sú to over-engineering pre tento projekt:**

- **❌ NO Clean Architecture**: 
  - Oqtane module pattern (Client/Server/Shared) už poskytuje separation of concerns
  - Pridávanie Domain/Application/Infrastructure layers je duplicitná abstrakcia
  - Pre portfólio aplikáciu s galériu, profilom a commission systémom to je zbytočná komplexita

- **❌ NO CQRS/MediatR**: 
  - Pre jednoduchú CRUD logiku je to overkill
  - Oqtane Manager + Controller pattern je dostatočný
  - CQRS má zmysel len pri high-traffic enterprise apps s complex domain logic

- **❌ NO Generic Repositories**: 
  ```csharp
  // ❌ BAD - Generic repository wrapper
  public class Repository<T> : IRepository<T> { ... }

  // ✅ GOOD - Používaj Oqtane ISqlDatabase priamo
  public class GalleryRepository : IGalleryRepository
  {
      private readonly ISqlDatabase _db;
      public async Task<Gallery> GetAsync(int id) 
          => await _db.GetItemAsync<Gallery>(id);
  }
  ```

- **❌ NO Additional abstraction layers**: 
  - Client/Server/Shared je tri-tier architecture - to stačí
  - Nepridávaj ďalšie vrstvy medzi Controller → Manager → Repository
  - Každá vrstva musí mať jasný dôvod existencie

- **❌ NO Domain-Driven Design (DDD)**:
  - Value objects, Aggregates, Domain Events sú pre tento projekt overkill
  - Gallery, ArtistProfile, Commission sú jednoduché entity, nie complex aggregates
  - Používaj Oqtane Shared models ako simple DTOs/POCOs

**Kedy zvážiť komplexnejšiu architektúru:**
- ✅ Aplikácia rastie na 100+ modulov (enterprise scale)
- ✅ Potrebuješ multiple UI frontends (Web + Native Mobile App + Desktop)
- ✅ Komplexná invariant-rich domain logic (napr. financial calculations, workflow engines)
- ✅ Microservices s distributed transactions

**Pre digitálne portfólio ilustrátora TOTO NEPLATÍ. Drž sa Oqtane patterns.**

### 2. Oqtane Specific Guidelines

#### Module Development

```csharp
// Štruktúra Oqtane modulu
Modules/
├── [ModuleName]/
│   ├── Client/
│   │   ├── Edit.razor           # Admin edit interface
│   │   ├── Index.razor          # Public display
│   │   └── ModuleInfo.cs        # Module metadata
│   ├── Server/
│   │   ├── Controllers/         # API endpoints
│   │   ├── Repository/          # Data access
│   │   └── Manager/             # Business logic
│   └── Shared/
│       └── Models/              # DTOs and entities
```

- **Dodržiavaj Oqtane naming conventions**:
  - Module názov: `[Namespace].[ModuleName]`
  - Controller: `[ModuleName]Controller`
  - Service interface: `I[ModuleName]Service`
  
- **Používaj Oqtane služby**:
  - `ISettingService` pre nastavenia modulu
  - `IFileService` pre upload súborov (artwork)
  - `ILogService` pre logovanie
  - `ISqlDatabase` pre databázové operácie
  
- **Security**:
  - Vždy kontroluj `ModuleSecurity` a `PageSecurity`
  - Používaj `[Authorize]` atribúty na kontroleroch
  - Validuj všetky vstupy na server-side

#### Theme Development

```csharp
// Theme pre portfolio ilustrátora
Themes/
├── ArtPortfolio.Theme/
│   ├── ThemeInfo.cs            # Theme metadata
│   ├── Layouts/                # Layout templates
│   │   ├── Default.razor
│   │   └── Gallery.razor       # Špeciálny layout pre galériu
│   └── wwwroot/
│       ├── css/
│       └── images/
```

- **Responsive design**: Priorita na vizuálne prezentáciu artwork
- **Performance**: Optimalizuj pre veľké obrázky (lazy loading, WebP)
- **Dark/Light mode**: Podporuj oba režimy

### 3. Database & Data Access

- **Používaj Oqtane abstrakciu**: `ISqlDatabase` namiesto priameho Entity Framework
- **Migrations**: Vytváraj SQL migration skripty v `Scripts/` foldered
- **Naming conventions**:
  ```sql
  -- Tabuľky: [ModuleName] prefix
  CREATE TABLE ArtPortfolioGallery (
      GalleryId INT IDENTITY(1,1) PRIMARY KEY,
      ModuleId INT NOT NULL,
      -- ...
  )
  ```
- **Soft deletes**: Používaj `IsDeleted` flag namiesto fyzického mazania
- **Audit fields**: Každá tabuľka by mala mať `CreatedBy`, `CreatedOn`, `ModifiedBy`, `ModifiedOn`

### 4. File & Image Management

```csharp
// Best practices pre artwork súbory
- Maximálna veľkosť: 10MB (konfigurovateľné)
- Podporované formáty: JPEG, PNG, WebP, GIF
- Automatická generácia thumbnails (200x200, 400x400, 800x800)
- Ukladanie: použiť IFileService pre správu súborov
- CDN-ready: Pripraviť na Azure Blob Storage integration
```

### 5. Aspire Local Development

```csharp
// AppHost.cs patterns
var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL pre Oqtane
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("artportfolio");

// Redis cache (optional, pre performance)
var redis = builder.AddRedis("cache");

// Main web application
builder.AddProject<Projects.ArtPortfolio_Web>("artportfolio-web")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres);

builder.Build().Run();
```

- **Connection strings**: Používaj Aspire automatic connection string injection
- **Health checks**: Implementuj pre všetky externé závislosti
- **Secrets**: Používaj User Secrets pre lokálny vývoj (`UserSecretsId` už nastavené)

### 6. Azure Container Apps Deployment

```yaml
# Deployment considerations
- Container registry: Azure Container Registry
- Database: Azure Database for PostgreSQL Flexible Server
- Storage: Azure Blob Storage (pre obrázky)
- Monitoring: Application Insights
- Environment variables:
  - ConnectionStrings__DefaultConnection
  - Oqtane__InstallMode=false (po prvotnej inštalácii)
```

- **Keep containers stateless**: Všetky súbory do Blob Storage
- **Configuration**: Používaj Azure App Configuration alebo Key Vault
- **Scaling**: Priprav na horizontal scaling (session state do Redis)

### 7. Performance & Optimization

- **Image optimization**:
  ```csharp
  // Automatic WebP conversion
  // Lazy loading pre gallery
  // Progressive image loading (blur → full quality)
  ```
- **Caching strategy**:
  - Output cache pre statické stránky
  - Distributed cache pre API responses
  - Browser cache pre static assets
- **Blazor Server**:
  - Minimalizuj SignalR traffic
  - Používaj `@key` directive pre lists
  - Virtualizácia pre veľké galérie (`<Virtualize>`)

### 8. Security Best Practices

- **Never commit secrets**: Používaj User Secrets / Key Vault
- **HTTPS only**: Enforce v produkcii
- **CORS**: Strict policy (len pre potrebné origins)
- **Input validation**:
  ```csharp
  // Vždy validuj na server-side
  ArgumentNullException.ThrowIfNull(model);
  if (string.IsNullOrWhiteSpace(model.Title))
      throw new ArgumentException("Title is required");
  ```
- **SQL Injection**: Používaj parameterizované queries (Oqtane ISqlDatabase to robí automaticky)
- **XSS Protection**: Oqtane má built-in sanitization, ale buď opatrný s `@((MarkupString)html)`

### 9. Logging & Monitoring

```csharp
// Používaj ILogService z Oqtane
public class GalleryController : Controller
{
    private readonly ILogService _logger;
    
    public GalleryController(ILogService logger)
    {
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Gallery model)
    {
        try
        {
            // business logic
            _logger.Log(LogLevel.Information, this, LogFunction.Create, 
                "Gallery created: {Title}", model.Title);
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, 
                "Failed to create gallery");
            throw;
        }
    }
}
```

- **Structured logging**: Používaj message templates s parametrami
- **Log levels**: 
  - Error: Exceptions
  - Warning: Validation failures, deprecated API usage
  - Information: Major operations (create, update, delete)
  - Debug: Development only
- **Azure Application Insights**: Automatic telemetry cez Aspire ServiceDefaults

### 10. Code Style & Conventions

```csharp
// C# 14.0 features - používaj moderne konštrukty
file class InternalHelper { } // file-scoped types

public class GalleryService
{
    // Primary constructors
    private readonly IDbContextFactory<ApplicationDbContext> _factory;
    
    public GalleryService(IDbContextFactory<ApplicationDbContext> factory)
    {
        _factory = factory;
    }
    
    // Collection expressions
    public List<string> GetSupportedFormats() => [".jpg", ".png", ".webp"];
    
    // Field-backed properties (C# 14)
    public string? _cachedValue;
    public string CachedValue
    {
        get => _cachedValue ??= LoadValue();
        set => field = value; // field keyword
    }
}
```

- **Naming**:
  - PascalCase: Classes, methods, properties
  - camelCase: Local variables, parameters
  - _camelCase: Private fields
- **Nullable reference types**: Enabled - používaj `?` explicitne
- **File-scoped namespaces**: Povinné pre nové súbory
- **Minimal APIs**: NE - používaj Oqtane controller pattern

### 11. Testing Strategy

```csharp
// Test project structure
ArtPortfolio.Tests/
├── Unit/
│   ├── Services/
│   └── Managers/
├── Integration/
│   └── Controllers/
└── E2E/
    └── Modules/
```

- **Unit tests**: xUnit + Moq
- **Integration tests**: WebApplicationFactory s test database
- **E2E tests**: Playwright (pre critical user flows)
- **Coverage**: Minimum 70% pre nový kód
- **Test naming**: `MethodName_Scenario_ExpectedBehavior`

### 12. Portfolio-Specific Features

#### Gallery Module
```csharp
// Požadované funkcionality:
- Multiple galleries per artist
- Tagging and categorization
- Search and filtering
- Lightbox view
- Download options (watermarked/original)
- Social media sharing
- View statistics
```

#### Artist Profile Module
```csharp
// Profil ilustrátora:
- Biography (multi-language support)
- Skills and techniques
- Awards and exhibitions
- Contact form
- Commission status (open/closed)
```

#### Commission Module
```csharp
// Systém pre objednávky:
- Price calculator (based on size, complexity, deadline)
- Request form with file upload
- Status tracking (inquiry → quote → payment → in progress → completed)
- Email notifications
```

---

## Common Patterns & Anti-Patterns

### ✅ DO

```csharp
// Používaj Oqtane služby
public class GalleryManager : IGalleryManager
{
    private readonly ISqlDatabase _db;
    private readonly IFileService _fileService;
    private readonly ISettingService _settingService;
    
    // Async all the way
    public async Task<Gallery> GetGalleryAsync(int galleryId, CancellationToken ct)
    {
        return await _db.GetItemAsync<Gallery>(galleryId, ct);
    }
    
    // Cancellation tokens pre long-running operations
    public async Task ProcessImagesAsync(List<IFormFile> files, CancellationToken ct)
    {
        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();
            await ProcessImageAsync(file, ct);
        }
    }
}
```

### ❌ DON'T

```csharp
// Nevytváraj priame Blazor komponenty mimo Oqtane modulu
// ❌ BAD: Components/Gallery.razor (v ArtPortfolio.Web root)
// ✅ GOOD: Modules/ArtPortfolio.Gallery/Client/Index.razor

// Nepoužívaj sync-over-async
// ❌ BAD
public Gallery GetGallery(int id) => GetGalleryAsync(id).Result;

// Nepridávaj state do Blazor Server komponentov
// ❌ BAD
private static List<Gallery> _cachedGalleries; // shared across users!
// ✅ GOOD
[Inject] private IMemoryCache Cache { get; set; }

// Neloguj PII (osobné údaje)
// ❌ BAD
_logger.LogInformation("User {Email} logged in", user.Email);
// ✅ GOOD
_logger.LogInformation("User {UserId} logged in", user.UserId);
```

---

## Quick Reference Commands

```bash
# Lokálny vývoj (Aspire)
dotnet run --project ArtPortfolio.AppHost

# Build
dotnet build

# Test
dotnet test

# Publish (pre ACA)
dotnet publish ArtPortfolio.Web -c Release

# Entity Framework migrations (ak sa použije v budúcnosti)
# Oqtane používa SQL skripty, nie EF migrations

# Docker build (pre ACA deployment)
docker build -t artportfolio-web:latest -f ArtPortfolio.Web/Dockerfile .
```

---

## Resources & Documentation

- [Oqtane Documentation](https://docs.oqtane.org/)
- [Oqtane GitHub](https://github.com/oqtane/oqtane.framework)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Azure Container Apps Documentation](https://learn.microsoft.com/azure/container-apps/)
- [Blazor Documentation](https://learn.microsoft.com/aspnet/core/blazor/)

---

## Change Log & Evolution

Tento dokument by mal byť aktualizovaný pri:
- Pridaní nového modulu alebo hlavnej funkcionality
- Zmene architektúry alebo tech stacku
- Zmenách v deployment stratégii
- Nových best practices alebo lessons learned

**Verzia**: 1.0  
**Dátum vytvorenia**: 2025  
**Posledná aktualizácia**: Initial version
