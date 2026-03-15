using UnityEngine;
using TMPro;

/// <summary>
/// Panel pro zobrazení herních statistik v hlavním menu
/// </summary>
public class MenuStatisticsPanel : MonoBehaviour
{
    [Header("Statistics Text Fields")]
    [SerializeField] private TMP_Text totalDeathsText;
    [SerializeField] private TMP_Text totalGamesText;
    [SerializeField] private TMP_Text totalPlaytimeText;
    
    [Header("Epoch Visits Text Fields")]
    [SerializeField] private TMP_Text pravekVisitsText;
    [SerializeField] private TMP_Text starOvekVisitsText;
    [SerializeField] private TMP_Text stredovekVisitsText;
    [SerializeField] private TMP_Text budoucnostVisitsText;
    [SerializeField] private TMP_Text totalEpochVisitsText;
    [SerializeField] private TMP_Text mostVisitedEpochText;
    
    private GameStatistics stats;
    
    void OnEnable()
    {
        // Při otevření panelu aktualizovat statistiky
        RefreshStatistics();
    }
    
    void Start()
    {
        stats = GameStatistics.Instance;
        RefreshStatistics();
    }
    
    /// <summary>
    /// Aktualizovat všechny statistiky
    /// </summary>
    public void RefreshStatistics()
    {
        if (stats == null)
        {
            stats = GameStatistics.Instance;
        }
        
        if (stats == null)
        {
            Debug.LogWarning("MenuStatisticsPanel: GameStatistics není dostupný!");
            ShowErrorMessage();
            return;
        }
        
        UpdateBasicStats();
        UpdateEpochStats();
    }
    
    /// <summary>
    /// Aktualizovat základní statistiky
    /// </summary>
    private void UpdateBasicStats()
    {
        // Celkový počet úmrtí
        if (totalDeathsText != null)
            totalDeathsText.text = stats.GetTotalDeaths().ToString();
        
        // Počet opakování hry
        if (totalGamesText != null)
            totalGamesText.text = stats.GetTotalGames().ToString();
        
        // Celkový čas hraní
        if (totalPlaytimeText != null)
            totalPlaytimeText.text = stats.GetTotalPlaytimeFormatted();
    }
    
    /// <summary>
    /// Aktualizovat statistiky epoch
    /// </summary>
    private void UpdateEpochStats()
    {
        // Návštěvy jednotlivých epoch
        if (pravekVisitsText != null)
            pravekVisitsText.text = stats.GetPravekVisits().ToString() + "x";
        
        if (starOvekVisitsText != null)
            starOvekVisitsText.text = stats.GetStarOvekVisits().ToString() + "x";
        
        if (stredovekVisitsText != null)
            stredovekVisitsText.text = stats.GetStredovekVisits().ToString() + "x";
        
        if (budoucnostVisitsText != null)
            budoucnostVisitsText.text = stats.GetBudoucnostVisits().ToString() + "x";
        
        // Celkový počet návštěv
        if (totalEpochVisitsText != null)
            totalEpochVisitsText.text = stats.GetTotalEpochVisits().ToString();
        
        // Nejnavštěvovanější epocha
        if (mostVisitedEpochText != null)
        {
            string mostVisited = GetMostVisitedEpochName();
            mostVisitedEpochText.text = string.IsNullOrEmpty(mostVisited) ? "---" : mostVisited;
        }
    }
    
    /// <summary>
    /// Získat název nejnavštěvovanější epochy
    /// </summary>
    private string GetMostVisitedEpochName()
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
    
    /// <summary>
    /// Zobrazit chybovou zprávu když GameStatistics není dostupný
    /// </summary>
    private void ShowErrorMessage()
    {
        string errorMsg = "N/A";
        
        if (totalDeathsText != null) totalDeathsText.text = errorMsg;
        if (totalGamesText != null) totalGamesText.text = errorMsg;
        if (totalPlaytimeText != null) totalPlaytimeText.text = errorMsg;
        if (pravekVisitsText != null) pravekVisitsText.text = errorMsg;
        if (starOvekVisitsText != null) starOvekVisitsText.text = errorMsg;
        if (stredovekVisitsText != null) stredovekVisitsText.text = errorMsg;
        if (budoucnostVisitsText != null) budoucnostVisitsText.text = errorMsg;
        if (totalEpochVisitsText != null) totalEpochVisitsText.text = errorMsg;
        if (mostVisitedEpochText != null) mostVisitedEpochText.text = errorMsg;
    }
    
    /// <summary>
    /// Resetovat všechny statistiky (volá se z tlačítka v menu)
    /// </summary>
    public void ResetStatistics()
    {
        if (stats != null)
        {
            stats.ResetAllStatistics();
            RefreshStatistics();
            Debug.Log("MenuStatisticsPanel: Statistiky resetovány!");
        }
    }
}
