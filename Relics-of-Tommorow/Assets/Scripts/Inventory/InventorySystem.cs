using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }
    
    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 36; // Jako Minecraft: 27 hlavní + 9 hotbar
    
    private List<InventorySlot> slots;
    
    public System.Action OnInventoryChanged;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeInventory();
    }
    
    private void InitializeInventory()
    {
        slots = new List<InventorySlot>();
        for (int i = 0; i < inventorySize; i++)
        {
            slots.Add(new InventorySlot());
        }
    }
    
    public bool AddItem(ItemData item, int quantity = 1)
    {
        if (item == null) return false;
        
        int remainingQuantity = quantity;
        
        // Nejdřív zkus přidat do existujících stacků
        if (item.isStackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty() && slots[i].item == item)
                {
                    remainingQuantity = slots[i].AddItem(item, remainingQuantity);
                    if (remainingQuantity <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }
        
        // Pak zkus přidat do prázdných slotů
        while (remainingQuantity > 0)
        {
            int emptySlotIndex = FindEmptySlot();
            if (emptySlotIndex == -1)
            {
                Debug.Log("Inventář je plný!");
                OnInventoryChanged?.Invoke();
                return false; // Inventář plný
            }
            
            remainingQuantity = slots[emptySlotIndex].AddItem(item, remainingQuantity);
        }
        
        OnInventoryChanged?.Invoke();
        return true;
    }
    
    public bool RemoveItem(ItemData item, int quantity = 1)
    {
        int remainingToRemove = quantity;
        
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty() && slots[i].item == item)
            {
                int amountToRemove = Mathf.Min(remainingToRemove, slots[i].quantity);
                slots[i].RemoveItem(amountToRemove);
                remainingToRemove -= amountToRemove;
                
                if (remainingToRemove <= 0)
                {
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }
        
        OnInventoryChanged?.Invoke();
        return remainingToRemove <= 0;
    }
    
    public bool HasItem(ItemData item, int quantity = 1)
    {
        int totalCount = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.item == item)
            {
                totalCount += slot.quantity;
            }
        }
        return totalCount >= quantity;
    }
    
    public int GetItemCount(ItemData item)
    {
        int count = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.item == item)
            {
                count += slot.quantity;
            }
        }
        return count;
    }
    
    private int FindEmptySlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty())
                return i;
        }
        return -1;
    }
    
    public InventorySlot GetSlot(int index)
    {
        if (index >= 0 && index < slots.Count)
            return slots[index];
        return null;
    }
    
    public void SwapSlots(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= slots.Count || indexB < 0 || indexB >= slots.Count)
            return;
        
        var temp = slots[indexA];
        slots[indexA] = slots[indexB];
        slots[indexB] = temp;
        
        OnInventoryChanged?.Invoke();
    }
    
    public int GetInventorySize()
    {
        return inventorySize;
    }
    
    public void ClearInventory()
    {
        foreach (var slot in slots)
        {
            slot.Clear();
        }
        OnInventoryChanged?.Invoke();
    }
    
    public List<InventorySlot> GetAllSlots()
    {
        return slots;
    }
}
