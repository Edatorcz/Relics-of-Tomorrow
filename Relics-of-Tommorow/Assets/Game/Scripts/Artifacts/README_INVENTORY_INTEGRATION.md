# Systém Artefaktů - Inventory Integration

## 🎮 Jak To Funguje

Artefakty jsou nyní **běžné itemy v inventáři**, které hráč:
1. **Sebere** do inventáře (jako jakýkoliv jiný item)
2. **Umístí** do hotbaru (sloty 1-9)
3. **Aktivuje** držením pravého tlačítka myši (RMB)

## ✨ Nové Funkce

### Držení Pro Aktivaci
- Hráč musí držet **pravé tlačítko myši (RMB)** po určitou dobu
- Výchozí čas: **1.5 sekundy** (nastavitelné per artifact)
- Vizuální progress bar ukazuje pokrok aktivace
- Po dokončení se artefakt aktivuje a zmizí z inventáře

### Integrace s Inventářem
- Artefakty se chovají jako normální itemy
- Můžete je přesouvat mezi sloty
- Stackable = false (každý artefakt je unikátní)
- Po použití zmizí z inventáře

## 📋 Nový Setup v Unity

### 1. Vytvoř Artifact Item

```
1. Pravé tlačítko > Create > Inventory > Item
2. Nastavení:
   - Item Name: např. "Ooga Booga"
   - Description: Popis efektu
   - Icon: Sprite ikona
   - Item Type: Artifact  (NOVÝ TYP!)
   - Max Stack Size: 1
   - Is Stackable: false
   
3. Artifact Settings sekce:
   - Artifact Data: Přiřaď ArtifactData ScriptableObject
   - Activation Hold Time: Jak dlouho držet RMB (default 1.5s)
```

### 2. Vytvoř ArtifactData

```
1. Pravé tlačítko > Create > Relics of Tomorrow > Artifact
2. Nastav parametry (damage boost, speed, atd.)
3. Nastav aura color, sounds, atd.
```

### 3. Propoj Je

```
V ItemData:
  Artifact Settings > Artifact Data = tvůj ArtifactData
```

### 4. Umísti do Světa

```
1. Vytvoř GameObject s ArtifactPickup
2. Přiřaď ItemData (ne ArtifactData!)
3. Item se přidá do inventáře při pickupu
```

### 5. Přidej ItemUseSystem

```
1. Na GameObject "GameManager" nebo "Player":
   - Add Component: ItemUseSystem
   - Přiřaď HotbarUI reference
   - (Volitelné) Přiřaď UI elementy pro progress bar
```

## 🎨 Vytvoření Artefaktu - Kompletní Příklad

### OOGA BOOGA

#### Krok 1: ArtifactData
```
Create > Relics of Tomorrow > Artifact
Název: "Ooga_Booga_Data"

Settings:
- Artifact Name: "Ooga Booga"
- Description: "Prastaré bojové heslo tvých předků..."
- Epoch: Pravek
- Effect Type: DamageBoost
- Effect Value: 1.5
- Aura Color: Orange (1, 0.5, 0, 0.3)
```

#### Krok 2: ItemData
```
Create > Inventory > Item
Název: "Ooga_Booga_Item"

Basic Info:
- Item Name: "Ooga Booga"
- Description: "Drž RMB pro aktivaci! +50% damage až do konce epochy"
- Icon: [Přiřaď sprite]
- Item Type: Artifact
- Max Stack Size: 1
- Is Stackable: false

Artifact Settings:
- Artifact Data: Ooga_Booga_Data (z kroku 1)
- Activation Hold Time: 1.5
```

#### Krok 3: Pickup ve Světě
```
1. Create Empty GameObject "Ooga_Booga_Pickup"
2. Add Component: ArtifactPickup
3. Set Item Data: Ooga_Booga_Item (z kroku 2)
4. (Volitelné) Přiřaď world model prefab
```

## 🎯 Jak Používat v Hře

1. **Najdi artefakt** ve světě
2. **Seber ho** (automaticky nebo stiskni E)
3. **Přesuň** do hotbaru (sloty 1-9) pokud není automaticky
4. **Vyber slot** s artefaktem (čísla 1-9)
5. **Drž RMB** dokud se nedokončí progress bar
6. **Profit!** Artefakt je aktivní až do konce epochy

## 💻 Technické Detaily

### Nové Skripty
- **ItemUseSystem.cs** - Řídí držení RMB a aktivaci itemů
- Upraveno: **ItemData.cs** - Nový ItemType.Artifact + artifact settings
- Upraveno: **ArtifactPickup.cs** - Přidává do inventáře místo přímé aktivace

### Upravené Skripty
- **HotbarUI.cs** - Beze změn (už podporuje item selection)
- **ArtifactManager.cs** - Beze změn (stále řídí aktivní efekty)

### Flow Diagram
```
Pickup → Inventory → Hotbar → Hold RMB → Activate → Effects Applied
   ↓         ↓          ↓          ↓           ↓            ↓
 World    ItemData   Selected   Progress   ArtifactMgr   Stats
                                  Bar                    Modified
```

## 🔧 Nastavení Progress Bar UI (Volitelné)

Pro vizuální feedback během držení RMB:

```
1. V Canvas vytvoř Image s Image Type: Filled
2. Set Fill Method: Radial 360
3. Přiřaď do ItemUseSystem > Progress Circle
4. Přidej UI Text "Drž RMB pro použití"
5. Přiřaď do ItemUseSystem > Use Prompt UI
```

## 🎨 UI Prompty

ItemUseSystem automaticky:
- ✅ Zobrazí "Drž RMB" když je artefakt vybraný
- ✅ Skryje prompt během držení
- ✅ Zobrazí kruhový progress bar během aktivace
- ✅ Změní barvu z žluté na zelenou při dokončení

## ⚙️ Konfigurace Času Držení

Každý artefakt může mít vlastní čas:

```csharp
// V ItemData inspectoru:
Activation Hold Time: 1.5  // Rychlá aktivace
Activation Hold Time: 3.0  // Pomalá, mocná aktivace
Activation Hold Time: 0.5  // Instant
```

## 🐛 Troubleshooting

### Artefakt se neaktivuje
- ✅ Zkontroluj že item má Item Type = Artifact
- ✅ Ujisti se, že Artifact Data je přiřazený
- ✅ Ověř že ItemUseSystem existuje ve scéně
- ✅ Zkontroluj že ArtifactManager existuje

### Progress bar se nezobrazuje
- Přiřaď UI Image do ItemUseSystem > Progress Circle
- Ujisti se, že image má Fill Type = Filled

### Item zmizí ale efekt nefunguje
- Zkontroluj Console log pro chybové hlášky
- Ověř že ArtifactData má správně nastavené Effect Type a Value

## 📊 Comparison: Starý vs. Nový Systém

### Starý Systém
```
Pickup → Okamžitá Aktivace → Efekt
```
❌ Žádná kontrola hráče  
❌ Nemůže uložit na později  
❌ Musí použít hned  

### Nový Systém
```
Pickup → Inventář → Hotbar → Volba Kdy Použít → Držení RMB → Aktivace
```
✅ Plná kontrola hráče  
✅ Strategické rozhodování  
✅ Integrováno s inventářem  
✅ Vizuální feedback  

## 🚀 Budoucí Vylepšení

Možné rozšíření:
- Multiple artifact slots (použít více najednou)
- Artifact cooldowns (použít znovu po čase)
- Artifact upgrady
- Kombinace artefaktů (combo effects)
- Trade/drop artefaktů
- Artifact durability (více použití)

## 📝 Quick Reference

### Klávesy
- **RMB** - Držet pro aktivaci artefaktu
- **1-9** - Vybrat hotbar slot
- **E** - Sebrat pickup (pokud auto-pickup vypnutý)
- **Tab/I** - Otevřít inventář

### Item Types
- Material - Crafting materiály
- Weapon - Zbraně
- Tool - Nástroje
- Consumable - Spotřební itemy
- Quest - Quest itemy
- **Artifact** - Aktivovatelné power-upy ⭐ NOVÝ

### Důležité Komponenty
- **ItemUseSystem** - Řídí použití itemů
- **ArtifactManager** - Řídí aktivní efekty
- **ArtifactPickup** - Pickup ve světě
- **HotbarUI** - Hotbar interface
- **InventorySystem** - Hlavní inventář

---

Systém je plně funkční a připravený k použití! 🎮
