using UnityEngine;

/// <summary>
/// Centrální ScriptableObject pro správu všech zvuků portálů
/// Umožňuje jednoduché nastavení všech zvuků na jednom místě
/// </summary>
[CreateAssetMenu(fileName = "PortalSoundManager", menuName = "Relics of Tomorrow/Portal Sound Manager")]
public class PortalSoundManager : ScriptableObject
{
    private static PortalSoundManager instance;
    
    [Header("Zvuky portálů")]
    [Tooltip("Zvuk při otevření portálu (magické, energetické)")]
    public AudioClip portalOpen;
    
    [Tooltip("Zvuk při průchodu portálem (whoosh + energetický zvuk)")]
    public AudioClip portalEnter;
    
    [Tooltip("Ambientní loop aktivního portálu")]
    public AudioClip portalAmbientLoop;
    
    [Tooltip("Zvuk při zavření portálu")]
    public AudioClip portalClose;
    
    [Header("Nastavení")]
    [Tooltip("Hlasitost ambientního loop zvuku (0-1)")]
    [Range(0f, 1f)]
    public float ambientLoopVolume = 0.3f;
    
    [Tooltip("Fade in/out čas pro ambient loop (sekundy)")]
    public float ambientFadeTime = 1f;
    
    /// <summary>
    /// Singleton instance - najde asset v Resources nebo v projektu
    /// </summary>
    public static PortalSoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Zkusit najít v Resources
                instance = Resources.Load<PortalSoundManager>("PortalSoundManager");
                
                // Pokud není v Resources, zkusit najít v projektu
                if (instance == null)
                {
                    instance = FindFirstAssetOfType<PortalSoundManager>();
                }
                
                if (instance == null)
                {
                    Debug.LogWarning("PortalSoundManager asset nebyl nalezen! Vytvořte ho pomocí Create > Relics of Tomorrow > Portal Sound Manager");
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
