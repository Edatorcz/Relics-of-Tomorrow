using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Kamenný obr - tank nepřítel pro kamennou dobu
/// Pomalý, silný, odolný s vysokým zdravím
/// </summary>
public class StoneAgeGiant : EnemyBase
{
    [Header("Giant Behavior")]
    [SerializeField] private float stompRange = 4f;
    [SerializeField] private float stompDamage = 35f;
    [SerializeField] private float stompCooldown = 4f;
    [SerializeField] private float shieldBlockChance = 0.4f;
    [SerializeField] private bool isBlocking = false;
    
    [Header("Tank Abilities")]
    [SerializeField] private float groundSlamRange = 6f;
    [SerializeField] private float groundSlamDamage = 25f;
    [SerializeField] private float groundSlamCooldown = 8f;
    [SerializeField] private float tauntRange = 15f;
    [SerializeField] private float damageReduction = 0.3f;
    
    [Header("Defensive Mode")]
    [SerializeField] private float lowHealthThreshold = 0.4f;
    [SerializeField] private bool inDefensiveMode = false;
    [SerializeField] private float defensiveBonus = 0.5f;
    [SerializeField] private float regenerationRate = 2f;
    
    [Header("Giant Sounds")]
    [SerializeField] private AudioClip roarSound;
    [SerializeField] private AudioClip stompSound;
    [SerializeField] private AudioClip groundSlamSound;
    [SerializeField] private AudioClip blockSound;
    [SerializeField] private AudioSource audioSource;
    
    private float lastStompTime;
    private float lastGroundSlamTime;
    private float lastTauntTime;
    private bool hasEnteredDefensiveMode = false;
    private Vector3 defensivePosition;
    private float blockTimer = 0f;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        // Obr má vysoké zdraví ale je pomalý
        maxHealth = 200f;
        currentHealth = maxHealth;
        damage = 30f;
        moveSpeed = 1.5f; // Velmi pomalý
        detectionRange = 10f;
        attackRange = 3f; // Dlouhé ruce
        attackCooldown = 2f; // Pomalé útoky
        
        defensivePosition = transform.position;
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Vypnout line of sight pro obrovského obra
        useLineOfSight = false;
        
        ChangeState(EnemyState.Idle);
    }
    
    protected override void HandleIdleState()
    {
        // Obr se moc nehýbe, jen čeká
        if (navMeshAgent != null && navMeshAgent.hasPath)
            navMeshAgent.ResetPath();
            
        // Velmi zřídka se otočí
        if (Random.Range(0f, 1f) < 0.001f)
        {
            transform.Rotate(0, Random.Range(-30f, 30f), 0);
        }
        
        // Pasivní regenerace v idle
        if (currentHealth < maxHealth)
        {
            Heal(regenerationRate * 0.5f * Time.deltaTime);
        }
    }
    
    protected override void HandlePatrolState()
    {
        // Obr nepatroluje daleko od domova
        if (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.5f)
        {
            GuardArea();
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
        
        // Kontrola defensive módu
        CheckDefensiveMode();
        
        // Ground slam útok na dálku
        if (distanceToPlayer <= groundSlamRange && distanceToPlayer > stompRange &&
            Time.time - lastGroundSlamTime >= groundSlamCooldown)
        {
            PerformGroundSlam();
            return;
        }
        
        // Stomp útok na blízko
        if (distanceToPlayer <= stompRange && 
            Time.time - lastStompTime >= stompCooldown)
        {
            PerformStomp();
            return;
        }
        
        // Normální melee útok
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }
        
        // Taunt hráče aby přišel blíž
        if (distanceToPlayer > groundSlamRange && 
            Time.time - lastTauntTime >= 6f)
        {
            PerformTaunt();
        }
        
        // Pomalý pohyb k hráči (pokud není v defensive módu)
        if (!inDefensiveMode)
        {
            MoveTowardsPlayer();
        }
        
        // Blocking behavior
        HandleBlocking();
    }
    
    protected override void HandleAttackState()
    {
        if (player == null) 
        { 
            ChangeState(EnemyState.Chasing); 
            return; 
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer > attackRange * 1.3f)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        if (navMeshAgent != null)
            navMeshAgent.ResetPath();
            
        LookAtPlayer();
        
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformGiantAttack();
        }
    }
    
    protected override void HandleStunnedState()
    {
        // Obr se těžko stunuje, rychle se zotavuje
        if (navMeshAgent != null && navMeshAgent.hasPath)
            navMeshAgent.ResetPath();
            
        if (Time.time - lastAttackTime >= 1f) // Krátký stun
        {
            ChangeState(EnemyState.Chasing);
        }
    }
    
    private void GuardArea()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 3f; // Malá oblast
        randomDirection.y = 0;
        Vector3 targetPosition = defensivePosition + randomDirection;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 3f, NavMesh.AllAreas))
        {
            navMeshAgent?.SetDestination(hit.position);
        }
    }
    
    private void PerformStomp()
    {
        lastStompTime = Time.time;
        
        // Zastavit pohyb
        if (navMeshAgent != null)
            navMeshAgent.ResetPath();
            
        // Stomp animace delay
        Invoke(nameof(ExecuteStomp), 1f);
        
        PlayStompSound();
    }
    
    private void ExecuteStomp()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= stompRange)
        {
            // Damage k hráči
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(stompDamage);
            }
            
            // Knockback effect (zde by byl implementován)
            // Particle effect (zde by byl implementován)
        }
        
        ChangeState(EnemyState.Chasing);
    }
    
    private void PerformGroundSlam()
    {
        lastGroundSlamTime = Time.time;
        
        if (navMeshAgent != null)
            navMeshAgent.ResetPath();
            
        Invoke(nameof(ExecuteGroundSlam), 1.5f);
        
        PlayGroundSlamSound();
    }
    
    private void ExecuteGroundSlam()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= groundSlamRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Damage klesá se vzdáleností
                float damageMultiplier = 1f - (distanceToPlayer / groundSlamRange);
                float finalDamage = groundSlamDamage * damageMultiplier;
                
                playerHealth.TakeDamage(finalDamage);
                Debug.Log($"{gameObject.name}: Ground slam hit for {finalDamage:F1} damage!");
            }
        }
        
        ChangeState(EnemyState.Chasing);
    }
    
    private void PerformTaunt()
    {
        lastTauntTime = Time.time;
        
        PlayRoarSound();
        
        // Taunt effect - možná přilákat hráče nebo zvýšit agro
    }
    
    private void PerformGiantAttack()
    {
        Attack(); // Base attack
        
        // Silný melee útok
        damage = 30f;
        
        Debug.Log($"{gameObject.name}: Giant smash attack!");
    }
    
    private void CheckDefensiveMode()
    {
        float healthPercent = currentHealth / maxHealth;
        
        if (healthPercent <= lowHealthThreshold && !hasEnteredDefensiveMode)
        {
            EnterDefensiveMode();
        }
    }
    
    private void EnterDefensiveMode()
    {
        if (hasEnteredDefensiveMode) return;
        
        hasEnteredDefensiveMode = true;
        inDefensiveMode = true;
        
        // Bonus defenzy
        damageReduction += defensiveBonus;
        
        // Vizuální změna
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.5f, 0.5f, 1f); // Modrá = defensive
        }
        
        PlayRoarSound();
    }
    
    private void HandleBlocking()
    {
        if (Random.Range(0f, 1f) < 0.01f) // 1% šance každý frame
        {
            StartBlocking();
        }
        
        if (isBlocking)
        {
            blockTimer -= Time.deltaTime;
            if (blockTimer <= 0f)
            {
                StopBlocking();
            }
        }
    }
    
    private void StartBlocking()
    {
        if (isBlocking) return;
        
        isBlocking = true;
        blockTimer = 2f;
        
        PlayBlockSound();
    }
    
    private void StopBlocking()
    {
        isBlocking = false;
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
                rotationSpeed * 0.5f * Time.deltaTime); // Pomalé otáčení
        }
    }
    
    private void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(this, currentHealth);
    }
    
    public override void TakeDamage(float damageAmount)
    {
        // Blocking šance
        if (isBlocking && Random.Range(0f, 1f) < shieldBlockChance)
        {
            PlayBlockSound();
            return;
        }
        
        // Damage reduction
        float reducedDamage = damageAmount * (1f - damageReduction);
        
        base.TakeDamage(reducedDamage);
    }
    
    private void PlayRoarSound()
    {
        if (audioSource != null && roarSound != null)
        {
            audioSource.PlayOneShot(roarSound);
        }
    }
    
    private void PlayStompSound()
    {
        if (audioSource != null && stompSound != null)
        {
            audioSource.PlayOneShot(stompSound);
        }
    }
    
    private void PlayGroundSlamSound()
    {
        if (audioSource != null && groundSlamSound != null)
        {
            audioSource.PlayOneShot(groundSlamSound);
        }
    }
    
    private void PlayBlockSound()
    {
        if (audioSource != null && blockSound != null)
        {
            audioSource.PlayOneShot(blockSound);
        }
    }
    
    protected override void UpdateAnimations()
    {
        base.UpdateAnimations();
        
        if (animator != null)
        {
            animator.SetBool("IsBlocking", isBlocking);
            animator.SetBool("InDefensiveMode", inDefensiveMode);
            animator.SetFloat("HealthPercent", currentHealth / maxHealth);
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Regenerace v defensive módu
        if (inDefensiveMode && currentHealth < maxHealth)
        {
            Heal(regenerationRate * Time.deltaTime);
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Stomp range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stompRange);
        
        // Ground slam range
        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
        Gizmos.DrawWireSphere(transform.position, groundSlamRange);
        
        // Taunt range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, tauntRange);
        
        // Defensive position
        Gizmos.color = Color.blue;
        Vector3 defPos = Application.isPlaying ? defensivePosition : transform.position;
        Gizmos.DrawWireSphere(defPos, 3f);
    }
}