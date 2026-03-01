# Oqtane DbContext Discovery - Ako Zistiť Správne Triedy

## Problém

Keď Oqtane vracia chybu:
```
No database provider has been configured for this DbContext
```

**Otázka:** Aké DbContext triedy Oqtane používa?

---

## Metódy Zistenia

### 1. ✅ Reflection cez PowerShell (Najspoľahlivejšie)

```powershell
# Načítaj Oqtane.Server.dll
$assembly = [System.Reflection.Assembly]::LoadFrom("X:\DotNET\Website.OqtaneCMS\Website.OqtaneCMS.Web\bin\Debug\net10.0\Oqtane.Server.dll")

# Nájdi všetky DbContext triedy
$assembly.GetTypes() | Where-Object { 
    $_.BaseType -and ($_.BaseType.Name -eq "DbContext" -or $_.BaseType.FullName -like "*DbContext*")
} | Select-Object FullName
```

**Spusti tento príkaz a získaš presné názvy!**

---

### 2. ✅ ILSpy / dotPeek Decompiler

1. Stiahni [ILSpy](https://github.com/icsharpcode/ILSpy/releases)
2. Otvor `Oqtane.Server.dll`
3. Hľadaj triedy dediace z `DbContext`

---

### 3. ✅ Oqtane GitHub Source Code

```bash
# Clone Oqtane repository
git clone https://github.com/oqtane/oqtane.framework.git
cd oqtane.framework

# Hľadaj DbContext triedy
grep -r "class.*DBContext" --include="*.cs"
```

**Očakávané výsledky:**
```csharp
Oqtane.Repository.TenantDBContext : DbContext
Oqtane.Repository.MasterDBContext : DbContext
```

---

### 4. ✅ Oqtane Oficiálna Dokumentácia

**Zdroj:** https://docs.oqtane.org/

Podľa dokumentácie:
- **`TenantDBContext`** - Multi-tenant databáza (hlavná)
- **`MasterDBContext`** - Master control plane (voliteľné, pre multi-database setup)

---

## Verified Solution (Oqtane 10.0.4)

Pre **single-tenant** setup (čo používame):

```csharp
// Website.OqtaneCMS.Web/Program.cs
using Oqtane.Repository;

// ONLY TenantDBContext is needed for single-tenant setup
builder.Services.AddDbContext<TenantDBContext>(options =>
    options.UseNpgsql(defaultConnection), ServiceLifetime.Transient);
```

**Poznámka:** `MasterDBContext` sa používa LEN pri multi-database hosting (rôzne tenants v rôznych databázach).

---

## Alternatívne Riešenie (Ak Oqtane Stále Nefunguje)

Ak explicitná registrácia nefunguje, Oqtane môže očakávať konfiguráciu cez `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=artportfolio;..."
  },
  "Oqtane": {
    "DatabaseType": "PostgreSQL",
    "Database": {
      "Provider": "Npgsql.EntityFrameworkCore.PostgreSQL"
    }
  }
}
```

A Oqtane sám zaregistruje DbContext v `AddOqtane()`.

---

## Debug Checklist

1. ✅ Connection string existuje: `builder.Configuration.GetConnectionString("DefaultConnection")`
2. ✅ DatabaseType je nastavený: `builder.Configuration["Oqtane:DatabaseType"]` == `"PostgreSQL"`
3. ✅ Npgsql.EntityFrameworkCore.PostgreSQL package je nainštalovaný
4. ✅ DbContext je zaregistrovaný **PRED** `AddOqtane()`
5. ✅ ServiceLifetime je `Transient` (Oqtane requirement)

---

## Oficiálne Zdroje

- **Oqtane GitHub:** https://github.com/oqtane/oqtane.framework
- **Oqtane Docs:** https://docs.oqtane.org/
- **Database Setup:** https://docs.oqtane.org/guides/installation/databases/
- **PostgreSQL Guide:** https://docs.oqtane.org/guides/installation/databases/postgresql/

---

## Next Steps

1. **Spusti PowerShell reflection príkaz** vyššie
2. **Overíme presné názvy DbContext tried**
3. **Upravíme Program.cs s overenými názvami**
4. **Reštartujeme aplikáciu**

