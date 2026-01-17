using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Římský lučištník - ranged nepřítel s lukem
/// Drží si odstup a střílí šípy
/// </summary>
public class AncientArcher : EnemyBase
{
    [Header("Archer Behavior")]
    // Optimal range - můžete použít pro positioning AI později
    // private float optimalRange = 10f;
    [SerializeField] private float minRange = 5f;
    [SerializeField] private float maxRange = 15f;
    [SerializeField] private GameObject arrowPrefab;
    
    [Header("Combat Abilities")]
    [SerializeField] private float arrowDamage = 20f;
    [SerializeField] private float arrowSpeed = 25f;
    [SerializeField] private float shootCooldown = 2f;
    [SerializeField] private int volleyArrows = 3;
    [SerializeField] private bool canVolley = true;
    
    [Header("Tactical AI")]
    [SerializeField] private float retreatSpeed = 5f;
    // Retreating state - můžete použít pro vizuální efekty
    // private bool isRetreating = false;
    
    private float lastShootTime;
    private bool isAttacking = false;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        maxHealth = 70f;
        currentHealth = maxHealth;
        damage = arrowDamage;
        moveSpeed = 3f;
        detectionRange = 20f;
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
        
        // Příliš blízko - ustoupit
        if (distanceToPlayer < minRange)
        {
            RetreatFromPlayer();
        }
        // V optimálním dosahu - útok
        else if (distanceToPlayer >= minRange && distanceToPlayer <= maxRange)
        {
            ChangeState(EnemyState.Attacking);
        }
        // Příliš daleko - přiblížit se
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
        
        // Příliš blízko - ustoupit
        if (distanceToPlayer < minRange)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // Zastav pohyb
        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.hasPath)
        {
            navMeshAgent.ResetPath();
        }
        
        // Otočit se k hráči
        Vector3 direction = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        
        // Střelba
        if (!isAttacking && Time.time >= lastShootTime + shootCooldown)
        {
            if (canVolley && Random.Range(0f, 1f) < 0.3f) // 30% šance na salvu
            {
                StartCoroutine(ShootVolley());
            }
            else
            {
                ShootArrow();
            }
        }
        
        // Příliš daleko - chase
        if (distanceToPlayer > maxRange)
        {
            ChangeState(EnemyState.Chasing);
        }
    }
    
    private void ShootArrow()
    {
        if (arrowPrefab == null || player == null) return;
        
        isAttacking = true;
        lastShootTime = Time.time;
        
        // Spawn šípu
        Vector3 spawnPosition = transform.position + Vector3.up * 1.5f + transform.forward * 0.5f;
        GameObject arrow = Instantiate(arrowPrefab, spawnPosition, transform.rotation);
        
        // Setup projectile
        EnemyProjectile projectile = arrow.GetComponent<EnemyProjectile>();
        if (projectile == null)
        {
            projectile = arrow.AddComponent<EnemyProjectile>();
        }
        
        projectile.SetOwner(this);
        projectile.SetDamage(arrowDamage);
        
        // Vypočítat směr s predict
        Vector3 targetPosition = player.position + Vector3.up * 1f;
        Vector3 direction = (targetPosition - spawnPosition).normalized;
        
        // Přidat Rigidbody pokud chybí
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = arrow.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = false;
        rb.linearVelocity = direction * arrowSpeed;
        
        // Otočit šíp směrem letu
        arrow.transform.rotation = Quaternion.LookRotation(direction);
        
        Debug.Log($"Archer shot arrow at {targetPosition}");
        
        Invoke(nameof(ResetAttack), 0.3f);
    }
    
    private System.Collections.IEnumerator ShootVolley()
    {
        isAttacking = true;
        lastShootTime = Time.time;
        
        for (int i = 0; i < volleyArrows; i++)
        {
            ShootArrow();
            yield return new WaitForSeconds(0.2f);
        }
        
        isAttacking = false;
    }
    
    private void RetreatFromPlayer()
    {
        if (player == null || navMeshAgent == null || !navMeshAgent.enabled) return;
        
        // isRetreating = true; // Zakomentováno - můžete použít pro animace
        
        // Utíkat opačným směrem
        Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
        Vector3 retreatPosition = transform.position + directionAwayFromPlayer * 5f;
        
        navMeshAgent.speed = retreatSpeed;
        navMeshAgent.SetDestination(retreatPosition);
        
        Invoke(nameof(StopRetreating), 1f);
    }
    
    private void StopRetreating()
    {
        // isRetreating = false; // Zakomentováno - můžete použít pro animace
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
