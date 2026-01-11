# DBProjekt – Půjčovna (D1)

Tento projekt je webová aplikace vytvořená v ASP.NET Core (Razor Pages), která slouží k evidenci uživatelů, majetku a výpůjček.
Aplikace využívá relační databázi Microsoft SQL Server a je vytvořena jako součást databázového portfolia.

Projekt splňuje zadání **D1 – Repository pattern**.

---

## Použité technologie
- ASP.NET Core (Razor Pages)
- Microsoft SQL Server
- C#
- ADO.NET (bez ORM / Entity Framework)
- Repository pattern

---

## Databáze
- Databázový systém: **Microsoft SQL Server**
- Databáze obsahuje:
  - 9 tabulek
  - vazbu M:N (Loans ↔ Assets)
  - 2 databázové pohledy (VIEW)
  - datové typy: string, number, float, bool, enum (CHECK), date/datetime

---

## Architektura aplikace
Aplikace je rozdělena do vrstev:
- **Pages** – uživatelské rozhraní (Razor Pages)
- **Services** – aplikační logika a transakce
- **Repositories** – práce s databází (Repository pattern)
- **Data** – databázová konfigurace a připojení

---

## Funkce aplikace
- CRUD operace nad uživateli
- CRUD operace nad majetkem
- Vytvoření výpůjčky jedním formulářem (zápis do více tabulek)
- Transakční převod bodů mezi uživateli
- Souhrnný report s agregacemi
- Import dat z CSV a JSON
- Konfigurovatelné připojení k databázi

---

## Vytvoření databáze
Databáze se vytváří pomocí SQL skriptů uložených ve složce `/sql`.

Postup:
1. Otevřete Microsoft SQL Server Management Studio (SSMS)
2. Připojte se k databázovému serveru (např. `localhost`)
3. Postupně spusťte následující skripty:
   - `CreateScript.sql` – vytvoření tabulek a vazeb
   - `ViewScript.sql` – vytvoření databázových pohledů
   - `ProcedureScript.sql` – vytvoření uložených procedur
   - `SeedDataScript.sql` – vložení testovacích dat (volitelné)

Po spuštění skriptů je databáze připravena k použití.

---

## Konfigurace databáze
Připojení k databázi se nastavuje v souboru:

src/PujcovnaUi/appsettings.json

Ukázka konfigurace:


{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=pujcovna;Trusted_Connection=True;TrustServerCertificate=True"
  }
}

---

## Spuštění aplikace
Aplikaci lze spustit bez použití vývojového prostředí (IDE) pomocí příkazové řádky.

Postup:
1. Otevřete příkazový řádek (Command Prompt / PowerShell)
2. Přejděte do složky aplikace a do terminálu napište:
   
   cd src/PujcovnaUi
3. Spusťte aplikaci
   
   dotnet run

Po úspěšném spuštění je aplikace dostupná v prohlížeči na adrese:
   http://localhost:5110
   