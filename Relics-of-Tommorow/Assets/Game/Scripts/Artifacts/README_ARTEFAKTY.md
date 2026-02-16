# Systém Artefaktů - Relics of Tomorrow

## Přehled

Systém artefaktů poskytuje hráči dočasné boosty během průchodu epochami. Každý artefakt je platný pouze pro aktuální epochu a jeho efekt zmizí po smrti hráče nebo poražení bosse.

## Vytvořené Soubory

### Core Systém
- **ArtifactData.cs** - ScriptableObject definující vlastnosti artefaktu
- **ArtifactManager.cs** - Singleton manager spravující aktivní artefakty
- **ArtifactPickup.cs** - Komponent pro sebrání artefaktu ve světě
- **ArtifactDatabase.cs** - Dokumentace a reference všech artefaktů
- **ArtifactUI.cs** - UI zobrazení aktivních artefaktů

### Integrace do Player systémů
- Upraveno: **PlayerHealth.cs** - Podpora defense a regen modifierů
- Upraveno: **PlayerCombat.cs** - Podpora attack speed, crit chance, life steal
- Upraveno: **PlayerMovement.cs** - Podpora speed modifieru
- Upraveno: **EpochManager.cs** - Reset artefaktů při smrti
- Upraveno: **BossPortalSpawner.cs** - Reset artefaktů při poražení bosse

## Navržené Artefakty

### PRAVĚK
1. **Ooga Booga** - +50% damage
2. **Sběr Bobulí** - 3x rychlejší regenerace
3. **Síla Mamuta** - +50 HP a +20% defense

### STAROVĚK
4. **Sparťanský Štít** - +40% defense
5. **Olympijský Oheň** - +40% speed, +30% stamina
6. **Filosof Kámen** - 25% crit chance, +15% damage

### STŘEDOVĚK
7. **Fragment Excaliburu** - +80% damage, 15% crit
8. **Dračí Šupina** - +50% defense, +30 HP
9. **Rytířská Ctnost** - +60% attack speed, +25% movement

### BUDOUCNOST
10. **Nano Reaktor** - 20% life steal, 2.5x regenerace
11. **Kvantový Chip** - 1.5x dash recharge, +30% speed
12. **Plazmový Katalyzátor** - +100% damage, +50% speed

## Jak Vytvořit Artefakt v Unity

### Krok 1: Vytvoř ScriptableObject

1. V Unity Project okně, pravé tlačítko myši
2. **Create > Relics of Tomorrow > Artifact**
3. Pojmenuj artefakt (např. "Ooga_Booga")

### Krok 2: Nastav Parametry

Podle dokumentace v `ArtifactDatabase.cs` nastav:
- **Artifact Name** - Jméno pro zobrazení
- **Description** - Popis efektu
- **Icon** - Sprite ikona
- **World Model Prefab** - 3D model (volitelné)
- **Epoch** - Ke které epoše patří
- **Effect Type** - Typ hlavního efektu
- **Effect Value** - Síla efektu
- **Secondary Effect** - Druhý efekt (volitelné)
- **Aura Color** - Barva aury kolem hráče

### Příklad: Ooga Booga
```
Artifact Name: Ooga Booga
Description: Prastaré bojové heslo tvých předků. Zvyšuje sílu tvých úderů o 50%!
Epoch: Pravek
Effect Type: DamageBoost
Effect Value: 1.5
Aura Color: RGB(1, 0.5, 0, 0.3)
```

### Krok 3: Vytvoř Pickup ve Světě

1. Vytvoř prázdný GameObject ve scéně
2. Přidej komponent **ArtifactPickup**
3. Přiřaď vytvořený ArtifactData do pole "Artifact Data"
4. (Volitelné) Přiřaď 3D model nebo nech vytvořit defaultní

## Setup v Unity

### 1. Vytvoř ArtifactManager GameObject

```
1. Create Empty GameObject "ArtifactManager"
2. Add Component: ArtifactManager
3. Přiřaď reference (automaticky se najdou při startu):
   - Player Health
   - Player Combat
   - Player Movement
```

### 2. Vytvoř ArtifactUI (volitelné)

```
1. V Canvas vytvoř Empty GameObject "ArtifactUI"
2. Add Component: ArtifactUI
3. Vytvoř Container pro artifact sloty
```

### 3. Přidej Layer pro Pickup

```
1. Edit > Project Settings > Tags and Layers
2. Přidej layer "Player" pokud neexistuje
3. V ArtifactPickup nastav Player Layer Mask
```

## Použití v Kódu

### Aktivovat Artefakt

```csharp
ArtifactData artifact = ...; // Reference na tvůj artifact
ArtifactManager.Instance.ActivateArtifact(artifact);
```

### Kontrola Aktivních Artefaktů

```csharp
if (ArtifactManager.Instance.HasArtifact(artifactData))
{
    Debug.Log("Hráč má tento artefakt!");
}

int count = ArtifactManager.Instance.GetActiveArtifactCount();
```

### Vyčistit Artefakty

```csharp
// Automaticky se volá při smrti nebo poražení bosse
ArtifactManager.Instance.ClearAllArtifacts();
```

## Přidání Vlastního Efektu

1. Přidej nový typ do `ArtifactEffectType` enum v ArtifactData.cs
2. Implementuj logiku v `ApplySingleEffect()` v ArtifactManager.cs
3. Přidej reset logiku do `ResetAllModifiers()`

### Příklad:

```csharp
// V ArtifactData.cs
public enum ArtifactEffectType
{
    // ... existující
    JumpBoost,  // Nový efekt
}

// V ArtifactManager.cs - ApplySingleEffect()
case ArtifactEffectType.JumpBoost:
    jumpMultiplier += value - 1f;
    if (playerMovement != null)
        playerMovement.ApplyJumpMultiplier(jumpMultiplier);
    break;
```

## Testování

1. Vytvoř testovací artefakt s jednoduchým efektem (např. speed boost)
2. Umísti ArtifactPickup do scény
3. Spusť hru a seberte artefakt
4. Zkontroluj Console log - měly by se objevit zprávy o aktivaci
5. Otestuj efekt (např. rychlejší pohyb)
6. Zemři nebo poraz bosse - artefakt by měl zmizet

## Známé Problémy a Řešení

### Artefakt se nesebrá
- Zkontroluj Player Layer v ArtifactPickup
- Ujisti se, že hráč má tag "Player"
- Zkontroluj pickup radius

### Efekt nefunguje
- Zkontroluj Console log pro chybové hlášky
- Ujisti se, že ArtifactManager existuje ve scéně
- Ověř, že PlayerHealth/Combat/Movement mají správné reference

### Artefakt nezaniká
- Zkontroluj, že PlayerHealth má OnPlayerDied event
- Ujisti se, že EpochManager a BossPortalSpawner volají ClearAllArtifacts()

## Rozšíření

### Možnosti dalšího vývoje:
- Stackování více artefaktů stejného typu
- Rare/Epic/Legendary varity s lepšími efekty
- Negativní efekty (risk/reward)
- Kombo efekty mezi artefakty
- Particle systém pro auru
- Zvukové efekty
- Animace při aktivaci
- Savování artefaktů mezi sezeními (persistentní)

## Support

Pro více informací viz dokumentace v:
- `ArtifactDatabase.cs` - Kompletní seznam všech artefaktů
- Komentáře v jednotlivých skriptech
- Unity Tooltips v Inspectoru
