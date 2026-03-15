# 🎮 Pause Menu - ESC pro návrat do hlavního menu

## Co jsem vytvořil:

### [PauseMenuManager.cs](Assets/Game/Scripts/Managers/PauseMenuManager.cs)
- Stiskni **ESC** = pause hru
- V pause menu klikni **"Quit to Menu"** = opusť run a vrať se do menu
- Automaticky vyčistí artefakty a inventář

### Upraveno: [PlayerCamera.cs](Assets/Game/Scripts/Player/PlayerCamera.cs)
- ESC už nekonflikuje s pause menu

---

## 📋 Jak nastavit v Unity:

### **Krok 1: Vytvoř Pause Menu Panel**

V **každé herní scéně** (Pravěk, Starověk, Středověk, Budoucnost):

1. **Najdi nebo vytvoř Canvas:**
   - Pokud Canvas není ve scéně: `GameObject → UI → Canvas`

2. **Vytvoř Pause Menu Panel:**
   ```
   Canvas → Create Empty → "PauseMenuPanel"
   ```

3. **Nastav panel jako překryvný:**
   - Width: 400-500
   - Height: 300-400
   - Color: Tmavá s alpha (např. rgba(0, 0, 0, 200))
   - Anchor: Střed obrazovky (0.5, 0.5)

### **Krok 2: Přidej tlačítka**

Vytvoř tuto strukturu v **PauseMenuPanel**:

```
PauseMenuPanel
├── Title (Text - TMP) → "PAUZA" / "PAUSED"
│
├── ResumeButton (Button)
│   └── Text → "POKRAČOVAT" / "RESUME"
│
├── QuitToMenuButton (Button)
│   └── Text → "NÁVRAT DO MENU" / "QUIT TO MENU"
│
└── QuitGameButton (Button) [volitelné]
    └── Text → "UKONČIT HRU" / "QUIT GAME"
```

**Jak vytvořit:**
- Pravý klik na PauseMenuPanel → `UI → Button - TextMeshPro`
- Pojmenuj button (ResumeButton, QuitToMenuButton, atd.)
- Změň text uvnitř buttonu

### **Krok 3: Přidat PauseMenuManager**

1. **Vytvoř GameObject:**
   ```
   Hierarchy → Create Empty → "PauseMenuManager"
   ```

2. **Přidej script:**
   ```
   Add Component → PauseMenuManager
   ```

3. **Připoj reference v Inspectoru:**
   - **Pause Menu Panel** → přetáhni PauseMenuPanel
   - **Resume Button** → přetáhni ResumeButton
   - **Quit To Menu Button** → přetáhni QuitToMenuButton
   - **Quit Game Button** → přetáhni QuitGameButton (volitelné)
   - **Menu Scene Name** → "Menu" (nebo název твé menu scény)

### **Krok 4: Nastavit panel jako neaktivní**

1. Vyber **PauseMenuPanel** v Hierarchy
2. V Inspectoru **odškrtni checkbox** nahoře
3. ✅ Panel je skrytý (objeví se až po ESC)

---

## 🎮 Jak to používat:

### Ve hře:
- **ESC** → Otevře pause menu
- **RESUME** → Zavře pause menu a pokračuje ve hře
- **QUIT TO MENU** → Okamžitě vrátí do hlavního menu (opustí run)
- **QUIT GAME** → Ukončí aplikaci

### Co se stane při "Quit to Menu":
✅ Time.timeScale = 1 (reset)
✅ Artefakty vyčištěny
✅ Inventář vyčištěn
✅ Vrácení do Menu scény

---

## 🎨 Styling (volitelné):

### Panel pozadí:
```
Image Component:
- Color: rgba(10, 10, 10, 230) - tmavá
- nebo rgba(20, 20, 50, 240) - modrá
```

### Tlačítka:
```
Normal: Světle šedá
Highlighted: Světle modrá / zelená
Pressed: Tmavší
Font Size: 18-24
```

### Title text:
```
Font Size: 32-48
Bold
Color: Bílá nebo zlatá
Alignment: Center
```

---

## 🔧 Možnosti úpravy:

### Změnit klávesu pro pause:
V **PauseMenuManager** Inspectoru:
- **Pause Key** → změň z Escape na jinou klávesu (např. P, Tab...)

### Změnit název menu scény:
V **PauseMenuManager** Inspectoru:
- **Menu Scene Name** → změň "Menu" na tvůj název

### Přidat potvrzovací dialog:
Před voláním `QuitToMenu()` přidej dialog:
```csharp
// Zeptat se hráče jestli opravdu chce opustit run?
```

---

## ⚠️ Důležité poznámky:

1. **PauseMenuManager musí být v každé herní scéně**
   - Nebo použij DontDestroyOnLoad (ale ne doporučeno kvůli scene transition)

2. **PlayerCamera už nebude reagovat na ESC když existuje PauseMenuManager**
   - To je správně - ESC má prioritu pro pause

3. **Time.timeScale = 0 zastaví vše včetně:**
   - Pohybu hráče
   - Nepřátel  
   - Animací
   - Physics
   - Ale NE UI

4. **Při návratu do menu se vyčistí:**
   - Všechny artefakty (ArtifactManager)
   - Celý inventář (InventorySystem)
   - To znamená hráč začne znovu

---

## 🚀 Quick Setup (rychlý postup):

1. **V herní scéně:**
   ```
   Canvas → Create Empty → "PauseMenuPanel"
   PauseMenuPanel → Add 3 Buttons (Resume, Quit, Exit)
   PauseMenuPanel → Deactivate (odškrtni)
   ```

2. **Vytvoř manager:**
   ```
   Create Empty → "PauseMenuManager"
   Add Component → PauseMenuManager
   Připoj všechny reference
   ```

3. **Test:**
   ```
   Play → ESC → Mělo by se objevit pause menu
   ```

4. **Hotovo!** 🎉

---

## 🐛 Řešení problémů:

### ESC nefunguje:
- Zkontroluj že PauseMenuManager je ve scéně
- Zkontroluj že Pause Key = Escape

### Panel se neobjevuje:
- Zkontroluj že pauseMenuPanel je připojený v Inspectoru
- Zkontroluj že Panel není permanent deaktivovaný

### tlačítka nefungují:
- Zkontroluj že buttons jsou připojené v Inspectoru
- Zkontroluj že EventSystem existuje ve scéně

### Hra se nepauzne:
- Time.timeScale by mělo být 0
- Check console logs

---

## 💡 Tipy:

- Použij animace (Animator) pro hezké fade in/out pause menu
- Přidej zvuk při otevření menu (AudioSource.PlayOneShot)
- Zobraz statistiky v pause menu (deaths, time played...)
- Přidej Options v pause menu (volume, graphics...)

