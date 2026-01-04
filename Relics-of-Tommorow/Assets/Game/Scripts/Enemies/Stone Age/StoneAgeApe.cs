using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Primitivní opice pro kamennou dobu
/// Jednoduchý, hloupější nepřítel s agresivním chováním ale slabšími schopnostmi
/// </summary>
public class StoneAgeApe : EnemyBase
{
    [Header("Ape Behavior")]
    [SerializeField] private float territoryRadius = 3f;
    [SerializeField] private float confusionTime = 1f;
    [SerializeField] private bool isAngered = false;
    [SerializeField] private float angerDuration = 8f;

    [Header("Simple Combat")]
    [SerializeField] private float swipeRange = 1.8f;
    [SerializeField] private bool canThrowRock = true;
    [SerializeField] private float rockThrowRange = 6f;
    [SerializeField] private float rockThrowCooldown = 4f;
    [SerializeField] private GameObject rockPrefab;

    [Header("Primitive Sounds")]
    [SerializeField] private AudioClip[] gruntSounds;
    [SerializeField] private AudioClip angerSound;
    [SerializeField] private AudioSource audioSource;

    private Vector3 homePosition;
    private bool isConfused = false;
    private float confusionTimer;
    private float angerTimer;
    private float lastRockThrowTime;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Initialize()
    {
        base.Initialize();

        maxHealth = 60f;
        currentHealth = maxHealth;
        damage = 15f;
        moveSpeed = 2.5f;
        detectionRange = 15f; // Zvětšeno z 6 na 15
        attackRange = swipeRange;

        homePosition = transform.position;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Vypnout line of sight pro hloupou opici
        useLineOfSight = false;

        ChangeState(EnemyState.Idle);
        
        // Debug informace
        Debug.Log($"{gameObject.name}: StoneAgeApe initialized");
        Debug.Log($"{gameObject.name}: Player reference: {(player != null ? player.name : "NULL")}");
        Debug.Log($"{gameObject.name}: Detection range: {detectionRange}");
        Debug.Log($"{gameObject.name}: NavMeshAgent: {(navMeshAgent != null ? "OK" : "MISSING")}");
        Debug.Log($"{gameObject.name}: UseLineOfSight: {useLineOfSight}");
    }

    protected override void HandleChaseState()
{
    if (player == null) { ChangeState(EnemyState.Idle); return; }

    float distanceToPlayer = Vector3.Distance(transform.position, player.position);

    // Debug pozic každých 30 framů
    if (Time.frameCount % 30 == 0)
    {
        Debug.Log($"{gameObject.name}: CHASE - Targeting player at: {player.position} (Distance: {distanceToPlayer:F1})");
        Debug.Log($"{gameObject.name}: NavMesh destination: {(navMeshAgent.hasPath ? navMeshAgent.destination.ToString() : "No path")}");
    }

    // původní logika
    if (canThrowRock && distanceToPlayer > attackRange && distanceToPlayer <= rockThrowRange &&
        Time.time - lastRockThrowTime >= rockThrowCooldown)
    {
        Debug.Log(">>> THROWING ROCK <<<");
        ThrowRock();
        return;
    }

    if (distanceToPlayer <= attackRange)
    {
        ChangeState(EnemyState.Attacking);
        return;
    }

    // Ujistit se, že jde k aktuální pozici hráče
    if (navMeshAgent != null) 
    {
        navMeshAgent.SetDestination(player.position);
        Debug.Log($"{gameObject.name}: Moving to player at: {player.position}");
    }
}

    protected override void HandleAttackState()
    {
        if (player == null) { ChangeState(EnemyState.Idle); return; }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > swipeRange * 1.3f)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }

        if (navMeshAgent != null) navMeshAgent.ResetPath();
        LookAtPlayer();

        if (Time.time - lastAttackTime >= attackCooldown) PerformApeAttack();
    }

    protected override void HandleStunnedState()
    {
        if (navMeshAgent != null && navMeshAgent.hasPath) navMeshAgent.ResetPath();
        transform.Rotate(0, 180f * Time.deltaTime, 0);
        confusionTimer -= Time.deltaTime;
        if (confusionTimer <= 0f)
        {
            isConfused = false;
            ChangeState(EnemyState.Chasing);
        }
    }

    protected override void HandleIdleState()
    {
        // Debug idle stavu
        if (Time.frameCount % 180 == 0) // Každé 3 sekundy
        {
            Debug.Log($"{gameObject.name}: IDLE STATE - Still waiting...");
        }
        
        // Nepatroluj náhodně - zůstaň v idle, dokud neuvidíš hráče
        if (navMeshAgent != null && navMeshAgent.hasPath) navMeshAgent.ResetPath();
        if (Random.Range(0f, 1f) < 0.001f) PlayGruntSound();
        if (Random.Range(0f, 1f) < 0.002f) transform.Rotate(0, Random.Range(-45f, 45f), 0);
    }

    protected override void HandlePatrolState()
    {
        if (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.5f) WanderAroundHome();
    }

    private void WanderAroundHome()
    {
        Vector3 randomDirection = Random.insideUnitSphere * territoryRadius;
        randomDirection.y = 0;
        Vector3 targetPosition = homePosition + randomDirection;
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, territoryRadius, NavMesh.AllAreas))
            navMeshAgent.SetDestination(hit.position);
    }

    private void LookAtPlayer()
    {
        if (player == null) return;
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void PerformApeAttack()
    {
        Attack();
        PlayGruntSound();
    }

    private void ThrowRock()
    {
        if (rockPrefab == null || player == null) return;

        Vector3 throwPosition = transform.position + Vector3.up * 1.5f;
        Vector3 targetPosition = player.position + Vector3.up * 0.5f;

        GameObject rock = Instantiate(rockPrefab, throwPosition, Quaternion.identity);
        Vector3 direction = (targetPosition - throwPosition).normalized;
        Rigidbody rockRb = rock.GetComponent<Rigidbody>();
        if (rockRb != null)
        {
            float throwForce = 8f;
            rockRb.AddForce(direction * throwForce + Vector3.up * 3f, ForceMode.Impulse);
        }

        RockProjectile rockScript = rock.GetComponent<RockProjectile>();
        if (rockScript != null)
        {
            rockScript.SetDamage(damage * 0.7f);
            rockScript.SetThrower(this);
        }

        lastRockThrowTime = Time.time;
        PlayGruntSound();
    }

    private void BecomeAngered()
    {
        if (isAngered) return;
        isAngered = true;
        angerTimer = angerDuration;
        moveSpeed *= 1.3f;
        attackCooldown *= 0.8f;
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = Color.red;
        PlayAngerSound();
    }

    private void Stumble()
    {
        isConfused = true;
        confusionTimer = confusionTime;
        ChangeState(EnemyState.Stunned);
        PlayGruntSound();
    }

    private void PlayGruntSound()
    {
        if (audioSource != null && gruntSounds.Length > 0)
            audioSource.PlayOneShot(gruntSounds[Random.Range(0, gruntSounds.Length)]);
    }

    private void PlayAngerSound()
    {
        if (audioSource != null && angerSound != null) audioSource.PlayOneShot(angerSound);
    }

    protected override void OnDamageReceived()
    {
        base.OnDamageReceived();
        if (Random.Range(0f, 1f) < 0.3f) Stumble();
        else BecomeAngered();
    }

    protected override void UpdateAnimations()
    {
        base.UpdateAnimations();
        if (animator != null)
        {
            animator.SetBool("IsAngered", isAngered);
            animator.SetBool("IsConfused", isConfused);
        }
    }

    protected override void Update()
    {
        base.Update();
        if (isAngered)
        {
            angerTimer -= Time.deltaTime;
            if (angerTimer <= 0f) CalmDown();
        }
    }

    private void CalmDown()
    {
        isAngered = false;
        moveSpeed = 2.5f;
        attackCooldown = 1.5f;
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = Color.white;
    }

    protected override void ChangeState(EnemyState newState)
    {
        if (Random.Range(0f, 1f) < 0.1f && newState != EnemyState.Dead)
        {
            base.ChangeState(EnemyState.Stunned);
            isConfused = true;
            confusionTimer = confusionTime * 0.5f;
        }
        else base.ChangeState(newState);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = new Color(0.6f, 0.3f, 0.1f);
        Vector3 center = Application.isPlaying ? homePosition : transform.position;
        Gizmos.DrawWireSphere(center, territoryRadius);
        if (canThrowRock)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, rockThrowRange);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, swipeRange);
    }
}