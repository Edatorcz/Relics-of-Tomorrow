using UnityEngine;

/// <summary>
/// Kamenný projektil pro primitivní nepřátele (StoneAgeApe)
/// Jednoduchý projektil s gravitací a náhodnou rotací
/// </summary>
public class RockProjectile : EnemyProjectile
{
    [Header("Rock Physics")]
    [SerializeField] private float rockMass = 2f;
    [SerializeField] private float spinSpeed = 180f;
    
    private Vector3 randomSpinAxis;
    private new Rigidbody rb;
    
    void Start()
    {
        // Základní inicializace
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Nastavení fyziky kamene
        rb.mass = rockMass;
        rb.useGravity = true;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.05f;
        
        // Náhodná rotace pro realističtější vzhled
        randomSpinAxis = Random.onUnitSphere;
        rb.angularVelocity = randomSpinAxis * spinSpeed * Mathf.Deg2Rad;
    }
    
    void Update()
    {
        // Kámen se točí během letu
        transform.Rotate(randomSpinAxis, spinSpeed * Time.deltaTime);
    }
    
    // Přetížení metody pro specifické chování kamene
    public void SetThrower(EnemyBase thrower)
    {
        SetOwner(thrower);
    }
}
