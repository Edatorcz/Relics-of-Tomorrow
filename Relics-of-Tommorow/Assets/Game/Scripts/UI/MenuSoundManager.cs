using UnityEngine;

/// <summary>
/// Centrální ScriptableObject pro správu všech zvuků v menu
/// Umožňuje jednoduché nastavení všech zvuků na jednom místě
/// </summary>
[CreateAssetMenu(fileName = "MenuSoundManager", menuName = "Relics of Tomorrow/Menu Sound Manager")]
public class MenuSoundManager : ScriptableObject
{
    private static MenuSoundManager instance;
    
    [Header("UI Zvuky")]
    [Tooltip("Zvuk při kliknutí na tlačítko")]
    public AudioClip buttonClick;
    
    [Tooltip("Zvuk při najetí myší na tlačítko")]
    public AudioClip buttonHover;
    
    [Tooltip("Zvuk při otevření menu/panelu")]
    public AudioClip menuOpen;
    
    [Tooltip("Zvuk při zavření menu/panelu")]
    public AudioClip menuClose;
    
    [Tooltip("Zvuk při posunu slideru")]
    public AudioClip sliderMove;
    
    [Header("Feedback Zvuky")]
    [Tooltip("Zvuk pro potvrzení/úspěch")]
    public AudioClip confirmSound;
    
    [Tooltip("Zvuk pro chybu/zamítnutí")]
    public AudioClip errorSound;
    
    [Tooltip("Zvuk pro varování")]
    public AudioClip warningSound;
    
    [Header("Background Hudba")]
    [Tooltip("Hlavní menu hudba (loop)")]
    public AudioClip mainMenuMusic;
    
    [Tooltip("Pause menu hudba (loop)")]
    public AudioClip pauseMenuMusic;
    
    [Header("Game Over Zvuky")]
    [Tooltip("Zvuk pro game over screen")]
    public AudioClip gameOverSound;
    
    [Tooltip("Zvuk pro victory screen")]
    public AudioClip victorySound;
    
    [Header("Nastavení")]
    [Tooltip("Hlasitost UI zvuků (0-1)")]
    [Range(0f, 1f)]
    public float uiVolume = 0.7f;
    
    [Tooltip("Hlasitost background hudby (0-1)")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    
    /// <summary>
    /// Singleton instance - najde asset v Resources nebo v projektu
    /// </summary>
    public static MenuSoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Zkusit najít v Resources
                instance = Resources.Load<MenuSoundManager>("MenuSoundManager");
                
                // Pokud není v Resources, zkusit najít v projektu
                if (instance == null)
                {
                    instance = FindFirstAssetOfType<MenuSoundManager>();
                }
                
                if (instance == null)
                {
                    Debug.LogWarning("MenuSoundManager asset nebyl nalezen! Vytvořte ho pomocí Create > Relics of Tomorrow > Menu Sound Manager");
                }
            }
            return instance;
        }
    }
    
    /// <summary>
    /// Pomocná metoda pro nalezení ScriptableObject v projektu
    /// </summary>
    private static T FindFirstAssetOfType<T>() where T : ScriptableObject
    {
#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        if (guids.Length > 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif
        return null;
    }
}
