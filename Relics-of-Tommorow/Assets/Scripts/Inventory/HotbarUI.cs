using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int hotbarSize = 9; // Spodní řada jako v Minecraftu
    [SerializeField] private int hotbarStartIndex = 0; // Hotbar začíná od slotu 0 (sloty 0-8)
    
    [Header("UI References")]
    [SerializeField] private Transform hotbarParent;
    [SerializeField] private GameObject hotbarSlotPrefab;
    [SerializeField] private Image selectionHighlight;
    
    private InventorySlotUI[] hotbarSlots;
    private int selectedSlotIndex = 0;
    
    void Start()
    {
        // FORCE hotbarStartIndex na 0 (přepíše hodnotu z Inspectoru)
        hotbarStartIndex = 0;
        
        Debug.Log("HotbarUI: Start() - Inicializace hotbaru");
        CreateHotbarSlots();
        UpdateSelection();
        
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += UpdateHotbar;
            Debug.Log("HotbarUI: Připojen k InventorySystem");
        }
        else
        {
            Debug.LogError("HotbarUI: InventorySystem.Instance je NULL!");
        }
    }
    
    void Update()
    {
        HandleHotbarInput();
    }
    
    private void CreateHotbarSlots()
    {
        if (hotbarSlotPrefab == null)
        {
            return;
        }
        
        if (hotbarParent == null)
        {
            return;
        }
        
        hotbarSlots = new InventorySlotUI[hotbarSize];
        
        for (int i = 0; i < hotbarSize; i++)
        {
            GameObject slotObj = Instantiate(hotbarSlotPrefab, hotbarParent);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            
            if (slotUI == null)
            {
                slotUI = slotObj.AddComponent<InventorySlotUI>();
            }
            
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
                Debug.Log($"HotbarUI: Stisknuta klávesa {i + 1}");
                SelectSlot(i);
            }
        }
        
        // Kolečko myši pro přepínání slotů
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Debug.Log($"HotbarUI: Mouse scroll: {scroll}");
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
        Debug.Log($"HotbarUI: Vybrán slot {index + 1}");
        UpdateSelection();
        UpdateEquippedItem();
    }
    
    private void UpdateSelection()
    {
        if (hotbarSlots == null || hotbarSlots.Length == 0)
        {
            return;
        }
        
        if (selectionHighlight != null && selectedSlotIndex < hotbarSlots.Length && hotbarSlots[selectedSlotIndex] != null)
        {
            selectionHighlight.transform.position = hotbarSlots[selectedSlotIndex].transform.position;
        }
    }
    
    private void UseSelectedItem()
    {
        if (InventorySystem.Instance == null)
        {
            return;
        }
        
        int inventoryIndex = hotbarStartIndex + selectedSlotIndex;
        InventorySlot slot = InventorySystem.Instance.GetSlot(inventoryIndex);
        
        if (slot != null && !slot.IsEmpty())
        {
            // Zde můžeš implementovat logiku používání itemů
            
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
        
        // Aktualizuj item v ruce když se inventář změní
        UpdateEquippedItem();
    }
    
    public ItemData GetSelectedItem()
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogError("HotbarUI.GetSelectedItem(): InventorySystem.Instance je NULL!");
            return null;
        }
        
        int inventoryIndex = hotbarStartIndex + selectedSlotIndex;
        Debug.Log($"HotbarUI.GetSelectedItem(): selectedSlotIndex={selectedSlotIndex}, hotbarStartIndex={hotbarStartIndex}, inventoryIndex={inventoryIndex}");
        
        InventorySlot slot = InventorySystem.Instance.GetSlot(inventoryIndex);
        
        if (slot == null)
        {
            Debug.LogWarning($"HotbarUI.GetSelectedItem(): Slot {inventoryIndex} je NULL!");
            return null;
        }
        
        if (slot.IsEmpty())
        {
            Debug.Log($"HotbarUI.GetSelectedItem(): Slot {inventoryIndex} je prázdný");
            return null;
        }
        
        Debug.Log($"HotbarUI.GetSelectedItem(): Vrací item '{slot.item.itemName}' ze slotu {inventoryIndex}");
        return slot.item;
    }
    
    public int GetSelectedSlotIndex()
    {
        return selectedSlotIndex;
    }
    
    /// <summary>
    /// Aktualizuje item držený v ruce podle vybraného slotu
    /// </summary>
    private void UpdateEquippedItem()
    {
        if (ItemHolder.Instance == null)
        {
            // Pokusi se najít ItemHolder ve scéně
            ItemHolder holder = FindFirstObjectByType<ItemHolder>();
            if (holder == null)
            {
                // Vytvoř ItemHolder automaticky na hráči
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    holder = player.AddComponent<ItemHolder>();
                    Debug.Log("HotbarUI: Automaticky vytvořen ItemHolder na hráči");
                }
                else
                {
                    Debug.LogWarning("HotbarUI: Nelze najít hráče s tagem 'Player'!");
                    return;
                }
            }
        }
        
        ItemData selectedItem = GetSelectedItem();
        if (selectedItem != null)
        {
            Debug.Log($"HotbarUI: UpdateEquippedItem() - equipuji '{selectedItem.itemName}'");
        }
        else
        {
            Debug.Log("HotbarUI: UpdateEquippedItem() - žádný item ve slotu");
        }
        ItemHolder.Instance.EquipItem(selectedItem);
    }
    
    void OnDestroy()    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= UpdateHotbar;
        }
    }
}
