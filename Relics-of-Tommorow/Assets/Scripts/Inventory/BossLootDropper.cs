using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BossLootDropper : MonoBehaviour
{
    [Header("Boss Loot Settings")]
    [SerializeField] private List<ItemData> bossLootTable = new List<ItemData>();
    [SerializeField] private GameObject itemPickupPrefab;
    
    [Header("Drop Settings")]
    [SerializeField] private int guaranteedDrops = 1; // Kolik itemů určitě spadne
    [SerializeField] private int maxBonusDrops = 2; // Kolik bonusových může spadnout
    [SerializeField] private float dropRadius = 3f;
    [SerializeField] private float dropHeight = 2f;
    [SerializeField] private float dropForce = 5f;
    
    /// <summary>
    /// Dropne boss loot na dané pozici
    /// </summary>
    public void DropBossLoot(Vector3 dropPosition)
    {
        // Najdi všechny boss loot itemy
        List<ItemData> validBossLoot = GetBossLootItems();
        
        if (validBossLoot.Count == 0)
        {
            Debug.LogWarning("BossLootDropper: Žádný boss loot není dostupný!");
            return;
        }
        
        // Urči kolik itemů se má dropnout
        int totalDrops = guaranteedDrops + Random.Range(0, maxBonusDrops + 1);
        totalDrops = Mathf.Min(totalDrops, validBossLoot.Count); // Nepřekročit počet dostupných itemů
        
        // Dropni itemy
        for (int i = 0; i < totalDrops; i++)
        {
            ItemData selectedItem = SelectRandomBossLoot(validBossLoot);
            
            if (selectedItem != null)
            {
                DropItem(selectedItem, dropPosition);
                
                // Pokud nechceš opakovat stejný item, odeber ho ze seznamu
                // validBossLoot.Remove(selectedItem);
            }
        }
    }
    
    private List<ItemData> GetBossLootItems()
    {
        List<ItemData> bossItems = new List<ItemData>();
        
        // Projdi všechny itemy v boss loot table
        foreach (ItemData item in bossLootTable)
        {
            if (item == null) continue;
            
            // Přidej všechny itemy co mají isBossLoot = true
            if (item.itemType == ItemData.ItemType.Weapon && item.weaponStats != null && item.weaponStats.isBossLoot)
            {
                bossItems.Add(item);
            }
        }
        
        return bossItems;
    }
    
    private ItemData SelectRandomBossLoot(List<ItemData> items)
    {
        if (items.Count == 0) return null;
        
        // Weighted selection podle spawn chance
        List<WeightedItem> weightedItems = new List<WeightedItem>();
        
        foreach (ItemData item in items)
        {
            if (item.itemType == ItemData.ItemType.Weapon)
            {
                float weight = item.weaponStats.spawnChance;
                if (weight > 0)
                {
                    weightedItems.Add(new WeightedItem { item = item, weight = weight });
                }
            }
        }
        
        if (weightedItems.Count == 0) return items[Random.Range(0, items.Count)];
        
        // Vypočítej celkovou váhu
        float totalWeight = weightedItems.Sum(x => x.weight);
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
        
        return weightedItems[weightedItems.Count - 1].item;
    }
    
    private void DropItem(ItemData item, Vector3 centerPosition)
    {
        // Najdi náhodnou pozici v radiusu
        Vector2 randomCircle = Random.insideUnitCircle * dropRadius;
        Vector3 spawnPosition = centerPosition + new Vector3(randomCircle.x, dropHeight, randomCircle.y);
        
        GameObject droppedItem = null;
        
        if (itemPickupPrefab != null)
        {
            // Použij prefab
            droppedItem = Instantiate(itemPickupPrefab, spawnPosition, Quaternion.identity);
            
            ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.SetItemData(item, 1);
            }
        }
        else if (item.worldModelPrefab != null)
        {
            // Použij world model
            droppedItem = Instantiate(item.worldModelPrefab, spawnPosition, Quaternion.identity);
            
            ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();
            if (pickup == null)
            {
                pickup = droppedItem.AddComponent<ItemPickup>();
            }
            pickup.SetItemData(item, 1);
        }
        
        // Přidej physics force pro efekt
        if (droppedItem != null)
        {
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb == null) rb = droppedItem.AddComponent<Rigidbody>();
            
            // Náhodný směr výbuchu
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(-1f, 1f)
            ).normalized;
            
            rb.AddForce(randomDirection * dropForce, ForceMode.Impulse);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, dropRadius);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, "BOSS LOOT DROP ZONE");
        #endif
    }
    
    // Helper class
    private class WeightedItem
    {
        public ItemData item;
        public float weight;
    }
}
