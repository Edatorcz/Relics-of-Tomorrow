using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    public static ItemDropper Instance { get; private set; }
    
    [Header("Drop Settings")]
    [SerializeField] private GameObject itemPickupPrefab; // Prefab ItemPickup objektu
    [SerializeField] private float dropForce = 5f;
    [SerializeField] private float dropUpwardForce = 2f;
    [SerializeField] private float dropDistance = 2f;
    
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
            Debug.LogWarning($"Item '{item.itemName}' nemá přiřazený worldModelPrefab ani není nastaven itemPickupPrefab!");
            return;
        }
        
        // Vytvoř pickup objekt na pozici
        Vector3 spawnPosition = dropPosition + dropDirection.normalized * dropDistance;
        GameObject droppedItem = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        
        // Nastav ItemData a množství
        ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();
        if (pickup == null)
        {
            // Pokud prefab nemá ItemPickup, přidej ho
            pickup = droppedItem.AddComponent<ItemPickup>();
        }
        pickup.SetItemData(item, quantity);
        
        // Přidej fyziku - vyhoď item směrem před hráče
        Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = droppedItem.AddComponent<Rigidbody>();
        }
        
        Vector3 throwDirection = dropDirection.normalized + Vector3.up * 0.5f;
        rb.AddForce(throwDirection * dropForce + Vector3.up * dropUpwardForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
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
