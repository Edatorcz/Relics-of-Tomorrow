using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Správce přechodu mezi epochami
/// Když hráč umře, přesune se do další epochy
/// </summary>
public class EpochManager : MonoBehaviour
{
    public static EpochManager Instance { get; private set; }
    
    [Header("Epoch Progression")]
    [SerializeField] private string[] epochScenes = new string[]
    {
        "Pravěk",      // 0
        "Starověk",    // 1
        "Středověk",   // 2
        "Budoucnost"   // 3
    };
    
    [SerializeField] private float transitionDelay = 2f; // Čekání před přechodem
    
    [Header("Death Settings")]
    [SerializeField] private bool loopEpochs = true; // Vrátit se na začátek po poslední epoše
    [SerializeField] private string gameOverScene = "Menu"; // Scéna po dokončení všech epoch
    
    private int currentEpochIndex = -1;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Přetrvá mezi scénami
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Zjistit aktuální epochu
        DetectCurrentEpoch();
        
        // Přihlásit se k události smrti hráče
        SubscribeToPlayerDeath();
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DetectCurrentEpoch();
        SubscribeToPlayerDeath();
    }
    
    /// <summary>
    /// Zjistit index aktuální epochy
    /// </summary>
    private void DetectCurrentEpoch()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        for (int i = 0; i < epochScenes.Length; i++)
        {
            if (epochScenes[i] == currentScene)
            {
                currentEpochIndex = i;
                Debug.Log($"EpochManager: Aktuální epocha: {currentScene} (index {i})");
                return;
            }
        }
        
        Debug.Log($"EpochManager: Scéna '{currentScene}' není epocha");
    }
    
    /// <summary>
    /// Přihlásit se k události smrti hráče
    /// </summary>
    private void SubscribeToPlayerDeath()
    {
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Odhlásit se od předchozích eventů
            playerHealth.OnPlayerDied -= OnPlayerDeath;
            // Přihlásit se znovu
            playerHealth.OnPlayerDied += OnPlayerDeath;
            Debug.Log("EpochManager: Přihlášen k PlayerHealth.OnPlayerDied");
        }
    }
    
    /// <summary>
    /// Zavolá se když hráč umře
    /// </summary>
    private void OnPlayerDeath()
    {
        Debug.Log("EpochManager: Hráč zemřel! Přechod do další epochy...");
        StartCoroutine(TransitionToNextEpoch());
    }
    
    /// <summary>
    /// Přejít do další epochy
    /// </summary>
    private IEnumerator TransitionToNextEpoch()
    {
        // Počkat chvíli (ukázat smrt, přehrát animaci, atd.)
        yield return new WaitForSeconds(transitionDelay);
        
        // SMAZAT INVENTÁŘ (hráč zemřel, přijde o věci)
        if (InventorySystem.Instance != null)
        {
            Debug.Log("EpochManager: Mažu inventář (smrt hráče)...");
            InventorySystem.Instance.ClearInventory();
        }
        
        // Smazat hráče
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log("EpochManager: Ničím hráče...");
            Destroy(player);
        }
        
        // Načíst další epochu
        LoadNextEpoch();
    }
    
    /// <summary>
    /// Načíst další epochu
    /// </summary>
    private void LoadNextEpoch()
    {
        if (currentEpochIndex < 0)
        {
            Debug.LogWarning("EpochManager: Nejsme v epoše, načítám první epochu");
            LoadEpoch(0);
            return;
        }
        
        int nextIndex = currentEpochIndex + 1;
        
        // Zkontrolovat jestli existuje další epocha
        if (nextIndex < epochScenes.Length)
        {
            Debug.Log($"EpochManager: Načítám další epochu: {epochScenes[nextIndex]}");
            LoadEpoch(nextIndex);
        }
        else if (loopEpochs)
        {
            Debug.Log("EpochManager: Poslední epocha dokončena, vracím se na začátek");
            LoadEpoch(0);
        }
        else
        {
            Debug.Log("EpochManager: Všechny epochy dokončeny, načítám game over scénu");
            LoadGameOver();
        }
    }
    
    /// <summary>
    /// Načíst konkrétní epochu
    /// </summary>
    private void LoadEpoch(int index)
    {
        if (index >= 0 && index < epochScenes.Length)
        {
            currentEpochIndex = index;
            SceneManager.LoadScene(epochScenes[index]);
        }
    }
    
    /// <summary>
    /// Načíst game over scénu
    /// </summary>
    private void LoadGameOver()
    {
        SceneManager.LoadScene(gameOverScene);
    }
    
    /// <summary>
    /// Manuální přechod do další epochy (pro testing nebo cheat)
    /// </summary>
    public void GoToNextEpoch()
    {
        LoadNextEpoch();
    }
    
    /// <summary>
    /// Manuální přechod do konkrétní epochy
    /// </summary>
    public void GoToEpoch(int index)
    {
        LoadEpoch(index);
    }
    
    /// <summary>
    /// Resetovat na první epochu
    /// </summary>
    public void RestartFromBeginning()
    {
        LoadEpoch(0);
    }
}
