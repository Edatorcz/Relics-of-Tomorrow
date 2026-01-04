using UnityEngine;

/// <summary>
/// Combat systém pro hráče - útok, blok
/// LMB = útok, RMB = blok
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackRange = 10f; // Zvětšeno z 3 na 10
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackKnockbackForce = 5f;
    [SerializeField] private LayerMask enemyLayers;
    
    [Header("Block Settings")]
    [SerializeField] private float maxBlockStamina = 100f;
    [SerializeField] private float blockStaminaDrainRate = 20f; // za sekundu
    [SerializeField] private float blockStaminaRegenRate = 15f;
    [SerializeField] private float blockStaminaRegenDelay = 1f;
    [SerializeField] private float blockDamageReduction = 0.7f; // 70% redukce
    
    [Header("Attack Effects")]
    [SerializeField] private GameObject attackEffect;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioSource audioSource;
    
    // Private variables
    private float lastAttackTime;
    private float currentBlockStamina;
    private bool isBlocking = false;
    private float lastBlockTime;
    private Camera playerCamera;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    
    // Public properties
    public bool IsBlocking => isBlocking;
    public float BlockStamina => currentBlockStamina;
    public float MaxBlockStamina => maxBlockStamina;
    public float BlockStaminaPercent => currentBlockStamina / maxBlockStamina;
    
    void Start()
    {
        currentBlockStamina = maxBlockStamina;
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null) playerCamera = Camera.main;
        
        Debug.Log($"PlayerCombat Start: Camera found = {playerCamera != null}");
        if (playerCamera != null)
        {
            Debug.Log($"Camera position: {playerCamera.transform.position}");
            Debug.Log($"Camera forward: {playerCamera.transform.forward}");
        }
        
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Nastavit enemy layer pokud není nastaveno
        if (enemyLayers == 0)
        {
            enemyLayers = LayerMask.GetMask("Default");
        }
    }
    
    void Update()
    {
        HandleAttackInput();
        HandleBlockInput();
        HandleBlockStaminaRegen();
    }
    
    void HandleAttackInput()
    {
        // LMB pro útok
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"LMB CLICK - Blocking: {isBlocking}, CanAttack: {Time.time - lastAttackTime >= attackCooldown}");
        }
        
        if (Input.GetMouseButtonDown(0) && !isBlocking)
        {
            TryAttack();
        }
    }
    
    void HandleBlockInput()
    {
        // RMB pro blok
        if (Input.GetMouseButton(1) && currentBlockStamina > 0)
        {
            if (!isBlocking)
            {
                StartBlocking();
            }
            
            // Drain stamina během bloku
            currentBlockStamina -= blockStaminaDrainRate * Time.deltaTime;
            currentBlockStamina = Mathf.Max(0, currentBlockStamina);
            
            lastBlockTime = Time.time;
            
            // Zastavit blok pokud dojde stamina
            if (currentBlockStamina <= 0)
            {
                StopBlocking();
            }
        }
        else
        {
            if (isBlocking)
            {
                StopBlocking();
            }
        }
    }
    
    void HandleBlockStaminaRegen()
    {
        // Regenerace staminy když se neblokuje
        if (!isBlocking && Time.time - lastBlockTime >= blockStaminaRegenDelay)
        {
            currentBlockStamina += blockStaminaRegenRate * Time.deltaTime;
            currentBlockStamina = Mathf.Min(maxBlockStamina, currentBlockStamina);
        }
    }
    
    void TryAttack()
    {
        // Cooldown check
        if (Time.time - lastAttackTime < attackCooldown)
            return;
        
        lastAttackTime = Time.time;
        
        PerformAttack();
    }
    
    void PerformAttack()
    {
        // Raycast z kamery
        if (playerCamera != null)
        {
            Debug.Log($"Using camera: {playerCamera.name}, Position: {playerCamera.transform.position}, Forward: {playerCamera.transform.forward}");
            
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            
            Debug.Log($"Ray origin: {ray.origin}, direction: {ray.direction}");
            Debug.Log($"Attack raycast: mask={enemyLayers.value}");
            
            // VIZUALIZACE - červená čára kam mířím
            Debug.DrawRay(ray.origin, ray.direction * attackRange, Color.red, 2f);
            
            // TEST - zkus raycast BEZ masky a ukaž co trefíš
            RaycastHit testHit;
            if (Physics.Raycast(ray, out testHit, attackRange))
            {
                Debug.Log($"TEST (no mask): Would hit '{testHit.collider.gameObject.name}' on layer {testHit.collider.gameObject.layer} ({LayerMask.LayerToName(testHit.collider.gameObject.layer)})");
            }
            else
            {
                Debug.Log("TEST (no mask): Would hit NOTHING");
            }
            
            // Raycast s enemy layer filtrem
            if (Physics.Raycast(ray, out hit, attackRange, enemyLayers))
            {
                Debug.Log($"HIT: {hit.collider.gameObject.name}, Layer: {hit.collider.gameObject.layer}");
                
                // Zelená čára k zásahu
                Debug.DrawLine(ray.origin, hit.point, Color.green, 2f);
                
                // Zkusit najít EnemyBase - nejdřív na stejném objektu, pak na parentu
                EnemyBase enemy = hit.collider.GetComponent<EnemyBase>();
                if (enemy == null)
                {
                    Debug.Log("Checking parent for EnemyBase...");
                    enemy = hit.collider.GetComponentInParent<EnemyBase>();
                }
                
                if (enemy != null)
                {
                    Debug.Log($"FOUND ENEMY: {enemy.gameObject.name}, calling TakeDamage({attackDamage})");
                    enemy.TakeDamage(attackDamage);
                    
                    // Knockback
                    Rigidbody enemyRb = hit.collider.GetComponent<Rigidbody>();
                    if (enemyRb != null)
                    {
                        Vector3 knockbackDirection = (hit.collider.transform.position - transform.position).normalized;
                        knockbackDirection.y = 0.3f; // Trochu nahoru
                        enemyRb.AddForce(knockbackDirection * attackKnockbackForce, ForceMode.Impulse);
                    }
                    
                    // Spawn attack effect na místě zásahu
                    SpawnAttackEffect(hit.point);
                }
                else
                {
                    Debug.LogWarning($"Hit {hit.collider.gameObject.name} but NO EnemyBase found!");
                }
            }
            else
            {
                Debug.Log("Raycast MISS - no enemy hit");
                // Útok do vzduchu - spawn effect před hráčem
                Vector3 effectPosition = transform.position + playerCamera.transform.forward * 2f;
                SpawnAttackEffect(effectPosition);
            }
        }
        else
        {
            Debug.LogError("PlayerCamera is NULL!");
        }
        
        // Přehrát zvuk
        PlayAttackSound();
        
        // Zde může být animace
    }
    
    void SpawnAttackEffect(Vector3 position)
    {
        if (attackEffect != null)
        {
            GameObject effect = Instantiate(attackEffect, position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }
    
    void PlayAttackSound()
    {
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
    
    void StartBlocking()
    {
        isBlocking = true;
        
        // Informovat PlayerHealth o blokování
        if (playerHealth != null)
        {
            playerHealth.SetBlocking(true, blockDamageReduction);
        }
        
        // Zpomalit pohyb při blokování (volitelné)
        if (playerMovement != null)
        {
            // Můžeš přidat slowdown effect
        }
    }
    
    void StopBlocking()
    {
        isBlocking = false;
        
        if (playerHealth != null)
        {
            playerHealth.SetBlocking(false, 0);
        }
    }
    
    // Veřejné metody
    public void AddBlockStamina(float amount)
    {
        currentBlockStamina += amount;
        currentBlockStamina = Mathf.Min(maxBlockStamina, currentBlockStamina);
    }
    
    public void SetAttackDamage(float damage)
    {
        attackDamage = damage;
    }
    
    // Debug vizualizace
    void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * attackRange);
        }
    }
}
