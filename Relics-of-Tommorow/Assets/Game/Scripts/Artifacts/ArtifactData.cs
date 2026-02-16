using UnityEngine;

/// <summary>
/// Enum pro různé typy artefaktů podle epoch
/// </summary>
public enum EpochType
{
    Pravek,      // Pravěk
    Starovek,    // Starověk
    Stredovek,   // Středověk
    Budoucnost   // Budoucnost
}

/// <summary>
/// Enum pro typy efektů artefaktů
/// </summary>
public enum ArtifactEffectType
{
    DamageBoost,        // Zvýšení poškození
    HealthBoost,        // Zvýšení maximálního zdraví
    SpeedBoost,         // Zvýšení rychlosti
    RegenBoost,         // Zvýšení regenerace
    DefenseBoost,       // Snížení přijatého poškození
    AttackSpeedBoost,   // Rychlejší útoky
    StaminaBoost,       // Více staminy
    CriticalChance,     // Šance na kritický zásah
    LifeSteal,          // Odsátí života
    ExpBoost,           // Více zkušeností (pokud máte XP systém)
    DropRateBoost,      // Lepší loot
    DashRecharge,       // Rychlejší dash
    Custom              // Vlastní efekt
}

/// <summary>
/// ScriptableObject pro definici artefaktu
/// Obsahuje všechny parametry a statistiky artefaktu
/// </summary>
[CreateAssetMenu(fileName = "NewArtifact", menuName = "Relics of Tomorrow/Artifact")]
public class ArtifactData : ScriptableObject
{
    [Header("Základní Info")]
    [Tooltip("Název artefaktu")]
    public string artifactName;
    
    [Tooltip("Popis efektu artefaktu")]
    [TextArea(3, 5)]
    public string description;
    
    [Tooltip("Ikona artefaktu pro UI")]
    public Sprite icon;
    
    [Tooltip("3D model artefaktu ve světě")]
    public GameObject worldModelPrefab;
    
    [Header("Epocha")]
    [Tooltip("Ke které epoše artefakt náleží")]
    public EpochType epoch;
    
    [Header("Efekty")]
    [Tooltip("Typ hlavního efektu artefaktu")]
    public ArtifactEffectType effectType;
    
    [Tooltip("Síla efektu (např. 1.5 = +50% damage, 20 = +20 health)")]
    public float effectValue;
    
    [Tooltip("Sekundární efekt (volitelný)")]
    public ArtifactEffectType secondaryEffectType;
    
    [Tooltip("Síla sekundárního efektu")]
    public float secondaryEffectValue;
    
    [Header("Vizuální Efekty")]
    [Tooltip("Barva auryn kolem hráče když je artefakt aktivní")]
    public Color auraColor = new Color(1f, 0.8f, 0.2f, 0.3f);
    
    [Tooltip("Particle efekt při aktivaci")]
    public GameObject activationParticles;
    
    [Header("Audio")]
    [Tooltip("Zvuk při sebrání artefaktu")]
    public AudioClip pickupSound;
    
    [Tooltip("Zvuk při aktivaci efektu")]
    public AudioClip activationSound;
}
