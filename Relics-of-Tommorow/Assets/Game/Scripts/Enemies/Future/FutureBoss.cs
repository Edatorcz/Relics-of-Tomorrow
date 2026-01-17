using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Future Boss - AI Commander with advanced technology
/// Má 3 fáze boje s různými útoky a mechanikami
/// </summary>
public class FutureBoss : EnemyBase
{
    [Header("Boss Stats")]
    [SerializeField] private float phase1HealthThreshold = 0.66f; // 66% HP
    [SerializeField] private float phase2HealthThreshold = 0.33f; // 33% HP
    [SerializeField] private int currentPhase = 1;
    
    [Header("Phase 1 - Tactical Commander")]
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private int dronesPerWave = 2;
    [SerializeField] private float droneSpawnCooldown = 15f;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private float laserDamage = 50f;
    [SerializeField] private float laserFireRate = 2f;
    
    [Header("Phase 2 - Advanced Warfare")]
    [SerializeField] private float energyShieldHealth = 200f;
    [SerializeField] private float currentShieldHealth;
    [SerializeField] private bool shieldActive = false;
    [SerializeField] private float teleportCooldown = 8f;
    // Teleport range - můžete použít pro omezení teleportace
    // private float teleportRange = 15f;
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private float rocketDamage = 70f;
    [SerializeField] private int rocketsPerVolley = 3;
    
    [Header("Phase 3 - Overcharge Mode")]
    // Overcharge flag - můžete použít pro vizuální efekty
    // private bool isOvercharged = false;
    [SerializeField] private float overchargeSpeedMultiplier = 1.5f;
    [SerializeField] private float overchargeDamageMultiplier = 2f;
    [SerializeField] private float plasmaBurstRadius = 10f;
    [SerializeField] private float plasmaBurstDamage = 100f;
    [SerializeField] private float plasmaBurstCooldown = 10f;
    [SerializeField] private GameObject empPrefab;
    [SerializeField] private float empRadius = 12f;
    [SerializeField] private float empDamage = 60f;
    // EMP stun duration - můžete implementovat stun efekt později
    // private float empStunDuration = 3f;
    
    [Header("Boss Abilities")]
    [SerializeField] private float healingAmount = 150f;
    [SerializeField] private Transform[] teleportPoints;
    
    [Header("Boss Loot")]
    [SerializeField] private BossLootDropper lootDropper;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem laserEffect;
    [SerializeField] private ParticleSystem teleportEffect;
    [SerializeField] private ParticleSystem shieldEffect;
    [SerializeField] private ParticleSystem overchargeEffect;
    [SerializeField] private ParticleSystem plasmaBurstEffect;
    [SerializeField] private Light bossLight;
    [SerializeField] private Color phase1Color = new Color(0f, 0.5f, 1f); // Blue
    [SerializeField] private Color phase2Color = new Color(0.5f, 0f, 1f); // Purple
    [SerializeField] private Color phase3Color = new Color(1f, 0f, 0f); // Red
    
    [Header("Audio")]
    [SerializeField] private AudioClip laserFireSound;
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private AudioClip shieldSound;
    [SerializeField] private AudioClip overchargeSound;
    [SerializeField] private AudioClip plasmaBurstSound;
    [SerializeField] private AudioClip phaseChangeSound;
    [SerializeField] private AudioSource audioSource;
    
    // Private variables
    private bool isAttacking = false;
    private bool isTeleporting = false;
    private float lastDroneSpawnTime;
    private float lastLaserFireTime;
    private float lastTeleportTime;
    private float lastPlasmaBurstTime;
    private Renderer bossRenderer;
    private List<GameObject> spawnedDrones = new List<GameObject>();
    
    protected override void Initialize()
    {
        base.Initialize();
        
        // Boss statistiky
        maxHealth = 800f;
        currentHealth = maxHealth;
        damage = 45f;
        moveSpeed = 3f;
        detectionRange = 25f;
        attackRange = 15f; // Mostly ranged
        attackCooldown = laserFireRate;
        
        currentShieldHealth = 0f;
        shieldActive = false;
        
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
        
        // Setup light
        if (bossLight != null)
        {
            bossLight.color = phase1Color;
        }
        
        // Auto-generate teleport points if not set
        if (teleportPoints == null || teleportPoints.Length == 0)
        {
            GenerateTeleportPoints();
        }
        
        ChangeState(EnemyState.Idle);
        Debug.Log("Future Boss initialized - AI Commander online!");
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (isDead) return;
        
        // Kontrola změny fáze
        CheckPhaseChange();
        
        // Speciální ability podle fáze
        HandlePhaseAbilities();
        
        // Shield regeneration v Phase 2
        if (currentPhase >= 2 && shieldActive && currentShieldHealth < energyShieldHealth)
        {
            currentShieldHealth = Mathf.Min(currentShieldHealth + 10f * Time.deltaTime, energyShieldHealth);
        }
    }
    
    protected override void HandleIdleState()
    {
        // Boss nikdy není idle
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
        if (playerTransform == null || isDead || isTeleporting) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Pokud je hráč v dosahu útoku
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }
        
        // Pronásledovat hráče (nebo teleportovat)
        if (currentPhase >= 2 && Time.time - lastTeleportTime >= teleportCooldown && distanceToPlayer > attackRange * 1.5f)
        {
            StartCoroutine(TeleportAbility());
        }
        else
        {
            MoveTowardsPlayer();
        }
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
        if (!isAttacking && Time.time >= lastLaserFireTime + attackCooldown)
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
        
        Debug.Log("Future Boss entering Phase 2 - ADVANCED WARFARE!");
        
        // Aktivovat energy shield
        shieldActive = true;
        currentShieldHealth = energyShieldHealth;
        
        if (shieldEffect != null)
        {
            shieldEffect.Play();
        }
        PlaySound(shieldSound);
        
        // Zvýšení rychlosti
        moveSpeed = 4.5f;
        attackCooldown = 1.5f;
        
        // Změna barvy
        if (bossRenderer != null)
        {
            bossRenderer.material.color = phase2Color;
        }
        
        if (bossLight != null)
        {
            bossLight.color = phase2Color;
        }
        
        // Vizuální efekt
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, transform.position, Quaternion.identity);
        }
        
        // Zvuk
        PlaySound(phaseChangeSound);
        
        // Boss se zahojí
        Heal(healingAmount);
        
        // Spawn drones
        SpawnDrones();
    }
    
    private void EnterPhase3()
    {
        currentPhase = 3;
        
        Debug.Log("Future Boss entering Phase 3 - OVERCHARGE MODE!");
        
        // Aktivovat overcharge
        // isOvercharged = true; // Zakomentováno - můžete použít pro vizuální efekty
        
        // Maximální statistiky
        moveSpeed *= overchargeSpeedMultiplier;
        attackCooldown = 1f;
        damage *= overchargeDamageMultiplier;
        
        // Změna barvy
        if (bossRenderer != null)
        {
            bossRenderer.material.color = phase3Color;
        }
        
        if (bossLight != null)
        {
            bossLight.color = phase3Color;
        }
        
        // Vizuální efekt
        if (overchargeEffect != null)
        {
            overchargeEffect.Play();
        }
        
        // Zvuk
        PlaySound(overchargeSound);
        
        // Větší heal ve 3. fázi
        Heal(healingAmount * 2f);
        
        // Okamžitý plasma burst
        StartCoroutine(PlasmaBurstAttack());
    }
    
    private void HandlePhaseAbilities()
    {
        if (currentState != EnemyState.Chasing && currentState != EnemyState.Attacking)
            return;
            
        if (playerTransform == null) return;
        
        // Phase 1 abilities - Drone summons
        if (currentPhase == 1)
        {
            if (Time.time - lastDroneSpawnTime >= droneSpawnCooldown)
            {
                SpawnDrones();
            }
        }
        
        // Phase 2 abilities - Rockets
        if (currentPhase == 2)
        {
            if (Random.value < 0.15f * Time.deltaTime)
            {
                StartCoroutine(FireRocketVolley());
            }
        }
        
        // Phase 3 abilities - Plasma burst + EMP
        if (currentPhase == 3)
        {
            if (Time.time - lastPlasmaBurstTime >= plasmaBurstCooldown)
            {
                StartCoroutine(PlasmaBurstAttack());
            }
            
            if (Random.value < 0.1f * Time.deltaTime)
            {
                StartCoroutine(EMPAttack());
            }
        }
    }
    
    private void PerformBossAttack()
    {
        isAttacking = true;
        lastLaserFireTime = Time.time;
        
        float roll = Random.value;
        
        if (currentPhase == 1)
        {
            // Phase 1: Basic laser attacks
            FireLaser();
        }
        else if (currentPhase == 2)
        {
            // Phase 2: Laser nebo rockets
            if (roll < 0.6f)
            {
                FireLaser();
            }
            else
            {
                StartCoroutine(FireRocketVolley());
            }
        }
        else // Phase 3
        {
            // Phase 3: Všechny útoky
            if (roll < 0.4f)
            {
                FireLaser();
            }
            else if (roll < 0.7f)
            {
                StartCoroutine(FireRocketVolley());
            }
            else
            {
                StartCoroutine(EMPAttack());
            }
        }
        
        StartCoroutine(ResetAttacking());
    }
    
    #region Attack Abilities
    
    private void FireLaser()
    {
        if (laserPrefab == null || playerTransform == null)
        {
            Debug.LogWarning("Laser prefab not set!");
            return;
        }
        
        Debug.Log("Future Boss fires laser!");
        PlaySound(laserFireSound);
        
        if (laserEffect != null)
        {
            laserEffect.Play();
        }
        
        // Spawn laser projectile
        Vector3 spawnPosition = transform.position + Vector3.up * 2f;
        GameObject laser = Instantiate(laserPrefab, spawnPosition, Quaternion.identity);
        
        // Setup projectile
        EnemyProjectile projectile = laser.GetComponent<EnemyProjectile>();
        if (projectile == null)
        {
            projectile = laser.AddComponent<EnemyProjectile>();
        }
        
        projectile.SetOwner(this);
        projectile.SetDamage(laserDamage);
        
        // Aim at player
        Vector3 direction = (playerTransform.position + Vector3.up - spawnPosition).normalized;
        
        Rigidbody rb = laser.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = laser.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = false;
        rb.linearVelocity = direction * 25f;
        laser.transform.rotation = Quaternion.LookRotation(direction);
    }
    
    private IEnumerator FireRocketVolley()
    {
        if (rocketPrefab == null || playerTransform == null) yield break;
        
        Debug.Log("Future Boss fires rocket volley!");
        
        for (int i = 0; i < rocketsPerVolley; i++)
        {
            FireRocket();
            yield return new WaitForSeconds(0.4f);
        }
    }
    
    private void FireRocket()
    {
        if (rocketPrefab == null || playerTransform == null) return;
        
        Vector3 spawnPosition = transform.position + Vector3.up * 2f + transform.right * Random.Range(-1f, 1f);
        GameObject rocket = Instantiate(rocketPrefab, spawnPosition, Quaternion.identity);
        
        EnemyProjectile projectile = rocket.GetComponent<EnemyProjectile>();
        if (projectile == null)
        {
            projectile = rocket.AddComponent<EnemyProjectile>();
        }
        
        projectile.SetOwner(this);
        projectile.SetDamage(rocketDamage);
        
        Vector3 direction = (playerTransform.position - spawnPosition).normalized;
        
        Rigidbody rb = rocket.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = rocket.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = true;
        rb.linearVelocity = direction * 15f;
        rocket.transform.rotation = Quaternion.LookRotation(direction);
    }
    
    private IEnumerator PlasmaBurstAttack()
    {
        Debug.Log("Future Boss unleashes PLASMA BURST!");
        lastPlasmaBurstTime = Time.time;
        isAttacking = true;
        
        PlaySound(plasmaBurstSound);
        
        // Vizuální efekt
        if (plasmaBurstEffect != null)
        {
            Instantiate(plasmaBurstEffect, transform.position, Quaternion.identity);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Damage všem v radiusu
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, plasmaBurstRadius);
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Player"))
            {
                PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(plasmaBurstDamage);
                    ApplyKnockback(col.transform, 3f);
                }
            }
        }
        
        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }
    
    private IEnumerator EMPAttack()
    {
        Debug.Log("Future Boss deploys EMP!");
        
        if (empPrefab != null)
        {
            GameObject emp = Instantiate(empPrefab, transform.position, Quaternion.identity);
            
            yield return new WaitForSeconds(0.8f);
            
            // EMP blast
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, empRadius);
            foreach (Collider col in hitColliders)
            {
                if (col.CompareTag("Player"))
                {
                    PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(empDamage);
                        // Could add stun effect here
                        Debug.Log($"Player hit by EMP! Damage: {empDamage}");
                    }
                }
            }
            
            Destroy(emp, 2f);
        }
    }
    
    private IEnumerator TeleportAbility()
    {
        if (teleportPoints == null || teleportPoints.Length == 0 || isTeleporting) yield break;
        
        isTeleporting = true;
        lastTeleportTime = Time.time;
        
        Debug.Log("Future Boss teleporting!");
        PlaySound(teleportSound);
        
        // Vizuální efekt na staré pozici
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, transform.position, Quaternion.identity);
        }
        
        yield return new WaitForSeconds(0.3f);
        
        // Najít nejlepší teleport point
        Transform bestPoint = GetBestTeleportPoint();
        if (bestPoint != null)
        {
            transform.position = bestPoint.position;
            
            // Vizuální efekt na nové pozici
            if (teleportEffect != null)
            {
                Instantiate(teleportEffect, transform.position, Quaternion.identity);
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        isTeleporting = false;
    }
    
    private void SpawnDrones()
    {
        if (dronePrefab == null)
        {
            Debug.LogWarning("Drone prefab not set!");
            return;
        }
        
        lastDroneSpawnTime = Time.time;
        
        Debug.Log($"Future Boss summons {dronesPerWave} combat drones!");
        
        // Odstranit mrtvé drony ze seznamu
        spawnedDrones.RemoveAll(drone => drone == null);
        
        for (int i = 0; i < dronesPerWave; i++)
        {
            float angle = i * (360f / dronesPerWave);
            Vector3 spawnOffset = Quaternion.Euler(0, angle, 0) * Vector3.forward * 6f;
            Vector3 spawnPosition = transform.position + spawnOffset + Vector3.up * 3f;
            
            GameObject drone = Instantiate(dronePrefab, spawnPosition, Quaternion.identity);
            spawnedDrones.Add(drone);
            
            // Drone automaticky detekuje hráče přes EnemyBase.player
        }
    }
    
    #endregion
    
    private void GenerateTeleportPoints()
    {
        // Auto-generate teleport points v kruhu kolem spawn pozice
        int pointCount = 8;
        teleportPoints = new Transform[pointCount];
        
        for (int i = 0; i < pointCount; i++)
        {
            float angle = i * (360f / pointCount);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * 10f;
            
            GameObject point = new GameObject($"TeleportPoint_{i}");
            point.transform.position = spawnPosition + offset;
            point.transform.SetParent(transform.parent);
            teleportPoints[i] = point.transform;
        }
    }
    
    private Transform GetBestTeleportPoint()
    {
        if (teleportPoints == null || teleportPoints.Length == 0 || playerTransform == null)
            return null;
        
        // Najít bod co je ve správné vzdálenosti od hráče
        Transform bestPoint = null;
        float bestScore = float.MinValue;
        
        foreach (Transform point in teleportPoints)
        {
            if (point == null) continue;
            
            float distanceToPlayer = Vector3.Distance(point.position, playerTransform.position);
            float distanceFromCurrent = Vector3.Distance(point.position, transform.position);
            
            // Preferovat body ve střední vzdálenosti od hráče
            float score = 0f;
            if (distanceToPlayer > attackRange * 0.5f && distanceToPlayer < attackRange * 1.2f)
            {
                score = 100f - Mathf.Abs(distanceToPlayer - attackRange);
            }
            
            // Bonus za vzdálenost od aktuální pozice
            score += distanceFromCurrent * 0.5f;
            
            if (score > bestScore)
            {
                bestScore = score;
                bestPoint = point;
            }
        }
        
        return bestPoint;
    }
    
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
        Debug.Log($"Future Boss healed for {amount}! Current HP: {currentHealth}/{maxHealth}");
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
        // Energy shield absorbuje damage v Phase 2+
        if (shieldActive && currentShieldHealth > 0)
        {
            currentShieldHealth -= damage;
            PlaySound(shieldSound);
            
            Debug.Log($"Future Boss shield hit! Shield: {currentShieldHealth}/{energyShieldHealth}");
            
            if (currentShieldHealth <= 0)
            {
                currentShieldHealth = 0;
                shieldActive = false;
                
                if (shieldEffect != null)
                {
                    shieldEffect.Stop();
                }
                
                Debug.Log("Future Boss shield depleted!");
            }
            
            return;
        }
        
        base.TakeDamage(damage);
    }
    
    protected override void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log("Future Boss defeated - AI Commander offline!");
        
        // Drop loot
        if (lootDropper != null)
        {
            lootDropper.DropBossLoot(transform.position);
        }
        
        // Destroy všechny spawnuté drony
        foreach (GameObject drone in spawnedDrones)
        {
            if (drone != null)
            {
                Destroy(drone);
            }
        }
        
        // Stop all coroutines
        StopAllCoroutines();
        
        // Disable components
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }
        
        // Disable effects
        if (shieldEffect != null) shieldEffect.Stop();
        if (overchargeEffect != null) overchargeEffect.Stop();
        if (bossLight != null) bossLight.enabled = false;
        
        // Death animation/effects could go here
        
        // Destroy after delay
        Destroy(gameObject, 3f);
    }
    
    private new void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw plasma burst radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, plasmaBurstRadius);
        
        // Draw EMP radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, empRadius);
        
        // Draw teleport points
        if (teleportPoints != null)
        {
            Gizmos.color = Color.magenta;
            foreach (Transform point in teleportPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 1f);
                }
            }
        }
    }
    
    protected override void HandleStunnedState()
    {
        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.hasPath)
        {
            navMeshAgent.ResetPath();
        }
        
        // Deaktivovat štít při stunu
        if (shieldActive)
        {
            shieldActive = false;
        }
        
        // Zrušit teleportaci
        if (isTeleporting)
        {
            isTeleporting = false;
        }
    }
}
