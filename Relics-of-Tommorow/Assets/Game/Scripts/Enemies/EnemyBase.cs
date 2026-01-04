using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Základní abstraktní třída pro všechny nepřátele
/// Obsahuje společnou logiku pro health, damage, states atd.
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackCooldown = 1f;
    [SerializeField] protected float attackKnockbackForce = 3f;
    
    [Header("Detection")]
    [SerializeField] protected float detectionRange = 10f;
    [SerializeField] protected float loseTargetRange = 15f;
    [SerializeField] protected LayerMask playerLayerMask = -1;
    [SerializeField] protected bool useLineOfSight = true; // Nová možnost
    
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float rotationSpeed = 5f;
    
    [Header("Components")]
    [SerializeField] protected NavMeshAgent navMeshAgent;
    [SerializeField] protected Animator animator;
    
    // Protected proměnné
    protected Transform player;
    protected Transform playerTransform;
    protected EnemyState currentState = EnemyState.Idle;
    protected float lastAttackTime;
    protected bool isDead = false;
    protected Vector3 spawnPosition; // Uložení spawn pozice
    protected Quaternion spawnRotation; // Uložení spawn rotace
    protected Color originalColor; // Uložení původní barvy
    
    // Events
    public System.Action<EnemyBase> OnDeath;
    public System.Action<EnemyBase, float> OnHealthChanged;
    public System.Action<EnemyBase> OnPlayerDetected;
    public System.Action<EnemyBase> OnPlayerLost;
    
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chasing,
        Attacking,
        Stunned,
        Dead
    }
    
    protected virtual void Start()
    {
        Initialize();
    }
    
    protected virtual void Initialize()
    {
        // Uložit spawn pozici a rotaci
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        
        // Uložit původní barvu
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            originalColor = renderer.material.color;
        }
        
        // Nastavení zdraví
        currentHealth = maxHealth;
        
        // Automatické nalezení komponent
        SetupComponents();
        
        // Nalezení hráče
        FindPlayer();
        
        // Nastavení NavMeshAgent
        SetupNavMeshAgent();
        
        // Vytvořit health bar
        SetupHealthBar();
    }
    
    protected virtual void SetupComponents()
    {
        // NavMeshAgent - pouze pokud již existuje nebo pokud je v levelu NavMesh
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            // NEBUDEME automaticky přidávat NavMeshAgent pokud není NavMesh
            // if (navMeshAgent == null)
            // {
            //     navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            // }
        }
        
        // Animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Collider - DŮLEŽITÉ pro PlayerCombat raycast!
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            // Přidat CapsuleCollider jako default
            CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.5f;
            capsule.center = new Vector3(0, 1f, 0);
            capsule.isTrigger = false; // NENÍ trigger, aby raycast fungoval!
        }
        else
        {
            // Ujistit se že NENÍ trigger
            if (collider.isTrigger)
            {
                collider.isTrigger = false;
            }
        }
        
        // Rigidbody - nastavit Collision Detection pro raycasty
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.linearDamping = 2f; // Střední tření - viditelný knockback ale nezabrousí moc daleko
            rb.angularDamping = 2f; // Tření rotace
        }
    }
    
    protected virtual void FindPlayer()
    {
        // Najdi všechny objekty s tagem "Player"
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        
        if (allPlayers.Length > 0)
        {
            // Vezmi první nalezený objekt
            GameObject playerObj = allPlayers[0];
            player = playerObj.transform;
            playerTransform = player;
            Debug.Log($"{gameObject.name}: Player found by tag: {player.name}");
            return;
        }
        
        // Fallback: Najdi podle jména
        GameObject playerByName = GameObject.Find("Player");
        if (playerByName != null)
        {
            player = playerByName.transform;
            playerTransform = player;
            Debug.Log($"{gameObject.name}: Player found by name: {player.name}");
            return;
        }
        
        // Fallback: Najdi podle komponenty PlayerHealth
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            player = playerHealth.transform;
            playerTransform = player;
            Debug.Log($"{gameObject.name}: Player found by PlayerHealth component: {player.name}");
            return;
        }
        
        Debug.LogWarning($"{gameObject.name}: Player not found! Make sure player has 'Player' tag or name.");
    }
    
    protected virtual void SetupNavMeshAgent()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.angularSpeed = rotationSpeed * 60f; // Převod na stupně za sekundu
            navMeshAgent.stoppingDistance = attackRange * 0.8f;
        }
    }
    
    protected virtual void Update()
    {
        if (isDead) return;
        
        HandleStateMachine();
        UpdateAnimations();
    }
    
    protected virtual void HandleStateMachine()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Patrol:
                HandlePatrolState();
                break;
            case EnemyState.Chasing:
                HandleChaseState();
                break;
            case EnemyState.Attacking:
                HandleAttackState();
                break;
            case EnemyState.Stunned:
                HandleStunnedState();
                break;
        }
        
        // Kontrola detekce hráče
        CheckPlayerDetection();
    }
    
    protected virtual void CheckPlayerDetection()
    {
        // Pokud není hráč nalezen, zkus ho najít
        if (player == null)
        {
            if (Time.frameCount % 60 == 0) // Každou sekundu
            {
                FindPlayer();
            }
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Debug každých 60 framů
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"{gameObject.name}: Player distance: {distanceToPlayer:F1}, Detection range: {detectionRange}, State: {currentState}");
            Debug.Log($"{gameObject.name}: Tracking '{player.name}' at position: {player.position}, Enemy position: {transform.position}");
            
            // Kontrola, jestli je to správný hráč - pokud je moc daleko, najdi znovu
            if (distanceToPlayer > 50f) // Pokud je player "podezřele" daleko
            {
                Debug.LogWarning($"{gameObject.name}: Player seems too far ({distanceToPlayer:F1}), searching for correct player...");
                FindPlayer();
            }
        }
        
        // Detekce hráče
        if (distanceToPlayer <= detectionRange && currentState != EnemyState.Chasing && currentState != EnemyState.Attacking)
        {
            bool canSee = useLineOfSight ? CanSeePlayer() : true; // Pokud je vypnutý raycast, vždy "vidí"
            
            if (canSee)
            {
                Debug.Log($"{gameObject.name}: Player detected! Switching to chase");
                ChangeState(EnemyState.Chasing);
                OnPlayerDetected?.Invoke(this);
            }
        }
        // Ztráta hráče
        else if (distanceToPlayer > loseTargetRange && (currentState == EnemyState.Chasing || currentState == EnemyState.Attacking))
        {
            Debug.Log($"{gameObject.name}: Lost player, going idle");
            ChangeState(EnemyState.Idle);
            OnPlayerLost?.Invoke(this);
        }
    }
    
    protected virtual bool CanSeePlayer()
    {
        if (player == null) return false;
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
        {
            return hit.transform == player;
        }
        
        return false;
    }
    
    // Abstraktní metody pro různé stavy
    protected abstract void HandleIdleState();
    protected abstract void HandlePatrolState();
    protected abstract void HandleChaseState();
    protected abstract void HandleAttackState();
    protected abstract void HandleStunnedState();
    
    /// <summary>
    /// Pomocná metoda pro pohyb k hráči - funguje i bez NavMesh
    /// </summary>
    protected virtual void MoveTowardsPlayer()
    {
        if (player == null) return;
        
        // Pokud máme NavMeshAgent, použij ho
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.SetDestination(player.position);
        }
        else
        {
            // Fallback: Přímý pohyb bez pathfindingu
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Jen v horizontální rovině
            
            // Pohyb
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            // Otáčení
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    protected virtual void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        
        ExitState(currentState);
        currentState = newState;
        EnterState(newState);
    }
    
    protected virtual void EnterState(EnemyState state)
    {
        Debug.Log($"{gameObject.name}: Entering state: {state}");
    }
    
    protected virtual void ExitState(EnemyState state)
    {
        // Override v potomcích podle potřeby
    }
    
    protected virtual void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Základní animace
        animator.SetFloat("Speed", navMeshAgent != null ? navMeshAgent.velocity.magnitude : 0f);
        animator.SetBool("IsChasing", currentState == EnemyState.Chasing);
        animator.SetBool("IsAttacking", currentState == EnemyState.Attacking);
        animator.SetBool("IsDead", isDead);
    }
    
    // Damage systém
    public virtual void TakeDamage(float damageAmount)
    {
        Debug.Log($"{gameObject.name}: TakeDamage called! Damage: {damageAmount}, Current HP: {currentHealth}, IsDead: {isDead}");
        
        if (isDead) 
        {
            Debug.Log($"{gameObject.name}: Already dead, ignoring damage");
            return;
        }
        
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        Debug.Log($"{gameObject.name}: After damage - HP: {currentHealth}/{maxHealth}");
        
        OnHealthChanged?.Invoke(this, currentHealth / maxHealth);
        
        if (currentHealth <= 0)
        {
            Debug.Log($"{gameObject.name}: HP reached 0, calling Die()");
            Die();
        }
        else
        {
            // Reakce na damage
            OnDamageReceived();
        }
    }
    
    protected virtual void OnDamageReceived()
    {
        // Můžeme přepnout do chase stavu pokud jsme byli idle
        if (currentState == EnemyState.Idle || currentState == EnemyState.Patrol)
        {
            ChangeState(EnemyState.Chasing);
        }
    }
    
    protected virtual void Die()
    {
        isDead = true;
        ChangeState(EnemyState.Dead);
        
        // Zakázat NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }
        
        OnDeath?.Invoke(this);
        
        // Smazat a respawnout
        Invoke(nameof(RespawnEnemy), 3f);
    }
    
    protected virtual void RespawnEnemy()
    {
        Debug.Log($"{gameObject.name}: Starting respawn...");
        
        // Resetovat stav
        isDead = false;
        currentHealth = maxHealth;
        
        // Reset Rigidbody (zastavit pohyb z knockbacku)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Kompletní reset NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }
        
        // Teleportace
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        
        // Znovu zapnout NavMeshAgent s malým delayem
        StartCoroutine(ReenableNavMeshAgent());
        
        // Layer
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        
        // Barva
        Renderer enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null && enemyRenderer.material != null)
        {
            enemyRenderer.material.color = originalColor;
        }
        
        // Health bar
        OnHealthChanged?.Invoke(this, 1f);
        
        Debug.Log($"{gameObject.name}: Respawn complete at {spawnPosition}");
    }
    
    private System.Collections.IEnumerator ReenableNavMeshAgent()
    {
        yield return new WaitForEndOfFrame();
        
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
            navMeshAgent.isStopped = false;
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.ResetPath();
            
            Debug.Log($"{gameObject.name}: NavMeshAgent re-enabled, can move: {navMeshAgent.enabled}");
        }
        
        // Reset do Idle stavu
        ChangeState(EnemyState.Idle);
    }
    
    public virtual void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            // Damage hráče
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            
            // Knockback hráče
            CharacterController playerController = player.GetComponent<CharacterController>();
            if (playerController != null)
            {
                Vector3 knockbackDirection = (player.position - transform.position).normalized;
                knockbackDirection.y = 0.2f;
                StartCoroutine(ApplyKnockbackToPlayer(playerController, knockbackDirection * attackKnockbackForce));
            }
            
            lastAttackTime = Time.time;
            
            // Trigger attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }
    }
    
    private System.Collections.IEnumerator ApplyKnockbackToPlayer(CharacterController controller, Vector3 force)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            if (controller != null && controller.enabled)
            {
                controller.Move(force * Time.deltaTime);
                force = Vector3.Lerp(force, Vector3.zero, elapsed / duration);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    // Gettery
    public EnemyState GetCurrentState() => currentState;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsDead() => isDead;
    public Transform GetPlayer() => player;
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    
    protected virtual void SetupHealthBar()
    {
        // Kontrola zda už health bar existuje
        EnemyHealthBar existingHealthBar = GetComponentInChildren<EnemyHealthBar>();
        if (existingHealthBar != null)
        {
            return;
        }
        
        // Vytvořit health bar GameObject
        GameObject healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(transform, false);
        healthBarObj.transform.localPosition = Vector3.zero;
        
        // Přidat EnemyHealthBar komponentu
        EnemyHealthBar healthBar = healthBarObj.AddComponent<EnemyHealthBar>();
    }
    
    // Debug
    protected virtual void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Lose target range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
    }
}
