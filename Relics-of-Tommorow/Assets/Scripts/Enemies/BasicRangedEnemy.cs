using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Ranged nepřítel, který střílí na hráče z dálky
/// Snaží se udržet distanci a útočí projektily
/// </summary>
public class BasicRangedEnemy : EnemyBase
{
    [Header("Ranged Combat")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float optimalDistance = 8f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 12f;
    
    [Header("Behavior")]
    [SerializeField] private float repositionCooldown = 3f;
    [SerializeField] private bool shouldKitePlayer = true; // Kiting = udržování distance
    
    private float lastRepositionTime;
    private Vector3 repositionTarget;
    private bool isRepositioning = false;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        // Vytvoříme shoot point pokud není nastaven
        if (shootPoint == null)
        {
            GameObject shootPointObj = new GameObject("ShootPoint");
            shootPointObj.transform.SetParent(transform);
            shootPointObj.transform.localPosition = Vector3.forward + Vector3.up * 1.5f;
            shootPoint = shootPointObj.transform;
        }
        
        // Nastavíme větší stopping distance
        if (navMeshAgent != null)
        {
            navMeshAgent.stoppingDistance = optimalDistance * 0.8f;
        }
    }
    
    protected override void HandleIdleState()
    {
        // Ranged enemy zůstává v idle a čeká na hráče
        if (navMeshAgent != null && navMeshAgent.hasPath)
        {
            navMeshAgent.ResetPath();
        }
    }
    
    protected override void HandlePatrolState()
    {
        // Ranged enemies obvykle nepatrolují, zůstávají na pozici
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
        
        // Pokud jsme v range pro útok
        if (distanceToPlayer <= maxDistance && distanceToPlayer >= minDistance)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }
        
        // Pokud je hráč příliš blízko, couvneme
        if (distanceToPlayer < minDistance && shouldKitePlayer)
        {
            MoveAwayFromPlayer();
        }
        // Pokud je příliš daleko, přiblížíme se
        else if (distanceToPlayer > maxDistance)
        {
            if (navMeshAgent != null)
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
        
        // Pokud je hráč mimo range, chase
        if (distanceToPlayer > maxDistance || distanceToPlayer < minDistance)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // Zastavit pohyb během střelby
        if (navMeshAgent != null && !isRepositioning)
        {
            navMeshAgent.ResetPath();
        }
        
        // Otočit se na hráče
        LookAtPlayer();
        
        // Střelba
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            ShootAtPlayer();
        }
        
        // Občasné přemístění pro lepší pozici
        if (Time.time - lastRepositionTime >= repositionCooldown && !isRepositioning)
        {
            ConsiderRepositioning();
        }
    }
    
    protected override void HandleStunnedState()
    {
        // Během stunu se nepohybujeme
        if (navMeshAgent != null && navMeshAgent.hasPath)
        {
            navMeshAgent.ResetPath();
        }
    }
    
    private void LookAtPlayer()
    {
        if (player == null) return;
        
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Pouze horizontální rotace
        
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void ShootAtPlayer()
    {
        if (projectilePrefab == null || shootPoint == null || player == null)
        {
            Debug.LogWarning($"{gameObject.name}: Missing projectile setup!");
            return;
        }
        
        // Vytvoření projektilu
        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
        
        // Nastavení směru a rychlosti
        Vector3 direction = (player.position - shootPoint.position).normalized;
        
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        if (projectileRb != null)
        {
            projectileRb.linearVelocity = direction * projectileSpeed;
        }
        
        // Nastavení damage projektilu
        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        if (projScript != null)
        {
            projScript.SetDamage(damage);
            projScript.SetOwner(this);
        }
        
        lastAttackTime = Time.time;
        
        // Animation trigger
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
        
        Debug.Log($"{gameObject.name}: Shot projectile at player");
    }
    
    private void MoveAwayFromPlayer()
    {
        if (player == null || navMeshAgent == null) return;
        
        Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
        Vector3 retreatPosition = transform.position + directionAwayFromPlayer * optimalDistance;
        
        // Najít nejbližší platný bod na NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatPosition, out hit, optimalDistance, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
        }
    }
    
    private void ConsiderRepositioning()
    {
        if (player == null || navMeshAgent == null) return;
        
        // Náhodně se rozhodneme, zda se přemístit
        if (Random.Range(0f, 1f) < 0.7f) // 70% šance na reposition
        {
            StartRepositioning();
        }
    }
    
    private void StartRepositioning()
    {
        Vector3 playerPos = player.position;
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(minDistance + 1f, maxDistance - 1f);
        
        Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
        Vector3 targetPosition = playerPos + direction * distance;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 5f, NavMesh.AllAreas))
        {
            repositionTarget = hit.position;
            navMeshAgent.SetDestination(repositionTarget);
            isRepositioning = true;
            lastRepositionTime = Time.time;
            
            // Skončíme repositioning když dorazíme
            Invoke(nameof(EndRepositioning), 2f);
        }
    }
    
    private void EndRepositioning()
    {
        isRepositioning = false;
    }
    
    protected override void EnterState(EnemyState state)
    {
        base.EnterState(state);
        
        switch (state)
        {
            case EnemyState.Chasing:
                if (navMeshAgent != null)
                {
                    navMeshAgent.speed = moveSpeed;
                }
                break;
                
            case EnemyState.Attacking:
                // Pomalejší pohyb během útočení
                if (navMeshAgent != null)
                {
                    navMeshAgent.speed = moveSpeed * 0.5f;
                }
                break;
        }
    }
    
    // Override attack method protože používáme projektily
    public override void Attack()
    {
        // Tato metoda je volána z base třídy, ale my používáme ShootAtPlayer
        // Můžeme zde přidat dodatečnou logiku pokud potřebujeme
    }
    
    protected override void OnDamageReceived()
    {
        base.OnDamageReceived();
        
        // Při poškození se pokusíme ustoupit
        if (shouldKitePlayer && currentState != EnemyState.Dead)
        {
            MoveAwayFromPlayer();
        }
    }
    
    // Debug Gizmos
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Optimal distance
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, optimalDistance);
        
        // Min distance
        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
        Gizmos.DrawWireSphere(transform.position, minDistance);
        
        // Max distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
        
        // Shoot point
        if (shootPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(shootPoint.position, 0.1f);
            Gizmos.DrawRay(shootPoint.position, shootPoint.forward * 2f);
        }
    }
}
