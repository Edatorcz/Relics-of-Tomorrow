using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Středověký lukostřelec - přesný ranged útočník
/// Rychlejší než ancient archer, více damage
/// </summary>
public class MedievalArcher : EnemyBase
{
    [Header("Archer Behavior")]
    // Optimal range - můžete použít pro positioning AI později
    // private float optimalRange = 12f;
    [SerializeField] private float minRange = 6f;
    [SerializeField] private float maxRange = 18f;
    [SerializeField] private GameObject arrowPrefab;
    
    [Header("Combat Abilities")]
    [SerializeField] private float arrowDamage = 25f;
    [SerializeField] private float arrowSpeed = 30f;
    [SerializeField] private float shootCooldown = 1.5f;
    [SerializeField] private bool canPowerShot = true;
    [SerializeField] private float powerShotDamage = 40f;
    [SerializeField] private float powerShotCooldown = 5f;
    
    [Header("Tactical AI")]
    [SerializeField] private float dodgeSpeed = 6f;
    [SerializeField] private float dodgeCooldown = 3f;
    [SerializeField] private bool canDodge = true;
    
    private bool isAttacking = false;
    private float lastShootTime;
    private float lastPowerShotTime;
    private float lastDodgeTime;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        maxHealth = 85f;
        currentHealth = maxHealth;
        damage = arrowDamage;
        moveSpeed = 3.5f;
        detectionRange = 22f;
        attackRange = maxRange;
        attackCooldown = shootCooldown;
        
        useLineOfSight = false;
        
        ChangeState(EnemyState.Idle);
    }
    
    protected override void HandleChaseState()
    {
        if (player == null)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Dodge pokud je hráč blízko
        if (canDodge && distanceToPlayer < minRange && Time.time >= lastDodgeTime + dodgeCooldown)
        {
            PerformDodge();
            return;
        }
        
        // V optim dosahu - útok
        if (distanceToPlayer >= minRange && distanceToPlayer <= maxRange)
        {
            ChangeState(EnemyState.Attacking);
        }
        // Přiblížit se nebo ustoupit
        else if (distanceToPlayer < minRange)
        {
            RetreatFromPlayer();
        }
        else
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
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer < minRange || distanceToPlayer > maxRange)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // Stop
        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.hasPath)
        {
            navMeshAgent.ResetPath();
        }
        
        // Otočit k hráči
        Vector3 direction = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        
        // Střelba
        if (!isAttacking && Time.time >= lastShootTime + shootCooldown)
        {
            if (canPowerShot && Time.time >= lastPowerShotTime + powerShotCooldown && Random.Range(0f, 1f) < 0.4f)
            {
                ShootPowerShot();
            }
            else
            {
                ShootArrow();
            }
        }
    }
    
    private void ShootArrow()
    {
        if (arrowPrefab == null || player == null) return;
        
        isAttacking = true;
        lastShootTime = Time.time;
        
        Vector3 spawnPosition = transform.position + Vector3.up * 1.5f + transform.forward * 0.5f;
        GameObject arrow = Instantiate(arrowPrefab, spawnPosition, transform.rotation);
        
        EnemyProjectile projectile = arrow.GetComponent<EnemyProjectile>();
        if (projectile == null)
        {
            projectile = arrow.AddComponent<EnemyProjectile>();
        }
        
        projectile.SetOwner(this);
        projectile.SetDamage(arrowDamage);
        
        Vector3 targetPosition = player.position + Vector3.up;
        Vector3 direction = (targetPosition - spawnPosition).normalized;
        
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = arrow.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = false;
        rb.linearVelocity = direction * arrowSpeed;
        arrow.transform.rotation = Quaternion.LookRotation(direction);
        
        Invoke(nameof(ResetAttack), 0.2f);
    }
    
    private void ShootPowerShot()
    {
        if (arrowPrefab == null || player == null) return;
        
        isAttacking = true;
        lastShootTime = Time.time;
        lastPowerShotTime = Time.time;
        
        Debug.Log("Medieval Archer: POWER SHOT!");
        
        Vector3 spawnPosition = transform.position + Vector3.up * 1.5f + transform.forward * 0.5f;
        GameObject arrow = Instantiate(arrowPrefab, spawnPosition, transform.rotation);
        
        // Zvětšit šíp
        arrow.transform.localScale *= 1.5f;
        
        EnemyProjectile projectile = arrow.GetComponent<EnemyProjectile>();
        if (projectile == null)
        {
            projectile = arrow.AddComponent<EnemyProjectile>();
        }
        
        projectile.SetOwner(this);
        projectile.SetDamage(powerShotDamage);
        
        Vector3 targetPosition = player.position + Vector3.up;
        Vector3 direction = (targetPosition - spawnPosition).normalized;
        
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = arrow.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = false;
        rb.linearVelocity = direction * (arrowSpeed * 1.5f);
        arrow.transform.rotation = Quaternion.LookRotation(direction);
        
        Invoke(nameof(ResetAttack), 0.5f);
    }
    
    private void PerformDodge()
    {
        lastDodgeTime = Time.time;
        
        Vector3 dodgeDirection = Random.insideUnitSphere;
        dodgeDirection.y = 0;
        dodgeDirection.Normalize();
        
        Vector3 dodgePosition = transform.position + dodgeDirection * 4f;
        
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.speed = dodgeSpeed;
            navMeshAgent.SetDestination(dodgePosition);
            
            Invoke(nameof(ResetDodgeSpeed), 0.5f);
        }
        
        Debug.Log("Medieval Archer dodged!");
    }
    
    private void RetreatFromPlayer()
    {
        if (player == null || navMeshAgent == null || !navMeshAgent.enabled) return;
        
        Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
        Vector3 retreatPosition = transform.position + directionAwayFromPlayer * 6f;
        
        navMeshAgent.SetDestination(retreatPosition);
    }
    
    private void ResetDodgeSpeed()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed;
        }
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
                Vector3 randomDirection = Random.insideUnitSphere * 8f;
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
