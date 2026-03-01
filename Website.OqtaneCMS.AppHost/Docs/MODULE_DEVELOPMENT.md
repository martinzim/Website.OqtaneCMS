# Oqtane Module Development Guide

Návod na vytváranie vlastných modulov pre Website.OqtaneCMS aplikáciu.

## Štruktúra Oqtane Modulu

```
Website.OqtaneCMS.Web/
└── Modules/
    └── [YourModule]/
        ├── Client/
        │   ├── Edit.razor              # Admin edit interface
        │   ├── Index.razor             # Public display
        │   ├── Settings.razor          # Module settings
        │   └── ModuleInfo.cs           # Module metadata
        ├── Server/
        │   ├── Controllers/
        │   │   └── [ModuleName]Controller.cs
        │   ├── Repository/
        │   │   ├── I[ModuleName]Repository.cs
        │   │   └── [ModuleName]Repository.cs
        │   └── Manager/
        │       ├── I[ModuleName]Manager.cs
        │       └── [ModuleName]Manager.cs
        ├── Shared/
        │   └── Models/
        │       └── [ModuleName].cs     # Entity model
        └── Scripts/
            └── [Version].sql           # Database migration scripts
```

## Príklad: Gallery Module

### 1. ModuleInfo.cs

```csharp
using Oqtane.Models;
using Oqtane.Modules;

namespace Website.OqtaneCMS.Modules.Gallery
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Gallery",
            Description = "Art Gallery Module for displaying artwork",
            Version = "1.0.0",
            ServerManagerType = "Website.OqtaneCMS.Modules.Gallery.Manager.GalleryManager, Website.OqtaneCMS.Web",
            ReleaseVersions = "1.0.0",
            Dependencies = "",
            PackageName = "Website.OqtaneCMS.Gallery"
        };
    }
}
```

### 2. Model (Shared/Models/Gallery.cs)

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace Website.OqtaneCMS.Modules.Gallery.Models
{
    [Table("ArtPortfolioGallery")]
    public class Gallery : IAuditable
    {
        [Key]
        public int GalleryId { get; set; }
        
        public int ModuleId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;
        
        public string? ThumbnailUrl { get; set; }
        
        public DateTime? PublishedDate { get; set; }
        
        public bool IsPublished { get; set; }
        
        // IAuditable implementation
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime ModifiedOn { get; set; }
    }
}
```

### 3. Repository Interface (Server/Repository/IGalleryRepository.cs)

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Website.OqtaneCMS.Modules.Gallery.Models;

namespace Website.OqtaneCMS.Modules.Gallery.Repository
{
    public interface IGalleryRepository
    {
        Task<IEnumerable<Models.Gallery>> GetGalleriesAsync(int moduleId, CancellationToken cancellationToken = default);
        Task<Models.Gallery?> GetGalleryAsync(int galleryId, CancellationToken cancellationToken = default);
        Task<Models.Gallery> AddGalleryAsync(Models.Gallery gallery, CancellationToken cancellationToken = default);
        Task<Models.Gallery> UpdateGalleryAsync(Models.Gallery gallery, CancellationToken cancellationToken = default);
        Task DeleteGalleryAsync(int galleryId, CancellationToken cancellationToken = default);
    }
}
```

### 4. Repository (Server/Repository/GalleryRepository.cs)

```csharp
using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.OqtaneCMS.Modules.Gallery.Repository
{
    public class GalleryRepository : IGalleryRepository, ITransientService
    {
        private readonly IDbContextFactory<GalleryContext> _contextFactory;

        public GalleryRepository(IDbContextFactory<GalleryContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Models.Gallery>> GetGalleriesAsync(int moduleId, CancellationToken cancellationToken = default)
        {
            await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);
            return await db.Gallery
                .Where(g => g.ModuleId == moduleId)
                .OrderByDescending(g => g.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<Models.Gallery?> GetGalleryAsync(int galleryId, CancellationToken cancellationToken = default)
        {
            await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);
            return await db.Gallery.FindAsync([galleryId], cancellationToken);
        }

        public async Task<Models.Gallery> AddGalleryAsync(Models.Gallery gallery, CancellationToken cancellationToken = default)
        {
            await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);
            db.Gallery.Add(gallery);
            await db.SaveChangesAsync(cancellationToken);
            return gallery;
        }

        public async Task<Models.Gallery> UpdateGalleryAsync(Models.Gallery gallery, CancellationToken cancellationToken = default)
        {
            await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);
            db.Entry(gallery).State = EntityState.Modified;
            await db.SaveChangesAsync(cancellationToken);
            return gallery;
        }

        public async Task DeleteGalleryAsync(int galleryId, CancellationToken cancellationToken = default)
        {
            await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var gallery = await db.Gallery.FindAsync([galleryId], cancellationToken);
            if (gallery is not null)
            {
                db.Gallery.Remove(gallery);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
```

### 5. Controller (Server/Controllers/GalleryController.cs)

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Controllers;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website.OqtaneCMS.Modules.Gallery.Manager;
using Website.OqtaneCMS.Modules.Gallery.Models;

namespace Website.OqtaneCMS.Modules.Gallery.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class GalleryController : ModuleControllerBase
    {
        private readonly IGalleryManager _manager;

        public GalleryController(IGalleryManager manager, ILogManager logger, IHttpContextAccessor accessor) 
            : base(logger, accessor)
        {
            _manager = manager;
        }

        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<Models.Gallery>> Get(string moduleId, CancellationToken cancellationToken)
        {
            return await _manager.GetGalleriesAsync(int.Parse(moduleId), cancellationToken);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.Gallery?> Get(int id, CancellationToken cancellationToken)
        {
            return await _manager.GetGalleryAsync(id, cancellationToken);
        }

        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.Gallery> Post([FromBody] Models.Gallery gallery, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(gallery);
            
            if (ModelState.IsValid)
            {
                return await _manager.AddGalleryAsync(gallery, cancellationToken);
            }
            
            throw new ArgumentException("Invalid gallery data");
        }

        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.Gallery> Put(int id, [FromBody] Models.Gallery gallery, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(gallery);
            
            if (ModelState.IsValid && gallery.GalleryId == id)
            {
                return await _manager.UpdateGalleryAsync(gallery, cancellationToken);
            }
            
            throw new ArgumentException("Invalid gallery data");
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, CancellationToken cancellationToken)
        {
            await _manager.DeleteGalleryAsync(id, cancellationToken);
        }
    }
}
```

### 6. Client Component (Client/Index.razor)

```razor
@using Website.OqtaneCMS.Modules.Gallery.Services
@using Website.OqtaneCMS.Modules.Gallery.Models
@namespace Website.OqtaneCMS.Modules.Gallery
@inherits ModuleBase
@inject IGalleryService GalleryService

<div class="gallery-container">
    @if (_galleries is null)
    {
        <p><em>Loading...</em></p>
    }
    else if (!_galleries.Any())
    {
        <p>No artwork found.</p>
    }
    else
    {
        <div class="gallery-grid">
            @foreach (var item in _galleries)
            {
                <div class="gallery-item">
                    <img src="@item.ThumbnailUrl" alt="@item.Title" />
                    <h3>@item.Title</h3>
                    @if (!string.IsNullOrEmpty(item.Description))
                    {
                        <p>@item.Description</p>
                    }
                </div>
            }
        </div>
    }
</div>

@code {
    private List<Models.Gallery>? _galleries;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _galleries = await GalleryService.GetGalleriesAsync(ModuleState.ModuleId);
        }
        catch (Exception ex)
        {
            await logger.LogError(ex, "Error loading galleries");
        }
    }
}
```

### 7. Database Migration (Scripts/01.00.00.sql)

```sql
-- Create Gallery table
CREATE TABLE ArtPortfolioGallery (
    GalleryId INTEGER PRIMARY KEY AUTOINCREMENT,
    ModuleId INTEGER NOT NULL,
    Title TEXT NOT NULL,
    Description TEXT,
    ImageUrl TEXT NOT NULL,
    ThumbnailUrl TEXT,
    PublishedDate TEXT,
    IsPublished INTEGER NOT NULL DEFAULT 0,
    CreatedBy TEXT NOT NULL,
    CreatedOn TEXT NOT NULL,
    ModifiedBy TEXT NOT NULL,
    ModifiedOn TEXT NOT NULL
);

-- Create index on ModuleId
CREATE INDEX IX_ArtPortfolioGallery_ModuleId ON ArtPortfolioGallery (ModuleId);
```

## Best Practices

1. **Naming Conventions**:
   - Module prefix: `Website.OqtaneCMS.[ModuleName]`
   - Table prefix: `Website.OqtaneCMS[ModuleName]`
   - Namespace: `Website.OqtaneCMS.Modules.[ModuleName]`

2. **Cancellation Tokens**:
   - Vždy pridávaj `CancellationToken` parameter k async metódam
   - Propaguj cez všetky vrstvy (Controller → Manager → Repository)

3. **Validation**:
   - Data Annotations na modeloch
   - Server-side validácia v kontroleroch
   - Client-side validácia v Blazor komponentoch

4. **Security**:
   - Používaj `[Authorize]` atribúty
   - Kontroluj `ModuleSecurity` permissions
   - Validuj všetky vstupy

5. **Error Handling**:
   - Try-catch bloky v komponentoch
   - Logovanie cez `ILogService`
   - User-friendly error messages

6. **Performance**:
   - Lazy loading pre veľké datasety
   - Caching pre statické dáta
   - Pagination pre listy

## Ďalšie kroky

Po vytvorení modulu:

1. Zaregistruj modul v Oqtane admin interface
2. Pridaj modul na stránku cez Page Management
3. Konfiguruj module settings
4. Testuj funkčnosť

## Užitočné linky

- [Oqtane Module Development](https://docs.oqtane.org/guides/modules/creating-modules/)
- [Oqtane API Reference](https://docs.oqtane.org/api/)
