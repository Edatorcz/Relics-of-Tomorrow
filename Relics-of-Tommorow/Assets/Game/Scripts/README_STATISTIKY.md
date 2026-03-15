# 📊 Systém herních statistik

## Přehled

Tento systém sleduje a zobrazuje různé herní statistiky hráče:
- 💀 Celkový počet úmrtí
- 🔄 Počet opakování hry
- 🌍 Návštěvy jednotlivých epoch (Pravěk, Starověk, Středověk, Budoucnost)
- ⏱️ Celkový čas hraní

## Soubory

### 1. `GameStatistics.cs`
- **Umístění:** `Assets/Game/Scripts/Managers/`
- **Typ:** Singleton Manager
- **Popis:** Centrální třída pro sledování a ukládání statistik pomocí PlayerPrefs

**Hlavní metody:**
- `RecordDeath()` - Zaznamenat smrt hráče
- `RecordNewGame()` - Zaznamenat začátek nové hry
- `RecordEpochVisit(string epochName)` - Zaznamenat návštěvu epochy
- `GetTotalDeaths()` - Získat počet úmrtí
- `GetTotalGames()` - Získat počet her
- `Get[Epocha]Visits()` - Získat návštěvy konkrétní epochy
- `GetTotalPlaytimeFormatted()` - Získat čas hraní ve formátu HH:MM:SS
- `ResetAllStatistics()` - Resetovat všechny statistiky

### 2. `StatisticsUI.cs`
- **Umístění:** `Assets/Game/Scripts/UI/`
- **Typ:** UI Component
- **Popis:** Dialog pro zobrazení statistik v herním UI

**Ovládání:**
- Stiskni **[O]** pro otevření/zavření
- Tažení okna myší

**Zobrazované sekce:**
- ⚔️ Obecné (úmrtí, opakování, celkové návštěvy)
- 🌍 Návštěvy epoch (s nejnavštěvovanější epochou)
- ⏱️ Čas hraní
- ⚠️ Tlačítko pro reset statistik

### 3. Integrace s existujícími systémy

#### `EpochManager.cs`
- Automaticky zaznamenává návštěvy epoch při načtení scény
- Zaznamenává novou hru při návratu do Pravěku

#### `PlayerHealth.cs`
- Zaznamenává úmrtí hráče při smrti

## Jak nastavit v Unity

### Krok 1: Přidat GameStatistics do scény
1. Vytvoř prázdný GameObject ve scéně (např. v menu scéně nebo ve scéně, která se nikdy nenačte)
2. Pojmenuj ho "GameStatistics"
3. Přidej component `GameStatistics.cs`
4. ✅ GameStatistics má DontDestroyOnLoad, takže přežije všechny scény

### Krok 2: Přidat StatisticsUI do potřebných scén
1. V každé scéně, kde chceš zobrazit statistiky (např. ve všech epochách)
2. Vytvoř prázdný GameObject
3. Pojmenuj ho "StatisticsUI"
4. Přidej component `StatisticsUI.cs`
5. (Volitelně) Nastav vlastní klávesovou zkratku místo [O]
6. (Volitelně) Uprav barvy v inspectoru

### Krok 3: Otestovat
1. Spusť hru
2. Stiskni [O] pro otevření statistik
3. Hraj hru, umři pár krát
4. Kontroluj, že se statistiky aktualizují

## Konfigurace

### GameStatistics
Všechny statistiky se ukládají do `PlayerPrefs` s následujícími klíči:
- `Stats_TotalDeaths`
- `Stats_TotalGames`
- `Stats_PravekVisits`
- `Stats_StarovekVisits`
- `Stats_StredovekVisits`
- `Stats_BudoucnostVisits`
- `Stats_TotalPlaytime`

### StatisticsUI
V Unity Inspectoru můžeš upravit:
- `Toggle Key` - Klávesa pro otevření/zavření (default: O)
- `Show On Start` - Zobrazit při startu hry
- Barvy UI (Background, Header, Label, Value, Epoch)

## Persistence

Statistiky jsou **persistentní** mezi sezeními:
- Ukládají se do PlayerPrefs
- Přežijí restart hry
- Přežijí restart Unity editoru
- ⚠️ Smazání PlayerPrefs vymaže všechny statistiky

### Jak vymazat statistiky

**V kódu:**
```csharp
GameStatistics.Instance.ResetAllStatistics();
```

**V Unity editoru:**
```
Edit → Clear All PlayerPrefs
```

**V UI:**
- Otevři statistiky (O)
- Klikni na tlačítko "⚠ Resetovat statistiky"

## Debug

Pro testování můžeš ručně zavolat:
```csharp
// Simulovat smrt
GameStatistics.Instance.RecordDeath();

// Simulovat novou hru
GameStatistics.Instance.RecordNewGame();

// Simulovat návštěvu epochy
GameStatistics.Instance.RecordEpochVisit("Pravěk");
```

## Poznámky

- GameStatistics je Singleton - automaticky se vytváří instance při prvním použití
- StatisticsUI automaticky najde GameStatistics.Instance
- Čas hraní se měří v `Time.realtimeSinceStartup` (i když je hra pozastavená)
- Při zavření aplikace se statistiky automaticky uloží

## Možná rozšíření

V budoucnu můžeš přidat další statistiky:
- Počet zabitých nepřátel
- Počet sebraných artefaktů
- Nejrychlejší průchod epochou
- Nejdelší survival streak
- Používané zbraně
- Způsobené damage
- Aktivované portály
- atd.

Stačí přidat:
1. Novou proměnnou do GameStatistics
2. Gettr metodu
3. UI pro zobrazení v StatisticsUI
4. Volání Record metody v příslušném systému
