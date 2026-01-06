# 📦 Návod na nastavení Inventory systému

## 🎯 KROK 1: Vytvoř InventorySystem GameObject

1. V Hierarchy pravý klik → **Create Empty**
2. Přejmenuj na "**InventorySystem**"
3. Přetáhni na něj skript **InventorySystem.cs**
4. V Inspectoru nastav **Inventory Size** (výchozí 36 = jako Minecraft)

---

## 🖼️ KROK 2: Vytvoř UI Canvas

### A) Vytvoř Canvas
1. Hierarchy pravý klik → **UI → Canvas**
2. Na Canvas nastav:
   - **Render Mode**: Screen Space - Overlay
   - **Canvas Scaler**: Scale With Screen Size
   - **Reference Resolution**: 1920 x 1080

### B) Vytvoř Inventory Panel
1. Na Canvas pravý klik → **UI → Panel**
2. Přejmenuj na "**InventoryPanel**"
3. Nastav velikost (Rect Transform):
   - **Width**: 800
   - **Height**: 600
   - **Anchors**: Center
4. Panel skryj (zatím ho nechceš vidět) - odškrtni checkbox vlevo nahoře

### C) Vytvoř Grid pro sloty
1. Na InventoryPanel pravý klik → **UI → Scroll View** (nebo jen prázdný GameObject)
2. Přejmenuj na "**SlotsParent**"
3. Přidej komponentu **Grid Layout Group**:
   - **Cell Size**: X=80, Y=80
   - **Spacing**: X=10, Y=10
   - **Constraint**: Fixed Column Count = **9** (jako Minecraft)

---

## 🎴 KROK 3: Vytvoř Slot Prefab

### A) Vytvoř slot
1. Na SlotsParent pravý klik → **UI → Image**
2. Přejmenuj na "**InventorySlot**"
3. Nastav Image:
   - **Color**: Tmavě šedá (např. R:50, G:50, B:50, A:200)
   - **Image Type**: Sliced (pokud máš sprite)

### B) Přidej ikonu itemu
1. Na InventorySlot pravý klik → **UI → Image**
2. Přejmenuj na "**ItemIcon**"
3. Nastav:
   - **Anchors**: Stretch-Stretch (roztáhni na celý slot)
   - **Left/Right/Top/Bottom**: 5 (malý padding)
   - **Color**: Bílá
   - Odškrtni **Raycast Target**
   - **Zhasni** checkbox vlevo nahoře (bude neviditelný, dokud není item)

### C) Přidej text pro množství
1. Na InventorySlot pravý klik → **UI → TextMeshPro - Text**
2. Přejmenuj na "**QuantityText**"
3. Nastav:
   - **Anchors**: Bottom Right (kotvit vpravo dole)
   - **Font Size**: 16-20
   - **Color**: Bílá
   - **Alignment**: Right, Bottom
   - **Pos X**: -5, **Pos Y**: 5

### D) Přidej skript InventorySlotUI
1. Na **InventorySlot** přetáhni skript **InventorySlotUI.cs**
2. V Inspectoru propoj:
   - **Item Icon** → přetáhni Image "ItemIcon"
   - **Quantity Text** → přetáhni TextMeshPro "QuantityText"
   - **Background Image** → přetáhni Image "InventorySlot" (sebe sama)

### E) Udělej z toho Prefab
1. **Přetáhni** celý "InventorySlot" z Hierarchy do složky **Assets** (nebo Assets/Prefabs)
2. Pak ho **smaž z Hierarchy** (bude se vytvářet automaticky)

---

## 🔗 KROK 4: Propoj InventoryUI

1. Na Canvas přetáhni skript **InventoryUI.cs**
2. V Inspectoru propoj:
   - **Inventory Panel** → přetáhni "InventoryPanel" GameObject
   - **Slots Parent** → přetáhni "SlotsParent" GameObject
   - **Slot Prefab** → přetáhni InventorySlot prefab ze složky Assets
   - **Toggle Key** → nastav E (nebo jinou klávesu)

---

## 📦 KROK 5: Vytvoř první Item

1. V Project pravý klik → **Create → Inventory → Item**
2. Pojmenuj např. "**Stone**"
3. V Inspectoru nastav:
   - **Item Name**: "Kámen"
   - **Description**: "Běžný kámen"
   - **Icon**: Přetáhni nějaký obrázek (sprite)
   - **Max Stack Size**: 64
   - **Is Stackable**: ✓ (zaškrtni)
   - **Item Type**: Material

---

## 🎮 KROK 6: Vytvoř sbíratelný item ve světě

### A) Vytvoř GameObject
1. Hierarchy pravý klik → **3D Object → Cube** (nebo jiný model)
2. Přejmenuj na "**Stone_Pickup**"
3. Nastav velikost: Scale (0.5, 0.5, 0.5)

### B) Přidej skript
1. Přetáhni na něj **ItemPickup.cs**
2. V Inspectoru:
   - **Item Data** → přetáhni "Stone" ScriptableObject
   - **Quantity**: 1
   - **Pickup Range**: 2
   - **Pickup Key**: F

### C) Udělej prefab
1. Přetáhni "Stone_Pickup" do Assets
2. Teď ho můžeš klonovat po mapě

---

## ✅ HOTOVO - Jak to použít?

1. **Spusť hru** (Play)
2. **Jdi k itemu** (Stone_Pickup)
3. **Zmáčkni F** → item se přidá do inventáře
4. **Zmáčkni E** → otevře se inventář
5. **Klikni a táhni** → přesuň item mezi sloty
6. **Zmáčkni E znovu** → zavře se inventář

---

## 🐛 Časté problémy

### "Nejde otevřít inventář"
- Zkontroluj, že na Canvas je skript **InventoryUI** a je správně propojený

### "Nejde sebrat item"
- Přidej tag "**Player**" na svého hráče (GameObject → Tag → Player)
- Zkontroluj, že ItemPickup má správně propojený ItemData

### "Nevidím ikonu itemu"
- V ItemData musíš mít přiřazený **Sprite** v poli Icon
- Zkontroluj, že InventorySlotUI má propojený ItemIcon

### "Nefunguje drag & drop"
- Zkontroluj, že máš v scéně **EventSystem** (měl se vytvořit automaticky s Canvas)
- Zkontroluj, že InventorySlotUI má správně propojený Background Image

---

## 💡 Tipy

- **Vytvoř více itemů**: Pravý klik → Create → Inventory → Item
- **Testování**: V InventorySystem můžeš kód upravit a na Start přidat nějaké itemy automaticky pro testování
- **Hotbar**: Prvních 9 slotů (indexy 0-8) můžeš použít jako hotbar
- **Vlastní ikony**: Najdi si pixelart ikony nebo je nakresli (64x64 px stačí)
