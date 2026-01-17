using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Future Robot - Fast melee robot with laser blade
/// Can dash attack and has energy shield
/// </summary>
public class FutureRobot : EnemyBase
{
    [Header("Robot Stats")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDamage = 40f;
    [SerializeField] private float dashDistance = 8f;
    [SerializeField] private float dashCooldown = 5f;
    
    [Header("Energy Shield")]
    [SerializeField] private float shieldHealth = 50f;
    [SerializeField] private float currentShieldHealth;
    [SerializeField] private float shieldRegenRate = 5f; // HP per second
    [SerializeField] private float shieldRegenDelay = 4f; // Delay after taking damage
    [SerializeField] private bool shieldActive = true;
    
    [Header("Laser Blade")]
    [SerializeField] private GameObject laserBladeEffect;
    [SerializeField] private float laserBladeDamage = 35f;
    [SerializeField] private float bladeComboChance = 0.3f;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem dashEffect;
    [SerializeField] private ParticleSystem shieldEffect;
    [SerializeField] private Color shieldColor = new Color(0f, 0.8f, 1f, 0.5f);
    
    [Header("Audio")]
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip laserSlashSound;
    [SerializeField] private AudioClip shieldHitSound;
    [SerializeField] private AudioClip shieldBreakSound;
    [SerializeField] private AudioSource audioSource;
    
    // Private variables
    private bool isDashing = false;
    private bool isAttacking = false;
    private float lastDashTime;
    private float lastShieldDamageTime;
    private Vector3 dashStartPosition;
    private Vector3 dashTargetPosition;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        // Robot statistiky
        maxHealth = 90f;
        currentHealth = maxHealth;
        damage = 30f;
        moveSpeed = 4.5f;
        detectionRange = 12f;
        attackRange = 2.5f;
        attackCooldown = 1.2f;
        
        currentShieldHealth = shieldHealth;
        shieldActive = true;
        
        // Setup audio
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Aktivovat shield effect
        if (shieldEffect != null)
        {
            shieldEffect.Play();
        }
        
        Debug.Log("Future Robot initialized!");
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (isDead) return;
        
        // Shield regeneration
        if (!shieldActive && Time.time - lastShieldDamageTime >= shieldRegenDelay)
        {
            RegenerateShield();
        }
        else if (shieldActive)
        {
            RegenerateShield();
        }
        
        // Dash attack logic
        if (!isDashing && currentState == EnemyState.Chasing && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            // Použít dash, pokud je hráč ve správné vzdálenosti
            if (distanceToPlayer > attackRange * 2 && distanceToPlayer < dashDistance && 
                Time.time - lastDashTime >= dashCooldown)
            {
                StartCoroutine(DashAttack());
            }
        }
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
        
        // Pronásledovat hráče (pokud nedashuje)
        if (!isDashing)
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
        if (distanceToPlayer > attackRange * 1.5f)
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
            PerformLaserBladeAttack();
        }
    }
    
    private void PerformLaserBladeAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        Debug.Log("Future Robot performs Laser Blade Attack!");
        PlaySound(laserSlashSound);
        
        // Aktivovat laser blade effect
        if (laserBladeEffect != null)
        {
            laserBladeEffect.SetActive(true);
            StartCoroutine(DeactivateLaserBlade());
        }
        
        // Deal damage
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(laserBladeDamage);
                    ApplyKnockback(playerTransform);
                    
                    // Šance na combo útok
                    if (Random.value < bladeComboChance)
                    {
                        StartCoroutine(ComboAttack());
                    }
                }
            }
        }
        
        StartCoroutine(ResetAttacking());
    }
    
    private IEnumerator ComboAttack()
    {
        yield return new WaitForSeconds(0.3f);
        
        Debug.Log("Future Robot performs COMBO attack!");
        PlaySound(laserSlashSound);
        
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(laserBladeDamage * 0.6f);
                }
            }
        }
    }
    
    private IEnumerator DashAttack()
    {
        isDashing = true;
        lastDashTime = Time.time;
        
        Debug.Log("Future Robot performs DASH ATTACK!");
        PlaySound(dashSound);
        
        // Aktivovat dash effect
        if (dashEffect != null)
        {
            dashEffect.Play();
        }
        
        // Uložit pozice
        dashStartPosition = transform.position;
        dashTargetPosition = playerTransform.position;
        
        // Disable NavMeshAgent během dashe
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.enabled = false;
        }
        
        // Dash direction
        Vector3 dashDirection = (dashTargetPosition - dashStartPosition).normalized;
        dashDirection.y = 0;
        
        float dashTime = 0f;
        float maxDashTime = dashDistance / dashSpeed;
        bool hitPlayer = false;
        
        while (dashTime < maxDashTime)
        {
            // Move
            transform.position += dashDirection * dashSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(dashDirection);
            
            // Check collision with player
            if (!hitPlayer && playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer <= 2f)
                {
                    PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(dashDamage);
                        ApplyKnockback(playerTransform, 2f);
                        Debug.Log($"Future Robot dash hit! Damage: {dashDamage}");
                        hitPlayer = true;
                    }
                }
            }
            
            dashTime += Time.deltaTime;
            yield return null;
        }
        
        // Re-enable NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
        }
        
        yield return new WaitForSeconds(0.5f);
        isDashing = false;
    }
    
    private IEnumerator DeactivateLaserBlade()
    {
        yield return new WaitForSeconds(0.5f);
        if (laserBladeEffect != null)
        {
            laserBladeEffect.SetActive(false);
        }
    }
    
    private void RegenerateShield()
    {
        if (currentShieldHealth < shieldHealth)
        {
            currentShieldHealth = Mathf.Min(currentShieldHealth + shieldRegenRate * Time.deltaTime, shieldHealth);
            
            // Reaktivovat shield když je plný
            if (!shieldActive && currentShieldHealth >= shieldHealth)
            {
                shieldActive = true;
                if (shieldEffect != null)
                {
                    shieldEffect.Play();
                }
                Debug.Log("Future Robot shield reactivated!");
            }
        }
    }
    
    public override void TakeDamage(float damage)
    {
        // Shield absorbuje damage
        if (shieldActive && currentShieldHealth > 0)
        {
            PlaySound(shieldHitSound);
            currentShieldHealth -= damage;
            
            Debug.Log($"Future Robot shield hit! Shield: {currentShieldHealth}/{shieldHealth}");
            
            if (currentShieldHealth <= 0)
            {
                currentShieldHealth = 0;
                shieldActive = false;
                PlaySound(shieldBreakSound);
                
                if (shieldEffect != null)
                {
                    shieldEffect.Stop();
                }
                
                Debug.Log("Future Robot shield broken!");
            }
            
            lastShieldDamageTime = Time.time;
        }
        else
        {
            // Damage na health
            base.TakeDamage(damage);
        }
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
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    protected override void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log("Future Robot destroyed!");
        
        // Stop all coroutines
        StopAllCoroutines();
        
        // Disable components
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }
        
        // Disable effects
        if (shieldEffect != null)
        {
            shieldEffect.Stop();
        }
        
        if (laserBladeEffect != null)
        {
            laserBladeEffect.SetActive(false);
        }
        
        // Death animation/effects could go here
        
        // Destroy after delay
        Destroy(gameObject, 2f);
    }
    
    private new void OnDrawGizmosSelected()
    {
        // Draw dash range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dashDistance);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
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
                Vector3 randomDirection = Random.insideUnitSphere * 10f;
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
