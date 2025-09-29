using UnityEngine;

/// <summary>
/// Jednoduchý health systém pro hráče
/// Integruje se s enemy damage systémem
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isInvulnerable = false;
    [SerializeField] private float invulnerabilityDuration = 1f;
    
    [Header("Regeneration")]
    [SerializeField] private bool enableHealthRegen = true;
    [SerializeField] private float regenRate = 5f; // HP per second
    [SerializeField] private float regenDelay = 3f; // Delay after taking damage
    
    // Events
    public System.Action<float, float> OnHealthChanged; // (current, max)
    public System.Action OnPlayerDied;
    public System.Action<float> OnPlayerDamaged; // damage amount
    
    // Private variables
    private bool isDead = false;
    private float lastDamageTime;
    private float invulnerabilityEndTime;
    
    void Start()
    {
        Initialize();
    }
    
    void Initialize()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log("PlayerHealth: Initialized");
    }
    
    void Update()
    {
        HandleInvulnerability();
        HandleHealthRegeneration();
    }
    
    void HandleInvulnerability()
    {
        if (isInvulnerable && Time.time >= invulnerabilityEndTime)
        {
            isInvulnerable = false;
        }
    }
    
    void HandleHealthRegeneration()
    {
        if (!enableHealthRegen || isDead || currentHealth >= maxHealth) return;
        
        // Regenerace začne až po určitém čase od posledního damage
        if (Time.time - lastDamageTime >= regenDelay)
        {
            float regenAmount = regenRate * Time.deltaTime;
            Heal(regenAmount);
        }
    }
    
    public void TakeDamage(float damageAmount)
    {
        if (isDead || isInvulnerable || damageAmount <= 0) return;
        
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        lastDamageTime = Time.time;
        
        // Aktivovat invulnerability
        if (invulnerabilityDuration > 0)
        {
            isInvulnerable = true;
            invulnerabilityEndTime = Time.time + invulnerabilityDuration;
        }
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnPlayerDamaged?.Invoke(damageAmount);
        
        Debug.Log($"PlayerHealth: Took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float healAmount)
    {
        if (isDead || healAmount <= 0) return;
        
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        //Debug.Log($"PlayerHealth: Healed {healAmount}. Health: {currentHealth}/{maxHealth}");
    }
    
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void FullHeal()
    {
        Heal(maxHealth);
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        OnPlayerDied?.Invoke();
        
        Debug.Log("PlayerHealth: Player died!");
        
        // Zde můžete přidat další logiku smrti (game over screen, respawn, atd.)
    }
    
    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        isInvulnerable = false;
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log("PlayerHealth: Player respawned");
    }
    
    // Gettery
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsDead() => isDead;
    public bool IsInvulnerable() => isInvulnerable;
    
    // Debug visualization
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        // Jednoduchý health bar
        float barWidth = 200f;
        float barHeight = 20f;
        float x = Screen.width - barWidth - 20f;
        float y = 20f;
        
        // Background
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(x - 2, y - 2, barWidth + 4, barHeight + 4), Texture2D.whiteTexture);
        
        // Health bar
        GUI.color = Color.red;
        GUI.DrawTexture(new Rect(x, y, barWidth, barHeight), Texture2D.whiteTexture);
        
        GUI.color = Color.green;
        float healthWidth = (currentHealth / maxHealth) * barWidth;
        GUI.DrawTexture(new Rect(x, y, healthWidth, barHeight), Texture2D.whiteTexture);
        
        // Text
        GUI.color = Color.white;
        GUI.Label(new Rect(x, y + barHeight + 5, barWidth, 20), $"Health: {currentHealth:F0}/{maxHealth:F0}");
        
        if (isInvulnerable)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(x, y + barHeight + 25, barWidth, 20), "INVULNERABLE");
        }
        
        GUI.color = Color.white;
    }
}
