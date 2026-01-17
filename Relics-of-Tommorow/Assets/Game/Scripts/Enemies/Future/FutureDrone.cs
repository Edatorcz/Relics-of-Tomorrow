using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Future Drone - Flying ranged enemy with laser projectiles
/// Hovers above ground, strafes around player
/// </summary>
public class FutureDrone : EnemyBase
{
    [Header("Drone Stats")]
    [SerializeField] private float hoverHeight = 3f;
    [SerializeField] private float hoverSpeed = 3.5f;
    [SerializeField] private float strafeSpeed = 4f;
    [SerializeField] private float strafeRadius = 8f;
    
    [Header("Laser Weapon")]
    [SerializeField] private GameObject laserProjectilePrefab;
    [SerializeField] private Transform laserSpawnPoint;
    [SerializeField] private float laserDamage = 22f;
    [SerializeField] private float laserSpeed = 20f;
    [SerializeField] private float laserFireRate = 1.5f;
    
    [Header("Rapid Fire Mode")]
    // Rapid firing state - můžete použít pro vizuální efekty
    // private bool isRapidFiring = false;
    [SerializeField] private int rapidFireBursts = 5;
    [SerializeField] private float rapidFireInterval = 0.2f;
    [SerializeField] private float rapidFireCooldown = 8f;
    [SerializeField] private float rapidFireChance = 0.3f;
    
    [Header("Movement Patterns")]
    [SerializeField] private float strafeChangeInterval = 2f;
    [SerializeField] private bool isStrafingLeft = true;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem laserChargeEffect;
    [SerializeField] private Light droneLight;
    [SerializeField] private Color normalLightColor = Color.blue;
    [SerializeField] private Color rapidFireLightColor = Color.red;
    
    [Header("Audio")]
    [SerializeField] private AudioClip laserFireSound;
    [SerializeField] private AudioClip rapidFireSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioSource audioSource;
    
    // Private variables
    private bool isAttacking = false;
    private float lastLaserFireTime;
    private float lastRapidFireTime;
    private float lastStrafeChangeTime;
    private Vector3 targetHoverPosition;
    private float currentAngle = 0f;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        // Drone statistiky
        maxHealth = 60f;
        currentHealth = maxHealth;
        damage = 22f;
        moveSpeed = hoverSpeed;
        detectionRange = 15f;
        attackRange = 12f; // Ranged attack
        attackCooldown = laserFireRate;
        
        // Setup audio
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Play hover sound loop
        if (hoverSound != null && audioSource != null)
        {
            audioSource.clip = hoverSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Setup laser spawn point
        if (laserSpawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("LaserSpawnPoint");
            spawnPoint.transform.SetParent(transform);
            spawnPoint.transform.localPosition = Vector3.forward * 0.5f;
            laserSpawnPoint = spawnPoint.transform;
        }
        
        // Setup light
        if (droneLight != null)
        {
            droneLight.color = normalLightColor;
        }
        
        // Disable NavMeshAgent pro létání (nepoužívá NavMesh)
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }
        
        Debug.Log("Future Drone initialized!");
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (isDead) return;
        
        // Maintain hover height
        MaintainHoverHeight();
        
        // Change strafe direction periodically
        if (Time.time - lastStrafeChangeTime >= strafeChangeInterval)
        {
            isStrafingLeft = !isStrafingLeft;
            lastStrafeChangeTime = Time.time;
        }
    }
    
    private void MaintainHoverHeight()
    {
        Vector3 currentPos = transform.position;
        float targetY = hoverHeight;
        
        // Smooth přechod na target výšku
        if (Mathf.Abs(currentPos.y - targetY) > 0.1f)
        {
            currentPos.y = Mathf.Lerp(currentPos.y, targetY, Time.deltaTime * 2f);
            transform.position = currentPos;
        }
    }
    
    protected override void HandleChaseState()
    {
        if (playerTransform == null || isDead) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Pokud je hráč v dosahu útoku
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }
        
        // Pohyb k hráči (létání)
        FlyTowardsPlayer();
    }
    
    protected override void HandleAttackState()
    {
        if (playerTransform == null || isDead)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Pokud je hráč příliš daleko
        if (distanceToPlayer > attackRange * 1.2f)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // Strafe movement kolem hráče
        StrafeAroundPlayer();
        
        // Otočit se k hráči
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Fire lasers
        if (!isAttacking && Time.time >= lastLaserFireTime + attackCooldown)
        {
            // Šance na rapid fire
            if (Random.value < rapidFireChance && Time.time - lastRapidFireTime >= rapidFireCooldown)
            {
                StartCoroutine(RapidFireMode());
            }
            else
            {
                FireLaser();
            }
        }
    }
    
    private void FlyTowardsPlayer()
    {
        if (playerTransform == null) return;
        
        Vector3 targetPosition = playerTransform.position;
        targetPosition.y = hoverHeight;
        
        // Keep some distance
        Vector3 direction = (targetPosition - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer > strafeRadius)
        {
            transform.position += direction * hoverSpeed * Time.deltaTime;
        }
        
        // Look at player
        Vector3 lookDirection = playerTransform.position - transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void StrafeAroundPlayer()
    {
        if (playerTransform == null) return;
        
        // Kruhový pohyb kolem hráče
        float strafeDirection = isStrafingLeft ? 1f : -1f;
        currentAngle += strafeDirection * strafeSpeed * Time.deltaTime;
        
        Vector3 offset = new Vector3(
            Mathf.Cos(currentAngle) * strafeRadius,
            0,
            Mathf.Sin(currentAngle) * strafeRadius
        );
        
        targetHoverPosition = playerTransform.position + offset;
        targetHoverPosition.y = hoverHeight;
        
        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, targetHoverPosition, Time.deltaTime * 2f);
    }
    
    private void FireLaser()
    {
        if (laserProjectilePrefab == null || playerTransform == null)
        {
            Debug.LogWarning("Laser projectile prefab not set!");
            return;
        }
        
        isAttacking = true;
        lastLaserFireTime = Time.time;
        
        Debug.Log("Future Drone fires laser!");
        PlaySound(laserFireSound);
        
        // Charge effect
        if (laserChargeEffect != null)
        {
            laserChargeEffect.Play();
        }
        
        // Spawn laser projectile
        Vector3 spawnPosition = laserSpawnPoint != null ? laserSpawnPoint.position : transform.position;
        GameObject laser = Instantiate(laserProjectilePrefab, spawnPosition, Quaternion.identity);
        
        // Setup projectile
        EnemyProjectile projectile = laser.GetComponent<EnemyProjectile>();
        if (projectile == null)
        {
            projectile = laser.AddComponent<EnemyProjectile>();
        }
        
        projectile.SetOwner(this);
        projectile.SetDamage(laserDamage);
        
        // Calculate direction with prediction
        Vector3 targetPosition = PredictPlayerPosition(0.5f);
        Vector3 direction = (targetPosition - spawnPosition).normalized;
        
        // Set velocity
        Rigidbody rb = laser.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = laser.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = false;
        rb.linearVelocity = direction * laserSpeed;
        laser.transform.rotation = Quaternion.LookRotation(direction);
        
        StartCoroutine(ResetAttacking());
    }
    
    private IEnumerator RapidFireMode()
    {
        // isRapidFiring = true; // Zakomentováno - můžete použít pro vizuální efekty
        lastRapidFireTime = Time.time;
        
        Debug.Log("Future Drone enters RAPID FIRE MODE!");
        PlaySound(rapidFireSound);
        
        // Change light color
        if (droneLight != null)
        {
            droneLight.color = rapidFireLightColor;
        }
        
        // Fire multiple bursts
        for (int i = 0; i < rapidFireBursts; i++)
        {
            if (playerTransform == null || isDead) break;
            
            FireLaser();
            yield return new WaitForSeconds(rapidFireInterval);
        }
        
        // Reset light color
        if (droneLight != null)
        {
            droneLight.color = normalLightColor;
        }
        
        // isRapidFiring = false; // Zakomentováno - můžete použít pro vizuální efekty
        lastLaserFireTime = Time.time; // Reset cooldown
    }
    
    private Vector3 PredictPlayerPosition(float predictionTime)
    {
        if (playerTransform == null) return Vector3.zero;
        
        // Try to get player velocity
        Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            return playerTransform.position + playerRb.linearVelocity * predictionTime;
        }
        
        return playerTransform.position;
    }
    
    private IEnumerator ResetAttacking()
    {
        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        
        // Evasive maneuver při zásahu
        if (!isDead && Random.value < 0.4f)
        {
            StartCoroutine(EvasiveManeuver());
        }
    }
    
    private IEnumerator EvasiveManeuver()
    {
        Vector3 evasiveDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        float evasiveTime = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < evasiveTime)
        {
            transform.position += evasiveDirection * strafeSpeed * 1.5f * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    protected override void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log("Future Drone destroyed!");
        
        // Stop all coroutines
        StopAllCoroutines();
        
        // Stop sounds
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        
        // Disable effects
        if (laserChargeEffect != null)
        {
            laserChargeEffect.Stop();
        }
        
        if (droneLight != null)
        {
            droneLight.enabled = false;
        }
        
        // Fall to ground with gravity
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = true;
        
        // Death animation/effects could go here
        
        // Destroy after delay
        Destroy(gameObject, 3f);
    }
    
    private new void OnDrawGizmosSelected()
    {
        // Draw strafe radius
        Gizmos.color = Color.cyan;
        if (playerTransform != null)
        {
            Gizmos.DrawWireSphere(playerTransform.position, strafeRadius);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, strafeRadius);
        }
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw hover height
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, hoverHeight, transform.position.z));
    }
    
    protected override void HandleIdleState()
    {
        // Drone hned letí k hráči
        if (player != null)
        {
            ChangeState(EnemyState.Chasing);
        }
    }
    
    protected override void HandlePatrolState()
    {
        // Drone nepatroluje - stále ve vzduchu
        if (player != null)
        {
            ChangeState(EnemyState.Chasing);
        }
        else
        {
            // Hover na místě - jemné kývání
            float bobAmount = 0.3f;
            float bobSpeed = 2f;
            targetHoverPosition.y = hoverHeight + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        }
    }
    
    protected override void HandleStunnedState()
    {
        // Drone padá když je stunnnutý
        Vector3 fallPosition = transform.position;
        fallPosition.y -= Time.deltaTime * 2f;
        transform.position = fallPosition;
    }
}
