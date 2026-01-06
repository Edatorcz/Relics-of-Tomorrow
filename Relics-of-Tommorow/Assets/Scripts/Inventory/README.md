# Inventář Systém - Návod k použití

## 📦 Hotové komponenty

### Skripty
1. **InventorySystem.cs** - Hlavní logika inventáře (singleton)
2. **InventorySlot.cs** - Datová třída pro slot inventáře
3. **InventoryUI.cs** - UI manager pro zobrazení inventáře
4. **InventorySlotUI.cs** - UI komponenta pro jednotlivé sloty
5. **ItemData.cs** - ScriptableObject pro definici itemů
6. **ItemPickup.cs** - Komponenta pro sebírání itemů ve světě
7. **ItemTooltip.cs** - Tooltip systém pro zobrazení info o itemu
8. **ItemDropper.cs** - Systém pro vyhazování itemů
9. **HotbarUI.cs** - Rychlý přístup k itemům (hotbar jako Minecraft)
10. **InventorySaveSystem.cs** - Ukládání a načítání inventáře

## 🎮 Funkce

### ✅ Základní funkce
- ✅ Stackování itemů (nastavitelné per item)
- ✅ Drag & Drop mezi sloty
- ✅ Rozdělení stacku pravým tlačítkem myši
- ✅ Tooltip při najetí myší (zobrazí název, popis, typ)
- ✅ Vyhazování itemů (Delete klávesa nebo drag mimo inventář)
- ✅ Hotbar s rychlým přístupem (1-9 klávesy, kolečko myši)
- ✅ Auto-save při ukončení hry
- ✅ Vizuální efekty na itemech ve světě (světlo, částice, rotace)

### 🎯 Ovládání
- **E** - Otevřít/zavřít inventář
- **Levé tlačítko myši** - Drag & Drop
- **Pravé tlačítko myši** - Rozdělit stack na polovinu
- **Střední tlačítko myši** - Rychlý přesun do prázdného slotu
- **Delete** - Vyhodit item (když je myš nad slotem)
- **Drag mimo inventář** - Vyhodit item
- **1-9** - Vybrat slot v hotbaru
- **Kolečko myši** - Přepínat sloty v hotbaru
- **F** - Sebrat item ze země

## 🔧 Nastavení v Unity

### 1. Vytvoř ItemData (ScriptableObjects)

1. V Unity: `pravý klik v Assets → Create → Inventory → Item`
2. Nastav:
   - Item Name (např. "Dřevo")
   - Description (popis itemu)
   - Icon (sprite ikony)
   - Max Stack Size (kolik jich jde stackovat)
   - Is Stackable (true/false)
   - Item Type (Material, Weapon, Tool, Consumable, Quest)
3. Ulož všechny ItemData do složky `Resources/Items/` (potřebné pro save/load)

### 2. Vytvoř Inventory UI

#### Hierarchie:
```
Canvas
├── InventoryPanel (GameObject)
│   ├── Background (Image)
│   └── SlotsGrid (Grid Layout Group)
│       └── SlotPrefab (prefab s InventorySlotUI)
├── HotbarPanel (GameObject)
│   ├── Background (Image)
│   ├── SelectionHighlight (Image - žlutý rámeček)
│   └── HotbarGrid (Horizontal Layout Group)
│       └── HotbarSlotPrefab (stejný jako SlotPrefab)
└── TooltipPanel (GameObject)
    ├── Background (Image)
    ├── ItemName (TextMeshProUGUI)
    ├── ItemDescription (TextMeshProUGUI)
    └── ItemType (TextMeshProUGUI)
```

#### SlotPrefab struktura:
```
Slot (Image + InventorySlotUI)
├── ItemIcon (Image)
├── QuantityText (TextMeshProUGUI)
└── SlotNumber (TextMeshProUGUI) - pouze pro hotbar
```

### 3. Nastav komponenty

#### InventorySystem
- Vytvoř prázdný GameObject "InventoryManager"
- Přidej komponenty:
  - InventorySystem
  - ItemDropper
  - InventorySaveSystem
- Nastav Inventory Size (36 = 27 hlavní + 9 hotbar)

#### InventoryUI
- Přidej na InventoryPanel
- Přiřaď:
  - Inventory Panel (samotný panel)
  - Slots Parent (SlotsGrid)
  - Slot Prefab (prefab slotu)
  - Toggle Key (E)

#### ItemTooltip
- Přidej na TooltipPanel
- Přiřaď všechny TextMeshPro komponenty

#### HotbarUI
- Přidej na HotbarPanel
- Přiřaď:
  - Hotbar Parent (HotbarGrid)
  - Hotbar Slot Prefab
  - Selection Highlight (žlutý rámeček)
  - Hotbar Size (9)
  - Hotbar Start Index (27)

#### ItemDropper
- Na InventoryManager objektu
- Přiřaď:
  - Item Pickup Prefab (prefab s ItemPickup komponentou)
  - Drop Force (5)
  - Drop Upward Force (2)
  - Drop Distance (2)

### 4. Vytvoř ItemPickup prefab

1. Vytvoř GameObject s:
   - 3D Model nebo Sprite (vizuál itemu)
   - ItemPickup script
   - Collider (Trigger = true)
   - Rigidbody (optional, pokud chceš fyziku)
2. Nastav:
   - Item Data (přiřaď ScriptableObject)
   - Quantity (kolik jich sebereš)
   - Pickup Range (2)
   - Pickup Key (F)
3. Ulož jako prefab do Assets/Prefabs/

### 5. Player setup

- Ujisti se, že má hráč Tag "Player"
- ItemPickup a ItemDropper to potřebují k nalezení hráče

## 💾 Save System

Inventář se automaticky ukládá při ukončení hry do:
`Application.persistentDataPath/inventory.json`

Můžeš také ručně zavolat:
```csharp
InventorySaveSystem saveSystem = FindObjectOfType<InventorySaveSystem>();
saveSystem.SaveInventory();  // Uložit
saveSystem.LoadInventory();  // Načíst
```

## 🎨 Doporučené nastavení UI

### Slot velikost: 64x64 px
### Grid Layout Group:
- Cell Size: 64x64
- Spacing: 5, 5
- Constraint: Fixed Column Count = 9 (jako Minecraft)

### Tooltip pozice:
- Offset: (10, -10)
- Automaticky se přizpůsobí, aby byl na obrazovce

## 📝 Příklad použití v kódu

```csharp
// Přidat item
InventorySystem.Instance.AddItem(mojeDrevoItemData, 10);

// Odstranit item
InventorySystem.Instance.RemoveItem(mojeDrevoItemData, 5);

// Zkontrolovat, zda má hráč item
if (InventorySystem.Instance.HasItem(mojeDrevoItemData, 3))
{
    Debug.Log("Máš alespoň 3 kusy dřeva!");
}

// Získat počet itemů
int pocet = InventorySystem.Instance.GetItemCount(mojeDrevoItemData);

// Získat vybraný item z hotbaru
HotbarUI hotbar = FindObjectOfType<HotbarUI>();
ItemData vybranyItem = hotbar.GetSelectedItem();
```

## 🐛 Řešení problémů

1. **Inventář se neotevírá**: Zkontroluj, zda je InventoryPanel správně přiřazen
2. **Itemy se nesbírají**: Ujisti se, že hráč má Tag "Player"
3. **Tooltip se nezobrazuje**: Zkontroluj, zda ItemTooltip.Instance není null
4. **Save/Load nefunguje**: ItemData musí být v `Resources/Items/` složce
5. **Drag & Drop nefunguje**: Zkontroluj, zda je na Canvas EventSystem

## 🎯 Další rozšíření (můžeš přidat)

- [ ] Crafting systém
- [ ] Vybavené itemy (zbraně, brnění)
- [ ] Chest systém (bedny na ukládání)
- [ ] Trade systém (obchodování)
- [ ] Item durability (odolnost itemů)
- [ ] Item enchanting (vylepšování)
- [ ] Quick stack (rychlé naskládání do chest)
- [ ] Item filter (filtrování podle typu)

Hotovo! 🎉
