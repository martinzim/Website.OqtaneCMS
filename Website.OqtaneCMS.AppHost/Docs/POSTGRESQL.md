# PostgreSQL Configuration for Website.OqtaneCMS

⚠️ **POZNÁMKA**: Pre detailné informácie o Azure PostgreSQL deployment, pozrite [AZURE_POSTGRESQL.md](AZURE_POSTGRESQL.md)

## Prečo PostgreSQL?

- ✅ **Najlacnejšia Azure databáza** (~$12/mesiac alebo FREE tier)
- ✅ **Oqtane má plnú podporu** (balíček `Oqtane.Database.PostgreSQL`)
- ✅ **Rovnaká databáza lokálne aj v Azure** - konzistencia
- ✅ **.NET Aspire natívna podpora**

## Lokálny vývoj

### Connection String Format

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=oqtane;Username=postgres;Password=yourpassword"
  },
  "Oqtane": {
    "DatabaseType": "PostgreSQL"
  }
}
```

**POZNÁMKA**: Aspire automaticky nastaví connection string pri použití `WithReference(oqtaneDb)`.

## Azure Production

### Azure Database for PostgreSQL Flexible Server

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-server.postgres.database.azure.com;Database=oqtane;Username=adminuser;Password=yourpassword;SSL Mode=Require"
  }
}
```

### Odporúčané tier

- **Burstable B1ms**: ~$12-15/mesiac (1 vCore, 2GB RAM)
- **Free tier**: 750 hodín/mesiac zdarma prvý rok

## Aspire Configuration

Aspire `AppHost.cs` automaticky konfiguruje PostgreSQL:

```csharp
var postgres = builder.AddPostgres("postgres", pgUsername, pgPassword)
    .WithDataVolume()
    .WithPgAdmin();

var oqtaneDb = postgres.AddDatabase("oqtane");

builder.AddProject<Projects.ArtPortfolio_Web>("artportfolio-web")
    .WithReference(oqtaneDb);
```

## Quick Start

### 1. Nastavenie User Secrets

```bash
cd Website.OqtaneCMS.AppHost
dotnet user-secrets set "Parameters:pg-username" "postgres"
dotnet user-secrets set "Parameters:pg-password" "YourPassword123!"
```

### 2. Spustenie

```bash
dotnet run --project Website.OqtaneCMS.AppHost
```

Aspire automaticky:
- Spustí PostgreSQL Docker container
- Vytvorí databázu `oqtane`
- Nastaví connection string
- Spustí pgAdmin na http://localhost:60751

## Migrácia z SQLite

Ak už máte SQLite databázu:

1. Zmeňte `DatabaseType` na `"PostgreSQL"` v `appsettings.json`
2. Aktualizujte connection string
3. Nastavte `InstallationMode` na `"Install"`
4. Spustite aplikáciu - Oqtane vytvorí PostgreSQL schému
5. Manuálne migrujte dáta (ak potrebné)
6. Nastavte `InstallationMode` späť na `"None"`

## Ďalšie informácie

Pre komplexný návod vrátane Azure deployment, cien a optimalizácií, pozrite:
- **[Azure PostgreSQL Guide](AZURE_POSTGRESQL.md)** - kompletný deployment guide

---

**Aktuálna konfigurácia**: PostgreSQL (lokálne + Azure compatible)
