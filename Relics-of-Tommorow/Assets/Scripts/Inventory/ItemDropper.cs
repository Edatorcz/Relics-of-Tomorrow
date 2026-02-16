using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    public static ItemDropper Instance { get; private set; }
    
    [Header("Drop Settings")]
    [SerializeField] private GameObject itemPickupPrefab; // Prefab ItemPickup objektu
    [SerializeField] private float spawnHeightOffset = 0.5f; // Výška nad zemí
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void DropItem(ItemData item, int quantity, Vector3 dropPosition, Vector3 dropDirection)
    {
        if (item == null) return;
        
        GameObject prefabToSpawn = null;
        
        // Pokud má item vlastní 3D model, použij ho
        if (item.worldModelPrefab != null)
        {
            prefabToSpawn = item.worldModelPrefab;
        }
        // Jinak použij univerzální pickup prefab
        else if (itemPickupPrefab != null)
        {
            prefabToSpawn = itemPickupPrefab;
        }
        else
        {
            return;
        }
        
        // Vytvoř pickup objekt přímo na pozici (na zemi)
        Vector3 spawnPosition = dropPosition;
        spawnPosition.y += spawnHeightOffset; // Malý offset aby nebyl v zemi
        GameObject droppedItem = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        
        // Nastav ItemData a množství
        ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();
        if (pickup == null)
        {
            // Pokud prefab nemá ItemPickup, přidej ho
            pickup = droppedItem.AddComponent<ItemPickup>();
        }
        pickup.SetItemData(item, quantity);
        
        // Ujisti se, že má správný collider
        EnsureProperCollider(droppedItem);
        
        // Přidej Rigidbody, ale BEZ síly - prostě položit na zem
        Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = droppedItem.AddComponent<Rigidbody>();
        }
        
        // Nastavení aby se choval normálně při pádu na zem
        rb.mass = 1f;
        rb.linearDamping = 1f;
        rb.angularDamping = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // ŽÁDNÁ síla - prostě spadne na zem gravitací
    }
    
    /// <summary>
    /// Zajisti že item má správný collider (pro Rigidbody musí být convex)
    /// </summary>
    private void EnsureProperCollider(GameObject obj)
    {
        // Zkontroluj všechny MeshCollidery
        MeshCollider[] meshColliders = obj.GetComponentsInChildren<MeshCollider>();
        foreach (MeshCollider mc in meshColliders)
        {
            if (!mc.convex)
            {
                mc.convex = true; // Nastav na convex aby fungoval s Rigidbody
            }
        }
        
        // Pokud nemá žádný collider, přidej BoxCollider
        if (obj.GetComponent<Collider>() == null)
        {
            BoxCollider box = obj.AddComponent<BoxCollider>();
            // Automaticky nastaví bounds podle Mesh
        }
    }
    
    public void DropItemFromPlayer(ItemData item, int quantity)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        Vector3 dropPosition = player.transform.position + Vector3.up * 1.5f; // Trochu výš než střed hráče
        Vector3 dropDirection = player.transform.forward;
        
        DropItem(item, quantity, dropPosition, dropDirection);
    }
}
