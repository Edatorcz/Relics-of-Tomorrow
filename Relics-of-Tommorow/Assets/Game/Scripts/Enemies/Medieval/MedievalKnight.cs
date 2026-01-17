using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Středověký rytíř v brnění - těžký melee bojovník
/// Pomalý ale velmi odolný
/// </summary>
public class MedievalKnight : EnemyBase
{
    [Header("Knight Behavior")]
    [SerializeField] private float armorReduction = 0.3f; // 30% damage reduction
    [SerializeField] private bool isDefending = false;
    [SerializeField] private float defendDuration = 2f;
    [SerializeField] private float defendCooldown = 5f;
    
    [Header("Combat Abilities")]
    [SerializeField] private float swordSlashDamage = 30f;
    [SerializeField] private float heavyStrikeDamage = 45f;
    [SerializeField] private float cleaveRange = 3f;
    [SerializeField] private float cleaveDamage = 35f;
    
    [Header("Tactical AI")]
    [SerializeField] private float chargeRange = 8f;
    [SerializeField] private bool canCharge = true;
    
    private bool isAttacking = false;
    private float lastDefendTime;
    private float defendTimer;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        maxHealth = 150f;
        currentHealth = maxHealth;
        damage = swordSlashDamage;
        moveSpeed = 2.5f; // Pomalý kvůli brnění
        detectionRange = 12f;
        attackRange = 2.8f;
        attackCooldown = 2f;
        
        useLineOfSight = false;
        
        ChangeState(EnemyState.Idle);
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (isDefending)
        {
            defendTimer -= Time.deltaTime;
            if (defendTimer <= 0)
            {
                isDefending = false;
            }
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
        
        // Charge attack při vzdálenosti
        if (canCharge && distanceToPlayer <= chargeRange && distanceToPlayer > attackRange && 
            Time.time >= lastDefendTime + defendCooldown)
        {
            if (Random.Range(0f, 1f) < 0.3f)
            {
                StartCoroutine(ChargeAttack());
                return;
            }
        }
        
        // Defend mode
        if (!isDefending && Time.time >= lastDefendTime + defendCooldown && Random.Range(0f, 1f) < 0.02f)
        {
            StartDefending();
            return;
        }
        
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
        }
        else if (!isDefending)
        {
            if (navMeshAgent != null && navMeshAgent.enabled)
            {
                navMeshAgent.SetDestination(player.position);
            }
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
            PerformKnightAttack();
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange)
        {
            ChangeState(EnemyState.Chasing);
        }
    }
    
    private void PerformKnightAttack()
    {
        float roll = Random.Range(0f, 1f);
        
        if (roll < 0.5f)
        {
            PerformSwordSlash();
        }
        else if (roll < 0.8f)
        {
            PerformHeavyStrike();
        }
        else
        {
            PerformCleaveAttack();
        }
    }
    
    private void PerformSwordSlash()
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
                    playerHealth.TakeDamage(swordSlashDamage);
                    Debug.Log($"Knight sword slash! Damage: {swordSlashDamage}");
                }
            }
        }
        
        Invoke(nameof(ResetAttack), 0.6f);
    }
    
    private void PerformHeavyStrike()
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
                    playerHealth.TakeDamage(heavyStrikeDamage);
                    Debug.Log($"Knight heavy strike! Damage: {heavyStrikeDamage}");
                }
            }
        }
        
        Invoke(nameof(ResetAttack), 1f);
    }
    
    private void PerformCleaveAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // AOE damage v kruhu
        Collider[] hits = Physics.OverlapSphere(transform.position, cleaveRange);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(cleaveDamage);
                    Debug.Log($"Knight cleave attack! Damage: {cleaveDamage}");
                }
            }
        }
        
        Invoke(nameof(ResetAttack), 1.2f);
    }
    
    private System.Collections.IEnumerator ChargeAttack()
    {
        if (player == null) yield break;
        
        isAttacking = true;
        lastAttackTime = Time.time;
        
        Debug.Log("Knight charging!");
        
        Vector3 chargeDirection = (player.position - transform.position).normalized;
        float chargeSpeed = 8f;
        float chargeTime = 0f;
        float maxChargeTime = 1.5f;
        
        while (chargeTime < maxChargeTime)
        {
            transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
            
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer <= 2f)
                {
                    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(heavyStrikeDamage * 1.2f);
                        Debug.Log("Knight charge hit!");
                    }
                    break;
                }
            }
            
            chargeTime += Time.deltaTime;
            yield return null;
        }
        
        isAttacking = false;
    }
    
    private void StartDefending()
    {
        isDefending = true;
        defendTimer = defendDuration;
        lastDefendTime = Time.time;
        
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.ResetPath();
        }
        
        Debug.Log("Knight defending!");
    }
    
    public override void TakeDamage(float damageAmount)
    {
        // Armor reduction
        float reducedDamage = damageAmount * (1f - armorReduction);
        
        // Extra reduction když defenduje
        if (isDefending)
        {
            reducedDamage *= 0.5f;
            Debug.Log("Knight blocked with defense stance!");
        }
        
        base.TakeDamage(reducedDamage);
    }
    
    private void ResetAttack()
    {
        isAttacking = false;
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
}
