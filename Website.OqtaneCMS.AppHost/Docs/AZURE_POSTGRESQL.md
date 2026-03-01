# Azure PostgreSQL Configuration Guide

## 🎯 Odporúčaná databáza: Azure Database for PostgreSQL Flexible Server

### Prečo PostgreSQL?

- ✅ **Oqtane 10.0.3 má plnú podporu** (balíček `Oqtane.Database.PostgreSQL`)
- ✅ **Najlacnejšia enterprise databáza v Azure**
- ✅ **Burstable tier (B1ms)**: ~$12-15/mesiac
- ✅ **Free tier**: 750 hodín/mesiac zdarma (prvý rok)
- ✅ **Rovnaká databáza lokálne aj v Azure** - konzistencia prostredia
- ✅ **.NET Aspire natívna podpora**

## 💰 Cenové porovnanie

| Databáza | Tier | Cena/mesiac | Oqtane podpora |
|----------|------|-------------|----------------|
| **PostgreSQL Flexible** | B1ms (1vCore, 2GB) | ~$12-15 | ✅ Výborná |
| **PostgreSQL Flexible** | Free tier | $0 (750h/mes) | ✅ Výborná |
| Azure SQL Database | Basic (5 DTU) | ~$5 | ✅ Dobrá |
| Azure SQL Database | S0 (10 DTU) | ~$15 | ✅ Dobrá |
| MySQL Flexible | B1s | ~$12 | ⚠️ Základná |

## 🚀 Setup - Lokálny vývoj

### 1. Konfigurácia User Secrets

```bash
cd Website.OqtaneCMS.AppHost
dotnet user-secrets init
dotnet user-secrets set "Parameters:pg-username" "postgres"
dotnet user-secrets set "Parameters:pg-password" "YourSecurePassword123!"
```

### 2. Spustenie lokálneho PostgreSQL (cez Aspire)

```bash
dotnet run --project Website.OqtaneCMS.AppHost
```

Aspire automaticky:
- Spustí PostgreSQL Docker container
- Vytvorí databázu `oqtane`
- Nastaví connection string do aplikácie
- Spustí pgAdmin na http://localhost:60751

### 3. pgAdmin pripojenie

- **URL**: http://localhost:60751
- **Email**: admin@admin.com (default)
- **Password**: admin (default)

Pridať server:
- **Host**: postgres (container name)
- **Port**: 5432
- **Database**: oqtane
- **Username**: postgres
- **Password**: (vaše heslo z user secrets)

## ☁️ Azure Deployment

### Vytvorenie Azure PostgreSQL Database

#### Option 1: Azure CLI

```bash
# Login
az login

# Vytvorenie resource group
az group create --name rg-artportfolio --location westeurope

# Vytvorenie PostgreSQL Flexible Server (Burstable B1ms tier)
az postgres flexible-server create \
  --resource-group rg-artportfolio \
  --name artportfolio-pg \
  --location westeurope \
  --admin-user pgadmin \
  --admin-password "YourSecurePassword123!" \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --version 16 \
  --storage-size 32 \
  --public-access 0.0.0.0 \
  --tags "Environment=Production" "App=Website.OqtaneCMS"

# Vytvorenie databázy
az postgres flexible-server db create \
  --resource-group rg-artportfolio \
  --server-name artportfolio-pg \
  --database-name oqtane

# Firewall rule (povoliť Azure services)
az postgres flexible-server firewall-rule create \
  --resource-group rg-artportfolio \
  --name artportfolio-pg \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

#### Option 2: Azure Portal

1. Otvorte Azure Portal
2. "Create a resource" → "Azure Database for PostgreSQL"
3. Vyberte **Flexible server**
4. Konfigurácia:
   - **Subscription**: Vaša subscription
   - **Resource group**: rg-artportfolio (nová)
   - **Server name**: artportfolio-pg
   - **Region**: West Europe
   - **PostgreSQL version**: 16
   - **Compute + storage**: 
     - Tier: **Burstable**
     - Compute size: **Standard_B1ms** (1 vCore, 2 GiB RAM)
     - Storage: **32 GiB**
   - **Admin username**: pgadmin
   - **Password**: (silné heslo)
5. **Networking**:
   - Public access: Yes
   - Firewall rules: Allow Azure services
6. Review + Create

### Connection String

Po vytvorení servera:

```
Host=artportfolio-pg.postgres.database.azure.com;Database=oqtane;Username=pgadmin;Password=YourPassword;SSL Mode=Require
```

## 🔐 Azure Container Apps konfigurácia

### Environment Variables

V Azure Container Apps nastavte:

```bash
az containerapp update \
  --name artportfolio-web \
  --resource-group rg-artportfolio \
  --set-env-vars \
    "ConnectionStrings__DefaultConnection=Host=artportfolio-pg.postgres.database.azure.com;Database=oqtane;Username=pgadmin;Password=YourPassword;SSL Mode=Require" \
    "Installation__InstallationMode=None" \
    "Oqtane__DatabaseType=PostgreSQL"
```

### Alebo použite Azure Key Vault

```bash
# Vytvorenie Key Vault
az keyvault create \
  --name kv-artportfolio \
  --resource-group rg-artportfolio \
  --location westeurope

# Uloženie connection string
az keyvault secret set \
  --vault-name kv-artportfolio \
  --name "ConnectionStrings--DefaultConnection" \
  --value "Host=artportfolio-pg.postgres.database.azure.com;Database=oqtane;Username=pgadmin;Password=YourPassword;SSL Mode=Require"

# V Program.cs pridať Key Vault integration (ServiceDefaults)
```

## 🔄 Migrácia z SQLite na PostgreSQL

Ak už máte dáta v SQLite:

### 1. Export SQLite dát

```bash
# Pomocou SQLite CLI
sqlite3 Oqtane.db .dump > oqtane_dump.sql
```

### 2. Import do PostgreSQL

```bash
# Upravte SQL pre PostgreSQL (napr. INTEGER PRIMARY KEY → SERIAL PRIMARY KEY)
# Potom importujte
psql -h artportfolio-pg.postgres.database.azure.com -U pgadmin -d oqtane -f oqtane_dump.sql
```

### 3. Alebo použite nástroj

- **pgLoader**: https://pgloader.io/
- **Oqtane Export/Import**: Cez admin panel

## 📊 Monitoring

### Azure Monitor

```bash
# Povoliť diagnostiku
az postgres flexible-server parameter set \
  --resource-group rg-artportfolio \
  --server-name artportfolio-pg \
  --name log_statement \
  --value all
```

### Application Insights

Aspire ServiceDefaults automaticky integruje Application Insights pre telemetriu.

## 💡 Optimalizácie

### Connection Pooling

PostgreSQL má efektívny connection pooling. V `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=oqtane;Username=pgadmin;Password=...;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100"
  }
}
```

### Backup

Azure automaticky robí backupy (7-35 dní retention). Pre manuálny backup:

```bash
az postgres flexible-server backup create \
  --resource-group rg-artportfolio \
  --name artportfolio-pg \
  --backup-name manual-backup-$(date +%Y%m%d)
```

## 🆓 Free Tier Usage

Azure PostgreSQL Free Tier (preview):
- **750 hodín/mesiac** Burstable B1ms instance
- **32 GiB storage**
- **Platné 12 mesiacov** od vytvorenia účtu

To stačí pre **24/7 prevádzku** menšieho portfólia!

## 🔗 Užitočné linky

- [Azure PostgreSQL Pricing](https://azure.microsoft.com/pricing/details/postgresql/flexible-server/)
- [Oqtane PostgreSQL Provider](https://github.com/oqtane/oqtane.databases/tree/master/Oqtane.Database.PostgreSQL)
- [.NET Aspire PostgreSQL](https://learn.microsoft.com/dotnet/aspire/database/postgresql-component)

---

**Odporúčanie**: Začnite s **Burstable B1ms tier** (~$12/mesiac) alebo **Free tier** (ak máte nárok). Pre produkčný web s ilustráciami to plne postačuje.
