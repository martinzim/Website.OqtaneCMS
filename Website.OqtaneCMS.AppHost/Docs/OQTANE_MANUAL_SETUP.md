# Manuálne nastavenie Oqtane pre PostgreSQL

Ak Installation Wizard neukazuje Database Types, nastav to manuálne:

## Krok 1: Zastav aplikáciu (Ctrl+C)

## Krok 2: Uprav appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=artportfolio;Username=oqtane;Password=OqtaneDevPassword123!"
  },
  "Installation": {
    "InstallationMode": "Production"
  },
  "Oqtane": {
    "InstallationFiles": "wwwroot",
    "Runtime": "Server",
    "SiteTemplate": "Default Site Template",
    "DatabaseType": "PostgreSQL"
  }
}
```

## Krok 3: Spusti aplikáciu znova

```powershell
.\start-dev.ps1
```

## Krok 4: Oqtane by mal automaticky vytvoriť databázovú schému

Oqtane detekuje že:
- `DatabaseType` je nastavený
- `DefaultConnection` existuje
- `InstallationMode` je "Production"

A automaticky vytvorí databázu bez wizardu.

## Overenie

Otvor pgAdmin a skontroluj, či v databáze `artportfolio` sú tabuľky začínajúce na `oqt_`.
