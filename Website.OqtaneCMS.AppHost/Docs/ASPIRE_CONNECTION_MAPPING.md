# Aspire Connection String Mapping pre Oqtane

## Problém

Oqtane očakáva connection string pod názvom `ConnectionStrings:DefaultConnection`, ale Aspire defaultne injektuje connection string pod názvom databázy (napr. `ConnectionStrings:artportfolio`).

## Riešenie

### ✅ Option 1: Aspire Connection Name Mapping (Odporúčané)

Upraviť `AppHost.cs`, aby Aspire injektoval connection string pod správnym názvom:

```csharp
// ✅ GOOD - Explicit connection name mapping
builder.AddProject<Projects.ArtPortfolio_Web>("artportfolio-web")
    .WithReference(postgresDb, connectionName: "DefaultConnection")  // 👈 Kľúčový parameter
    .WaitFor(postgresDb);
```

**Výhody:**
- ✅ Čisté riešenie na jednom mieste
- ✅ Funguje pre lokálne aj Azure deployment
- ✅ Žiadna manuálna konfigurácia v `Program.cs`
- ✅ Aspire Dashboard ukazuje správny názov connection stringu

**Ako to funguje:**
Aspire automaticky vytvorí environment variable:
```
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=artportfolio;...
```

---

### Option 2: Manuálne Mapovanie v Program.cs (Fallback)

Ak nemôžeš upraviť `AppHost.cs`, môžeš mapovať v `Program.cs`:

```csharp
// Map Aspire connection string to Oqtane DefaultConnection
var aspireConnectionString = builder.Configuration.GetConnectionString("artportfolio");
if (!string.IsNullOrEmpty(aspireConnectionString))
{
    builder.Configuration["ConnectionStrings:DefaultConnection"] = aspireConnectionString;
}
```

**Nevýhody:**
- ❌ Duplicitná logika
- ❌ Hard-coded názov databázy (`"artportfolio"`)
- ❌ Musí sa pamätať pri zmenách v AppHost

---

## Overenie Konfigurácie

### 1. Aspire Dashboard

Po spustení `.\start-dev.ps1`:

1. Otvor Aspire Dashboard (`https://localhost:17xxx`)
2. Klikni na **artportfolio-web** resource
3. Prejdi na **Environment** tab
4. Hľadaj: `ConnectionStrings__DefaultConnection`

**Očakávaný výsledok:**
```
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=artportfolio;Username=oqtane;Password=OqtaneDevPassword123!
```

### 2. Oqtane Installation Wizard

Pri prvom spustení by sa mal **Database Type dropdown** správne zobraziť s možnosťou **PostgreSQL**.

### 3. Programmatické Overenie

V `Program.cs` môžeš dočasne pridať debug output:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// DEBUG: Check connection string
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"DefaultConnection: {defaultConn}");
```

---

## Deployment Poznámky

### Lokálny vývoj
```csharp
// AppHost.cs - Local
var postgresDb = postgres.AddDatabase("artportfolio");

builder.AddProject<Projects.ArtPortfolio_Web>("artportfolio-web")
    .WithReference(postgresDb, connectionName: "DefaultConnection");
```

### Azure Deployment
```csharp
// AppHost.cs - Azure
var postgres = builder.AddAzurePostgresFlexibleServer("postgres");
var postgresDb = postgres.AddDatabase("artportfolio");

builder.AddProject<Projects.ArtPortfolio_Web>("artportfolio-web")
    .WithReference(postgresDb, connectionName: "DefaultConnection");
```

Mapovanie funguje identicky pre oba scenáre! 🎉

---

## Príklady Pre Iné Databázy

### SQL Server
```csharp
var sqlDb = sqlServer.AddDatabase("mydb");

builder.AddProject<Projects.MyApp>("myapp")
    .WithReference(sqlDb, connectionName: "DefaultConnection");
```

### MySQL
```csharp
var mysqlDb = mysql.AddDatabase("mydb");

builder.AddProject<Projects.MyApp>("myapp")
    .WithReference(mysqlDb, connectionName: "DefaultConnection");
```

---

## Troubleshooting

### Problem: Dropdown stále prázdny

1. **Reštartuj aplikáciu** (Ctrl+C → `.\start-dev.ps1`)
2. **Vymaž browser cache** (Hard refresh: Ctrl+Shift+R)
3. **Skontroluj Aspire Dashboard** - je connection string tam?

### Problem: Connection string má zlý formát

Aspire automaticky formátuje connection string. Ak potrebuješ custom formát:

```csharp
builder.AddProject<Projects.ArtPortfolio_Web>("artportfolio-web")
    .WithReference(postgresDb, connectionName: "DefaultConnection")
    .WithEnvironment("ConnectionStrings__DefaultConnection", 
        "Host=localhost;Port=5432;Database=custom;...");  // Override
```

### Problem: Multiple connection strings

Oqtane používa LEN `DefaultConnection`. Ak potrebuješ viac databáz:

```csharp
// Primárna Oqtane databáza
.WithReference(postgresDb, connectionName: "DefaultConnection")

// Sekundárna databáza (pre custom module)
.WithReference(analyticsDb, connectionName: "Analytics")
```

---

## Best Practices

1. ✅ **Vždy používaj `connectionName` parameter** pri `WithReference()`
2. ✅ **Udržuj rovnaký názov** pre lokálne aj Azure deployment
3. ✅ **Dokumentuj custom connection strings** v README
4. ✅ **Testuj cez Aspire Dashboard** pred deployment
5. ✅ **Nehard-koduj connection strings** v appsettings.json (používaj Aspire injection)

---

## Verzia

- **Aspire:** 13.1.1+
- **.NET:** 10.0
- **Oqtane:** 10.0.4+
- **Dátum:** 2025-01-XX
