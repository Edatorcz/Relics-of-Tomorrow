using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Římský gladiátor - disciplinovaný melee bojovník
/// Používá štít a meč, umí blokovat útoky
/// </summary>
public class AncientGladiator : EnemyBase
{
    [Header("Gladiator Behavior")]
    [SerializeField] private float shieldBlockChance = 0.4f;
    [SerializeField] private bool isBlocking = false;
    [SerializeField] private float blockDuration = 1.5f;
    [SerializeField] private float counterAttackDamage = 35f;
    
    [Header("Combat Abilities")]
    [SerializeField] private float slashDamage = 25f;
    [SerializeField] private float thrustDamage = 30f;
    [SerializeField] private float shieldBashDamage = 20f;
    [SerializeField] private float shieldBashRange = 2.5f;
    
    [Header("Tactical AI")]
    [SerializeField] private float circleDistance = 4f;
    [SerializeField] private float circleSpeed = 2f;
    // Circling behavior - můžete implementovat později
    // private bool isCircling = false;
    
    private float blockTimer = 0f;
    private float circleAngle = 0f;
    private Vector3 circleCenter;
    private bool isAttacking = false;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        maxHealth = 100f;
        currentHealth = maxHealth;
        damage = slashDamage;
        moveSpeed = 3.5f;
        detectionRange = 15f;
        attackRange = 2.5f;
        attackCooldown = 1.5f;
        
        useLineOfSight = false;
        
        ChangeState(EnemyState.Idle);
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (isBlocking)
        {
            blockTimer -= Time.deltaTime;
            if (blockTimer <= 0)
            {
                isBlocking = false;
            }
        }
    }
    
    protected override void HandleIdleState()
    {
        if (Random.Range(0f, 1f) < 0.01f)
        {
            ChangeState(EnemyState.Patrol);
        }
    }
    
    protected override void HandlePatrolState()
    {
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            if (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.5f)
            {
                Vector3 randomDirection = Random.insideUnitSphere * 10f;
                randomDirection += transform.position;
                randomDirection.y = transform.position.y;
                
                navMeshAgent.SetDestination(randomDirection);
            }
        }
        
        if (Random.Range(0f, 1f) < 0.005f)
        {
            ChangeState(EnemyState.Idle);
        }
    }
    
    protected override void HandleStunnedState()
    {
        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.hasPath)
        {
            navMeshAgent.ResetPath();
        }
    }
    
    protected override void HandleChaseState()
    {
        if (player == null)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Rozhodnutí o blokování
        if (!isBlocking && Random.Range(0f, 1f) < 0.02f) // 2% šance každý frame
        {
            StartBlocking();
            return;
        }
        
        // Krouží kolem hráče v dosahu
        if (distanceToPlayer <= circleDistance && !isBlocking)
        {
            CircleAroundPlayer();
        }
        else if (distanceToPlayer > attackRange && !isBlocking)
        {
            // Běž k hráči
            if (navMeshAgent != null && navMeshAgent.enabled)
            {
                navMeshAgent.SetDestination(player.position);
            }
        }
        else if (distanceToPlayer <= attackRange && !isBlocking)
        {
            ChangeState(EnemyState.Attacking);
        }
    }
    
    protected override void HandleAttackState()
    {
        if (player == null)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformRandomAttack();
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange)
        {
            ChangeState(EnemyState.Chasing);
        }
    }
    
    private void PerformRandomAttack()
    {
        float roll = Random.Range(0f, 1f);
        
        if (roll < 0.4f)
        {
            PerformSlashAttack();
        }
        else if (roll < 0.7f)
        {
            PerformThrustAttack();
        }
        else
        {
            PerformShieldBash();
        }
    }
    
    private void PerformSlashAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Otočit se k hráči
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            
            // Damage hráče pokud je v dosahu
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= attackRange)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(slashDamage);
                    Debug.Log($"Gladiator slash attack! Damage: {slashDamage}");
                }
            }
        }
        
        Invoke(nameof(ResetAttack), 0.5f);
    }
    
    private void PerformThrustAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= attackRange)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(thrustDamage);
                    Debug.Log($"Gladiator thrust attack! Damage: {thrustDamage}");
                }
            }
        }
        
        Invoke(nameof(ResetAttack), 0.6f);
    }
    
    private void PerformShieldBash()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= shieldBashRange)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(shieldBashDamage);
                    // TODO: Přidat stun efekt
                    Debug.Log($"Gladiator shield bash! Damage: {shieldBashDamage}");
                }
            }
        }
        
        Invoke(nameof(ResetAttack), 0.8f);
    }
    
    private void StartBlocking()
    {
        isBlocking = true;
        blockTimer = blockDuration;
        
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.ResetPath();
        }
        
        Debug.Log("Gladiator blocking!");
    }
    
    private void CircleAroundPlayer()
    {
        if (player == null) return;
        
        circleCenter = player.position;
        circleAngle += circleSpeed * Time.deltaTime;
        
        Vector3 offset = new Vector3(
            Mathf.Sin(circleAngle) * circleDistance,
            0,
            Mathf.Cos(circleAngle) * circleDistance
        );
        
        Vector3 targetPosition = circleCenter + offset;
        
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.SetDestination(targetPosition);
        }
        
        // Otočit se k hráči
        Vector3 lookDirection = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z));
    }
    
    public override void TakeDamage(float damageAmount)
    {
        // Šance zablokovat damage
        if (isBlocking && Random.Range(0f, 1f) < shieldBlockChance)
        {
            Debug.Log("Gladiator blocked the attack!");
            // Counter attack
            if (player != null)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(counterAttackDamage * 0.5f);
                }
            }
            return;
        }
        
        base.TakeDamage(damageAmount);
    }
    
    private void ResetAttack()
    {
        isAttacking = false;
    }
}
