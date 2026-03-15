using UnityEngine;

/// <summary>
/// Sleduje a ukládá všechny herní statistiky hráče
/// Používá PlayerPrefs pro persistenci mezi sezeními
/// </summary>
public class GameStatistics : MonoBehaviour
{
    public static GameStatistics Instance { get; private set; }
    
    // Klíče pro PlayerPrefs
    private const string KEY_TOTAL_DEATHS = "Stats_TotalDeaths";
    private const string KEY_TOTAL_GAMES = "Stats_TotalGames";
    private const string KEY_PRAVEK_VISITS = "Stats_PravekVisits";
    private const string KEY_STAROVEK_VISITS = "Stats_StarovekVisits";
    private const string KEY_STREDOVEK_VISITS = "Stats_StredovekVisits";
    private const string KEY_BUDOUCNOST_VISITS = "Stats_BudoucnostVisits";
    private const string KEY_TOTAL_PLAYTIME = "Stats_TotalPlaytime"; // v sekundách
    private const string KEY_CURRENT_SESSION_START = "Stats_CurrentSessionStart";
    
    // Statistiky
    private int totalDeaths = 0;
    private int totalGames = 0;
    private int pravekVisits = 0;
    private int starOvekVisits = 0;
    private int stredovekVisits = 0;
    private int budoucnostVisits = 0;
    private float totalPlaytime = 0f; // v sekundách
    private float sessionStartTime = 0f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadStatistics();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        sessionStartTime = Time.realtimeSinceStartup;
    }
    
    void OnApplicationQuit()
    {
        // Uložit čas hraní při ukončení
        UpdatePlaytime();
        SaveStatistics();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Aplikace jde do pozadí - uložit data
            UpdatePlaytime();
            SaveStatistics();
        }
    }
    
    /// <summary>
    /// Načíst všechny statistiky z PlayerPrefs
    /// </summary>
    private void LoadStatistics()
    {
        totalDeaths = PlayerPrefs.GetInt(KEY_TOTAL_DEATHS, 0);
        totalGames = PlayerPrefs.GetInt(KEY_TOTAL_GAMES, 0);
        pravekVisits = PlayerPrefs.GetInt(KEY_PRAVEK_VISITS, 0);
        starOvekVisits = PlayerPrefs.GetInt(KEY_STAROVEK_VISITS, 0);
        stredovekVisits = PlayerPrefs.GetInt(KEY_STREDOVEK_VISITS, 0);
        budoucnostVisits = PlayerPrefs.GetInt(KEY_BUDOUCNOST_VISITS, 0);
        totalPlaytime = PlayerPrefs.GetFloat(KEY_TOTAL_PLAYTIME, 0f);
        
        Debug.Log($"GameStatistics: Načteny statistiky - Úmrtí: {totalDeaths}, Hry: {totalGames}, " +
                  $"Pravěk: {pravekVisits}, Starověk: {starOvekVisits}, Středověk: {stredovekVisits}, Budoucnost: {budoucnostVisits}");
    }
    
    /// <summary>
    /// Uložit všechny statistiky do PlayerPrefs
    /// </summary>
    private void SaveStatistics()
    {
        PlayerPrefs.SetInt(KEY_TOTAL_DEATHS, totalDeaths);
        PlayerPrefs.SetInt(KEY_TOTAL_GAMES, totalGames);
        PlayerPrefs.SetInt(KEY_PRAVEK_VISITS, pravekVisits);
        PlayerPrefs.SetInt(KEY_STAROVEK_VISITS, starOvekVisits);
        PlayerPrefs.SetInt(KEY_STREDOVEK_VISITS, stredovekVisits);
        PlayerPrefs.SetInt(KEY_BUDOUCNOST_VISITS, budoucnostVisits);
        PlayerPrefs.SetFloat(KEY_TOTAL_PLAYTIME, totalPlaytime);
        PlayerPrefs.Save();
        
        Debug.Log("GameStatistics: Statistiky uloženy");
    }
    
    /// <summary>
    /// Aktualizovat celkový čas hraní
    /// </summary>
    private void UpdatePlaytime()
    {
        float sessionTime = Time.realtimeSinceStartup - sessionStartTime;
        totalPlaytime += sessionTime;
        sessionStartTime = Time.realtimeSinceStartup;
    }
    
    // === PUBLIC METODY PRO ZAZNAMENÁVÁNÍ UDÁLOSTÍ ===
    
    /// <summary>
    /// Zaznamenat smrt hráče
    /// </summary>
    public void RecordDeath()
    {
        totalDeaths++;
        SaveStatistics();
        Debug.Log($"GameStatistics: Zaznamenána smrt. Celkem: {totalDeaths}");
    }
    
    /// <summary>
    /// Zaznamenat začátek nové hry
    /// </summary>
    public void RecordNewGame()
    {
        totalGames++;
        SaveStatistics();
        Debug.Log($"GameStatistics: Zaznamenána nová hra. Celkem: {totalGames}");
    }
    
    /// <summary>
    /// Zaznamenat návštěvu epochy
    /// </summary>
    /// <param name="epochName">Název epochy (Pravěk, Starověk, Středověk, Budoucnost)</param>
    public void RecordEpochVisit(string epochName)
    {
        switch (epochName)
        {
            case "Pravěk":
            case "pravek":
                pravekVisits++;
                break;
            case "Starověk":
            case "starovek":
            case "Starovek":
                starOvekVisits++;
                break;
            case "Středověk":
            case "stredovek":
            case "Stredovek":
                stredovekVisits++;
                break;
            case "Budoucnost":
            case "budoucnost":
                budoucnostVisits++;
                break;
            default:
                Debug.LogWarning($"GameStatistics: Neznámá epocha '{epochName}'");
                return;
        }
        
        SaveStatistics();
        Debug.Log($"GameStatistics: Zaznamenána návštěva epochy '{epochName}'");
    }
    
    // === GETTERY PRO ZÍSKÁNÍ STATISTIK ===
    
    public int GetTotalDeaths() => totalDeaths;
    public int GetTotalGames() => totalGames;
    public int GetPravekVisits() => pravekVisits;
    public int GetStarOvekVisits() => starOvekVisits;
    public int GetStredovekVisits() => stredovekVisits;
    public int GetBudoucnostVisits() => budoucnostVisits;
    
    /// <summary>
    /// Získat celkový čas hraní v sekundách
    /// </summary>
    public float GetTotalPlaytimeSeconds()
    {
        UpdatePlaytime();
        return totalPlaytime;
    }
    
    /// <summary>
    /// Získat celkový čas hraní jako formátovaný string (HH:MM:SS)
    /// </summary>
    public string GetTotalPlaytimeFormatted()
    {
        float playtime = GetTotalPlaytimeSeconds();
        int hours = Mathf.FloorToInt(playtime / 3600f);
        int minutes = Mathf.FloorToInt((playtime % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(playtime % 60f);
        
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
    
    /// <summary>
    /// Resetovat všechny statistiky (pro testing nebo reset hry)
    /// </summary>
    public void ResetAllStatistics()
    {
        totalDeaths = 0;
        totalGames = 0;
        pravekVisits = 0;
        starOvekVisits = 0;
        stredovekVisits = 0;
        budoucnostVisits = 0;
        totalPlaytime = 0f;
        
        PlayerPrefs.DeleteKey(KEY_TOTAL_DEATHS);
        PlayerPrefs.DeleteKey(KEY_TOTAL_GAMES);
        PlayerPrefs.DeleteKey(KEY_PRAVEK_VISITS);
        PlayerPrefs.DeleteKey(KEY_STAROVEK_VISITS);
        PlayerPrefs.DeleteKey(KEY_STREDOVEK_VISITS);
        PlayerPrefs.DeleteKey(KEY_BUDOUCNOST_VISITS);
        PlayerPrefs.DeleteKey(KEY_TOTAL_PLAYTIME);
        PlayerPrefs.Save();
        
        Debug.Log("GameStatistics: Všechny statistiky resetovány");
    }
    
    /// <summary>
    /// Získat celkový počet návštěv všech epoch
    /// </summary>
    public int GetTotalEpochVisits()
    {
        return pravekVisits + starOvekVisits + stredovekVisits + budoucnostVisits;
    }
}
