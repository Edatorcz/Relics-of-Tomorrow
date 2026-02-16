using UnityEngine;

/// <summary>
/// Script pro projektily vystřelené nepřáteli
/// Automaticky se zničí po určitém čase nebo při kolizi
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private bool destroyOnHit = true;
    [SerializeField] private LayerMask hitLayers = -1;
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private TrailRenderer trail;
    
    private EnemyBase owner;
    protected Rigidbody rb;
    private bool hasHit = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Automatické zničení po určitém čase
        Destroy(gameObject, lifetime);
        
        // Získání trail renderer pokud existuje
        if (trail == null)
        {
            trail = GetComponent<TrailRenderer>();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }
    
    private void HandleHit(Collider hitCollider)
    {
        if (hasHit) return;
        
        // Kontrola zda je objekt v hit layers
        if ((hitLayers.value & (1 << hitCollider.gameObject.layer)) == 0)
        {
            return;
        }
        
        // Ignorovat vlastníka projektilu
        if (owner != null && hitCollider.transform == owner.transform)
        {
            return;
        }
        
        hasHit = true;
        
        // Pokusit se způsobit damage hráči
        if (hitCollider.CompareTag("Player"))
        {
            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Projectile hit player for {damage} damage");
            }
        }
        
        // Spawn hit effect
        SpawnHitEffect(hitCollider.ClosestPoint(transform.position));
        
        // Zastavit projektil
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Zničit projektil
        if (destroyOnHit)
        {
            DestroyProjectile();
        }
    }
    
    private void SpawnHitEffect(Vector3 hitPoint)
    {
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, hitPoint, Quaternion.identity);
            Destroy(effect, 2f); // Zničit efekt po 2 sekundách
        }
    }
    
    private void DestroyProjectile()
    {
        // Pokud máme trail, počkáme až se dokončí
        if (trail != null)
        {
            trail.transform.SetParent(null);
            Destroy(trail.gameObject, trail.time);
        }
        
        Destroy(gameObject, 0.1f);
    }
    
    // Veřejné metody pro nastavení projektilu
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    public void SetOwner(EnemyBase newOwner)
    {
        owner = newOwner;
    }
    
    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;
        
        // Aktualizovat destroy timer
        CancelInvoke();
        Invoke(nameof(TimeoutDestroy), lifetime);
    }
    
    private void TimeoutDestroy()
    {
        if (!hasHit)
        {
            DestroyProjectile();
        }
    }
    
    public void SetSpeed(float speed)
    {
        if (rb != null)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * speed;
        }
    }
}
