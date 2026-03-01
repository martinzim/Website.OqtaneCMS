# Environment Configuration Guide

Konfigurácia environment variables pre rôzne prostredia.

## 🏠 Lokálny vývoj (Aspire)

### User Secrets (Odporúčané)

```bash
cd Website.OqtaneCMS.AppHost
dotnet user-secrets init
dotnet user-secrets set "Parameters:pg-username" "postgres"
dotnet user-secrets set "Parameters:pg-password" "YourLocalPassword123!"
```

**Výhoda**: Heslo nie je v git repository.

### Alternatíva: appsettings.Development.json

```json
{
  "Parameters": {
    "pg-username": "postgres",
    "pg-password": "YourLocalPassword123!"
  }
}
```

⚠️ **POZOR**: Nepridávajte tento súbor do gitu!

---

## ☁️ Azure Container Apps

### Environment Variables

Pri deployment do Azure Container Apps nastavte:

```bash
az containerapp create \
  --name artportfolio-web \
  --resource-group rg-artportfolio \
  --environment aca-env \
  --image yourregistry.azurecr.io/artportfolio-web:latest \
  --env-vars \
    "ConnectionStrings__DefaultConnection=secretref:db-connection-string" \
    "Installation__InstallationMode=None" \
    "Oqtane__DatabaseType=PostgreSQL" \
    "Oqtane__Runtime=Server" \
    "ASPNETCORE_ENVIRONMENT=Production"
```

### Azure Container Apps Secrets

```bash
az containerapp secret set \
  --name artportfolio-web \
  --resource-group rg-artportfolio \
  --secrets \
    db-connection-string="Host=artportfolio-pg.postgres.database.azure.com;Database=oqtane;Username=pgadmin;Password=YourSecurePassword;SSL Mode=Require"
```

---

## 🔐 Azure Key Vault (Produkcia)

### 1. Vytvorenie Key Vault

```bash
az keyvault create \
  --name kv-artportfolio \
  --resource-group rg-artportfolio \
  --location westeurope
```

### 2. Uloženie secrets

```bash
# Database connection string
az keyvault secret set \
  --vault-name kv-artportfolio \
  --name "ConnectionStrings--DefaultConnection" \
  --value "Host=artportfolio-pg.postgres.database.azure.com;Database=oqtane;Username=pgadmin;Password=YourPassword;SSL Mode=Require"

# Oqtane installation mode
az keyvault secret set \
  --vault-name kv-artportfolio \
  --name "Installation--InstallationMode" \
  --value "None"
```

### 3. Integrácia s Container Apps

```bash
# Enable managed identity
az containerapp identity assign \
  --name artportfolio-web \
  --resource-group rg-artportfolio \
  --system-assigned

# Get the identity principal ID
PRINCIPAL_ID=$(az containerapp identity show \
  --name artportfolio-web \
  --resource-group rg-artportfolio \
  --query principalId -o tsv)

# Grant access to Key Vault
az keyvault set-policy \
  --name kv-artportfolio \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list
```

### 4. Program.cs integrácia

```csharp
var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var keyVaultName = builder.Configuration["KeyVault:Name"];
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    
    builder.Configuration.AddAzureKeyVault(
        keyVaultUri,
        new DefaultAzureCredential());
}
```

---

## 📝 Kompletný zoznam environment variables

### Povinné

| Variable | Popis | Príklad |
|----------|-------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=...;Database=oqtane;...` |
| `Installation__InstallationMode` | Oqtane install mode | `Install` alebo `None` |
| `Oqtane__DatabaseType` | Typ databázy | `PostgreSQL` |

### Odporúčané

| Variable | Popis | Default | Príklad |
|----------|-------|---------|---------|
| `Oqtane__Runtime` | Blazor runtime mode | `Server` | `Server` |
| `Oqtane__SiteTemplate` | Default site template | - | `Default Site Template` |
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Production` | `Production` |
| `ASPNETCORE_URLS` | Listening URLs | `http://+:8080` | `http://+:8080` |

### Voliteľné

| Variable | Popis | Default |
|----------|-------|---------|
| `Logging__LogLevel__Default` | Default log level | `Information` |
| `Logging__LogLevel__Microsoft.AspNetCore` | ASP.NET log level | `Warning` |
| `Logging__LogLevel__Npgsql` | PostgreSQL log level | `Warning` |
| `AllowedHosts` | Allowed host headers | `*` |

---

## 🧪 Testovanie (Staging)

### appsettings.Staging.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Installation": {
    "InstallationMode": "None"
  },
  "Oqtane": {
    "DatabaseType": "PostgreSQL",
    "Runtime": "Server"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Npgsql": "Information"
    }
  }
}
```

### Staging Container Apps

```bash
az containerapp create \
  --name artportfolio-web-staging \
  --resource-group rg-artportfolio-staging \
  --environment aca-env-staging \
  --image yourregistry.azurecr.io/artportfolio-web:staging \
  --env-vars \
    "ConnectionStrings__DefaultConnection=secretref:db-connection-string-staging" \
    "Installation__InstallationMode=None" \
    "Oqtane__DatabaseType=PostgreSQL" \
    "ASPNETCORE_ENVIRONMENT=Staging"
```

---

## 🔍 Debugging Environment Variables

### Výpis všetkých env vars (Development)

```csharp
// Program.cs
if (app.Environment.IsDevelopment())
{
    var config = app.Configuration as IConfigurationRoot;
    var debugView = config.GetDebugView();
    app.Logger.LogInformation("Configuration: {ConfigDebugView}", debugView);
}
```

### Kontrola v Container Apps

```bash
# List all environment variables
az containerapp show \
  --name artportfolio-web \
  --resource-group rg-artportfolio \
  --query properties.template.containers[0].env

# Check logs
az containerapp logs show \
  --name artportfolio-web \
  --resource-group rg-artportfolio \
  --follow
```

---

## ⚠️ Security Best Practices

### ❌ NIKDY

```bash
# ❌ Necommitujte secrets do gitu
"ConnectionStrings": {
  "DefaultConnection": "Host=server;Password=SecretPassword123"
}

# ❌ Nepoužívajte weak passwords
"pg-password": "123456"

# ❌ Nepoužívajte plain text v production
--env-vars "Password=MySecretPass"
```

### ✅ VŽDY

```bash
# ✅ Používajte User Secrets lokálne
dotnet user-secrets set "Parameters:pg-password" "StrongPassword123!@#"

# ✅ Používajte Key Vault v production
az keyvault secret set --vault-name kv-app --name "DbPassword" --value "..."

# ✅ Používajte secretref v Container Apps
--env-vars "ConnectionString=secretref:db-connection-string"

# ✅ Používajte strong passwords
openssl rand -base64 32
```

---

## 📋 Checklist pre deployment

### Lokálny vývoj
- [ ] User secrets nastavené
- [ ] Docker Desktop spustený
- [ ] Aspire funguje (`dotnet run --project Website.OqtaneCMS.AppHost`)

### Azure Staging
- [ ] PostgreSQL Flexible Server vytvorený
- [ ] Container Apps environment vytvorený
- [ ] Secrets nastavené v Container Apps
- [ ] Connection string testovaný
- [ ] Firewall rules nastavené
- [ ] Installation wizard dokončený
- [ ] `InstallationMode` zmenený na `None`

### Azure Production
- [ ] Key Vault vytvorený
- [ ] Managed identity enabled
- [ ] Key Vault access policy nastavený
- [ ] Všetky secrets v Key Vault
- [ ] Backups konfigurované
- [ ] Monitoring enabled (Application Insights)
- [ ] SSL/TLS enforced
- [ ] Custom domain nastavená (ak používate)

---

## 🔗 Súvisiace dokumenty

- [QUICKSTART.md](QUICKSTART.md) - Rýchle spustenie
- [AZURE_POSTGRESQL.md](AZURE_POSTGRESQL.md) - Azure database setup
- [README.md](../README.md) - Hlavná dokumentácia

---

**Aktualizované**: 2025-01-XX  
**Verzia**: 1.0
