using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Singleton manager pro správu artefaktů hráče
/// Stará se o aktivaci, deaktivaci a aplikaci efektů artefaktů
/// </summary>
public class ArtifactManager : MonoBehaviour
{
    public static ArtifactManager Instance { get; private set; }
    
    [Header("Active Artifacts")]
    [SerializeField] private List<ArtifactData> activeArtifacts = new List<ArtifactData>();
    
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerMovement playerMovement;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject auraEffectPrefab;
    private GameObject currentAura;
    
    // Modifikátory pro různé statistiky
    private float damageMultiplier = 1f;
    private float healthBonus = 0f;
    private float speedMultiplier = 1f;
    private float regenMultiplier = 1f;
    private float defenseMultiplier = 1f;
    private float attackSpeedMultiplier = 1f;
    private float staminaMultiplier = 1f;
    private float criticalChance = 0f;
    private float lifeStealPercent = 0f;
    
    // Eventy
    public System.Action<ArtifactData> OnArtifactActivated;
    public System.Action<ArtifactData> OnArtifactDeactivated;
    public System.Action OnAllArtifactsCleared;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        FindPlayerReferences();
        SubscribeToEvents();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    /// <summary>
    /// Najde reference na player komponenty
    /// </summary>
    private void FindPlayerReferences()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            Debug.Log($"ArtifactManager: PlayerHealth {(playerHealth != null ? "NALEZEN" : "NENALEZEN")}");
        }
        
        if (playerCombat == null)
        {
            playerCombat = FindFirstObjectByType<PlayerCombat>();
            Debug.Log($"ArtifactManager: PlayerCombat {(playerCombat != null ? "NALEZEN" : "NENALEZEN")}");
        }
            
        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
            Debug.Log($"ArtifactManager: PlayerMovement {(playerMovement != null ? "NALEZEN" : "NENALEZEN")}");
        }
    }
    
    /// <summary>
    /// Přihlásí se k eventům smrti a přechodu epoch
    /// </summary>
    private void SubscribeToEvents()
    {
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied += ClearAllArtifacts;
        }
        
        // Přihlásit se k epochManager eventu pokud existuje
        if (EpochManager.Instance != null)
        {
            // Můžete přidat event do EpochManager pro konec epochy
        }
    }
    
    /// <summary>
    /// Odhlásí se od eventů
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied -= ClearAllArtifacts;
        }
    }
    
    /// <summary>
    /// Aktivuje artefakt a aplikuje jeho efekty
    /// </summary>
    public void ActivateArtifact(ArtifactData artifact)
    {
        if (artifact == null)
        {
            Debug.LogWarning("ArtifactManager: Pokus o aktivaci null artefaktu!");
            return;
        }
        
        // KRITICKÉ: Ujistit se, že máme PlayerCombat HNED před aktivací
        if (playerCombat == null)
        {
            Debug.LogWarning("ArtifactManager: PlayerCombat je NULL! Hledám ho...");
            FindPlayerReferences();
        }
        
        // Zkontrolovat zda již artefakt není aktivní
        if (activeArtifacts.Contains(artifact))
        {
            Debug.Log($"ArtifactManager: Artefakt {artifact.artifactName} již je aktivní!");
            return;
        }
        
        Debug.Log($"=== ArtifactManager: Začínám aktivaci artefaktu '{artifact.artifactName}' ===");
        Debug.Log($"Effect Type: {artifact.effectType}, Effect Value: {artifact.effectValue}");
        Debug.Log($"Secondary Effect Type: {artifact.secondaryEffectType}, Secondary Value: {artifact.secondaryEffectValue}");
        Debug.Log($"PlayerCombat reference: {(playerCombat != null ? "OK" : "NULL!")}");
        
        // Přidat do seznamu
        activeArtifacts.Add(artifact);
        
        // Aplikovat efekty
        ApplyArtifactEffect(artifact);
        
        // Vizuální efekty
        CreateAuraEffect(artifact);
        PlayActivationEffects(artifact);
        
        // Event
        OnArtifactActivated?.Invoke(artifact);
        
        Debug.Log($"ArtifactManager: Artefakt {artifact.artifactName} AKTIVOVÁN! Damage multiplier = {damageMultiplier}");
        Debug.Log($"=== KONEC AKTIVACE ===");
    }
    
    /// <summary>
    /// Deaktivuje konkrétní artefakt
    /// </summary>
    public void DeactivateArtifact(ArtifactData artifact)
    {
        if (!activeArtifacts.Contains(artifact))
            return;
        
        // Odebrat z seznamu
        activeArtifacts.Remove(artifact);
        
        // Přepočítat všechny efekty
        RecalculateAllEffects();
        
        // Event
        OnArtifactDeactivated?.Invoke(artifact);
        
        Debug.Log($"ArtifactManager: Deaktivován artefakt {artifact.artifactName}");
    }
    
    /// <summary>
    /// Vymaže všechny aktivní artefakty (při smrti nebo dokončení epochy)
    /// </summary>
    public void ClearAllArtifacts()
    {
        Debug.Log("ArtifactManager: Mažu všechny artefakty!");
        
        activeArtifacts.Clear();
        ResetAllModifiers();
        
        // Zničit vizuální efekty
        if (currentAura != null)
        {
            Destroy(currentAura);
            currentAura = null;
        }
        
        // Event
        OnAllArtifactsCleared?.Invoke();
    }
    
    /// <summary>
    /// Aplikuje efekt artefaktu na hráče
    /// </summary>
    private void ApplyArtifactEffect(ArtifactData artifact)
    {
        Debug.Log($"ApplyArtifactEffect: Aplikuji hlavní efekt {artifact.effectType} = {artifact.effectValue}");
        // Hlavní efekt
        ApplySingleEffect(artifact.effectType, artifact.effectValue);
        
        // Sekundární efekt
        if (artifact.secondaryEffectType != ArtifactEffectType.Custom)
        {
            Debug.Log($"ApplyArtifactEffect: Aplikuji sekundární efekt {artifact.secondaryEffectType} = {artifact.secondaryEffectValue}");
            ApplySingleEffect(artifact.secondaryEffectType, artifact.secondaryEffectValue);
        }
    }
    
    /// <summary>
    /// Aplikuje jednotlivý efekt
    /// </summary>
    private void ApplySingleEffect(ArtifactEffectType effectType, float value)
    {
        switch (effectType)
        {
            case ArtifactEffectType.DamageBoost:
                Debug.Log($"ArtifactManager.ApplySingleEffect: Před aplikací - damageMultiplier = {damageMultiplier}, value = {value}");
                damageMultiplier += value - 1f;
                Debug.Log($"ArtifactManager.ApplySingleEffect: Po aplikaci - damageMultiplier = {damageMultiplier}");
                if (playerCombat != null)
                {
                    playerCombat.ApplyDamageMultiplier(damageMultiplier);
                    Debug.Log($"ArtifactManager: Volám playerCombat.ApplyDamageMultiplier({damageMultiplier})");
                }
                else
                {
                    Debug.LogError("ArtifactManager: playerCombat je NULL! Damage boost nebude aplikován!");
                }
                break;
                
            case ArtifactEffectType.HealthBoost:
                healthBonus += value;
                if (playerHealth != null)
                    playerHealth.IncreaseMaxHealth(value);
                break;
                
            case ArtifactEffectType.SpeedBoost:
                speedMultiplier += value - 1f;
                if (playerMovement != null)
                    playerMovement.ApplySpeedMultiplier(speedMultiplier);
                break;
                
            case ArtifactEffectType.RegenBoost:
                regenMultiplier += value - 1f;
                if (playerHealth != null)
                    playerHealth.ApplyRegenMultiplier(regenMultiplier);
                break;
                
            case ArtifactEffectType.DefenseBoost:
                defenseMultiplier += value - 1f;
                if (playerHealth != null)
                    playerHealth.ApplyDefenseMultiplier(defenseMultiplier);
                break;
                
            case ArtifactEffectType.AttackSpeedBoost:
                attackSpeedMultiplier += value - 1f;
                if (playerCombat != null)
                    playerCombat.ApplyAttackSpeedMultiplier(attackSpeedMultiplier);
                break;
                
            case ArtifactEffectType.StaminaBoost:
                staminaMultiplier += value - 1f;
                // TODO: Aplikovat na stamina systém pokud existuje
                break;
                
            case ArtifactEffectType.CriticalChance:
                criticalChance += value;
                if (playerCombat != null)
                    playerCombat.SetCriticalChance(criticalChance);
                break;
                
            case ArtifactEffectType.LifeSteal:
                lifeStealPercent += value;
                if (playerCombat != null)
                    playerCombat.SetLifeSteal(lifeStealPercent);
                break;
        }
    }
    
    /// <summary>
    /// Přepočítá všechny efekty ze všech aktivních artefaktů
    /// </summary>
    private void RecalculateAllEffects()
    {
        // Reset
        ResetAllModifiers();
        
        // Aplikovat všechny artefakty znovu
        foreach (var artifact in activeArtifacts)
        {
            ApplyArtifactEffect(artifact);
        }
    }
    
    /// <summary>
    /// Resetuje všechny modifikátory na výchozí hodnoty
    /// </summary>
    private void ResetAllModifiers()
    {
        damageMultiplier = 1f;
        healthBonus = 0f;
        speedMultiplier = 1f;
        regenMultiplier = 1f;
        defenseMultiplier = 1f;
        attackSpeedMultiplier = 1f;
        staminaMultiplier = 1f;
        criticalChance = 0f;
        lifeStealPercent = 0f;
        
        // Resetovat hodnoty na hráči
        if (playerHealth != null)
        {
            playerHealth.ResetModifiers();
        }
        if (playerCombat != null)
        {
            playerCombat.ResetModifiers();
            playerCombat.ApplyDamageMultiplier(1f);
        }
        if (playerMovement != null)
        {
            playerMovement.ResetModifiers();
        }
        
        Debug.Log("ArtifactManager: Všechny modifikátory resetovány");
    }
    
    /// <summary>
    /// Vytvoří vizuální auru kolem hráče
    /// </summary>
    private void CreateAuraEffect(ArtifactData artifact)
    {
        // Pokud už existuje aura, zničit ji
        if (currentAura != null)
        {
            Destroy(currentAura);
        }
        
        // Pokud máme prefab pro auru, vytvořit ji
        if (auraEffectPrefab != null && playerHealth != null)
        {
            currentAura = Instantiate(auraEffectPrefab, playerHealth.transform);
            
            // Nastavit barvu podle artefaktu
            ParticleSystem ps = currentAura.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = artifact.auraColor;
            }
        }
    }
    
    /// <summary>
    /// Přehraje efekty aktivace
    /// </summary>
    private void PlayActivationEffects(ArtifactData artifact)
    {
        // TODO: Přehrát zvuk a particle efekty
        if (artifact.activationSound != null)
        {
            // AudioSource.PlayClipAtPoint(artifact.activationSound, playerHealth.transform.position);
        }
    }
    
    // Gettery pro modifikátory
    public float GetDamageMultiplier() => damageMultiplier;
    public float GetSpeedMultiplier() => speedMultiplier;
    public float GetDefenseMultiplier() => defenseMultiplier;
    public float GetAttackSpeedMultiplier() => attackSpeedMultiplier;
    public float GetCriticalChance() => criticalChance;
    public float GetLifeStealPercent() => lifeStealPercent;
    
    public List<ArtifactData> GetActiveArtifacts() => new List<ArtifactData>(activeArtifacts);
    public bool HasArtifact(ArtifactData artifact) => activeArtifacts.Contains(artifact);
    public int GetActiveArtifactCount() => activeArtifacts.Count;
}
