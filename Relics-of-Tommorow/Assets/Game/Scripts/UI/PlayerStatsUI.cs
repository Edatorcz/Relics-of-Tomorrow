using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI pro zobrazení všech statistik hráče
/// Otevře se/zavře klávesou K
/// </summary>
public class PlayerStatsUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.K;
    [SerializeField] private bool showOnStart = false;
    
    [Header("UI Style")]
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    [SerializeField] private Color headerColor = new Color(0.2f, 0.6f, 1f, 1f);
    [SerializeField] private Color labelColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private Color valueColor = new Color(0.3f, 1f, 0.3f, 1f);
    [SerializeField] private Color modifiedColor = new Color(1f, 0.8f, 0.2f, 1f);
    
    private bool isVisible = false;
    private PlayerHealth playerHealth;
    private PlayerCombat playerCombat;
    private PlayerMovement playerMovement;
    private ArtifactManager artifactManager;
    
    private GUIStyle backgroundStyle;
    private GUIStyle headerStyle;
    private GUIStyle labelStyle;
    private GUIStyle valueStyle;
    private GUIStyle modifiedValueStyle;
    private GUIStyle sectionStyle;
    
    private Rect windowRect;
    private Vector2 scrollPosition;
    
    void Start()
    {
        isVisible = showOnStart;
        FindPlayerComponents();
        InitializeStyles();
        
        // Pozice okna - střed obrazovky
        windowRect = new Rect(Screen.width / 2 - 250, Screen.height / 2 - 300, 500, 600);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
            
            // Při otevření refresh player references
            if (isVisible)
            {
                FindPlayerComponents();
            }
        }
    }
    
    void FindPlayerComponents()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        
        if (playerCombat == null)
            playerCombat = FindFirstObjectByType<PlayerCombat>();
            
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();
            
        artifactManager = ArtifactManager.Instance;
    }
    
    void InitializeStyles()
    {
        // Background
        backgroundStyle = new GUIStyle();
        backgroundStyle.normal.background = MakeTex(2, 2, backgroundColor);
        
        // Header
        headerStyle = new GUIStyle();
        headerStyle.fontSize = 20;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = headerColor;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.padding = new RectOffset(0, 0, 10, 10);
        
        // Section header
        sectionStyle = new GUIStyle();
        sectionStyle.fontSize = 16;
        sectionStyle.fontStyle = FontStyle.Bold;
        sectionStyle.normal.textColor = headerColor;
        sectionStyle.padding = new RectOffset(5, 0, 10, 5);
        
        // Label (stat name)
        labelStyle = new GUIStyle();
        labelStyle.fontSize = 14;
        labelStyle.normal.textColor = labelColor;
        labelStyle.padding = new RectOffset(10, 0, 2, 2);
        
        // Value (normal)
        valueStyle = new GUIStyle();
        valueStyle.fontSize = 14;
        valueStyle.fontStyle = FontStyle.Bold;
        valueStyle.normal.textColor = valueColor;
        valueStyle.alignment = TextAnchor.MiddleRight;
        valueStyle.padding = new RectOffset(0, 10, 2, 2);
        
        // Modified value (with artifact boost)
        modifiedValueStyle = new GUIStyle(valueStyle);
        modifiedValueStyle.normal.textColor = modifiedColor;
    }
    
    void OnGUI()
    {
        if (!isVisible) return;
        
        // Zajistit že styles jsou inicializované
        if (backgroundStyle == null)
            InitializeStyles();
        
        windowRect = GUI.Window(0, windowRect, DrawStatsWindow, "", backgroundStyle);
    }
    
    void DrawStatsWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        // Header
        GUILayout.Label("STATISTIKY HRÁČE", headerStyle);
        GUILayout.Label($"Stiskni [{toggleKey}] pro zavření", labelStyle);
        
        GUILayout.Space(10);
        
        // Scroll view pro dlouhý obsah
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(500));
        
        // === HEALTH STATS ===
        DrawHealthStats();
        
        GUILayout.Space(10);
        
        // === COMBAT STATS ===
        DrawCombatStats();
        
        GUILayout.Space(10);
        
        // === MOVEMENT STATS ===
        DrawMovementStats();
        
        GUILayout.Space(10);
        
        // === ARTIFACT STATS ===
        DrawArtifactStats();
        
        GUILayout.EndScrollView();
        
        GUILayout.EndVertical();
        
        // Umožnit tažení okna
        GUI.DragWindow();
    }
    
    void DrawHealthStats()
    {
        if (playerHealth == null)
        {
            GUILayout.Label("⚠ PlayerHealth nenalezen", labelStyle);
            return;
        }
        
        GUILayout.Label("❤️ ZDRAVÍ", sectionStyle);
        
        float currentHP = playerHealth.GetCurrentHealth();
        float maxHP = playerHealth.GetMaxHealth();
        float hpPercent = playerHealth.GetHealthPercentage() * 100f;
        
        DrawStat("Aktuální HP", $"{currentHP:F0} / {maxHP:F0}", false);
        DrawStat("HP Procenta", $"{hpPercent:F1}%", false);

        
        // Defense multiplier
        float defenseMult = artifactManager != null ? artifactManager.GetDefenseMultiplier() : 1f;
        bool hasDefenseBoost = defenseMult > 1f;
        DrawStat("Defense Multiplier", $"{defenseMult:F2}x", hasDefenseBoost);
        if (hasDefenseBoost)
        {
            float reduction = (1f - (1f / defenseMult)) * 100f;
            DrawStat("  → Redukce Damage", $"-{reduction:F1}%", true);
        }
    }
    
    void DrawCombatStats()
    {
        if (playerCombat == null)
        {
            GUILayout.Label("⚠ PlayerCombat nenalezen", labelStyle);
            return;
        }
        
        GUILayout.Label("⚔️ COMBAT", sectionStyle);
        
        // Attack speed
        float attackSpeedMult = artifactManager != null ? artifactManager.GetAttackSpeedMultiplier() : 1f;
        bool hasAttackSpeedBoost = attackSpeedMult > 1f;
        DrawStat("Attack Speed", $"{attackSpeedMult:F2}x", hasAttackSpeedBoost);
        
        // Critical chance
        float critChance = artifactManager != null ? artifactManager.GetCriticalChance() : 0f;
        bool hasCrit = critChance > 0f;
        DrawStat("Critical Chance", $"{critChance * 100f:F1}%", hasCrit);
        
        // Life steal
        float lifeSteal = artifactManager != null ? artifactManager.GetLifeStealPercent() : 0f;
        bool hasLifeSteal = lifeSteal > 0f;
        DrawStat("Life Steal", $"{lifeSteal * 100f:F1}%", hasLifeSteal);
        
        // Damage multiplier
        float damageMult = artifactManager != null ? artifactManager.GetDamageMultiplier() : 1f;
        bool hasDamageBoost = damageMult > 1f;
        DrawStat("Damage Multiplier", $"{damageMult:F2}x", hasDamageBoost);
        
        // Block stamina
        DrawStat("Block Stamina", $"{playerCombat.BlockStamina:F0} / {playerCombat.MaxBlockStamina:F0}", false);
    }
    
    void DrawMovementStats()
    {
        if (playerMovement == null)
        {
            GUILayout.Label("⚠ PlayerMovement nenalezen", labelStyle);
            return;
        }
        
        GUILayout.Label("🏃 POHYB", sectionStyle);
        
        // Speed multiplier
        float speedMult = artifactManager != null ? artifactManager.GetSpeedMultiplier() : 1f;
        bool hasSpeedBoost = speedMult > 1f;
        DrawStat("Speed Multiplier", $"{speedMult:F2}x", hasSpeedBoost);
    }
    
    void DrawArtifactStats()
    {
        GUILayout.Label("✨ ARTEFAKTY", sectionStyle);
        
        if (artifactManager == null)
        {
            GUILayout.Label("⚠ ArtifactManager nenalezen", labelStyle);
            return;
        }
        
        var activeArtifacts = artifactManager.GetActiveArtifacts();
        DrawStat("Aktivní Artefakty", activeArtifacts.Count.ToString(), activeArtifacts.Count > 0);
        
        if (activeArtifacts.Count > 0)
        {
            GUILayout.Space(5);
            foreach (var artifact in activeArtifacts)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"  • {artifact.artifactName}", labelStyle);
                GUILayout.Label($"({artifact.epoch})", modifiedValueStyle);
                GUILayout.EndHorizontal();
                
                // Zobrazit efekty
                string effectText = GetEffectDescription(artifact.effectType, artifact.effectValue);
                GUILayout.Label($"    {effectText}", labelStyle);
                
                if (artifact.secondaryEffectType != ArtifactEffectType.Custom)
                {
                    string secondaryText = GetEffectDescription(artifact.secondaryEffectType, artifact.secondaryEffectValue);
                    GUILayout.Label($"    {secondaryText}", labelStyle);
                }
            }
        }
    }
    
    void DrawStat(string label, string value, bool isModified)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, labelStyle, GUILayout.Width(250));
        GUILayout.FlexibleSpace();
        GUILayout.Label(value, isModified ? modifiedValueStyle : valueStyle, GUILayout.Width(150));
        GUILayout.EndHorizontal();
    }
    
    string GetEffectDescription(ArtifactEffectType effectType, float value)
    {
        switch (effectType)
        {
            case ArtifactEffectType.DamageBoost:
                return $"Damage: +{(value - 1f) * 100f:F0}%";
            case ArtifactEffectType.HealthBoost:
                return $"Max HP: +{value:F0}";
            case ArtifactEffectType.SpeedBoost:
                return $"Speed: +{(value - 1f) * 100f:F0}%";
            case ArtifactEffectType.RegenBoost:
                return $"Regenerace: {value:F1}x";
            case ArtifactEffectType.DefenseBoost:
                return $"Defense: +{((value - 1f) * 100f):F0}%";
            case ArtifactEffectType.AttackSpeedBoost:
                return $"Attack Speed: +{(value - 1f) * 100f:F0}%";
            case ArtifactEffectType.CriticalChance:
                return $"Crit Chance: {value * 100f:F0}%";
            case ArtifactEffectType.LifeSteal:
                return $"Life Steal: {value * 100f:F0}%";
            default:
                return $"{effectType}: {value}";
        }
    }
    
    // Helper pro vytvoření textury
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
