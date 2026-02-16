using UnityEngine;

/// <summary>
/// Laserový projektil pro futuristické nepřátele
/// Rychlý, přímočarý, bez gravitace
/// </summary>
public class LaserProjectile : EnemyProjectile
{
    [Header("Laser Physics")]
    [SerializeField] private float laserSpeed = 30f;
    [SerializeField] private Color laserColor = Color.cyan;
    [SerializeField] private float glowIntensity = 2f;
    
    [Header("Visual Effects")]
    [SerializeField] private TrailRenderer laserTrail;
    [SerializeField] private Light laserLight;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Laser nemá gravitaci
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        
        // Setup trail
        if (laserTrail == null)
        {
            laserTrail = gameObject.AddComponent<TrailRenderer>();
            laserTrail.time = 0.3f;
            laserTrail.startWidth = 0.1f;
            laserTrail.endWidth = 0.02f;
            laserTrail.material = new Material(Shader.Find("Sprites/Default"));
            laserTrail.startColor = laserColor;
            laserTrail.endColor = new Color(laserColor.r, laserColor.g, laserColor.b, 0f);
        }
        
        // Setup light
        if (laserLight == null)
        {
            GameObject lightObj = new GameObject("LaserLight");
            lightObj.transform.parent = transform;
            lightObj.transform.localPosition = Vector3.zero;
            
            laserLight = lightObj.AddComponent<Light>();
            laserLight.type = LightType.Point;
            laserLight.color = laserColor;
            laserLight.range = 3f;
            laserLight.intensity = glowIntensity;
            laserLight.shadows = LightShadows.None;
        }
        
        // Setup mesh renderer barvy
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.SetColor("_EmissionColor", laserColor * glowIntensity);
            renderer.material.EnableKeyword("_EMISSION");
        }
    }
    
    void Update()
    {
        // Pulzující světlo
        if (laserLight != null)
        {
            laserLight.intensity = glowIntensity + Mathf.Sin(Time.time * 20f) * 0.5f;
        }
    }
    
    /// <summary>
    /// Nastavit rychlost laseru
    /// </summary>
    public new void SetSpeed(float speed)
    {
        laserSpeed = speed;
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * laserSpeed;
        }
    }
    
    /// <summary>
    /// Vystřelit laser směrem
    /// </summary>
    public void Launch(Vector3 direction)
    {
        transform.rotation = Quaternion.LookRotation(direction);
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * laserSpeed;
        }
    }
}
