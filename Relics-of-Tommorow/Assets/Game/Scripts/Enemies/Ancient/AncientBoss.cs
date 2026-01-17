using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Ancient Boss - Centurion Commander
/// Disciplinovaný římský velitel s taktickými schopnostmi
/// </summary>
public class AncientBoss : EnemyBase
{
    [Header("Boss Stats")]
    [SerializeField] private float phase1HealthThreshold = 0.66f;
    [SerializeField] private float phase2HealthThreshold = 0.33f;
    [SerializeField] private int currentPhase = 1;
    
    [Header("Phase 1 - Disciplined Commander")]
    [SerializeField] private float shieldWallDuration = 4f;
    [SerializeField] private float shieldWallDefense = 0.8f; // 80% damage reduction
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int minionsPerWave = 2;
    
    [Header("Phase 2 - Tactical Assault")]
    [SerializeField] private GameObject spearPrefab;
    [SerializeField] private float spearThrowDamage = 40f;
    [SerializeField] private int spearsPerVolley = 5;
    [SerializeField] private float chargeSpeed = 8f;
    [SerializeField] private float chargeDamage = 50f;
    
    [Header("Phase 3 - Roman Fury")]
    [SerializeField] private float whirlwindDuration = 5f;
    [SerializeField] private float whirlwindDamage = 30f;
    [SerializeField] private float whirlwindRadius = 5f;
    // Heal per kill - můžete použít v Phase 3 později
    // private float healPerKill = 50f;
    
    [Header("Boss Abilities")]
    [SerializeField] private float commandShoutRadius = 15f;
    // Command shout buff - můžete použít pro buff na miniony později
    // private float commandShoutBuff = 1.5f;
    
    [Header("Boss Loot")]
    [SerializeField] private BossLootDropper lootDropper;
    
    private bool isInShieldWall = false;
    private bool isCharging = false;
    private bool isWhirlwinding = false;
    private bool isAttacking = false;
    private float lastMinionSpawnTime;
    private float lastSpearThrowTime;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        maxHealth = 600f;
        currentHealth = maxHealth;
        damage = 40f;
        moveSpeed = 3.5f;
        detectionRange = 25f;
        attackRange = 3f;
        attackCooldown = 1.8f;
        
        useLineOfSight = false;
        
        ChangeState(EnemyState.Idle);
    }
    
    protected override void Update()
    {
        base.Update();
        
        CheckPhaseTransition();
        
        if (isWhirlwinding)
        {
            WhirlwindDamageCheck();
        }
    }
    
    private void CheckPhaseTransition()
    {
        float healthPercent = currentHealth / maxHealth;
        
        if (currentPhase == 1 && healthPercent <= phase1HealthThreshold)
        {
            currentPhase = 2;
            StartCoroutine(TransitionToPhase2());
        }
        else if (currentPhase == 2 && healthPercent <= phase2HealthThreshold)
        {
            currentPhase = 3;
            StartCoroutine(TransitionToPhase3());
        }
    }
    
    private IEnumerator TransitionToPhase2()
    {
        Debug.Log("Centurion entering Phase 2!");
        
        // Heal a bit
        currentHealth = Mathf.Min(currentHealth + 100f, maxHealth);
        
        // Spawn minions
        SpawnMinions();
        
        yield return new WaitForSeconds(1f);
        
        // Boost stats
        moveSpeed *= 1.2f;
        damage *= 1.3f;
    }
    
    private IEnumerator TransitionToPhase3()
    {
        Debug.Log("Centurion entering Phase 3 - FURY MODE!");
        
        // Heal more
        currentHealth = Mathf.Min(currentHealth + 150f, maxHealth);
        
        // Command shout
        CommandShout();
        
        yield return new WaitForSeconds(1f);
        
        // Final boost
        moveSpeed *= 1.3f;
        damage *= 1.5f;
        attackCooldown *= 0.7f;
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
            PerformBossAttack();
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange && !isCharging && !isWhirlwinding)
        {
            ChangeState(EnemyState.Chasing);
        }
    }
    
    private void PerformBossAttack()
    {
        float roll = Random.Range(0f, 1f);
        
        if (currentPhase == 1)
        {
            if (roll < 0.3f && !isInShieldWall)
            {
                StartCoroutine(ShieldWallAbility());
            }
            else if (roll < 0.6f)
            {
                PerformSlashAttack();
            }
            else
            {
                SpawnMinions();
            }
        }
        else if (currentPhase == 2)
        {
            if (roll < 0.3f)
            {
                StartCoroutine(SpearVolley());
            }
            else if (roll < 0.6f)
            {
                StartCoroutine(ChargeAttack());
            }
            else
            {
                PerformPowerAttack();
            }
        }
        else // Phase 3
        {
            if (roll < 0.4f && !isWhirlwinding)
            {
                StartCoroutine(WhirlwindAttack());
            }
            else if (roll < 0.7f)
            {
                StartCoroutine(SpearVolley());
            }
            else
            {
                CommandShout();
            }
        }
    }
    
    private IEnumerator ShieldWallAbility()
    {
        isInShieldWall = true;
        isAttacking = true;
        lastAttackTime = Time.time;
        
        Debug.Log("Centurion raises shield wall!");
        
        // Stop movement
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.ResetPath();
        }
        
        yield return new WaitForSeconds(shieldWallDuration);
        
        isInShieldWall = false;
        isAttacking = false;
    }
    
    private IEnumerator SpearVolley()
    {
        if (spearPrefab == null || player == null) yield break;
        
        isAttacking = true;
        lastAttackTime = Time.time;
        lastSpearThrowTime = Time.time;
        
        Debug.Log("Centurion throwing spear volley!");
        
        for (int i = 0; i < spearsPerVolley; i++)
        {
            ThrowSpear();
            yield return new WaitForSeconds(0.3f);
        }
        
        isAttacking = false;
    }
    
    private void ThrowSpear()
    {
        if (spearPrefab == null || player == null) return;
        
        Vector3 spawnPosition = transform.position + Vector3.up * 2f;
        GameObject spear = Instantiate(spearPrefab, spawnPosition, transform.rotation);
        
        EnemyProjectile projectile = spear.GetComponent<EnemyProjectile>();
        if (projectile == null)
        {
            projectile = spear.AddComponent<EnemyProjectile>();
        }
        
        projectile.SetOwner(this);
        projectile.SetDamage(spearThrowDamage);
        
        Vector3 direction = (player.position + Vector3.up - spawnPosition).normalized;
        
        Rigidbody rb = spear.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = spear.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = true;
        rb.linearVelocity = direction * 20f;
        spear.transform.rotation = Quaternion.LookRotation(direction);
    }
    
    private IEnumerator ChargeAttack()
    {
        if (player == null) yield break;
        
        isCharging = true;
        isAttacking = true;
        lastAttackTime = Time.time;
        
        Debug.Log("Centurion charging!");
        
        Vector3 chargeDirection = (player.position - transform.position).normalized;
        float chargeTime = 0f;
        float maxChargeTime = 2f;
        
        while (chargeTime < maxChargeTime && isCharging)
        {
            transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
            
            // Check collision with player
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer <= 2f)
                {
                    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(chargeDamage);
                        Debug.Log($"Centurion charge hit! Damage: {chargeDamage}");
                    }
                    break;
                }
            }
            
            chargeTime += Time.deltaTime;
            yield return null;
        }
        
        isCharging = false;
        isAttacking = false;
    }
    
    private IEnumerator WhirlwindAttack()
    {
        isWhirlwinding = true;
        isAttacking = true;
        lastAttackTime = Time.time;
        
        Debug.Log("Centurion whirlwind attack!");
        
        float elapsed = 0f;
        while (elapsed < whirlwindDuration)
        {
            transform.Rotate(0, 720f * Time.deltaTime, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        isWhirlwinding = false;
        isAttacking = false;
    }
    
    private void WhirlwindDamageCheck()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= whirlwindRadius)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null && Time.frameCount % 30 == 0) // Damage každou půl sekundu
            {
                playerHealth.TakeDamage(whirlwindDamage * Time.deltaTime);
            }
        }
    }
    
    private void SpawnMinions()
    {
        if (minionPrefab == null) return;
        
        lastMinionSpawnTime = Time.time;
        
        for (int i = 0; i < minionsPerWave; i++)
        {
            Vector3 spawnOffset = Random.insideUnitSphere * 3f;
            spawnOffset.y = 0;
            Vector3 spawnPosition = transform.position + spawnOffset;
            
            Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
        }
        
        Debug.Log($"Centurion spawned {minionsPerWave} minions!");
    }
    
    private void CommandShout()
    {
        Debug.Log("Centurion commands his troops!");
        
        // Buff nearby allies
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, commandShoutRadius);
        foreach (Collider col in nearbyEnemies)
        {
            EnemyBase enemy = col.GetComponent<EnemyBase>();
            if (enemy != null && enemy != this)
            {
                // TODO: Apply buff
            }
        }
    }
    
    private void PerformSlashAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= attackRange)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }
        
        Invoke(nameof(ResetAttack), 0.5f);
    }
    
    private void PerformPowerAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= attackRange)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage * 1.5f);
                }
            }
        }
        
        Invoke(nameof(ResetAttack), 0.7f);
    }
    
    public override void TakeDamage(float damageAmount)
    {
        // Reduced damage during shield wall
        if (isInShieldWall)
        {
            damageAmount *= (1f - shieldWallDefense);
            Debug.Log($"Shield wall reduced damage to {damageAmount}");
        }
        
        base.TakeDamage(damageAmount);
    }
    
    protected override void Die()
    {
        Debug.Log("Centurion defeated!");
        
        if (lootDropper != null)
        {
            lootDropper.DropBossLoot(transform.position);
        }
        
        base.Die();
    }
    
    private void ResetAttack()
    {
        isAttacking = false;
    }
    
    protected override void HandleIdleState()
    {
        // Boss nikdy neidluje
        if (player != null)
        {
            ChangeState(EnemyState.Chasing);
        }
    }
    
    protected override void HandlePatrolState()
    {
        // Boss nepatroluje
        if (player != null)
        {
            ChangeState(EnemyState.Chasing);
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
        
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
        }
        else
        {
            if (navMeshAgent != null && navMeshAgent.enabled)
            {
                navMeshAgent.SetDestination(player.position);
            }
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
