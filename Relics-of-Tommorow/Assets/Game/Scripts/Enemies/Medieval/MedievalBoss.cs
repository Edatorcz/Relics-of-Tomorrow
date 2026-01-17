using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Medieval Boss - Epic Medieval King/Warlord
/// Má 3 fáze boje s různými útoky a mechanikami
/// </summary>
public class MedievalBoss : EnemyBase
{
    [Header("Boss Stats")]
    [SerializeField] private float phase1HealthThreshold = 0.66f; // 66% HP
    [SerializeField] private float phase2HealthThreshold = 0.33f; // 33% HP
    [SerializeField] private int currentPhase = 1;
    
    [Header("Phase 1 - Royal Guard")]
    [SerializeField] private GameObject guardPrefab;
    [SerializeField] private int guardsPerWave = 3;
    [SerializeField] private float guardSpawnCooldown = 12f;
    [SerializeField] private float swordSlashDamage = 45f;
    [SerializeField] private float royalStrikeDamage = 60f;
    
    [Header("Phase 2 - Dual Wielding Fury")]
    [SerializeField] private float dualWieldDamage = 35f;
    [SerializeField] private float flameEnchantmentDamage = 25f;
    [SerializeField] private float flameAuraDamage = 10f;
    [SerializeField] private float flameAuraRadius = 4f;
    [SerializeField] private bool hasFlameAura = false;
    [SerializeField] private float comboAttackDuration = 3f;
    
    [Header("Phase 3 - Berserker Rage")]
    [SerializeField] private float berserkerDamageMultiplier = 1.5f;
    [SerializeField] private float areaAttackRadius = 7f;
    [SerializeField] private float areaAttackDamage = 70f;
    [SerializeField] private float areaAttackCooldown = 8f;
    [SerializeField] private float executionerStrikeDamage = 100f;
    // Berserking state flag - můžete použít pro vizuální efekty
    // private bool isBerserking = false;
    
    [Header("Boss Abilities")]
    [SerializeField] private float healingAmount = 120f;
    [SerializeField] private float warCryRadius = 15f;
    // War cry stun duration - můžete implementovat stun efekt později
    // private float warCryStunDuration = 2f;
    
    [Header("Boss Loot")]
    [SerializeField] private BossLootDropper lootDropper;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem flameEffect;
    [SerializeField] private ParticleSystem phaseChangeEffect;
    [SerializeField] private ParticleSystem areaAttackEffect;
    [SerializeField] private Color phase1Color = new Color(0.7f, 0.7f, 0.8f); // Steel gray
    [SerializeField] private Color phase2Color = new Color(1f, 0.4f, 0f); // Orange flame
    [SerializeField] private Color phase3Color = new Color(0.8f, 0f, 0f); // Blood red
    
    [Header("Audio")]
    [SerializeField] private AudioClip warCrySound;
    [SerializeField] private AudioClip swordClashSound;
    [SerializeField] private AudioClip flameSound;
    [SerializeField] private AudioClip phaseChangeSound;
    [SerializeField] private AudioSource audioSource;
    
    // Private variables
    private bool isAttacking = false;
    private float lastGuardSpawnTime;
    private float lastAreaAttackTime;
    private Renderer bossRenderer;
    private bool isComboAttacking = false;
    private int comboCount = 0;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        // Boss statistiky
        maxHealth = 700f;
        currentHealth = maxHealth;
        damage = 40f;
        moveSpeed = 3.5f;
        detectionRange = 22f;
        attackRange = 3.5f;
        attackCooldown = 1.5f;
        
        // Boss nemá line of sight - vždy vidí hráče
        useLineOfSight = false;
        
        // Setup audio
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Setup renderer
        bossRenderer = GetComponent<Renderer>();
        if (bossRenderer != null)
        {
            bossRenderer.material.color = phase1Color;
        }
        
        ChangeState(EnemyState.Idle);
        Debug.Log("Medieval Boss initialized!");
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (isDead) return;
        
        // Kontrola změny fáze
        CheckPhaseChange();
        
        // Speciální ability podle fáze
        HandlePhaseAbilities();
        
        // Flame aura damage v Phase 2
        if (hasFlameAura && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= flameAuraRadius)
            {
                ApplyFlameAuraDamage();
            }
        }
    }
    
    protected override void HandleIdleState()
    {
        // Boss nikdy není idle - hned detekuje hráče
        if (navMeshAgent != null && navMeshAgent.hasPath)
            navMeshAgent.ResetPath();
    }
    
    protected override void HandlePatrolState()
    {
        // Boss nepatroluje
        if (navMeshAgent != null && navMeshAgent.hasPath)
            navMeshAgent.ResetPath();
    }
    
    protected override void HandleChaseState()
    {
        if (playerTransform == null || isDead) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Pokud je hráč v dosahu útoku, přepnout na attack
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }
        
        // Pronásledovat hráče
        MoveTowardsPlayer();
    }
    
    protected override void HandleAttackState()
    {
        if (playerTransform == null || isDead)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Pokud je hráč mimo dosah
        if (distanceToPlayer > attackRange * 1.3f)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // Otočit se k hráči
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Provést útok
        if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformBossAttack();
        }
    }
    
    private void CheckPhaseChange()
    {
        float healthPercent = currentHealth / maxHealth;
        
        // Přechod do Phase 2
        if (currentPhase == 1 && healthPercent <= phase1HealthThreshold)
        {
            EnterPhase2();
        }
        // Přechod do Phase 3
        else if (currentPhase == 2 && healthPercent <= phase2HealthThreshold)
        {
            EnterPhase3();
        }
    }
    
    private void EnterPhase2()
    {
        currentPhase = 2;
        
        Debug.Log("Medieval Boss entering Phase 2 - Dual Wielding!");
        
        // Zvýšení rychlosti
        moveSpeed = 5f;
        attackCooldown = 1f;
        
        // Aktivovat flame auru
        hasFlameAura = true;
        
        // Změna barvy
        if (bossRenderer != null)
        {
            bossRenderer.material.color = phase2Color;
        }
        
        // Vizuální efekt
        if (phaseChangeEffect != null)
        {
            Instantiate(phaseChangeEffect, transform.position, Quaternion.identity);
        }
        
        if (flameEffect != null)
        {
            flameEffect.Play();
        }
        
        // Zvuk
        PlaySound(phaseChangeSound);
        
        // Boss se zahojí
        Heal(healingAmount);
        
        // War cry
        StartCoroutine(WarCryAbility());
    }
    
    private void EnterPhase3()
    {
        currentPhase = 3;
        
        Debug.Log("Medieval Boss entering Phase 3 - BERSERKER RAGE!");
        
        // Maximální statistiky
        moveSpeed = 6.5f;
        attackCooldown = 0.8f;
        damage *= berserkerDamageMultiplier;
        // isBerserking = true; // Zakomentováno - můžete použít pro vizuální efekty
        
        // Změna barvy
        if (bossRenderer != null)
        {
            bossRenderer.material.color = phase3Color;
        }
        
        // Vizuální efekt
        if (phaseChangeEffect != null)
        {
            Instantiate(phaseChangeEffect, transform.position, Quaternion.identity);
        }
        
        // Zvuk
        PlaySound(phaseChangeSound);
        
        // Větší heal ve 3. fázi
        Heal(healingAmount * 1.5f);
        
        // Okamžitý area attack
        StartCoroutine(AreaAttack());
    }
    
    private void HandlePhaseAbilities()
    {
        if (currentState != EnemyState.Chasing && currentState != EnemyState.Attacking)
            return;
            
        if (playerTransform == null) return;
        
        // Phase 1 abilities - Summon guards
        if (currentPhase == 1)
        {
            if (Time.time - lastGuardSpawnTime >= guardSpawnCooldown)
            {
                SpawnGuards();
            }
        }
        
        // Phase 2 abilities - Combo attacks
        if (currentPhase == 2)
        {
            if (!isComboAttacking && Random.value < 0.2f * Time.deltaTime)
            {
                StartCoroutine(ComboAttack());
            }
        }
        
        // Phase 3 abilities - Area attacks
        if (currentPhase == 3)
        {
            if (Time.time - lastAreaAttackTime >= areaAttackCooldown)
            {
                StartCoroutine(AreaAttack());
            }
        }
    }
    
    private void PerformBossAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        float roll = Random.value;
        
        if (currentPhase == 1)
        {
            // Phase 1: Royal strike nebo sword slash
            if (roll < 0.4f)
            {
                RoyalStrike();
            }
            else
            {
                SwordSlash();
            }
        }
        else if (currentPhase == 2)
        {
            // Phase 2: Flame enchanted attacks
            if (roll < 0.5f)
            {
                FlameEnchantedAttack();
            }
            else
            {
                DualWieldAttack();
            }
        }
        else // Phase 3
        {
            // Phase 3: Berserker attacks
            if (roll < 0.3f)
            {
                ExecutionerStrike();
            }
            else
            {
                BerserkerSlash();
            }
        }
        
        StartCoroutine(ResetAttacking());
    }
    
    #region Attack Abilities
    
    private void SwordSlash()
    {
        Debug.Log("Medieval Boss performs Sword Slash!");
        PlaySound(swordClashSound);
        
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(swordSlashDamage);
                    ApplyKnockback(playerTransform);
                }
            }
        }
    }
    
    private void RoyalStrike()
    {
        Debug.Log("Medieval Boss performs Royal Strike!");
        PlaySound(swordClashSound);
        
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange * 1.2f)
            {
                PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(royalStrikeDamage);
                    ApplyKnockback(playerTransform, 1.5f);
                }
            }
        }
    }
    
    private void FlameEnchantedAttack()
    {
        Debug.Log("Medieval Boss performs Flame Enchanted Attack!");
        PlaySound(flameSound);
        
        if (flameEffect != null)
        {
            flameEffect.Play();
        }
        
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(dualWieldDamage + flameEnchantmentDamage);
                    ApplyKnockback(playerTransform);
                }
            }
        }
    }
    
    private void DualWieldAttack()
    {
        Debug.Log("Medieval Boss performs Dual Wield Attack!");
        PlaySound(swordClashSound);
        
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // Dva rychlé útoky
                    playerHealth.TakeDamage(dualWieldDamage);
                    StartCoroutine(DelayedDualWieldHit(playerHealth));
                }
            }
        }
    }
    
    private IEnumerator DelayedDualWieldHit(PlayerHealth playerHealth)
    {
        yield return new WaitForSeconds(0.2f);
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(dualWieldDamage);
        }
    }
    
    private void ExecutionerStrike()
    {
        Debug.Log("Medieval Boss performs EXECUTIONER STRIKE!");
        PlaySound(swordClashSound);
        
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange * 1.5f)
            {
                PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(executionerStrikeDamage);
                    ApplyKnockback(playerTransform, 2f);
                }
            }
        }
    }
    
    private void BerserkerSlash()
    {
        Debug.Log("Medieval Boss performs Berserker Slash!");
        PlaySound(swordClashSound);
        
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }
    }
    
    private IEnumerator ComboAttack()
    {
        isComboAttacking = true;
        float startTime = Time.time;
        comboCount = 0;
        
        Debug.Log("Medieval Boss starts COMBO ATTACK!");
        
        while (Time.time - startTime < comboAttackDuration && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(dualWieldDamage * 0.7f);
                    comboCount++;
                    PlaySound(swordClashSound);
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log($"Medieval Boss combo finished! {comboCount} hits!");
        isComboAttacking = false;
    }
    
    private IEnumerator AreaAttack()
    {
        Debug.Log("Medieval Boss performs AREA ATTACK!");
        lastAreaAttackTime = Time.time;
        isAttacking = true;
        
        // Stop movement
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.ResetPath();
        }
        
        // Visual effect
        if (areaAttackEffect != null)
        {
            Instantiate(areaAttackEffect, transform.position, Quaternion.identity);
        }
        
        PlaySound(warCrySound);
        
        yield return new WaitForSeconds(0.5f);
        
        // Damage all in radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, areaAttackRadius);
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Player"))
            {
                PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(areaAttackDamage);
                    ApplyKnockback(col.transform, 3f);
                }
            }
        }
        
        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }
    
    private IEnumerator WarCryAbility()
    {
        Debug.Log("Medieval Boss unleashes WAR CRY!");
        PlaySound(warCrySound);
        
        yield return new WaitForSeconds(0.3f);
        
        // Stun effect in radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, warCryRadius);
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Player"))
            {
                // Could implement stun effect on player
                Debug.Log("Player affected by War Cry!");
            }
        }
    }
    
    private void SpawnGuards()
    {
        if (guardPrefab == null)
        {
            Debug.LogWarning("Guard prefab not set!");
            return;
        }
        
        lastGuardSpawnTime = Time.time;
        
        Debug.Log($"Medieval Boss summons {guardsPerWave} Royal Guards!");
        
        for (int i = 0; i < guardsPerWave; i++)
        {
            float angle = i * (360f / guardsPerWave);
            Vector3 spawnOffset = Quaternion.Euler(0, angle, 0) * Vector3.forward * 5f;
            Vector3 spawnPosition = transform.position + spawnOffset;
            
            GameObject guard = Instantiate(guardPrefab, spawnPosition, Quaternion.identity);
            
            // Nastavit target na hráče
            // Guard automaticky detekuje hráče přes EnemyBase.player
        }
    }
    
    private void ApplyFlameAuraDamage()
    {
        // Damage over time
        if (playerTransform != null && Time.frameCount % 30 == 0) // Every 30 frames (~0.5s)
        {
            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(flameAuraDamage);
            }
        }
    }
    
    #endregion
    
    private IEnumerator ResetAttacking()
    {
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }
    
    private void ApplyKnockback(Transform target, float multiplier = 1f)
    {
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            Vector3 knockbackDirection = (target.position - transform.position).normalized;
            targetRb.AddForce(knockbackDirection * attackKnockbackForce * multiplier, ForceMode.Impulse);
        }
    }
    
    private void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Medieval Boss healed for {amount}! Current HP: {currentHealth}/{maxHealth}");
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    public override void TakeDamage(float damage)
    {
        // Reduced damage in shield wall (Phase 1 ability)
        if (isInShieldWall)
        {
            damage *= 0.5f;
            Debug.Log("Medieval Boss blocking with shield!");
        }
        
        base.TakeDamage(damage);
    }
    
    protected override void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log("Medieval Boss defeated!");
        
        // Drop loot
        if (lootDropper != null)
        {
            lootDropper.DropBossLoot(transform.position);
        }
        
        // Stop all coroutines
        StopAllCoroutines();
        
        // Disable components
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }
        
        // Death animation/effects could go here
        
        // Destroy after delay
        Destroy(gameObject, 3f);
    }
    
    protected override void HandleStunnedState()
    {
        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.hasPath)
        {
            navMeshAgent.ResetPath();
        }
        
        // Deaktivovat flame auru při stunu
        if (hasFlameAura)
        {
            hasFlameAura = false;
        }
    }
    
    private bool isInShieldWall = false;
}
