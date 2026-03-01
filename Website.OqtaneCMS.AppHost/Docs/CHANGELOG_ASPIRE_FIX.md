# Changelog - Aspire Connection String Fix

## 2025-01-XX - Connection String Mapping Fix

### 🐛 Problém

Oqtane Installation Wizard zobrazoval prázdny "Database Type" dropdown, pretože:
- Aspire injektoval connection string ako `ConnectionStrings:artportfolio`
- Oqtane očakával `ConnectionStrings:DefaultConnection`
- Bez connection stringu Oqtane nevedel, ktoré databázové typy má ponúknuť

### ✅ Riešenie

**1. AppHost.cs - Explicitné mapovanie názvu connection stringu**

```diff
// Website.OqtaneCMS.AppHost/AppHost.cs
builder.AddProject<Projects.ArtPortfolio_Web>("artportfolio-web")
-   .WithReference(postgresDb)
+   .WithReference(postgresDb, connectionName: "DefaultConnection")
    .WaitFor(postgresDb);
```

**Výsledok:**
- Aspire teraz injektuje: `ConnectionStrings__DefaultConnection=Host=localhost;...`
- Oqtane správne detekuje PostgreSQL provider
- Installation Wizard dropdown zobrazuje "PostgreSQL" option

**2. Program.cs - Zjednodušená konfigurácia**

Odstránené manuálne mapovanie (už nie je potrebné):
```diff
-// Map Aspire connection string to Oqtane DefaultConnection
-var aspireConnectionString = builder.Configuration.GetConnectionString("artportfolio");
-if (!string.IsNullOrEmpty(aspireConnectionString))
-{
-    builder.Configuration["ConnectionStrings:DefaultConnection"] = aspireConnectionString;
-}
```

**3. HttpClient Factory Pattern**

Opravené vytváranie `HttpClient` inštancií:
```diff
-return new HttpClient { BaseAddress = new Uri(baseAddress) };
+var client = httpClientFactory.CreateClient("OqtaneClient");
+client.BaseAddress = new Uri(baseAddress);
+return client;
```

**Výhody:**
- ✅ Správne používa `IHttpClientFactory`
- ✅ Connection pooling
- ✅ Žiadny socket exhaustion

### 📄 Nová Dokumentácia

- **`docs/ASPIRE_CONNECTION_MAPPING.md`** - Podrobný návod na connection string mapping
- **`docs/OQTANE_MANUAL_SETUP.md`** - Fallback manuálne nastavenie

### 🔄 Zmeny v Súboroch

| Súbor | Zmena | Dôvod |
|-------|-------|-------|
| `AppHost.cs` | `connectionName: "DefaultConnection"` | Aspire injektuje na správne miesto |
| `Program.cs` | Odstránené manuálne mapovanie | Už nie je potrebné |
| `Program.cs` | `IHttpClientFactory` pattern | Anti-pattern fix |
| `appsettings.json` | Bez zmeny | Connection string injektovaný cez Aspire |

### 📊 Overenie

**Pred Fix:**
```
ConnectionStrings__artportfolio=Host=localhost;...  ❌
ConnectionStrings__DefaultConnection=               ❌ (prázdne)
→ Database Type dropdown: prázdny ❌
```

**Po Fixe:**
```
ConnectionStrings__DefaultConnection=Host=localhost;... ✅
→ Database Type dropdown: PostgreSQL ✅
```

### 🧪 Testing Checklist

- [x] Build successful
- [x] Aspire Dashboard zobrazuje správny connection string
- [x] Oqtane Installation Wizard zobrazuje PostgreSQL v dropdown
- [x] HttpClient factory správne reusuje connections
- [x] Dokumentácia aktualizovaná

### 🚀 Deployment Notes

Tento fix funguje pre:
- ✅ Lokálne PostgreSQL (Docker)
- ✅ Azure PostgreSQL Flexible Server
- ✅ Iné Aspire-supported databázy (SQL Server, MySQL)

Jediná zmena potrebná: `connectionName: "DefaultConnection"` parameter.

### 📚 Related Documentation

- `docs/ASPIRE_CONNECTION_MAPPING.md` - Podrobný návod
- `docs/OQTANE_SETUP.md` - Installation wizard
- `docs/FIRST_RUN_CHECKLIST.md` - Krok-po-kroku

### 🎓 Lessons Learned

1. **Aspire connection naming je flexibilné** - používaj `connectionName` parameter
2. **Vždy používaj IHttpClientFactory** - nie `new HttpClient()`
3. **Testuj cez Aspire Dashboard** - environment variables sú tam viditeľné
4. **Dokumentuj custom mappings** - pomôže to pri troubleshooting

---

**Commit Message:**
```
fix: Map Aspire PostgreSQL connection to Oqtane DefaultConnection

- Add connectionName parameter to WithReference in AppHost
- Remove manual connection string mapping from Program.cs
- Fix HttpClient instantiation using IHttpClientFactory
- Add ASPIRE_CONNECTION_MAPPING.md documentation

Fixes #1 (Oqtane Installation Wizard empty Database Type dropdown)
```
