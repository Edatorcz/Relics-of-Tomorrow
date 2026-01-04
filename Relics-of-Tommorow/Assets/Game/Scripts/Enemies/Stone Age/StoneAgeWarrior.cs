using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Kamenný válečník - agresivní melee bojovník pro kamennou dobu
/// Rychlý, agresivní, ale křehký
/// </summary>
public class StoneAgeWarrior : EnemyBase
{
    [Header("Warrior Behavior")]
    [SerializeField] private float chargeSpeed = 6f;
    [SerializeField] private float chargeRange = 8f;
    [SerializeField] private float comboAttackChance = 0.4f;
    [SerializeField] private bool isCharging = false;
    [SerializeField] private float retreatHealthThreshold = 0.3f;
    
    [Header("Combat Abilities")]
    [SerializeField] private float slashDamage = 20f;
    [SerializeField] private float comboDamage = 12f;
    [SerializeField] private float stunDuration = 0.5f;
    [SerializeField] private float berserkerModeThreshold = 0.5f;
    [SerializeField] private bool inBerserkerMode = false;
    
    [Header("Warrior Sounds")]
    [SerializeField] private AudioClip warCrySound;
    [SerializeField] private AudioClip chargeSound;
    [SerializeField] private AudioClip slashSound;
    [SerializeField] private AudioSource audioSource;
    
    private bool hasSeenPlayer = false;
    private float chargeTimer = 0f;
    private float comboTimer = 0f;
    private int comboCount = 0;
    private Vector3 chargeTarget;
    private float originalSpeed;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        // Válečník má střední stats ale je rychlý
        maxHealth = 80f;
        currentHealth = maxHealth;
        damage = slashDamage;
        moveSpeed = 4f;
        detectionRange = 12f;
        attackRange = 2.2f;
        attackCooldown = 1.2f;
        
        originalSpeed = moveSpeed;
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Vypnout line of sight pro agresivního válečníka
        useLineOfSight = false;
        
        ChangeState(EnemyState.Idle);
    }
    
    protected override void HandleIdleState()
    {
        // Válečník patroluje aktivněji
        if (Random.Range(0f, 1f) < 0.02f) // 2% šance
        {
            ChangeState(EnemyState.Patrol);
        }
        
        if (navMeshAgent != null && navMeshAgent.hasPath) 
            navMeshAgent.ResetPath();
            
        // Válečník je neklidný
        if (Random.Range(0f, 1f) < 0.005f)
        {
            transform.Rotate(0, Random.Range(-90f, 90f), 0);
        }
    }
    
    protected override void HandlePatrolState()
    {
        if (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 1f)
        {
            PatrolArea();
        }
    }
    
    protected override void HandleChaseState()
    {
        if (player == null) 
        { 
            ChangeState(EnemyState.Idle); 
            return; 
        }
        
        if (!hasSeenPlayer)
        {
            hasSeenPlayer = true;
            PlayWarCry();
            EnterBerserkerMode();
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Pokud je v dosahu charge a není už charging
        if (distanceToPlayer <= chargeRange && distanceToPlayer > attackRange && !isCharging)
        {
            StartCharge();
            return;
        }
        
        // Pokud je v dosahu útoku
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }
        
        // Běžné pronásledování
        if (navMeshAgent != null && !isCharging)
        {
            navMeshAgent.SetDestination(player.position);
        }
        
        // Handle charging
        if (isCharging)
        {
            HandleCharging();
        }
    }
    
    protected override void HandleAttackState()
    {
        if (player == null) 
        { 
            ChangeState(EnemyState.Chasing); 
            return; 
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Pokud je hráč příliš daleko
        if (distanceToPlayer > attackRange * 1.5f)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // Zastavit pohyb
        if (navMeshAgent != null)
            navMeshAgent.ResetPath();
            
        // Otočit se na hráče
        LookAtPlayer();
        
        // Útok
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformWarriorAttack();
        }
        
        // Kontrola combo útoků
        if (comboCount > 0 && Time.time - comboTimer < 2f)
        {
            if (Time.time - lastAttackTime >= attackCooldown * 0.6f) // Rychlejší combo
            {
                PerformComboAttack();
            }
        }
    }
    
    protected override void HandleStunnedState()
    {
        if (navMeshAgent != null && navMeshAgent.hasPath)
            navMeshAgent.ResetPath();
            
        // Kratší stun pro válečníka
        if (Time.time - lastAttackTime >= stunDuration)
        {
            ChangeState(EnemyState.Chasing);
        }
    }
    
    private void PatrolArea()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 8f;
        randomDirection.y = 0;
        Vector3 targetPosition = transform.position + randomDirection;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 8f, NavMesh.AllAreas))
        {
            navMeshAgent?.SetDestination(hit.position);
        }
    }
    
    private void StartCharge()
    {
        isCharging = true;
        chargeTarget = player.position;
        chargeTimer = 2f; // Max charge time
        
        // Boost rychlosti
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = chargeSpeed;
            navMeshAgent.SetDestination(chargeTarget);
        }
        
        PlayChargeSound();
    }
    
    private void HandleCharging()
    {
        chargeTimer -= Time.deltaTime;
        
        // Pokud dosáhl cíle nebo čas vypršel
        if (chargeTimer <= 0f || 
            Vector3.Distance(transform.position, chargeTarget) < 1.5f)
        {
            EndCharge();
        }
        
        // Pokud je blízko hráče během charge
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            EndCharge();
            PerformChargeAttack();
        }
    }
    
    private void EndCharge()
    {
        isCharging = false;
        
        // Restore původní rychlost
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed;
        }
    }
    
    private void PerformWarriorAttack()
    {
        Attack(); // Base attack
        damage = slashDamage;
        
        PlaySlashSound();
        
        // Šance na combo
        if (Random.Range(0f, 1f) < comboAttackChance)
        {
            comboCount = Random.Range(1, 3); // 1-2 combo útoky
            comboTimer = Time.time;
        }
    }
    
    private void PerformComboAttack()
    {
        Attack(); // Base attack
        damage = comboDamage;
        
        PlaySlashSound();
        comboCount--;
        lastAttackTime = Time.time;
    }
    
    private void PerformChargeAttack()
    {
        damage = slashDamage * 1.5f; // Silnější charge útok
        Attack();
        
        PlaySlashSound();
        
        // Stun po charge útoku
        lastAttackTime = Time.time;
        ChangeState(EnemyState.Stunned);
    }
    
    private void EnterBerserkerMode()
    {
        if (inBerserkerMode) return;
        
        float healthPercent = currentHealth / maxHealth;
        if (healthPercent <= berserkerModeThreshold)
        {
            inBerserkerMode = true;
            
            // Boost stats
            moveSpeed *= 1.4f;
            attackCooldown *= 0.7f;
            damage *= 1.2f;
            
            // Visual effect
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }
            
            PlayWarCry();
        }
    }
    
    private void LookAtPlayer()
    {
        if (player == null) return;
        
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 
                rotationSpeed * 2f * Time.deltaTime); // Rychlejší otáčení
        }
    }
    
    private void PlayWarCry()
    {
        if (audioSource != null && warCrySound != null)
        {
            audioSource.PlayOneShot(warCrySound);
        }
    }
    
    private void PlayChargeSound()
    {
        if (audioSource != null && chargeSound != null)
        {
            audioSource.PlayOneShot(chargeSound);
        }
    }
    
    private void PlaySlashSound()
    {
        if (audioSource != null && slashSound != null)
        {
            audioSource.PlayOneShot(slashSound);
        }
    }
    
    protected override void OnDamageReceived()
    {
        base.OnDamageReceived();
        
        // Kontrola berserker módu
        EnterBerserkerMode();
        
        // Kontrola retreat
        float healthPercent = currentHealth / maxHealth;
        if (healthPercent <= retreatHealthThreshold && Random.Range(0f, 1f) < 0.3f)
        {
            // Krátký retreat
            ChangeState(EnemyState.Stunned);
        }
    }
    
    protected override void UpdateAnimations()
    {
        base.UpdateAnimations();
        
        if (animator != null)
        {
            animator.SetBool("IsCharging", isCharging);
            animator.SetBool("InBerserkerMode", inBerserkerMode);
            animator.SetInteger("ComboCount", comboCount);
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Charge range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chargeRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Charge target
        if (isCharging)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, chargeTarget);
            Gizmos.DrawWireSphere(chargeTarget, 1f);
        }
    }
}