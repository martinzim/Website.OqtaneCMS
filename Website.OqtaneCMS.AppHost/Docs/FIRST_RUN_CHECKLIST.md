# ✅ Prvotné Spustenie - Checklist

## Pred Spustením

- [ ] **.NET 10 SDK** nainštalované
  ```powershell
  dotnet --version
  # Očakávaný výstup: 10.x.x
  ```

- [ ] **Docker Desktop** spustený
  ```powershell
  docker ps
  # Ak funguje, zobrazí sa zoznam kontajnerov (môže byť prázdny)
  ```

- [ ] **Visual Studio 2026** alebo **VS Code** nainštalované

- [ ] **Git repository** naklonovaný
  ```powershell
  cd X:\DotNET\Website.OqtaneCMS
  ```

## Prvé Spustenie

### Krok 1: Spustiť Aspire

```powershell
# Option A: Quick start script
.\start-dev.ps1

# Option B: Manuálne
dotnet run --project Website.OqtaneCMS.AppHost
```

**Očakávaný výstup:**
```
info: Aspire.Hosting[0]
      Aspire version: 13.0.xxx
info: Aspire.Hosting.DistributedApplication[0]
      Now listening on: http://localhost:15xxx
      Application started. Press Ctrl+C to shut down.
```

### Krok 2: Overiť Aspire Dashboard

- [ ] Aspire Dashboard sa otvorí automaticky v prehliadači
- [ ] URL: `http://localhost:15xxx` (zobrazí sa v konzole)
- [ ] Vidíš 3 resources:
  - [ ] `postgres` - PostgreSQL container (Status: Running)
  - [ ] `pgAdmin` - Database admin tool (Status: Running)
  - [ ] `artportfolio-web` - Main application (Status: Running)

### Krok 3: Otvor Aplikáciu

- [ ] Klikni na `artportfolio-web` v Aspire Dashboard
- [ ] Alebo otvor priamo: `https://localhost:7xxx` (port sa zobrazí v Dashboard)
- [ ] **Očakávané:** Oqtane Installation Wizard sa zobrazí

### Krok 4: Oqtane Installation Wizard

#### Screen 1: Database Connection
- [ ] **Database Type:** PostgreSQL ✅ (už vybraté)
- [ ] **Connection String:** ✅ (automaticky injektovaný)
- [ ] Klikni **Next**

#### Screen 2: Site Configuration
- [ ] **Site Name:** `Website.OqtaneCMS Dev`
- [ ] **Site Template:** Default Site Template
- [ ] **Runtime:** Server
- [ ] **Alias:** `localhost:7xxx` ✅ (automaticky)
- [ ] Klikni **Next**

#### Screen 3: Admin Account
- [ ] **Username:** `admin`
- [ ] **Email:** `admin@artportfolio.local`
- [ ] **Password:** [Tvoje silné heslo]
- [ ] **Confirm Password:** [Rovnaké heslo]
- [ ] ⚠️ **Poznač si heslo!**
- [ ] Klikni **Next**

#### Screen 4: SMTP (Optional)
- [ ] Pre lokálny vývoj klikni **Skip**
- [ ] (Alebo vyplň SMTP server ak máš)

#### Screen 5: Install
- [ ] Klikni **Install**
- [ ] Počkaj 30-60 sekúnd
- [ ] **Očakávané:** "Installation completed successfully!"

### Krok 5: Po Inštalácii

- [ ] **Reštartuj aplikáciu**
  ```powershell
  # V konzole kde beží Aspire:
  Ctrl+C
  
  # Znova spusti:
  dotnet run --project Website.OqtaneCMS.AppHost
  ```

- [ ] Otvor aplikáciu znova
- [ ] **Očakávané:** Homepage s default obsahom (nie installation wizard)
- [ ] Vidíš "Login" button v pravom hornom rohu

### Krok 6: Prihlásenie

- [ ] Klikni na **Login**
- [ ] **Username:** `admin`
- [ ] **Password:** [Tvoje heslo z kroku 4]
- [ ] Klikni **Login**
- [ ] **Očakávané:** Prihlásený ako admin, vidíš edit toolbar

### Krok 7: Vyskúšaj Edit Mode

- [ ] Klikni na **Edit Mode** toggle (v toolbar)
- [ ] **Očakávané:** Moduly majú teraz edit ikony
- [ ] Klikni na gear ikonu pri akomkoľvek module
- [ ] Vyskúšaj zmeniť obsah
- [ ] Klikni **Save**
- [ ] Vypni **Edit Mode**
- [ ] **Očakávané:** Zmeny sú viditeľné

## Overenie Konfigurácie

### PostgreSQL

- [ ] Otvor pgAdmin: `http://localhost:60751`
- [ ] Prihlásenie:
  - Email: `admin@admin.com`
  - Password: `admin`
- [ ] Register Server:
  - Name: `Website.OqtaneCMS Local`
  - Host: `host.docker.internal`
  - Port: `5432`
  - Database: `artportfolio`
  - Username: `oqtane`
  - Password: `OqtaneDevPassword123!`
- [ ] **Očakávané:** Vidíš databázu `artportfolio` s 50+ tabulkami

### Aspire Dashboard Features

- [ ] **Resources tab:** Všetky services sú "Running"
- [ ] **Logs tab:** Vidíš logy z `artportfolio-web`
- [ ] **Metrics tab:** CPU/Memory metrics
- [ ] **Traces tab:** HTTP request traces

## Riešenie Problémov

### ❌ "Docker Desktop is not running"
```powershell
# Spusti Docker Desktop
# Počkaj kým sa načíta (ikona v system tray prestane blikať)
# Znova spusti Aspire
```

### ❌ "Port 5432 already in use"
```powershell
# Zastaviť existujúce PostgreSQL
docker stop $(docker ps -q --filter "ancestor=postgres")

# Alebo zmeň port v AppHost.cs (riadok 29)
```

### ❌ Oqtane Installation Wizard sa nezobrazí
```powershell
# Skontroluj appsettings.json
# Má byť:
"Installation": {
  "InstallationMode": "Install"
}
```

### ❌ "Database connection failed"
```powershell
# V Aspire Dashboard → artportfolio-web → Environment
# Skontroluj: ConnectionStrings__artportfolio
# Má obsahovať: Host=localhost;Port=5432;Database=artportfolio;...

# Alebo test v pgAdmin:
docker exec -it <postgres_container_id> psql -U oqtane -d artportfolio
```

### ❌ Aplikácia sa zasekla v "Install" režime
```json
// Manuálne uprav Website.OqtaneCMS.Web/appsettings.json
{
  "Installation": {
    "InstallationMode": "Production"
  }
}
```

## Hotovo! 🎉

Ak si prešiel všetkými krokmi, máš:

✅ Oqtane CMS beží lokálne  
✅ PostgreSQL databázu nakonfigurovanú  
✅ Admin prístup  
✅ Default homepage  
✅ pgAdmin pre správu databázy  
✅ Aspire Dashboard pre monitoring  

### Ďalšie Kroky

📖 **Vytvor vlastný module:** `docs/MODULE_DEVELOPMENT.md`  
📖 **Prispôsob theme:** `docs/THEME_DEVELOPMENT.md` (TODO)  
📖 **Deploy do Azure:** `docs/AZURE_DEPLOYMENT.md` (TODO)  

### Užitočné Príkazy

```powershell
# Spustiť dev
.\start-dev.ps1

# Zastaviť všetko
Ctrl+C

# Vyčistiť databázu (stratíš data!)
docker volume rm artportfolio-apphost-postgres-data

# Skontrolovať logy
# V Aspire Dashboard → Logs tab
```

---

**Potrebuješ pomoc?**  
- Dokumentácia: `docs/OQTANE_SETUP.md`
- GitHub Issues: https://github.com/martinzim/Website.OqtaneCMS/issues
