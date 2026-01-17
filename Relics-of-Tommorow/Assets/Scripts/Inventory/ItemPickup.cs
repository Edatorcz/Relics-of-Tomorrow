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
        // Najdi hráče přes hlavní kameru (kamera je child hráče)
        if (Camera.main != null)
        {
            // Kamera je child hráče, takže parent kamery je hráč
            Transform cameraParent = Camera.main.transform.parent;
            if (cameraParent != null)
            {
                playerTransform = cameraParent;
                Debug.Log($"ItemPickup: Hráč nalezen přes kameru pro {gameObject.name}");
            }
            else
            {
                // Pokud kamera nemá parenta, použij přímo kameru
                playerTransform = Camera.main.transform;
                Debug.Log($"ItemPickup: Použita přímo kamera (nemá parenta) pro {gameObject.name}");
            }
        }
        
        // Fallback: zkus najít podle tagu
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log($"ItemPickup: Hráč nalezen podle tagu pro {gameObject.name}");
            }
            else
            {
                Debug.LogError($"ItemPickup: Hráč NENALEZEN pro {gameObject.name}! Zkontroluj že Camera.main existuje nebo hráč má tag 'Player'");
            }
        }
        
        startPosition = transform.position;
        
        // Debug ItemData
        if (itemData != null)
        {
            Debug.Log($"ItemPickup {gameObject.name}: ItemData = {itemData.itemName}");
        }
        else
        {
            Debug.LogWarning($"ItemPickup {gameObject.name}: ItemData je NULL! Item nepůjde sebrat!");
        }
        
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
        main.startSize = 0.015f; // Malinké částice
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
        // Pokud ještě nemáme hráče, zkus ho najít
        if (playerTransform == null)
        {
            TryFindPlayer();
            if (playerTransform == null)
            {
                return; // Stále nemáme hráče, zkusíme příště
            }
        }
        
        // Ověř že playerTransform stále existuje (nebyl zničen)
        if (playerTransform == null || playerTransform.gameObject == null)
        {
            playerTransform = null; // Reset reference
            return;
        }
        
        // Kontrola jestli jsme equipped (child ItemHolder/Hand)
        if (transform.parent != null && (transform.parent.name == "Hand" || transform.parent.name == "ItemHolder"))
        {
            return; // Jsme equipnutý, neděláme bobování/rotaci
        }
        
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
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        canPickup = distance <= pickupRange;
        
        // Debug: zobraz kdy je item v dosahu
        if (canPickup && Time.frameCount % 60 == 0) // Každou sekundu
        {
            Debug.Log($"<color=green>ItemPickup {gameObject.name}: V DOSAHU! Vzdálenost={distance:F2}m (range={pickupRange}), Stiskni {pickupKey}</color>");
            Debug.Log($"  - ItemData: {(itemData != null ? itemData.itemName : "NULL")}");
            Debug.Log($"  - InventorySystem.Instance: {(InventorySystem.Instance != null ? "OK" : "NULL")}");
        }
        
        if (Input.GetKeyDown(pickupKey))
        {
            Debug.Log($"<color=yellow>F STISKNUTO! canPickup={canPickup}, distance={distance:F2}m</color>");
            if (canPickup)
            {
                Debug.Log($"<color=cyan>Volám Pickup()...</color>");
                Pickup();
            }
        }
    }
    
    private void TryFindPlayer()
    {
        // Nejdřív zkus tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log($"<color=lime>ItemPickup: Hráč NALEZEN podle tagu!</color>");
            return;
        }
        
        // Zkus přes kameru
        if (Camera.main != null)
        {
            Transform cameraParent = Camera.main.transform.parent;
            if (cameraParent != null)
            {
                playerTransform = cameraParent;
                Debug.Log($"<color=lime>ItemPickup: Hráč NALEZEN přes kameru!</color>");
                return;
            }
        }
        
        // Zkus najít PlayerMovement
        PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null)
        {
            playerTransform = pm.transform;
            Debug.Log($"<color=lime>ItemPickup: Hráč NALEZEN přes PlayerMovement!</color>");
            return;
        }
    }
    
    private void Pickup()
    {
        Debug.Log($"ItemPickup.Pickup() voláno pro {gameObject.name}");
        
        if (itemData == null)
        {
            Debug.LogError($"ItemPickup {gameObject.name}: itemData je NULL! Nelze sebrat!");
            return;
        }
        
        if (InventorySystem.Instance == null)
        {
            Debug.LogError($"ItemPickup {gameObject.name}: InventorySystem.Instance je NULL!");
            return;
        }
        
        Debug.Log($"ItemPickup: Pokus o přidání {itemData.itemName} do inventáře");
        bool success = InventorySystem.Instance.AddItem(itemData, quantity);
        
        if (success)
        {
            Debug.Log($"ItemPickup: {itemData.itemName} úspěšně přidán do inventáře!");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning($"ItemPickup: Nepodařilo se přidat {itemData.itemName} - inventář plný?");
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
        
        Debug.Log($"ItemPickup.SetItemData() voláno: {gameObject.name} -> ItemData = {(data != null ? data.itemName : "NULL")}");
        
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
