using UnityEngine;
using TMPro;

/// <summary>
/// UI Dialog pro zobrazení herních statistik
/// Otevře se/zavře klávesou O
/// </summary>
public class StatisticsUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.O;
    [SerializeField] private bool showOnStart = false;
    
    [Header("UI Style")]
    [SerializeField] private Color backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.95f);
    [SerializeField] private Color headerColor = new Color(1f, 0.7f, 0.2f, 1f);
    [SerializeField] private Color labelColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private Color valueColor = new Color(0.3f, 1f, 0.3f, 1f);
    [SerializeField] private Color epochColor = new Color(0.5f, 0.8f, 1f, 1f);
    
    private bool isVisible = false;
    private GameStatistics stats;
    
    private GUIStyle backgroundStyle;
    private GUIStyle headerStyle;
    private GUIStyle titleStyle;
    private GUIStyle labelStyle;
    private GUIStyle valueStyle;
    private GUIStyle sectionStyle;
    private GUIStyle epochLabelStyle;
    
    private Rect windowRect;
    private Vector2 scrollPosition;
    
    void Start()
    {
        isVisible = showOnStart;
        stats = GameStatistics.Instance;
        InitializeStyles();
        
        // Pozice okna - střed obrazovky
        windowRect = new Rect(Screen.width / 2 - 300, Screen.height / 2 - 250, 600, 500);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
            
            // Při otevření refresh stats reference
            if (isVisible && stats == null)
            {
                stats = GameStatistics.Instance;
            }
        }
    }
    
    void InitializeStyles()
    {
        // Background
        backgroundStyle = new GUIStyle();
        backgroundStyle.normal.background = MakeTex(2, 2, backgroundColor);
        backgroundStyle.padding = new RectOffset(15, 15, 15, 15);
        
        // Title (main header)
        titleStyle = new GUIStyle();
        titleStyle.fontSize = 24;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = headerColor;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.padding = new RectOffset(0, 0, 10, 15);
        
        // Section header
        sectionStyle = new GUIStyle();
        sectionStyle.fontSize = 18;
        sectionStyle.fontStyle = FontStyle.Bold;
        sectionStyle.normal.textColor = headerColor;
        sectionStyle.padding = new RectOffset(5, 0, 15, 8);
        
        // Header (subsection)
        headerStyle = new GUIStyle();
        headerStyle.fontSize = 14;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = headerColor;
        headerStyle.padding = new RectOffset(0, 0, 5, 5);
        
        // Label (stat name)
        labelStyle = new GUIStyle();
        labelStyle.fontSize = 15;
        labelStyle.normal.textColor = labelColor;
        labelStyle.padding = new RectOffset(20, 0, 3, 3);
        
        // Value (stat value)
        valueStyle = new GUIStyle();
        valueStyle.fontSize = 15;
        valueStyle.fontStyle = FontStyle.Bold;
        valueStyle.normal.textColor = valueColor;
        valueStyle.alignment = TextAnchor.MiddleRight;
        valueStyle.padding = new RectOffset(0, 20, 3, 3);
        
        // Epoch label (pro epochy)
        epochLabelStyle = new GUIStyle(valueStyle);
        epochLabelStyle.normal.textColor = epochColor;
    }
    
    void OnGUI()
    {
        if (!isVisible) return;
        
        // Zajistit že styles jsou inicializované
        if (backgroundStyle == null)
            InitializeStyles();
        
        windowRect = GUI.Window(1, windowRect, DrawStatisticsWindow, "", backgroundStyle);
    }
    
    void DrawStatisticsWindow(int windowID)
    {
        if (stats == null)
        {
            stats = GameStatistics.Instance;
            
            if (stats == null)
            {
                GUILayout.Label("⚠ GameStatistics není dostupný!", labelStyle);
                GUILayout.Label("Přidej GameStatistics do scény!", labelStyle);
                GUI.DragWindow();
                return;
            }
        }
        
        GUILayout.BeginVertical();
        
        // Main Title
        GUILayout.Label("📊 HERNÍ STATISTIKY", titleStyle);
        GUILayout.Label($"Stiskni [{toggleKey}] pro zavření", headerStyle);
        
        GUILayout.Space(5);
        
        // Scroll view
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(380));
        
        // === ZÁKLADNÍ STATISTIKY ===
        DrawBasicStats();
        
        GUILayout.Space(10);
        
        // === NÁVŠTĚVY EPOCH ===
        DrawEpochStats();
        
        GUILayout.Space(10);
        
        // === ČAS HRANÍ ===
        DrawPlaytimeStats();
        
        GUILayout.EndScrollView();
        
        // Reset button
        GUILayout.Space(10);
        DrawResetButton();
        
        GUILayout.EndVertical();
        
        // Umožnit tažení okna
        GUI.DragWindow();
    }
    
    void DrawBasicStats()
    {
        GUILayout.Label("⚔️ OBECNÉ", sectionStyle);
        
        DrawStatRow("Celkový počet úmrtí", stats.GetTotalDeaths().ToString());
        DrawStatRow("Počet opakování hry", stats.GetTotalGames().ToString());
        
        int totalVisits = stats.GetTotalEpochVisits();
        DrawStatRow("Celkový počet návštěv epoch", totalVisits.ToString());
    }
    
    void DrawEpochStats()
    {
        GUILayout.Label("🌍 NÁVŠTĚVY EPOCH", sectionStyle);
        
        DrawEpochRow("Pravěk", stats.GetPravekVisits());
        DrawEpochRow("Starověk", stats.GetStarOvekVisits());
        DrawEpochRow("Středověk", stats.GetStredovekVisits());
        DrawEpochRow("Budoucnost", stats.GetBudoucnostVisits());
        
        // Nejnavštěvovanější epocha
        GUILayout.Space(8);
        string mostVisited = GetMostVisitedEpoch();
        if (!string.IsNullOrEmpty(mostVisited))
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("🏆 Nejnavštěvovanější epocha:", labelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(mostVisited, epochLabelStyle);
            GUILayout.EndHorizontal();
        }
    }
    
    void DrawPlaytimeStats()
    {
        GUILayout.Label("⏱️ ČAS HRANÍ", sectionStyle);
        
        string playtime = stats.GetTotalPlaytimeFormatted();
        DrawStatRow("Celkový čas hraní", playtime);
        
        float seconds = stats.GetTotalPlaytimeSeconds();
        int hours = Mathf.FloorToInt(seconds / 3600f);
        int minutes = Mathf.FloorToInt((seconds % 3600f) / 60f);
        
        if (hours > 0)
        {
            DrawStatRow("  → V hodinách", $"{hours} h {minutes} min");
        }
    }
    
    void DrawStatRow(string label, string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, labelStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label(value, valueStyle);
        GUILayout.EndHorizontal();
    }
    
    void DrawEpochRow(string epochName, int visits)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"  {epochName}", labelStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label($"{visits}x", epochLabelStyle);
        GUILayout.EndHorizontal();
    }
    
    void DrawResetButton()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        // Vytvoř button style
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.textColor = Color.red;
        buttonStyle.fontSize = 12;
        buttonStyle.padding = new RectOffset(10, 10, 5, 5);
        
        if (GUILayout.Button("⚠ Resetovat statistiky", buttonStyle, GUILayout.Width(180)))
        {
            if (stats != null)
            {
                stats.ResetAllStatistics();
                Debug.Log("StatisticsUI: Statistiky resetovány!");
            }
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
    
    string GetMostVisitedEpoch()
    {
        int pravek = stats.GetPravekVisits();
        int starovek = stats.GetStarOvekVisits();
        int stredovek = stats.GetStredovekVisits();
        int budoucnost = stats.GetBudoucnostVisits();
        
        int max = Mathf.Max(pravek, starovek, stredovek, budoucnost);
        
        if (max == 0) return "";
        
        if (pravek == max) return "Pravěk";
        if (starovek == max) return "Starověk";
        if (stredovek == max) return "Středověk";
        if (budoucnost == max) return "Budoucnost";
        
        return "";
    }
    
    private Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        
        return texture;
    }
}
