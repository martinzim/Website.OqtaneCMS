# Oqtane Lokálne Nastavenie - Prvotná Inštalácia

## Prehľad

Tento dokument popisuje kroky pre prvotné nastavenie Oqtane CMS s lokálnou PostgreSQL databázou pomocou .NET Aspire orchestrácie.

## Predpoklady

- ✅ .NET 10 SDK
- ✅ Docker Desktop (spustený)
- ✅ Visual Studio 2026 alebo VS Code
- ✅ PowerShell 7+

## Rýchly Štart

### 1. Prvotné Spustenie

```powershell
# Spustiť Aspire AppHost (lokálny vývoj)
cd X:\DotNET\Website.OqtaneCMS
dotnet run --project Website.OqtaneCMS.AppHost
```

**Čo sa stane:**
1. Aspire stiahne a spustí PostgreSQL Docker container
2. Spustí pgAdmin na `http://localhost:60751`
3. Vytvorí databázu `artportfolio`
4. Spustí Website.OqtaneCMS.Web aplikáciu
5. Aspire Dashboard bude dostupný na `http://localhost:15xxx` (port sa zobrazí v konzole)

### 2. Oqtane Inštalačný Wizard

Pri prvom spustení sa automaticky otvorí Oqtane Installation Wizard:

**URL:** `https://localhost:7xxx` (port sa zobrazí v Aspire Dashboard)

#### Krok 1: Databázové Pripojenie
```
✅ Automaticky nakonfigurované cez Aspire!

Database Type: PostgreSQL
Connection String: Injected automaticky
```

**Poznámka:** Aspire automaticky injektuje connection string vo formáte:
```
Host=localhost;Port=5432;Database=artportfolio;Username=oqtane;Password=OqtaneDevPassword123!
```

#### Krok 2: Site Configuration

**Recommended Settings:**
```
Site Name: Website.OqtaneCMS Dev
Site Template: Default Site Template
Runtime: Server (Blazor Server mode)

Aliases:
- localhost:7xxx (automaticky)
```

#### Krok 3: Admin Account

**Vytvor admin účet:**
```
Username: admin
Email: admin@artportfolio.local (lokálny vývoj)
Password: [silné heslo - minimálne 8 znakov]
```

⚠️ **DÔLEŽITÉ:** Heslo si zapíš - bude potrebné na prihlásenie!

#### Krok 4: SMTP Configuration (Optional)

Pre lokálny vývoj môžeš preskočiť alebo použiť:
```
SMTP Host: localhost
SMTP Port: 25
(alebo "Skip" pre teraz)
```

### 3. Po Inštalácii

Po dokončení inštalácie:

1. **Reštartuj aplikáciu** (Ctrl+C v konzole, potom znova `dotnet run`)
   - Toto prepne Oqtane z "Install" do "Production" režimu
   - `appsettings.json` sa automaticky upraví

2. **Prihlás sa:**
   - Klikni na "Login" v pravom hornom rohu
   - Username: `admin`
   - Password: [tvoje heslo z kroku 3]

3. **Základná stránka:**
   - Oqtane vytvorí default homepage s sample obsahom
   - Môžeš začať editovať cez "Edit Mode" (ikona v toolbar)

## Konfigurácia

### Aspire Konfigurácia

**Súbor:** `Website.OqtaneCMS.AppHost/appsettings.Development.json`

```json
{
  "Parameters": {
    "pg-username": "oqtane",
    "pg-password": "OqtaneDevPassword123!"
  },
  "UseAzurePostgreSQL": "false"
}
```

**Zmena hesla:**
1. Uprav `pg-password` v `appsettings.Development.json`
2. Vymaž Docker volume:
   ```powershell
   docker volume rm artportfolio-apphost-postgres-data
   ```
3. Reštartuj Aspire

### Oqtane Konfigurácia

**Súbor:** `Website.OqtaneCMS.Web/appsettings.json`

Po inštalácii sa automaticky upraví:
```json
{
  "Installation": {
    "InstallationMode": "Production"  // Zmení sa z "Install"
  }
}
```

## pgAdmin Prístup

**URL:** `http://localhost:60751`

**Prihlásenie:**
- Email: `admin@admin.com` (default pgAdmin)
- Password: `admin` (default pgAdmin)

**Pridaj PostgreSQL server:**
1. Right-click "Servers" → "Register" → "Server"
2. General Tab:
   - Name: `Website.OqtaneCMS Local`
3. Connection Tab:
   - Host: `host.docker.internal` (pre Docker on Windows)
   - Port: `5432`
   - Database: `artportfolio`
   - Username: `oqtane`
   - Password: `OqtaneDevPassword123!`
4. Save

## Aspire Dashboard

**URL:** `http://localhost:15xxx` (zobrazí sa pri štarte)

**Funkcie:**
- 📊 **Resources:** Zoznam všetkých services (postgres, pgAdmin, artportfolio-web)
- 📈 **Metrics:** Performance metrics (CPU, memory, requests)
- 📜 **Logs:** Consolidated logs zo všetkých services
- 🔍 **Traces:** Distributed tracing

**Užitočné:**
- Klikni na "artportfolio-web" → "Logs" pre application logs
- "postgres" → "Logs" pre databázové logy
- Environment variables sú zobrazené pre každý resource

## Častá Riešenie Problémov

### Problem: "Docker Desktop is not running"
```powershell
# Spusti Docker Desktop a počkaj kým sa úplne načíta
# Skontroluj status:
docker ps
```

### Problem: "Port 5432 already in use"
```powershell
# Zastaviť existujúce PostgreSQL procesy
docker ps -a | findstr postgres
docker stop <container_id>

# Alebo zmeň port v AppHost.cs
```

### Problem: Oqtane nedetekuje databázu
1. Skontroluj Aspire Dashboard - je postgres "Running"?
2. Pozri connection string v Logs:
   ```
   Aspire Dashboard → artportfolio-web → Environment → ConnectionStrings__artportfolio
   ```
3. Test connection cez pgAdmin

### Problem: "Installation failed"
```powershell
# Resetuj databázu a začni odznova
docker volume rm artportfolio-apphost-postgres-data
dotnet run --project Website.OqtaneCMS.AppHost
```

### Problem: Aplikácia sa zasekla v "Install" režime
```json
// Manuálne uprav Website.OqtaneCMS.Web/appsettings.json
{
  "Installation": {
    "InstallationMode": "Production"
  }
}
```

## Databázová Schéma

Po inštalácii Oqtane vytvorí cca 50+ tabuliek:

**Hlavné tabuľky:**
- `oqt_Alias` - Site aliases
- `oqt_Module` - Installed modules
- `oqt_ModuleDefinition` - Module definitions
- `oqt_Page` - Pages
- `oqt_PageModule` - Page-module relationships
- `oqt_Site` - Sites
- `oqt_Theme` - Installed themes
- `oqt_User` - Users

**Pozrieť schému v pgAdmin:**
1. Connect to server
2. Databases → artportfolio → Schemas → public → Tables

## Ďalšie Kroky

### 1. Vyskúšaj Oqtane Features

**Admin Dashboard:**
- 🔧 Control Panel → Klik na gear icon vpravo hore
- 📄 Pages → Pridaj novú stránku
- 🧩 Modules → Pridaj module na stránku
- 🎨 Themes → Zmena theme (Default → dostupné themes)

**Edit Mode:**
- Klik na "Edit" toggle v toolbar
- Drag & drop pre reorganizáciu modulov
- Inline editing pre content

### 2. Vytvor Vlastný Oqtane Module

Podľa `docs/MODULE_DEVELOPMENT.md`:
```powershell
# TODO: Vytvor gallery module scaffold
dotnet new oqtane-module -n Website.OqtaneCMS.Gallery
```

### 3. Prispôsob Theme

```
Modules/Themes/Website.OqtaneCMS.Theme/
├── Layouts/
│   └── Default.razor
├── wwwroot/
│   └── css/
│       └── theme.css
└── ThemeInfo.cs
```

## Produkčná Konfigurácia

Po vývoji prepni na Azure PostgreSQL:

**Súbor:** `Website.OqtaneCMS.AppHost/appsettings.json`
```json
{
  "UseAzurePostgreSQL": "true"
}
```

Detaily v: `docs/AZURE_POSTGRESQL.md`

## Užitočné Príkazy

```powershell
# Spustiť lokálny dev
dotnet run --project Website.OqtaneCMS.AppHost

# Build celého solution
dotnet build

# Watch mode (auto-rebuild)
dotnet watch --project Website.OqtaneCMS.AppHost

# Zastaviť všetky Docker containers
docker stop $(docker ps -q)

# Vyčistiť Docker volumes (stratíš data!)
docker volume prune

# Skontrolovať PostgreSQL connection
docker exec -it <postgres_container> psql -U oqtane -d artportfolio
```

## Zabezpečenie - Lokálny Vývoj

⚠️ **Tieto credentials sú LEN pre lokálny vývoj!**

**NIE pre produkciu:**
- `pg-password`: `OqtaneDevPassword123!`
- pgAdmin default passwords
- HTTP (ne-HTTPS) pre pgAdmin

**Pre produkciu používaj:**
- Azure Key Vault pre secrets
- Managed Identity pre Azure services
- HTTPS všade
- Silné, unikátne heslá

## References

- [Oqtane Documentation](https://docs.oqtane.org/)
- [Oqtane Installation Guide](https://docs.oqtane.org/guides/installation/)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

## Podpora

**Problém s inštaláciou?**
1. Skontroluj Aspire Dashboard logs
2. Pozri `docs/TROUBLESHOOTING.md` (TODO)
3. GitHub Issues: https://github.com/martinzim/Website.OqtaneCMS/issues

---

**Verzia:** 1.0  
**Dátum:** 2025-01-XX  
**Autor:** Website.OqtaneCMS Team
