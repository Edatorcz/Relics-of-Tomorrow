using UnityEngine;

/// <summary>
/// Centrální ScriptableObject pro správu všech zvuků hráče
/// Umožňuje jednoduché nastavení všech zvuků na jednom místě
/// </summary>
[CreateAssetMenu(fileName = "PlayerSoundManager", menuName = "Relics of Tomorrow/Player Sound Manager")]
public class PlayerSoundManager : ScriptableObject
{
    private static PlayerSoundManager instance;
    
    [Header("Pohyb")]
    [Tooltip("Kroky při chůzi (pole zvuků pro variaci)")]
    public AudioClip[] footsteps;
    
    [Tooltip("Zvuk skoku")]
    public AudioClip jump;
    
    [Tooltip("Dopad po skoku")]
    public AudioClip land;
    
    [Tooltip("Dash/Sprint")]
    public AudioClip dash;
    
    [Header("Boj")]
    [Tooltip("Švih zbraně (3-4 varianty pro komba)")]
    public AudioClip[] attackSwing;
    
    [Tooltip("Zásah nepřítele")]
    public AudioClip attackHit;
    
    [Tooltip("Blokování útoků")]
    public AudioClip block;
    
    [Header("Poškození & Smrt")]
    [Tooltip("Poškození hráče (3-4 varianty)")]
    public AudioClip[] hurt;
    
    [Tooltip("Smrt hráče")]
    public AudioClip death;
    
    [Tooltip("Léčení/regenerace")]
    public AudioClip heal;
    
    [Header("Dýchání & Status")]
    [Tooltip("Těžké dýchání při nízké stamině")]
    public AudioClip breathHeavy;
    
    [Tooltip("Varování nízké staminy")]
    public AudioClip staminaLow;
    
    [Header("Nastavení")]
    [Tooltip("Hlasitost kroků (0-1)")]
    [Range(0f, 1f)]
    public float footstepVolume = 0.5f;
    
    [Tooltip("Hlasitost bojových zvuků (0-1)")]
    [Range(0f, 1f)]
    public float combatVolume = 0.7f;
    
    [Tooltip("Hlasitost zvuků poškození (0-1)")]
    [Range(0f, 1f)]
    public float damageVolume = 0.8f;
    
    /// <summary>
    /// Získá náhodný zvuk krokůFootsteps
    /// </summary>
    public AudioClip GetRandomFootstep()
    {
        if (footsteps == null || footsteps.Length == 0) return null;
        return footsteps[Random.Range(0, footsteps.Length)];
    }
    
    /// <summary>
    /// Získá náhodný zvuk švihu zbraně
    /// </summary>
    public AudioClip GetRandomAttackSwing()
    {
        if (attackSwing == null || attackSwing.Length == 0) return null;
        return attackSwing[Random.Range(0, attackSwing.Length)];
    }
    
    /// <summary>
    /// Získá náhodný zvuk poškození
    /// </summary>
    public AudioClip GetRandomHurt()
    {
        if (hurt == null || hurt.Length == 0) return null;
        return hurt[Random.Range(0, hurt.Length)];
    }
    
    /// <summary>
    /// Singleton instance - najde asset v Resources nebo v projektu
    /// </summary>
    public static PlayerSoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Zkusit najít v Resources
                instance = Resources.Load<PlayerSoundManager>("PlayerSoundManager");
                
                // Pokud není v Resources, zkusit najít v projektu
                if (instance == null)
                {
                    instance = FindFirstAssetOfType<PlayerSoundManager>();
                }
                
                if (instance == null)
                {
                    Debug.LogWarning("PlayerSoundManager asset nebyl nalezen! Vytvořte ho pomocí Create > Relics of Tomorrow > Player Sound Manager");
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
