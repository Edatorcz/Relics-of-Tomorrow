using UnityEngine;

/// <summary>
/// Centrální ScriptableObject pro správu všech zvuků artefaktů
/// Umožňuje jednoduché nastavení všech zvuků na jednom místě
/// </summary>
[CreateAssetMenu(fileName = "ArtifactSoundManager", menuName = "Relics of Tomorrow/Artifact Sound Manager")]
public class ArtifactSoundManager : ScriptableObject
{
    private static ArtifactSoundManager instance;
    
    [Header("Obecné zvuky artefaktů")]
    [Tooltip("Zvuk při sebrání artefaktu")]
    public AudioClip artifactPickup;
    
    [Tooltip("Obecný zvuk aktivace (pokud není specifický)")]
    public AudioClip artifactActivateGeneric;
    
    [Tooltip("Zvuk při deaktivaci artefaktu")]
    public AudioClip artifactDeactivate;
    
    [Header("Zvuky podle typu efektu")]
    [Tooltip("Zvuk pro boost damage efekt")]
    public AudioClip damageBoostSound;
    
    [Tooltip("Zvuk pro boost rychlosti")]
    public AudioClip speedBoostSound;
    
    [Tooltip("Zvuk pro léčivý/health efekt")]
    public AudioClip healBoostSound;
    
    [Tooltip("Zvuk pro obranný efekt")]
    public AudioClip defenseBoostSound;
    
    [Tooltip("Zvuk pro attack speed boost")]
    public AudioClip attackSpeedBoostSound;
    
    [Tooltip("Zvuk pro stamina boost")]
    public AudioClip staminaBoostSound;
    
    [Tooltip("Zvuk pro critical chance boost")]
    public AudioClip criticalChanceSound;
    
    [Tooltip("Zvuk pro life steal efekt")]
    public AudioClip lifeStealSound;
    
    [Tooltip("Zvuk pro regeneraci")]
    public AudioClip regenBoostSound;
    
    /// <summary>
    /// Získá zvuk podle typu efektu artefaktu
    /// </summary>
    public AudioClip GetSoundForEffect(ArtifactEffectType effectType)
    {
        switch (effectType)
        {
            case ArtifactEffectType.DamageBoost:
                return damageBoostSound != null ? damageBoostSound : artifactActivateGeneric;
                
            case ArtifactEffectType.SpeedBoost:
                return speedBoostSound != null ? speedBoostSound : artifactActivateGeneric;
                
            case ArtifactEffectType.HealthBoost:
                return healBoostSound != null ? healBoostSound : artifactActivateGeneric;
                
            case ArtifactEffectType.RegenBoost:
                return regenBoostSound != null ? regenBoostSound : artifactActivateGeneric;
                
            case ArtifactEffectType.DefenseBoost:
                return defenseBoostSound != null ? defenseBoostSound : artifactActivateGeneric;
                
            case ArtifactEffectType.AttackSpeedBoost:
                return attackSpeedBoostSound != null ? attackSpeedBoostSound : artifactActivateGeneric;
                
            case ArtifactEffectType.StaminaBoost:
                return staminaBoostSound != null ? staminaBoostSound : artifactActivateGeneric;
                
            case ArtifactEffectType.CriticalChance:
                return criticalChanceSound != null ? criticalChanceSound : artifactActivateGeneric;
                
            case ArtifactEffectType.LifeSteal:
                return lifeStealSound != null ? lifeStealSound : artifactActivateGeneric;
                
            default:
                return artifactActivateGeneric;
        }
    }
    
    /// <summary>
    /// Singleton instance - najde asset v Resources nebo v projektu
    /// </summary>
    public static ArtifactSoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Zkusit najít v Resources
                instance = Resources.Load<ArtifactSoundManager>("ArtifactSoundManager");
                
                // Pokud není v Resources, zkusit najít v projektu
                if (instance == null)
                {
                    instance = FindFirstAssetOfType<ArtifactSoundManager>();
                }
                
                if (instance == null)
                {
                    Debug.LogWarning("ArtifactSoundManager asset nebyl nalezen! Vytvořte ho pomocí Create > Relics of Tomorrow > Artifact Sound Manager");
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
