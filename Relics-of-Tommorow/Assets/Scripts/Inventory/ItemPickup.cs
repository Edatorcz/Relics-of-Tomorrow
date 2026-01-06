using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;
    
    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private KeyCode pickupKey = KeyCode.F;
    
    [Header("Visual")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;
    
    private Transform playerTransform;
    private Vector3 startPosition;
    private bool canPickup = false;
    private Light itemLight;
    private ParticleSystem particles;
    
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        startPosition = transform.position;
        
        // Přidat světlo
        CreateItemLight();
        
        // Přidat particle efekt
        CreateParticles();
    }
    
    private void CreateItemLight()
    {
        GameObject lightObj = new GameObject("ItemLight");
        lightObj.transform.parent = transform;
        lightObj.transform.localPosition = Vector3.zero;
        
        itemLight = lightObj.AddComponent<Light>();
        itemLight.type = LightType.Point;
        itemLight.color = new Color(1f, 0.9f, 0.4f); // Zlatá/žlutá barva
        itemLight.range = 5f;
        itemLight.intensity = 2f;
        itemLight.shadows = LightShadows.None;
    }
    
    private void CreateParticles()
    {
        GameObject particleObj = new GameObject("ItemParticles");
        particleObj.transform.parent = transform;
        particleObj.transform.localPosition = Vector3.zero;
        
        particles = particleObj.AddComponent<ParticleSystem>();
        
        // Hlavní modul
        var main = particles.main;
        main.startLifetime = 1.5f;
        main.startSpeed = 0.5f;
        main.startSize = 0.03f; // Malinké částice
        main.startColor = new Color(1f, 0.9f, 0.4f, 0.8f); // Zlatá barva
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Emise
        var emission = particles.emission;
        emission.rateOverTime = 20f;
        
        // Shape - koule kolem itemu
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        // Velikost přes čas - zmenšovat
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Barva přes čas - fade out
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f),
                new GradientColorKey(new Color(1f, 1f, 1f), 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
        
        // Renderer
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }
    
    void Update()
    {
        // Rotace a bobování
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Pulzující světlo
        if (itemLight != null)
        {
            itemLight.intensity = 2f + Mathf.Sin(Time.time * 3f) * 0.5f;
        }
        
        // Kontrola vzdálenosti od hráče
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            canPickup = distance <= pickupRange;
            
            if (canPickup && Input.GetKeyDown(pickupKey))
            {
                Pickup();
            }
        }
    }
    
    private void Pickup()
    {
        if (InventorySystem.Instance != null)
        {
            bool success = InventorySystem.Instance.AddItem(itemData, quantity);
            if (success)
            {
                Debug.Log($"Sebral jsi {quantity}x {itemData.itemName}");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventář je plný!");
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
    
    // Metoda pro dynamické nastavení ItemData (pro drop systém)
    public void SetItemData(ItemData data, int amount)
    {
        itemData = data;
        quantity = amount;
        
        // Aktualizuj vizuál podle ikony itemu
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        if (itemData == null) return;
        
        // Pokud má objekt SpriteRenderer, nastav sprite podle ikony
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && itemData.icon != null)
        {
            spriteRenderer.sprite = itemData.icon;
        }
        
        // Pokud má objekt child s názvem "ItemIcon", aktualizuj ho
        Transform iconTransform = transform.Find("ItemIcon");
        if (iconTransform != null)
        {
            SpriteRenderer iconRenderer = iconTransform.GetComponent<SpriteRenderer>();
            if (iconRenderer != null && itemData.icon != null)
            {
                iconRenderer.sprite = itemData.icon;
            }
            
            // Pro 3D mesh můžeš přidat MeshRenderer logiku
            MeshRenderer meshRenderer = iconTransform.GetComponent<MeshRenderer>();
            if (meshRenderer != null && itemData.icon != null)
            {
                // Vytvoř material s texturou z ikony
                Material mat = new Material(Shader.Find("Unlit/Transparent"));
                mat.mainTexture = itemData.icon.texture;
                meshRenderer.material = mat;
            }
        }
    }
}
