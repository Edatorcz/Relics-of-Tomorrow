using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;
    
    public InventorySlot()
    {
        item = null;
        quantity = 0;
    }
    
    public InventorySlot(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
    
    public bool IsEmpty()
    {
        return item == null || quantity <= 0;
    }
    
    public bool CanAddItem(ItemData itemToAdd)
    {
        if (IsEmpty()) return true;
        if (item == itemToAdd && item.isStackable && quantity < item.maxStackSize)
            return true;
        return false;
    }
    
    public int AddItem(ItemData itemToAdd, int amount)
    {
        if (IsEmpty())
        {
            item = itemToAdd;
            quantity = Mathf.Min(amount, itemToAdd.maxStackSize);
            return amount - quantity;
        }
        
        if (item == itemToAdd && item.isStackable)
        {
            int spaceLeft = item.maxStackSize - quantity;
            int amountToAdd = Mathf.Min(amount, spaceLeft);
            quantity += amountToAdd;
            return amount - amountToAdd;
        }
        
        return amount;
    }
    
    public void RemoveItem(int amount)
    {
        quantity -= amount;
        if (quantity <= 0)
        {
            Clear();
        }
    }
    
    public void Clear()
    {
        item = null;
        quantity = 0;
    }
}
