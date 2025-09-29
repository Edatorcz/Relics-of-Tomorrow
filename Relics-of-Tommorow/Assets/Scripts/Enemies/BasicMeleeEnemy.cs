using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Základní melee nepřítel pro roguelike hru
/// Hlídá určitou oblast, pronásleduje hráče a útočí v blízkém souboji
/// </summary>
public class BasicMeleeEnemy : EnemyBase
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 5f;
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private bool shouldPatrol = true;
    
    [Header("Combat Settings")]
    [SerializeField] private float lungeForce = 5f;
    [SerializeField] private bool canLunge = true;
    
    private Vector3 startPosition;
    private Vector3 patrolTarget;
    private float patrolTimer;
    private bool isWaitingAtPatrolPoint;
    private Rigidbody rb;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        // Uložení startovní pozice pro patrol
        startPosition = transform.position;
        
        // Získání Rigidbody pro lunge
        rb = GetComponent<Rigidbody>();
        
        // Nastavení prvního patrol cíle
        if (shouldPatrol)
        {
            SetNewPatrolTarget();
            ChangeState(EnemyState.Patrol);
        }
    }
    
    protected override void HandleIdleState()
    {
        // V idle stavu se nepohybujeme
        if (navMeshAgent != null && navMeshAgent.hasPath)
        {
            navMeshAgent.ResetPath();
        }
        
        // Pokud máme patrol, přejdeme do patrol stavu
        if (shouldPatrol)
        {
            ChangeState(EnemyState.Patrol);
        }
    }
    
    protected override void HandlePatrolState()
    {
        if (!shouldPatrol)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        // Pokud jsme u cíle
        if (navMeshAgent != null && !navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            if (!isWaitingAtPatrolPoint)
            {
                // Začít čekat
                isWaitingAtPatrolPoint = true;
                patrolTimer = Time.time + patrolWaitTime;
            }
            else if (Time.time >= patrolTimer)
            {
                // Čekání dokončeno, nový cíl
                SetNewPatrolTarget();
                isWaitingAtPatrolPoint = false;
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
        
        // Pokud jsme v dosahu útoku
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }
        
        // Pronásledovat hráče
        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(player.position);
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
        
        // Pokud je hráč příliš daleko, pokračovat v pronásledování
        if (distanceToPlayer > attackRange * 1.2f)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // Zastavit pohyb během útoku
        if (navMeshAgent != null)
        {
            navMeshAgent.ResetPath();
        }
        
        // Otočit se na hráče
        LookAtPlayer();
        
        // Útok
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
        }
    }
    
    protected override void HandleStunnedState()
    {
        // Během stunu se nepohybujeme
        if (navMeshAgent != null && navMeshAgent.hasPath)
        {
            navMeshAgent.ResetPath();
        }
        
        // Automaticky se vrátíme do chase/idle po určitém čase
        // Toto může být implementováno podle potřeby
    }
    
    private void SetNewPatrolTarget()
    {
        // Náhodný bod v okruhu od startovní pozice
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection.y = 0; // Držíme na zemi
        
        Vector3 targetPosition = startPosition + randomDirection;
        
        // Zkusíme najít platný bod na NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, patrolRadius, NavMesh.AllAreas))
        {
            patrolTarget = hit.position;
        }
        else
        {
            patrolTarget = startPosition; // Fallback na start pozici
        }
        
        // Nastavit cíl
        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(patrolTarget);
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
    
    private void PerformAttack()
    {
        Attack(); // Volání base metody pro damage
        
        // Lunge útok pokud je povolen
        if (canLunge && rb != null)
        {
            Vector3 lungeDirection = (player.position - transform.position).normalized;
            lungeDirection.y = 0;
            rb.AddForce(lungeDirection * lungeForce, ForceMode.Impulse);
        }
        
        Debug.Log($"{gameObject.name}: Performed melee attack!");
    }
    
    protected override void EnterState(EnemyState state)
    {
        base.EnterState(state);
        
        switch (state)
        {
            case EnemyState.Patrol:
                if (navMeshAgent != null)
                {
                    navMeshAgent.speed = moveSpeed * 0.5f; // Pomalejší patrol
                }
                break;
                
            case EnemyState.Chasing:
                if (navMeshAgent != null)
                {
                    navMeshAgent.speed = moveSpeed;
                }
                break;
                
            case EnemyState.Attacking:
                if (navMeshAgent != null)
                {
                    navMeshAgent.ResetPath();
                }
                break;
        }
    }
    
    // Override damage reakce pro agresivnější chování
    protected override void OnDamageReceived()
    {
        base.OnDamageReceived();
        
        // Při poškození se stanou agresivnějšími
        moveSpeed *= 1.1f;
        attackCooldown *= 0.9f;
        
        // Krátký stun efekt
        if (currentState != EnemyState.Dead)
        {
            ChangeState(EnemyState.Stunned);
            Invoke(nameof(RecoverFromStun), 0.5f);
        }
    }
    
    private void RecoverFromStun()
    {
        if (currentState == EnemyState.Stunned)
        {
            ChangeState(EnemyState.Chasing);
        }
    }
    
    // Debug Gizmos
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Patrol radius
        Gizmos.color = Color.green;
        Vector3 patrolCenter = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(patrolCenter, patrolRadius);
        
        // Patrol target
        if (Application.isPlaying && shouldPatrol)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(patrolTarget, 0.3f);
            Gizmos.DrawLine(transform.position, patrolTarget);
        }
    }
}
