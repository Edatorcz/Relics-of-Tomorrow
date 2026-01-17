using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LootSpawner : MonoBehaviour
{
    [Header("Loot Settings")]
    [SerializeField] private List<ItemData> availableItems = new List<ItemData>();
    [SerializeField] private GameObject itemPickupPrefab; // Prefab s ItemPickup komponentou
    
    [Header("Spawn Settings")]
    [SerializeField] private int minItemsToSpawn = 1;
    [SerializeField] private int maxItemsToSpawn = 3;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float spawnHeight = 1f;
    
    [Header("Room Type")]
    [SerializeField] private bool isBossRoom = false;
    
    private void Start()
    {
        SpawnLoot();
    }
    
    public void SpawnLoot()
    {
        if (availableItems == null || availableItems.Count == 0)
        {
            Debug.LogWarning("LootSpawner: Žádné dostupné itemy pro spawn!");
            return;
        }
        
        // Filtruj itemy podle typu místnosti
        List<ItemData> spawnableItems = GetSpawnableItems();
        
        if (spawnableItems.Count == 0)
        {
            Debug.Log("LootSpawner: Žádné spawnableItems po filtraci.");
            return;
        }
        
        // Urči kolik itemů se má spawnout
        int itemsToSpawn = Random.Range(minItemsToSpawn, maxItemsToSpawn + 1);
        
        for (int i = 0; i < itemsToSpawn; i++)
        {
            ItemData selectedItem = SelectRandomItemByWeight(spawnableItems);
            
            if (selectedItem != null)
            {
                SpawnItem(selectedItem);
            }
        }
    }
    
    private List<ItemData> GetSpawnableItems()
    {
        List<ItemData> filtered = new List<ItemData>();
        
        foreach (ItemData item in availableItems)
        {
            if (item == null) continue;
            
            // Kontrola boss loot - platí pro všechny typy itemů
            if (item.itemType == ItemData.ItemType.Weapon && item.weaponStats != null && item.weaponStats.isBossLoot)
            {
                // Boss loot se NESPAWNUJE v normálních roomkách
                continue;
            }
            
            // Všechny ostatní itemy můžou spawnout
            filtered.Add(item);
        }
        
        return filtered;
    }
    
    private ItemData SelectRandomItemByWeight(List<ItemData> items)
    {
        if (items.Count == 0) return null;
        
        // Vytvoř weighted list podle spawn chance
        List<WeightedItem> weightedItems = new List<WeightedItem>();
        
        foreach (ItemData item in items)
        {
            float weight = 50f; // Default weight pro non-weapon itemy
            
            // Pro zbraně použij spawn chance
            if (item.itemType == ItemData.ItemType.Weapon)
            {
                weight = item.weaponStats.spawnChance;
            }
            
            // Pokud je weight 0, přeskoč
            if (weight <= 0) continue;
            
            weightedItems.Add(new WeightedItem { item = item, weight = weight });
        }
        
        if (weightedItems.Count == 0) return null;
        
        // Vypočítej celkovou váhu
        float totalWeight = weightedItems.Sum(x => x.weight);
        
        // Vyber náhodně podle váhy
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var weightedItem in weightedItems)
        {
            currentWeight += weightedItem.weight;
            if (randomValue <= currentWeight)
            {
                return weightedItem.item;
            }
        }
        
        // Fallback
        return weightedItems[weightedItems.Count - 1].item;
    }
    
    private void SpawnItem(ItemData item)
    {
        // Najdi náhodnou pozici v radiusu
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, spawnHeight, randomCircle.y);
        
        // Raycast dolů pro najití země
        RaycastHit hit;
        if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out hit, 20f))
        {
            spawnPosition = hit.point + Vector3.up * 0.5f;
        }
        
        // Spawn item
        GameObject spawnedItem = null;
        
        if (itemPickupPrefab != null)
        {
            // Použij prefab
            spawnedItem = Instantiate(itemPickupPrefab, spawnPosition, Quaternion.identity);
            
            ItemPickup pickup = spawnedItem.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.SetItemData(item, 1);
            }
        }
        else if (item.worldModelPrefab != null)
        {
            // Použij world model z ItemData
            spawnedItem = Instantiate(item.worldModelPrefab, spawnPosition, Quaternion.identity);
            
            // Přidej ItemPickup komponentu, pokud ji nemá
            ItemPickup pickup = spawnedItem.GetComponent<ItemPickup>();
            if (pickup == null)
            {
                pickup = spawnedItem.AddComponent<ItemPickup>();
            }
            pickup.SetItemData(item, 1);
        }
        else
        {
            Debug.LogWarning($"LootSpawner: Nelze spawnout item {item.itemName} - chybí prefab!");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isBossRoom ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        // Nakresli text pro typ roomky
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
            isBossRoom ? "BOSS ROOM" : "Normal Room");
        #endif
    }
    
    // Helper třída pro weighted selection
    private class WeightedItem
    {
        public ItemData item;
        public float weight;
    }
}
