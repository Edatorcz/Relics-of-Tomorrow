using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class InventorySaveData
{
    [System.Serializable]
    public class SlotData
    {
        public string itemName; // Uložíme jméno itemData
        public int quantity;
        
        public SlotData(string name, int qty)
        {
            itemName = name;
            quantity = qty;
        }
    }
    
    public List<SlotData> slots = new List<SlotData>();
}

public class InventorySaveSystem : MonoBehaviour
{
    private string saveFilePath;
    
    void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "inventory.json");
    }
    
    public void SaveInventory()
    {
        if (InventorySystem.Instance == null) return;
        
        InventorySaveData saveData = new InventorySaveData();
        List<InventorySlot> slots = InventorySystem.Instance.GetAllSlots();
        
        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                saveData.slots.Add(new InventorySaveData.SlotData("", 0));
            }
            else
            {
                saveData.slots.Add(new InventorySaveData.SlotData(slot.item.name, slot.quantity));
            }
        }
        
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
    }
    
    public void LoadInventory()
    {
        if (InventorySystem.Instance == null) return;
        
        if (!File.Exists(saveFilePath))
        {
            return;
        }
        
        string json = File.ReadAllText(saveFilePath);
        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);
        
        if (saveData == null || saveData.slots == null) return;
        
        // Vyčisti inventář
        InventorySystem.Instance.ClearInventory();
        
        // Načti data
        for (int i = 0; i < saveData.slots.Count && i < InventorySystem.Instance.GetInventorySize(); i++)
        {
            InventorySaveData.SlotData slotData = saveData.slots[i];
            
            if (string.IsNullOrEmpty(slotData.itemName) || slotData.quantity <= 0)
                continue;
            
            // Najdi ItemData podle jména
            ItemData item = Resources.Load<ItemData>($"Items/{slotData.itemName}");
            if (item != null)
            {
                InventorySlot slot = InventorySystem.Instance.GetSlot(i);
                if (slot != null)
                {
                    slot.item = item;
                    slot.quantity = slotData.quantity;
                }
            }
            else
            {
                Debug.LogWarning($"Item '{slotData.itemName}' nenalezen v Resources/Items/");
            }
        }
        
        InventorySystem.Instance.OnInventoryChanged?.Invoke();
        Debug.Log("Inventář načten!");
    }
    
    // Auto-save při ukončení aplikace
    void OnApplicationQuit()
    {
        SaveInventory();
    }
}
