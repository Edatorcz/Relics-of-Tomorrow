using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    
    [Header("3D Model")]
    public GameObject worldModelPrefab; // 3D model pro drop/pickup
    
    [Header("Stack Settings")]
    public int maxStackSize = 64;
    public bool isStackable = true;
    
    [Header("Item Type")]
    public ItemType itemType;
    
    public enum ItemType
    {
        Material,
        Weapon,
        Tool,
        Consumable,
        Quest
    }
}
