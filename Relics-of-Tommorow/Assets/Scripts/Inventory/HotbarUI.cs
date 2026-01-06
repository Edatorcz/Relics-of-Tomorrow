using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int hotbarSize = 9; // Spodní řada jako v Minecraftu
    [SerializeField] private int hotbarStartIndex = 27; // První 27 slotů je hlavní inventář
    
    [Header("UI References")]
    [SerializeField] private Transform hotbarParent;
    [SerializeField] private GameObject hotbarSlotPrefab;
    [SerializeField] private Image selectionHighlight;
    
    private InventorySlotUI[] hotbarSlots;
    private int selectedSlotIndex = 0;
    
    void Start()
    {
        CreateHotbarSlots();
        UpdateSelection();
        
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += UpdateHotbar;
        }
    }
    
    void Update()
    {
        HandleHotbarInput();
    }
    
    private void CreateHotbarSlots()
    {
        hotbarSlots = new InventorySlotUI[hotbarSize];
        
        for (int i = 0; i < hotbarSize; i++)
        {
            GameObject slotObj = Instantiate(hotbarSlotPrefab, hotbarParent);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            slotUI.Initialize(hotbarStartIndex + i);
            hotbarSlots[i] = slotUI;
            
            // Přidat číslo slotu (1-9)
            TextMeshProUGUI numberText = slotObj.GetComponentInChildren<TextMeshProUGUI>();
            if (numberText != null && numberText.gameObject.name == "SlotNumber")
            {
                numberText.text = ((i + 1) % 10).ToString(); // 1-9, 0 pro desátý
            }
        }
    }
    
    private void HandleHotbarInput()
    {
        // Číselné klávesy 1-9 pro výběr slotu
        for (int i = 0; i < 9 && i < hotbarSize; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
        
        // Kolečko myši pro přepínání slotů
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                SelectSlot((selectedSlotIndex - 1 + hotbarSize) % hotbarSize);
            }
            else if (scroll < 0)
            {
                SelectSlot((selectedSlotIndex + 1) % hotbarSize);
            }
        }
        
        // Použití vybraného itemu (například levé tlačítko myši)
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            UseSelectedItem();
        }
    }
    
    private void SelectSlot(int index)
    {
        if (index < 0 || index >= hotbarSize) return;
        
        selectedSlotIndex = index;
        UpdateSelection();
    }
    
    private void UpdateSelection()
    {
        if (selectionHighlight != null && hotbarSlots.Length > selectedSlotIndex)
        {
            selectionHighlight.transform.position = hotbarSlots[selectedSlotIndex].transform.position;
        }
    }
    
    private void UseSelectedItem()
    {
        int inventoryIndex = hotbarStartIndex + selectedSlotIndex;
        InventorySlot slot = InventorySystem.Instance.GetSlot(inventoryIndex);
        
        if (slot != null && !slot.IsEmpty())
        {
            // Zde můžeš implementovat logiku používání itemů
            Debug.Log($"Použil jsi: {slot.item.itemName}");
            
            // Například pro consumables:
            if (slot.item.itemType == ItemData.ItemType.Consumable)
            {
                // Použij item a odeber ze stacku
                // slot.RemoveItem(1);
                // InventorySystem.Instance.OnInventoryChanged?.Invoke();
            }
        }
    }
    
    private void UpdateHotbar()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            int inventoryIndex = hotbarStartIndex + i;
            InventorySlot slot = InventorySystem.Instance.GetSlot(inventoryIndex);
            hotbarSlots[i].UpdateSlot(slot);
        }
    }
    
    public ItemData GetSelectedItem()
    {
        int inventoryIndex = hotbarStartIndex + selectedSlotIndex;
        InventorySlot slot = InventorySystem.Instance.GetSlot(inventoryIndex);
        return slot != null && !slot.IsEmpty() ? slot.item : null;
    }
    
    public int GetSelectedSlotIndex()
    {
        return selectedSlotIndex;
    }
    
    void OnDestroy()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= UpdateHotbar;
        }
    }
}
