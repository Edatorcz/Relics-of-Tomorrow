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
    
    [Header("Detection")]
    [SerializeField] protected float detectionRange = 10f;
    [SerializeField] protected float loseTargetRange = 15f;
    [SerializeField] protected LayerMask playerLayerMask = -1;
    
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
        // Nastavení zdraví
        currentHealth = maxHealth;
        
        // Automatické nalezení komponent
        SetupComponents();
        
        // Nalezení hráče
        FindPlayer();
        
        // Nastavení NavMeshAgent
        SetupNavMeshAgent();
        
        Debug.Log($"{gameObject.name}: Enemy initialized");
    }
    
    protected virtual void SetupComponents()
    {
        // NavMeshAgent
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            if (navMeshAgent == null)
            {
                navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            }
        }
        
        // Animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
    
    protected virtual void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerTransform = player;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Player not found! Make sure player has 'Player' tag");
        }
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
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Detekce hráče
        if (distanceToPlayer <= detectionRange && currentState != EnemyState.Chasing && currentState != EnemyState.Attacking)
        {
            if (CanSeePlayer())
            {
                ChangeState(EnemyState.Chasing);
                OnPlayerDetected?.Invoke(this);
            }
        }
        // Ztráta hráče
        else if (distanceToPlayer > loseTargetRange && (currentState == EnemyState.Chasing || currentState == EnemyState.Attacking))
        {
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
        if (isDead) return;
        
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        OnHealthChanged?.Invoke(this, currentHealth / maxHealth);
        
        if (currentHealth <= 0)
        {
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
        
        // Destroy po určitém čase
        Destroy(gameObject, 5f);
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
            
            lastAttackTime = Time.time;
            
            // Trigger attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }
    }
    
    // Gettery
    public EnemyState GetCurrentState() => currentState;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsDead() => isDead;
    public Transform GetPlayer() => player;
    
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
