using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Pause menu - ESC pro pausu a možnost vrátit se do menu
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitToMenuButton;
    [SerializeField] private Button quitGameButton;
    
    [Header("Settings")]
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    
    private bool isPaused = false;
    
    void Start()
    {
        // Skrýt pause menu na začátku
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        // Připojit tlačítka
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        
        if (quitToMenuButton != null)
            quitToMenuButton.onClick.AddListener(QuitToMenu);
        
        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(QuitGame);
    }
    
    void Update()
    {
        // ESC pro toggle pause
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    /// <summary>
    /// Pausnout hru
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
        
        // Odemknout kurzor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("PauseMenuManager: Game paused");
    }
    
    /// <summary>
    /// Resumovat hru
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        // Zamknout kurzor zpět
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("PauseMenuManager: Game resumed");
    }
    
    /// <summary>
    /// Vrátit se do hlavního menu (opustit run)
    /// </summary>
    public void QuitToMenu()
    {
        Debug.Log("PauseMenuManager: Quitting to menu...");
        
        // Resumovat time scale před načtením scény
        isPaused = false;
        Time.timeScale = 1f;
        
        // Resetovat kurzor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Vyčistit artefakty (hráč opustil run)
        if (ArtifactManager.Instance != null)
        {
            ArtifactManager.Instance.ClearAllArtifacts();
        }
        
        // Vyčistit inventář
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.ClearInventory();
        }
        
        // Resetovat EpochManager pro novou hru
        if (EpochManager.Instance != null)
        {
            EpochManager.Instance.ResetForNewGame();
        }
        
        // Načíst menu scénu
        SceneManager.LoadScene(menuSceneName);
    }
    
    /// <summary>
    /// Ukončit hru (aplikaci)
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("PauseMenuManager: Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
