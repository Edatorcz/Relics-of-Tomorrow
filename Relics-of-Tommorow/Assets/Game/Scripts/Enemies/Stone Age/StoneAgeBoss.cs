using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Stone Age Boss - epický finální boss kamenné doby
/// Má 3 fáze boje s různými útoky a mechanikami
/// </summary>
public class StoneAgeBoss : EnemyBase
{
    [Header("Boss Stats")]
    [SerializeField] private float phase1HealthThreshold = 0.66f; // 66% HP
    [SerializeField] private float phase2HealthThreshold = 0.33f; // 33% HP
    [SerializeField] private int currentPhase = 1;
    
    [Header("Phase 1 - Warrior")]
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeDamage = 40f;
    [SerializeField] private float chargeCooldown = 8f;
    [SerializeField] private float chargeDistance = 15f;
    [SerializeField] private bool isCharging = false;
    
    [Header("Phase 2 - Berserker")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private float rockThrowCooldown = 3f;
    [SerializeField] private int rocksPerVolley = 5;
    [SerializeField] private float rockDamage = 25f;
    [SerializeField] private float spinAttackDuration = 3f;
    [SerializeField] private float spinDamage = 30f;
    [SerializeField] private bool isSpinning = false;
    
    [Header("Phase 3 - Ancient Fury")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int minionsToSpawn = 3;
    [SerializeField] private float minionSpawnCooldown = 15f;
    [SerializeField] private float groundPoundRadius = 8f;
    [SerializeField] private float groundPoundDamage = 50f;
    [SerializeField] private float groundPoundCooldown = 10f;
    
    [Header("Boss Abilities")]
    [SerializeField] private float roarRadius = 12f;
    [SerializeField] private float healingAmount = 100f;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem chargeEffect;
    [SerializeField] private ParticleSystem phaseChangeEffect;
    [SerializeField] private ParticleSystem groundPoundEffect;
    [SerializeField] private Color phase1Color = Color.gray;
    [SerializeField] private Color phase2Color = new Color(1f, 0.5f, 0f); // Orange
    [SerializeField] private Color phase3Color = Color.red;
    
    [Header("Audio")]
    [SerializeField] private AudioClip bossRoarSound;
    [SerializeField] private AudioClip chargeSound;
    [SerializeField] private AudioClip groundPoundSound;
    [SerializeField] private AudioClip phaseChangeSound;
    [SerializeField] private AudioClip rockThrowSound;
    [SerializeField] private AudioSource audioSource;
    
    // Private variables
    private float lastChargeTime;
    private float lastRockThrowTime;
    private float lastGroundPoundTime;
    private float lastMinionSpawnTime;
    private Vector3 chargeStartPosition;
    private Vector3 chargeTargetPosition;
    private float chargeTimer;
    private Renderer bossRenderer;
    private Coroutine currentAbilityCoroutine;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        // Boss statistiky
        maxHealth = 500f;
        currentHealth = maxHealth;
        damage = 35f;
        moveSpeed = 3f;
        detectionRange = 20f;
        attackRange = 4f;
        attackCooldown = 2f;
        
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
        
        Debug.Log("Stone Age Boss spawned with " + maxHealth + " HP");
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (isDead) return;
        
        // Kontrola změny fáze
        CheckPhaseChange();
        
        // Speciální ability podle fáze
        HandlePhaseAbilities();
    }
    
    protected override void HandleIdleState()
    {
        // Boss nikdy není idle - hned detekuje hráče
        if (navMeshAgent != null && navMeshAgent.hasPath)
            navMeshAgent.ResetPath();
    }
    
    protected override void HandlePatrolState()
    {
        // Boss nepatroluje - zůstává na místě
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
        
        // Pronásledovat hráče (funguje i bez NavMesh)
        if (!isCharging)
        {
            MoveTowardsPlayer();
        }
    }
    
    protected override void HandleStunnedState()
    {
        // Boss nemůže být stunnnutý (nebo můžeš přidat speciální mechaniku)
        if (navMeshAgent != null && navMeshAgent.hasPath)
            navMeshAgent.ResetPath();
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
        Debug.Log("Boss entered Phase 2 - Berserker Mode!");
        
        // Zvýšení rychlosti
        moveSpeed = 4.5f;
        attackCooldown = 1.5f;
        
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
        
        // Zvuk
        PlaySound(phaseChangeSound);
        
        // Boss se zahojí trochu
        Heal(healingAmount);
        
        // Okamžitý roar attack
        StartCoroutine(RoarAttack());
    }
    
    private void EnterPhase3()
    {
        currentPhase = 3;
        Debug.Log("Boss entered Phase 3 - Ancient Fury!");
        
        // Maximální statistiky
        moveSpeed = 6f;
        attackCooldown = 1f;
        damage = 50f;
        
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
        
        // Ground pound + spawn minions
        StartCoroutine(GroundPoundAttack());
        SpawnMinions();
    }
    
    private void HandlePhaseAbilities()
    {
        if (currentState != EnemyState.Chasing && currentState != EnemyState.Attacking)
            return;
            
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Phase 1 abilities - Charge attack
        if (currentPhase == 1)
        {
            if (Time.time - lastChargeTime >= chargeCooldown && distanceToPlayer > attackRange * 2 && !isCharging)
            {
                StartCoroutine(ChargeAttack());
            }
        }
        
        // Phase 2 abilities - Rock throw + spin
        if (currentPhase == 2)
        {
            if (Time.time - lastRockThrowTime >= rockThrowCooldown && distanceToPlayer > attackRange)
            {
                StartCoroutine(ThrowRockVolley());
            }
            
            if (distanceToPlayer <= attackRange * 1.5f && !isSpinning)
            {
                StartCoroutine(SpinAttack());
            }
        }
        
        // Phase 3 abilities - Ground pound + minions
        if (currentPhase == 3)
        {
            if (Time.time - lastGroundPoundTime >= groundPoundCooldown)
            {
                StartCoroutine(GroundPoundAttack());
            }
            
            if (Time.time - lastMinionSpawnTime >= minionSpawnCooldown)
            {
                SpawnMinions();
            }
        }
    }
    
    #region Attack Abilities
    
    private IEnumerator ChargeAttack()
    {
        isCharging = true;
        lastChargeTime = Time.time;
        
        Debug.Log("Boss charging!");
        PlaySound(chargeSound);
        
        // Aktivovat vizuální efekt
        if (chargeEffect != null)
        {
            chargeEffect.Play();
        }
        
        // Uložit pozice
        chargeStartPosition = transform.position;
        chargeTargetPosition = playerTransform.position;
        
        // Otočit se na hráče
        Vector3 direction = (chargeTargetPosition - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        
        // Nabít útok (0.5s delay)
        yield return new WaitForSeconds(0.5f);
        
        // Charge!
        float chargeDuration = chargeDistance / chargeSpeed;
        chargeTimer = 0f;
        
        while (chargeTimer < chargeDuration)
        {
            chargeTimer += Time.deltaTime;
            
            // Pohyb dopředu
            transform.position += transform.forward * chargeSpeed * Time.deltaTime;
            
            // Kontrola kolize s hráčem
            Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                    playerHealth.TakeDamage(chargeDamage);
                        Rigidbody playerRb = hit.GetComponent<Rigidbody>();
                        if (playerRb != null)
                        {
                            Vector3 knockbackDir = (hit.transform.position - transform.position).normalized;
                            playerRb.AddForce(knockbackDir * 15f, ForceMode.Impulse);
                        }
                    }
                    break; // Jen jeden hit
                }
            }
            
            yield return null;
        }
        
        if (chargeEffect != null)
        {
            chargeEffect.Stop();
        }
        
        isCharging = false;
    }
    
    private IEnumerator ThrowRockVolley()
    {
        lastRockThrowTime = Time.time;
        
        Debug.Log("Boss throwing rocks!");
        
        for (int i = 0; i < rocksPerVolley; i++)
        {
            ThrowRock();
            PlaySound(rockThrowSound);
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    private void ThrowRock()
    {
        if (rockPrefab == null || playerTransform == null) return;
        
        // Spawn pozice (nad bossem)
        Vector3 spawnPos = transform.position + Vector3.up * 2f + transform.forward * 1f;
        
        // Vytvoř projektil
        GameObject rock = Instantiate(rockPrefab, spawnPos, Quaternion.identity);
        
        // Směr k hráči s predikci
        Vector3 targetPos = playerTransform.position + Vector3.up;
        Vector3 direction = (targetPos - spawnPos).normalized;
        
        // Přidat Rigidbody a sílu
        Rigidbody rb = rock.GetComponent<Rigidbody>();
        if (rb == null) rb = rock.AddComponent<Rigidbody>();
        
        rb.linearVelocity = direction * 15f;
        rb.useGravity = true;
        
        // Přidat damage component
        RockProjectile projectile = rock.GetComponent<RockProjectile>();
        if (projectile == null)
        {
            projectile = rock.AddComponent<RockProjectile>();
        }
        projectile.SetDamage(rockDamage);
        projectile.SetOwner(this);
        
        // Auto destroy
        Destroy(rock, 5f);
    }
    
    private IEnumerator SpinAttack()
    {
        isSpinning = true;
        
        Debug.Log("Boss spinning!");
        
        float spinTimer = 0f;
        float spinSpeed = 720f; // 2 otáčky za sekundu
        
        while (spinTimer < spinAttackDuration)
        {
            spinTimer += Time.deltaTime;
            
            // Rotace
            transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
            
            // Damage kolem dokola
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange * 1.5f);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(spinDamage * Time.deltaTime);
                    }
                }
            }
            
            yield return null;
        }
        
        isSpinning = false;
    }
    
    private IEnumerator GroundPoundAttack()
    {
        lastGroundPoundTime = Time.time;
        
        Debug.Log("Boss ground pound!");
        PlaySound(groundPoundSound);
        
        // Skok nahoru
        Vector3 startPos = transform.position;
        Vector3 jumpPos = startPos + Vector3.up * 5f;
        
        float jumpTime = 0f;
        float jumpDuration = 0.8f;
        
        while (jumpTime < jumpDuration)
        {
            jumpTime += Time.deltaTime;
            float t = jumpTime / jumpDuration;
            transform.position = Vector3.Lerp(startPos, jumpPos, t);
            yield return null;
        }
        
        // Pád dolů
        yield return new WaitForSeconds(0.2f);
        
        float fallTime = 0f;
        float fallDuration = 0.3f;
        
        while (fallTime < fallDuration)
        {
            fallTime += Time.deltaTime;
            float t = fallTime / fallDuration;
            transform.position = Vector3.Lerp(jumpPos, startPos, t);
            yield return null;
        }
        
        // Dopad - AOE damage
        if (groundPoundEffect != null)
        {
            Instantiate(groundPoundEffect, transform.position, Quaternion.identity);
        }
        
        Collider[] hits = Physics.OverlapSphere(transform.position, groundPoundRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    float damageFalloff = 1f - (distance / groundPoundRadius);
                    float finalDamage = groundPoundDamage * damageFalloff;
                    
                    playerHealth.TakeDamage(finalDamage);
                    
                    // Knockback
                    Rigidbody playerRb = hit.GetComponent<Rigidbody>();
                    if (playerRb != null)
                    {
                        Vector3 knockbackDir = (hit.transform.position - transform.position).normalized;
                        playerRb.AddForce((knockbackDir + Vector3.up) * 10f, ForceMode.Impulse);
                    }
                }
            }
        }
    }
    
    private IEnumerator RoarAttack()
    {
        Debug.Log("Boss roars!");
        PlaySound(bossRoarSound);
        
        // Stun všechny v dosahu
        Collider[] hits = Physics.OverlapSphere(transform.position, roarRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // Zde můžeš přidat stun mechaniku na hráče
                Debug.Log("Player stunned by roar!");
            }
        }
        
        yield return new WaitForSeconds(1f);
    }
    
    private void SpawnMinions()
    {
        if (minionPrefab == null)
        {
            Debug.LogWarning("Minion prefab not assigned!");
            return;
        }
        
        lastMinionSpawnTime = Time.time;
        Debug.Log("Boss spawning minions!");
        
        for (int i = 0; i < minionsToSpawn; i++)
        {
            // Náhodná pozice kolem bosse
            float angle = i * (360f / minionsToSpawn);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * 5f;
            Vector3 spawnPos = transform.position + offset;
            
            GameObject minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
            Debug.Log($"Minion {i+1} spawned at {spawnPos}");
        }
    }
    
    #endregion
    
    protected override void HandleAttackState()
    {
        if (playerTransform == null || isDead) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Otáčet se na hráče
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        
        // Pokud je hráč moc daleko, začít ho honit
        if (distanceToPlayer > attackRange * 1.2f)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // Základní útok
        if (Time.time - lastAttackTime >= attackCooldown && !isCharging && !isSpinning)
        {
            Attack();
        }
    }
    
    public override void Attack()
    {
        lastAttackTime = Time.time;
        
        // Animace útoku (pokud je nastavena)
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Damage na hráče
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer <= attackRange)
            {
                PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"Boss hit player for {damage} damage!");
                    
                    // Knockback
                    Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
                    if (playerRb != null)
                    {
                        Vector3 knockbackDirection = (playerTransform.position - transform.position).normalized;
                        playerRb.AddForce(knockbackDirection * attackKnockbackForce * 2f, ForceMode.Impulse);
                    }
                }
            }
        }
    }
    
    public override void TakeDamage(float amount)
    {
        if (isDead) return;
        
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"Boss took {amount} damage. Health: {currentHealth}/{maxHealth}");
        
        // Visual feedback
        if (bossRenderer != null)
        {
            StartCoroutine(DamageFlash());
        }
        
        // Event
        OnHealthChanged?.Invoke(this, currentHealth / maxHealth);
        
        // Aggro na hráče
        if (currentState == EnemyState.Idle || currentState == EnemyState.Patrol)
        {
            ChangeState(EnemyState.Chasing);
        }
        
        // Check death
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }
    
    private IEnumerator DamageFlash()
    {
        Color flashColor = Color.white;
        bossRenderer.material.color = flashColor;
        yield return new WaitForSeconds(0.1f);
        
        // Vrátit barvu podle fáze
        if (currentPhase == 1) bossRenderer.material.color = phase1Color;
        else if (currentPhase == 2) bossRenderer.material.color = phase2Color;
        else if (currentPhase == 3) bossRenderer.material.color = phase3Color;
    }
    
    protected override void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log("Boss defeated!");
        
        // Stop all abilities
        StopAllCoroutines();
        
        // Change state
        ChangeState(EnemyState.Dead);
        
        // Disable components
        if (navMeshAgent != null) navMeshAgent.enabled = false;
        
        // Event
        OnDeath?.Invoke(this);
        
        // Destroy after delay
        Destroy(gameObject, 3f);
    }
    
    protected void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        Debug.Log($"Boss healed for {amount}. Health: {currentHealth}/{maxHealth}");
        
        // Event
        OnHealthChanged?.Invoke(this, currentHealth / maxHealth);
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Debug visualizace
    private new void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Ground pound radius (phase 3)
        if (currentPhase >= 3)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, groundPoundRadius);
        }
        
        // Roar radius
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, roarRadius);
    }
}
