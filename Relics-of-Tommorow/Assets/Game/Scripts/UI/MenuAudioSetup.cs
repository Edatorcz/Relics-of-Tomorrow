using UnityEngine;

/// <summary>
/// Zajišťuje, že v menu scéně je AudioListener na Main Camera
/// Přidej tento skript na jakýkoliv GameObject v menu scéně (např. Canvas nebo EventSystem)
/// </summary>
public class MenuAudioSetup : MonoBehaviour
{
    [Header("Nastavení")]
    [Tooltip("Automaticky najít Main Camera a přidat AudioListener pokud chybí")]
    [SerializeField] private bool autoSetup = true;
    
    void Start()
    {
        if (autoSetup)
        {
            SetupAudioListener();
        }
    }
    
    void SetupAudioListener()
    {
        // Najít Main Camera
        Camera mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("MenuAudioSetup: Main Camera nebyla nalezena! Zkontroluj, že máš kameru s tagem 'MainCamera'");
            return;
        }
        
        // Zkontrolovat, jestli existuje AudioListener
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        
        if (listeners.Length == 0)
        {
            // Žádný AudioListener - přidat na Main Camera
            AudioListener listener = mainCamera.gameObject.AddComponent<AudioListener>();
            Debug.Log($"MenuAudioSetup: AudioListener byl přidán na {mainCamera.name}");
        }
        else if (listeners.Length > 1)
        {
            // Více než jeden AudioListener - varování
            Debug.LogWarning($"MenuAudioSetup: Nalezeno {listeners.Length} AudioListenerů! Měl by být pouze jeden. Unity použije první.");
            
            // Vypsat kde všechny jsou
            foreach (AudioListener listener in listeners)
            {
                Debug.LogWarning($"  - AudioListener na: {listener.gameObject.name}");
            }
        }
        else
        {
            // Právě jeden AudioListener - OK
            Debug.Log($"MenuAudioSetup: AudioListener nalezen na {listeners[0].gameObject.name} - OK!");
        }
        
        // Zkontrolovat jestli je AudioListener.volume > 0
        if (AudioListener.volume <= 0)
        {
            Debug.LogWarning($"MenuAudioSetup: AudioListener.volume je {AudioListener.volume}! Zvuky nebudou slyšet. Nastavuji na 1.0");
            AudioListener.volume = 1.0f;
        }
        else
        {
            Debug.Log($"MenuAudioSetup: AudioListener.volume = {AudioListener.volume}");
        }
    }
    
    /// <summary>
    /// Veřejná metoda pro manuální kontrolu zvukového setupu
    /// Můžeš zavolat z Debug menu
    /// </summary>
    [ContextMenu("Zkontrolovat Audio Setup")]
    public void CheckAudioSetup()
    {
        Debug.Log("=== AUDIO SETUP CHECK ===");
        
        // Zkontrolovat AudioListener
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        Debug.Log($"AudioListeners ve scéně: {listeners.Length}");
        foreach (AudioListener listener in listeners)
        {
            Debug.Log($"  - {listener.gameObject.name} (enabled: {listener.enabled})");
        }
        
        // Zkontrolovat AudioListener.volume
        Debug.Log($"AudioListener.volume: {AudioListener.volume}");
        
        // Zkontrolovat MenuSoundManager
        MenuSoundManager soundManager = MenuSoundManager.Instance;
        if (soundManager != null)
        {
            Debug.Log("MenuSoundManager: Nalezen ✓");
            Debug.Log($"  - UI Volume: {soundManager.uiVolume}");
            Debug.Log($"  - Music Volume: {soundManager.musicVolume}");
            Debug.Log($"  - Button Click: {(soundManager.buttonClick != null ? "✓" : "✗")}");
            Debug.Log($"  - Menu Music: {(soundManager.mainMenuMusic != null ? "✓" : "✗")}");
        }
        else
        {
            Debug.LogWarning("MenuSoundManager: NENALEZEN! Vytvoř ho pomocí Create > Relics of Tomorrow > Menu Sound Manager");
        }
        
        // Zkontrolovat AudioSource komponenty
        AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        Debug.Log($"AudioSources ve scéně: {audioSources.Length}");
        
        Debug.Log("=== END CHECK ===");
    }
}
