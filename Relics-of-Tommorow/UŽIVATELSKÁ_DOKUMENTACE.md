# Relics of Tomorrow - Uživatelská dokumentace

## 🎮 Úvod do hry

**Relics of Tomorrow** je akční roguelike hra s prvky RPG, ve které se pohybujete napříč různými historickými epochami. Každá úroveň je procedurálně generovaná, což znamená, že se nikdy neopakuje a každé hraní je jedinečné. Vaším úkolem je procházet místnostmi, bojovat s nepřáteli, sbírat mocné artefakty a porazit závěrečné bossy jednotlivých epoch.

---

## 🎯 Základní herní mechaniky

### Ovládání

#### Pohyb
- **W, A, S, D** - základní pohyb postavy
- **Shift (držet)** - běh (zvýšená rychlost pohybu) - **spotřebovává staminu!**
- **C** - dřep (snížení postavy)
- **Mezerník** - skok
- **Ctrl (během běhu)** - úhybný manévr (dash/roll), během kterého jste dočasně nezranitelní - **stojí 30 staminy!**

#### Boj
- **Levé tlačítko myši (LMB)** - útok zbraní
- **Pravé tlačítko myši (RMB)** - blokování (drží se)
  - Blokování snižuje přijímané poškození o 70%
  - Spotřebovává staminu - při jejím vyčerpání nelze blokovat
  - Stamina se automaticky regeneruje po ukončení blokování

#### Kamera
- **Pohyb myši** - rozhlížení se po okolí (FPS pohled)

### Systém zdraví a života
- Hráč má určité množství zdraví (HP)
- Při obdržení poškození od nepřátel se zdraví snižuje
- Smrt znamená restart úrovně
- Během úhybného manévru (roll) jste krátkodobě nezranitelní vůči útokům

### Systém staminy
- **Stamina se zobrazuje** jako modrý bar pod health barem v pravém horním rohu
- Stamina se **spotřebovává** při:
  - **Blokování (RMB)** - ubíhá během držení
  - **Sprintování (Shift)** - ubíhá pomalu během běhu
  - **Dash/Roll (Ctrl během běhu)** - spotřebuje 30 staminy najednou
- **Regenerace staminy:**
  - Stamina se automaticky doplňuje, když neblokujete
  - Čím méně staminy máte, tím červenější je bar
- **Omezení:**
  - Bez staminy **nemůžete blokovat**
  - Bez staminy **nemůžete sprintovat** (běžet)
  - Bez 30 staminy **nemůžete dashovat**
- **Tip:** Nespamujte dash a sprint - hlídejte si staminu!

### Systém zbraní a inventáře
- Sbírejte různé zbraně z různých epoch
- Každá zbraň má unikátní statistiky:
  - **Poškození** - kolik HP uberete nepříteli
  - **Dosah** - jak daleko můžete útočit
  - **Rychlost útoku** - jak rychle můžete útočit
  - **Obranná hodnota** - bonusová obrana při blokování
- Inventář obsahuje **hotbar** (rychlý přístup) a **hlavní inventář**
- Zbraně lze vybavit a vyměnit během hry

---

## 🌍 Herní světy (Epochy)

Hra obsahuje 5 různých epoch, z nichž každá má svůj unikátní vizuální styl, nepřátele a mechaniky:

### 1. **Pravěk (Stone Age)**
- **Prostředí:** Kamenné jeskyně s organickými formacemi
- **Vizuální prvky:** Stalaktity, stalagmity, skalní útvary
- **Nepřátelé:** Primitivní válečníci, divocí lovci
- **Boss:** Mohutný Stone Age Boss s několika fázemi boje
- **Zbraně:** Kamenné sekery, hole, primitivní zbraně

### 2. **Starověk (Ancient Era)**
- **Prostředí:** Antické chrámy a amfiteátry
- **Nepřátelé:** Gladiátoři, stráže
- **Zbraně:** Meče, kopí, štíty

### 3. **Středověk (Medieval Era)**
- **Prostředí:** Gotické hrady a zbrojnice
- **Vizuální prvky:** Kamenné zdi, pochodně, středověká architektura
- **Nepřátelé:** Rytíři, lučištníci
- **Zbraně:** Meče, sekery, bojové kladiva

### 4. **Současnost (Modern Era)**
- **Prostředí:** Moderní průmyslové a kancelářské prostory
- **Vizuální prvky:** Betonové konstrukce, elektrické osvětlení
- **Nepřátelé:** Moderní vojáci, strážci
- **Zbraně:** Moderní melee zbraně, bojové nože

### 5. **Budoucnost (Future)**
- **Prostředí:** Futuristické sci-fi prostory
- **Vizuální prvky:** Neonová světla, hologramy, energetické bariéry
- **Nepřátelé:** Roboti, drony, bezpečnostní jednotky
- **Zbraně:** Energetické zbraně, futuristické čepele

---

## 🗺️ Struktura úrovní

### Procedurální generování
Každá úroveň je náhodně vygenerovaná a obsahuje:

#### Typy místností:
1. **Startovní místnost** - zde začínáte, bez nepřátel
2. **Normální místnosti** - obsahují nepřátele (počet roste s postupem)
3. **Pokladové místnosti** - žádní nepřátelé, ale cenné předměty a zbraně
4. **Odbočky** - vedlejší místnosti s extra obsahem
5. **Boss místnost** - největší místnost s finálním bossem úrovně

### Progrese obtížnosti
- Počet nepřátel v místnostech postupně narůstá
- Čím dál postoupíte, tím těžší boje vás čekají
- Boss místnost je vždy na konci a obsahuje nejsilnějšího nepřítele

### Propojení místností
- Místnosti jsou spojeny chodbami
- Některé místnosti mají více východů a vedou k odbočkám
- Orientujte se podle mapy a postupujte k cíli

---

## ⚔️ Bojovdash/roll (Ctrl během běhu) k vyhnutí se útokům
- Během dashu jste krátkodobě nezranitelní
- **Dash stojí 30 staminy** - nelze ho spamovat bez staminy!
- Dash má také cooldown - musíte počkat mezi použitími
- Útočte levým tlačítkem myši
- Zaměřte se na nepřítele a útočte v dosahu vaší zbraně
- Každá zbraň má cooldown (čekací dobu) mezi útoky
- Pozorujte animace nepřátel a hledejte okamžik k útoku

### Obrana
- Držením pravého tlačítka myši aktivujete blokování
- Blokování výrazně snižuje přijímané poškození (70% redukce)
- **Sledujte staminu!** Při vyčerpání staminy nelze blokovat
- Stamina se regeneruje automaticky po ukončení blokování

### Úhybné manévry
- Použijte roll (dvojité Shift) k vyhnutí se útokům
- Během rollu jste krátkodobě nezranitelní
- Roll má cooldown - nelze ho spamovat
- Ideální pro únik z nebezpečných situací nebo vyhnutí se silným útokům

### Tipy pro boj:
- **Kombinujte útok a obranu** - nelze útočit během blokování
- **Využívajte prostředí** - obíhejte nepřátele, využívejte překážky
- **HLÍDEJTE STAMINU!** - je klíčová pro přežití:
  - Neblokujte zbytečně - nechte staminu regenerovat
  - Nesprintujte neustále - běžte jen když je to nutné
  - Šetřete staminu na dash v kritických momentech
- **Dash jako záchrana** - použijte POUZE v kritických momentech (stojí 30 staminy!)
- **Plánujte akce** - sledujte stamina bar a rozhodujte se podle nějenerovat
- **Roll jako záchrana** - použijte v kritických momentech

---

## 🎒 Systém předmětů a lootu

### Sbírání předmětů
- **Ztracené zbraně** - objevují se po porážce nepřátel nebo v místnostech
- Přistupte k předmětu a stiskněte klávesu pro sebráni (obvykle **E** nebo automaticky)
- Předměty jsou barevně označeny podle kvality

### Typy předmětů:
- **Zbraně** - zvyšují vaše útočné schopnosti
- **Materiály** - slouží pro crafting (budoucí implementace)
- **Spotřební předměty** - léčení, posílení
- **Quest předměty** - důležité pro postup v příběhu

### Boss loot
- Bosové vždy upustí speciální, mocné předměty
- Boss loot má vyšší kvalitu než běžné zbraně
- Vždy se vyplatí porazit bosse!

---

## 🎲 Roguelike prvky

### Každé hraní je unikátní
- **Procedurální generování** - jiný layout místností při každém spuštění
- **Náhodný loot** - různé zbraně a předměty při každém pokusu
- **Variabilní nepřátelé** - různý počet a rozmístění

### Permadeath (trvalá smrt)
- Po smrti se vrátíte na začátek úrovně
- Neztratíte však pokrok mezi epochami
- Naučte se ze svých chyb a zkoušejte nové strategie

---

## 🏆 Cíle hry

### Krátkodobé cíle:
- Projít všemi místnostmi v aktuální úrovni
- Porazit všechny nepřátele
- Sebrat cenné zbraně a předměty
- Přežít a dojít k boss místnosti

### Dlouhodobé cíle:
- Porazit bosse v každé epoše
- Odemknout a prozkoumat všechny epochy
- Nalézt nejlepší zbraně z každé epochy
- Zvládnout všechny obtížnosti

---

## 💡 Tipy pro začátečníky

### 1. **Naučte se ovládání**
- Procvičte si pohyb, útok a blokování v první místnosti
- Vyzkoušejte roll ve složitějších situacích

### 2. **Buďte trpěliví**
- Nepožeňte se slepě do boje
- Pozorujte pohyb nepřátel a naučte se jejich vzorce

### 3. **Využívejte prostředí**
- Místnosti mají různé velikosti a překážky
- Využijte je ke své výhodě při boji
Hlídejte staminu jako oko v hlavě!**
- Stamina bar je pod health barem - sledujte ho!
- Blokování, sprint a dash spotřebovávají staminu
- Bez staminy jste zranitelní - plánujte své akce
- Nechte staminu regenerovat - nehrajte agresivně pořád
- Sprint jen když je to nutné, dash jen v nouzi
- Mocnější zbraň znamená snazší boje

### 5. **Šetřete staminu**
- Blokování je silné, ale nejste nezranitelní
- Nechte staminu regenerovat mezi útoky

### 6. **Učte se z porážek**
- Každá smrt je příležitost se zlepšit
- Zkuste jiný přístup, jinou zbraň, jinou strategii

---

## 🐛 Známé problémy a omezení

- Některé modely zbraní mohou mít dočasné grafické nedostatky
- Pokud se zasekneš, zkus použít roll k osvobození
- V případě pádu skrz mapu se hra automaticky restartuje

---

## 📝 Závěr

**Relics of Tomorrow** nabízí dynamický roguelike zážitek plný akce, strategie a objevování. Každé hraní je jiné a každá epocha přináší nové výzvy. Využijte své bojové dovednosti, naučte se ovládání a pronikněte tajemstvími různých časových period!

Hodně štěstí na vaší cestě časem! ⚔️🛡️

---

*Dokumentace vytvořena pro maturitní práci - projekt Relics of Tomorrow*  
*Verze: 1.0*  
*Autor: [Tvoje jméno]*
