using UnityEngine;

/// <summary>
/// Pickup komponent pro artefakty ve světě
/// Přidává artefakt jako item do inventáře místo okamžité aktivace
/// </summary>
public class ArtifactPickup : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private ItemData itemData; // Item který obsahuje ArtifactData
    
    [Header("Pickup Settings")]
    [SerializeField] private float pickupRadius = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private bool autoPickup = true;
    [SerializeField] private KeyCode manualPickupKey = KeyCode.E;
    
    [Header("Visual Effects")]
    [SerializeField] private bool rotateArtifact = true;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private bool floatArtifact = true;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatSpeed = 2f;
    
    [Header("Particle Effects")]
    [SerializeField] private GameObject glowParticles;
    private new ParticleSystem particleSystem;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    private Transform playerTransform;
    private bool isPlayerNearby = false;
    private bool hasBeenPickedUp = false;
    private Vector3 startPosition;
    private GameObject artifactModel;
    
    void Start()
    {
        Initialize();
    }
    
    void Initialize()
    {
        startPosition = transform.position;
        
        // NIKDY nevytvářet nový model - při dropování se už objekt vytvoří z prefabu
        // Najít existující model v children
        if (itemData != null)
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    artifactModel = child.gameObject;
                    Debug.Log($"ArtifactPickup.Initialize(): Použit existující model '{child.name}'");
                    break;
                }
            }
            
            if (artifactModel == null)
            {
                Debug.LogWarning($"ArtifactPickup.Initialize(): Žádný model nenalezen v children!");
            }
        }
        
        // Vytvořit glow efekt pokud neexistuje
        if (glowParticles == null)
        {
            CreateDefaultGlowEffect();
        }
        
        // Audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.5f;
        }
        
        Debug.Log($"ArtifactPickup: Inicializován {itemData?.itemName}");
    }
    
    void Update()
    {
        if (hasBeenPickedUp) return;
        
        // Rotace a levitace
        if (rotateArtifact && artifactModel != null)
        {
            artifactModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        if (floatArtifact)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
        
        // Detekce hráče
        CheckForPlayer();
        
        // Manuální pickup
        if (!autoPickup && isPlayerNearby && Input.GetKeyDown(manualPickupKey))
        {
            PickupArtifact();
        }
    }
    
    void CheckForPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRadius, playerLayer);
        
        bool wasPlayerNearby = isPlayerNearby;
        isPlayerNearby = false;
        
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                isPlayerNearby = true;
                playerTransform = col.transform;
                
                if (autoPickup)
                {
                    PickupArtifact();
                }
                break;
            }
        }
        
        // UI prompt pro manuální pickup
        if (!wasPlayerNearby && isPlayerNearby && !autoPickup)
        {
            // TODO: Zobrazit UI prompt "Stiskni E pro sebrání"
        }
        else if (wasPlayerNearby && !isPlayerNearby)
        {
            // TODO: Skrýt UI prompt
        }
    }
    
    void PickupArtifact()
    {
        if (hasBeenPickedUp || itemData == null) return;
        
        // Přidat item do inventáře místo přímé aktivace
        if (InventorySystem.Instance != null)
        {
            bool added = InventorySystem.Instance.AddItem(itemData, 1);
            
            if (added)
            {
                hasBeenPickedUp = true;
                
                // Efekty
                PlayPickupEffects();
                
                // Zničit pickup
                Destroy(gameObject, 0.5f);
                
                Debug.Log($"ArtifactPickup: Hráč sebral {itemData.itemName} do inventáře!");
            }
            else
            {
                Debug.Log($"ArtifactPickup: Inventář je plný!");
                // TODO: Zobrazit UI message "Inventář je plný"
            }
        }
        else
        {
            Debug.LogError("ArtifactPickup: InventorySystem nebyl nalezen!");
        }
    }
    
    void PlayPickupEffects()
    {
        // Zvuk
        if (itemData.artifactData != null && itemData.artifactData.pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(itemData.artifactData.pickupSound);
        }
        
        // Particle burst
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
        
        // Skrýt celý objekt ihned (nejen model)
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = false;
        }
        
        // Deaktivovat collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }
    
    void CreateDefaultGlowEffect()
    {
        GameObject glowObj = new GameObject("GlowEffect");
        glowObj.transform.parent = transform;
        glowObj.transform.localPosition = Vector3.zero;
        
        particleSystem = glowObj.AddComponent<ParticleSystem>();
        
        var main = particleSystem.main;
        main.startLifetime = 2f;
        main.startSpeed = 0.5f;
        main.startSize = 0.3f;
        main.maxParticles = 30;
        main.loop = true;
        
        Color glowColor = (itemData != null && itemData.artifactData != null) ? itemData.artifactData.auraColor : new Color(1f, 0.8f, 0.2f, 0.8f);
        main.startColor = glowColor;
        
        var emission = particleSystem.emission;
        emission.rateOverTime = 15f;
        
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
    }
    
    void OnDrawGizmosSelected()
    {
        // Vizualizace pickup radiusu
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
    
    // Public metody pro nastavení
    public void SetItemData(ItemData data)
    {
        itemData = data;
    }
    
    public ItemData GetItemData()
    {
        return itemData;
    }
}
