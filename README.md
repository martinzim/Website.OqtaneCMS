# Website.OqtaneCMS

**Oqtane CMS** + **.NET 10 Blazor Server**.

## 🚀 Technológie

- **.NET 10** (C# 14.0)
- **Blazor Server** (interactive server-side rendering)
- **Oqtane CMS 10.0.4** (headless/integrated CMS)
- **.NET Aspire 13.0** (local development orchestration)
- **PostgreSQL** (lokálne + Azure Database for PostgreSQL Flexible Server)
- **Azure Container Apps** (production deployment target)

## 📁 Štruktúra projektu

```
Website.OqtaneCMS/
├── Website.OqtaneCMS.Web/              # Hlavná Blazor Server aplikácia s Oqtane
├── Website.OqtaneCMS.AppHost/          # Aspire orchestration host
├── Website.OqtaneCMS.ServiceDefaults/  # Zdieľané predvoľby služieb (telemetria, kontrola stavu)
├── docs/                               # Dokumentácia
│   ├── OQTANE_SETUP.md                 # 📘 Kompletný sprievodca inštaláciou
│   ├── AZURE_POSTGRESQL.md             # Sprievodca nastavením Azure databázy
│   ├── POSTGRESQL.md                   # Konfigurácia PostgreSQL
│   └── MODULE_DEVELOPMENT.md           # Sprievodca vývojom Oqtane modulov
└── start-dev.ps1                       # 🚀 Quick start skript
```

## ⚡ Rýchly Štart

### Požiadavky

- ✅ [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- ✅ [Docker Desktop](https://www.docker.com/products/docker-desktop) **(musí byť spustený!)**
- ✅ [Visual Studio 2026](https://visualstudio.microsoft.com/) alebo [Visual Studio Code](https://code.visualstudio.com/)

### 1-Click Start (Odporúčané)

```powershell
# Windows PowerShell
.\start-dev.ps1
```

**Alebo manuálne:**

```bash
# Spustenie cez Aspire orchestráciu
dotnet run --project Website.OqtaneCMS.AppHost
```

### Čo sa stane?

1. 🐳 Aspire stiahne a spustí PostgreSQL container
2. 🗄️ Vytvorí databázu `OqtaneCMS`
3. 🛠️ Spustí pgAdmin na `http://localhost:60751`
4. 🌐 Spustí Website.OqtaneCMS.Web
5. 📊 Otvorí Aspire Dashboard

### Prvé Prihlásenie

1. **Otvor aplikáciu** (URL sa zobrazí v Aspire Dashboard)
2. **Oqtane Installation Wizard** sa spustí automaticky
3. **Databáza:** Už nakonfigurovaná! ✅
4. **Vytvor admin účet:**
   - Username: `admin`
   - Password: [tvoje silné heslo]
   - Email: `admin@email.com`
5. **Site Name:** `Website.OqtaneCMS`
6. **Klikni Install** 🎉

**✨ Hotovo! Oqtane je pripravený.**

📖 **Podrobné inštrukcie:** `docs/OQTANE_SETUP.md`

## 🛠️ Vývoj

### Konfigurácia databázy

**Default credentials (lokálny vývoj):**
- Username: `oqtane`
- Password: `OqtaneDevPassword123!`

**Zmena hesla:**

```json
// Website.OqtaneCMS.AppHost/appsettings.Development.json
{
  "Parameters": {
    "pg-username": "oqtane",
    "pg-password": "StrongPassword123!"
  }
}
```

### pgAdmin Prístup

**URL:** `http://localhost:60751`
- Email: `admin@admin.com`
- Password: `admin`

### Aspire Dashboard

- 📊 Metrics & Logs
- 🔍 Distributed Tracing
- ⚙️ Environment Variables
- 📈 Performance Monitoring

## 📝 Prvotná inštalácia Oqtane

Pri prvom spustení sa automaticky spustí **Oqtane Installation Wizard**:

1. Otvorte aplikáciu v prehliadači
2. **Database Type**: **PostgreSQL** ✅ (automaticky)
3. **Connection String**: ✅ (injektovaný cez Aspire)
4. Vytvorte **Admin účet**
5. **Site Name**: napr. "Website.OqtaneCMS"
6. Kliknite na **Install**

Po inštalácii sa vytvorí:
- PostgreSQL databázová schéma (50+ tabuliek)
- Default homepage so sample obsahom
- Admin panel pre správu obsahu

⚠️ **Po inštalácii:** Reštartuj aplikáciu (Ctrl+C → `dotnet run`)

📖 **Detailný návod:** `docs/OQTANE_SETUP.md`
}
```

## 🔧 Konfigurácia

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Installation": {
    "InstallationMode": "Install"
  },
  "Oqtane": {
    "InstallationFiles": "wwwroot",
    "Runtime": "Server",
    "SiteTemplate": "Default Site Template",
    "DatabaseType": "PostgreSQL"
  }
}
```

**Poznámka**: Connection string sa automaticky nastaví cez Aspire pri lokálnom vývoji.

## 💾 Databáza

### Lokálny vývoj
- **PostgreSQL 16** (Docker container cez Aspire)
- **pgAdmin** dostupný na http://localhost:60751

### Azure Production
- **Azure Database for PostgreSQL Flexible Server**
- **Burstable B1ms tier**: ~$12/mesiac
- **Free tier**: 750 hodín/mesiac zdarma (prvý rok)

📚 **Detailné guides**:
- [docs/AZURE_POSTGRESQL.md](docs/AZURE_POSTGRESQL.md) - Azure deployment
- [docs/DATABASE_COMPARISON.md](docs/DATABASE_COMPARISON.md) - Porovnanie Azure databáz

## 🏗️ Vývoj

### Build

```bash
dotnet build
```

### Test

```bash
dotnet test
```

### Vytvorenie nového Oqtane modulu

Pozri dokumentáciu v [docs/MODULE_DEVELOPMENT.md](docs/MODULE_DEVELOPMENT.md) pre detailný návod.

## 📦 Deployment

### Azure Container Apps

```bash
# Build container image
docker build -t website-oqtanecms-web:latest -f Website.OqtaneCMS.Web/Dockerfile .

# Push to Azure Container Registry
az acr login --name yourregistry
docker tag website-oqtanecms-web:latest yourregistry.azurecr.io/website-oqtanecms-web:latest
docker push yourregistry.azurecr.io/website-oqtanecms-web:latest
```

📚 **Azure deployment guide**: [docs/AZURE_POSTGRESQL.md](docs/AZURE_POSTGRESQL.md)

## 📚 Dokumentácia

- [docs/ASPIRE_AZURE_POSTGRES.md](docs/ASPIRE_AZURE_POSTGRES.md) - **NOVÉ!** Aspire Azure PostgreSQL integration
- [docs/QUICKSTART.md](docs/QUICKSTART.md) - 5-minútový quick start
- [docs/AZURE_POSTGRESQL.md](docs/AZURE_POSTGRESQL.md) - Azure database setup a deployment
- [docs/DATABASE_COMPARISON.md](docs/DATABASE_COMPARISON.md) - Porovnanie Azure databáz
- [docs/POSTGRESQL.md](docs/POSTGRESQL.md) - PostgreSQL konfigurácia
- [docs/MODULE_DEVELOPMENT.md](docs/MODULE_DEVELOPMENT.md) - Vývoj Oqtane modulov
- [docs/ENVIRONMENT_CONFIG.md](docs/ENVIRONMENT_CONFIG.md) - Environment variables
- [docs/SETUP_CHANGES.md](docs/SETUP_CHANGES.md) - História zmien
- [.github/copilot-instructions.md](.github/copilot-instructions.md) - Development guidelines

### Externé zdroje

- [Oqtane Documentation](https://docs.oqtane.org/)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Blazor Documentation](https://learn.microsoft.com/aspnet/core/blazor/)

## 🔍 pgAdmin pripojenie

Po spustení Aspire:

- **URL**: http://localhost:60751
- **Email**: admin@admin.com (default)
- **Password**: admin (default)

Pridať server:
- **Host**: postgres (container name)
- **Port**: 5432
- **Database**: oqtane
- **Username**: postgres
- **Password**: (vaše heslo z user secrets)

---

**Version**: 1.0.0  
**Created**: 2026  
**Framework**: .NET 10 | Oqtane 10.0.3 | PostgreSQL 16
