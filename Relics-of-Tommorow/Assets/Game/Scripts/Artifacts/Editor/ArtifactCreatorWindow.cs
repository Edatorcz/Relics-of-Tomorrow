using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor utility pro rychlé vytváření artefaktů
/// Automaticky vytvoří ItemData i ArtifactData najednou
/// </summary>
public class ArtifactCreatorWindow : EditorWindow
{
    private string artifactName = "Nový Artefakt";
    private string description = "Popis efektu artefaktu";
    private EpochType epoch = EpochType.Pravek;
    private ArtifactEffectType effectType = ArtifactEffectType.DamageBoost;
    private float effectValue = 1.5f;
    private ArtifactEffectType secondaryEffectType = ArtifactEffectType.Custom;
    private float secondaryEffectValue = 0f;
    private float holdTime = 1.5f;
    private Color auraColor = new Color(1f, 0.8f, 0.2f, 0.3f);
    private Sprite icon;
    
    [MenuItem("Tools/Relics of Tomorrow/Artifact Creator")]
    public static void ShowWindow()
    {
        GetWindow<ArtifactCreatorWindow>("Artifact Creator");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Vytvoření Nového Artefaktu", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        artifactName = EditorGUILayout.TextField("Název Artefaktu", artifactName);
        description = EditorGUILayout.TextArea(description, GUILayout.Height(60));
        icon = (Sprite)EditorGUILayout.ObjectField("Ikona", icon, typeof(Sprite), false);
        
        EditorGUILayout.Space();
        GUILayout.Label("Epocha a Efekty", EditorStyles.boldLabel);
        
        epoch = (EpochType)EditorGUILayout.EnumPopup("Epocha", epoch);
        effectType = (ArtifactEffectType)EditorGUILayout.EnumPopup("Hlavní Efekt", effectType);
        effectValue = EditorGUILayout.FloatField("Síla Efektu", effectValue);
        
        EditorGUILayout.Space();
        secondaryEffectType = (ArtifactEffectType)EditorGUILayout.EnumPopup("Sekundární Efekt", secondaryEffectType);
        if (secondaryEffectType != ArtifactEffectType.Custom)
        {
            secondaryEffectValue = EditorGUILayout.FloatField("Síla Sekundárního", secondaryEffectValue);
        }
        
        EditorGUILayout.Space();
        GUILayout.Label("Aktivace a Vizuální", EditorStyles.boldLabel);
        
        holdTime = EditorGUILayout.FloatField("Čas Držení (s)", holdTime);
        auraColor = EditorGUILayout.ColorField("Barva Aury", auraColor);
        
        EditorGUILayout.Space(20);
        
        if (GUILayout.Button("Vytvořit Artefakt", GUILayout.Height(40)))
        {
            CreateArtifact();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Vytvoří ItemData i ArtifactData ve složce Assets/Game/Data/Artifacts/\n\n" +
            "Nezapomeň po vytvoření:\n" +
            "1. Přiřadit ikonu (sprite)\n" +
            "2. (Volitelně) Přiřadit world model prefab\n" +
            "3. (Volitelně) Přiřadit zvuky",
            MessageType.Info
        );
    }
    
    void CreateArtifact()
    {
        if (string.IsNullOrEmpty(artifactName))
        {
            EditorUtility.DisplayDialog("Chyba", "Název artefaktu nesmí být prázdný!", "OK");
            return;
        }
        
        // Vytvoř složky pokud neexistují
        string folderPath = "Assets/Game/Data/Artifacts";
        if (!AssetDatabase.IsValidFolder("Assets/Game"))
            AssetDatabase.CreateFolder("Assets", "Game");
        if (!AssetDatabase.IsValidFolder("Assets/Game/Data"))
            AssetDatabase.CreateFolder("Assets/Game", "Data");
        if (!AssetDatabase.IsValidFolder("Assets/Game/Data/Artifacts"))
            AssetDatabase.CreateFolder("Assets/Game/Data", "Artifacts");
        
        // Sanitize jméno pro soubory
        string safeName = artifactName.Replace(" ", "_");
        
        // 1. Vytvoř ArtifactData
        ArtifactData artifactData = ScriptableObject.CreateInstance<ArtifactData>();
        artifactData.artifactName = artifactName;
        artifactData.description = description;
        artifactData.icon = icon;
        artifactData.epoch = epoch;
        artifactData.effectType = effectType;
        artifactData.effectValue = effectValue;
        artifactData.secondaryEffectType = secondaryEffectType;
        artifactData.secondaryEffectValue = secondaryEffectValue;
        artifactData.auraColor = auraColor;
        
        string artifactDataPath = $"{folderPath}/{safeName}_Data.asset";
        AssetDatabase.CreateAsset(artifactData, artifactDataPath);
        
        // 2. Vytvoř ItemData
        ItemData itemData = ScriptableObject.CreateInstance<ItemData>();
        itemData.itemName = artifactName;
        itemData.description = $"{description}\n\n[Drž RMB pro aktivaci]";
        itemData.icon = icon;
        itemData.itemType = ItemData.ItemType.Artifact;
        itemData.maxStackSize = 1;
        itemData.isStackable = false;
        itemData.artifactData = artifactData;
        itemData.activationHoldTime = holdTime;
        
        string itemDataPath = $"{folderPath}/{safeName}_Item.asset";
        AssetDatabase.CreateAsset(itemData, itemDataPath);
        
        // Save a refresh
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Select v Project okně
        Selection.activeObject = itemData;
        EditorGUIUtility.PingObject(itemData);
        
        EditorUtility.DisplayDialog(
            "Úspěch!",
            $"Artefakt '{artifactName}' byl vytvořen!\n\n" +
            $"Vytvořené soubory:\n" +
            $"- {safeName}_Data.asset (ArtifactData)\n" +
            $"- {safeName}_Item.asset (ItemData)\n\n" +
            $"ItemData je nyní vybrán v Project okně.",
            "OK"
        );
    }
}

/// <summary>
/// Quick presets pro rychlé vytváření artefaktů
/// </summary>
public class ArtifactPresets
{
    [MenuItem("Tools/Relics of Tomorrow/Create Preset/Pravěk - Ooga Booga")]
    public static void CreateOogaBooga()
    {
        CreatePresetArtifact(
            "Ooga Booga",
            "Prastaré bojové heslo tvých předků. Zvyšuje sílu tvých úderů o 50%!",
            EpochType.Pravek,
            ArtifactEffectType.DamageBoost,
            1.5f,
            new Color(1f, 0.5f, 0f, 0.3f)
        );
    }
    
    [MenuItem("Tools/Relics of Tomorrow/Create Preset/Pravěk - Sběr Bobulí")]
    public static void CreateSberBobuli()
    {
        CreatePresetArtifact(
            "Sběr Bobulí",
            "Znalost prastarých léčivých bobulí. Tvé zdraví se regeneruje 3x rychleji!",
            EpochType.Pravek,
            ArtifactEffectType.RegenBoost,
            3.0f,
            new Color(0.2f, 1f, 0.2f, 0.3f)
        );
    }
    
    [MenuItem("Tools/Relics of Tomorrow/Create Preset/Pravěk - Síla Mamuta")]
    public static void CreateSilaMamuta()
    {
        CreatePresetArtifact(
            "Síla Mamuta",
            "Energie mocného mamuta proudí tvým tělem. Zvyšuje zdraví o 50 bodů a obranu o 20%!",
            EpochType.Pravek,
            ArtifactEffectType.HealthBoost,
            50f,
            new Color(0.6f, 0.4f, 0.2f, 0.3f),
            ArtifactEffectType.DefenseBoost,
            1.2f
        );
    }
    
    private static void CreatePresetArtifact(
        string name,
        string description,
        EpochType epoch,
        ArtifactEffectType effectType,
        float effectValue,
        Color auraColor,
        ArtifactEffectType secondaryType = ArtifactEffectType.Custom,
        float secondaryValue = 0f)
    {
        string folderPath = "Assets/Game/Data/Artifacts";
        
        // Vytvoř složky
        if (!AssetDatabase.IsValidFolder("Assets/Game"))
            AssetDatabase.CreateFolder("Assets", "Game");
        if (!AssetDatabase.IsValidFolder("Assets/Game/Data"))
            AssetDatabase.CreateFolder("Assets/Game", "Data");
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets/Game/Data", "Artifacts");
        
        string safeName = name.Replace(" ", "_");
        
        // ArtifactData
        ArtifactData artifactData = ScriptableObject.CreateInstance<ArtifactData>();
        artifactData.artifactName = name;
        artifactData.description = description;
        artifactData.epoch = epoch;
        artifactData.effectType = effectType;
        artifactData.effectValue = effectValue;
        artifactData.secondaryEffectType = secondaryType;
        artifactData.secondaryEffectValue = secondaryValue;
        artifactData.auraColor = auraColor;
        
        AssetDatabase.CreateAsset(artifactData, $"{folderPath}/{safeName}_Data.asset");
        
        // ItemData
        ItemData itemData = ScriptableObject.CreateInstance<ItemData>();
        itemData.itemName = name;
        itemData.description = $"{description}\n\n[Drž RMB pro aktivaci]";
        itemData.itemType = ItemData.ItemType.Artifact;
        itemData.maxStackSize = 1;
        itemData.isStackable = false;
        itemData.artifactData = artifactData;
        itemData.activationHoldTime = 1.5f;
        
        AssetDatabase.CreateAsset(itemData, $"{folderPath}/{safeName}_Item.asset");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Preset artefakt '{name}' vytvořen!");
    }
}
#endif
