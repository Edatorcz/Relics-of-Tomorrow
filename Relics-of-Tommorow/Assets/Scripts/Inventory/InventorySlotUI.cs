using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image backgroundImage;
    
    [Header("Drop Settings")]
    [SerializeField] private KeyCode dropKey = KeyCode.Delete;
    
    private int slotIndex;
    private static InventorySlotUI draggedSlot;
    private static GameObject draggedIcon;
    
    public void Initialize(int index)
    {
        slotIndex = index;
    }
    
    void Update()
    {
        // Stisknutí Delete klávesy - drop celého stacku
        if (Input.GetKeyDown(dropKey))
        {
            InventorySlot slot = InventorySystem.Instance.GetSlot(slotIndex);
            if (slot != null && !slot.IsEmpty())
            {
                // Zkontroluj, zda je myš nad tímto slotem
                if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == gameObject ||
                    IsMouseOverSlot())
                {
                    DropItem(slot.item, slot.quantity);
                }
            }
        }
    }
    
    private bool IsMouseOverSlot()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(), 
            Input.mousePosition, 
            null
        );
    }
    
    private void DropItem(ItemData item, int quantity)
    {
        if (ItemDropper.Instance != null)
        {
            ItemDropper.Instance.DropItemFromPlayer(item, quantity);
            InventorySystem.Instance.RemoveItem(item, quantity);
        }
    }
    
    public void UpdateSlot(InventorySlot slot)
    {
        if (slot == null || slot.IsEmpty())
        {
            itemIcon.enabled = false;
            quantityText.text = "";
            return;
        }
        
        itemIcon.enabled = true;
        itemIcon.sprite = slot.item.icon;
        
        if (slot.quantity > 1)
        {
            quantityText.text = slot.quantity.ToString();
        }
        else
        {
            quantityText.text = "";
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Pravé tlačítko - rozdělit stack na polovinu
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            InventorySlot slot = InventorySystem.Instance.GetSlot(slotIndex);
            if (slot == null || slot.IsEmpty() || slot.quantity <= 1) return;
            
            int splitAmount = slot.quantity / 2;
            int emptySlotIndex = FindEmptySlot();
            
            if (emptySlotIndex != -1)
            {
                // Rozdělení stacku
                InventorySlot emptySlot = InventorySystem.Instance.GetSlot(emptySlotIndex);
                emptySlot.item = slot.item;
                emptySlot.quantity = splitAmount;
                slot.quantity -= splitAmount;
                
                InventorySystem.Instance.OnInventoryChanged?.Invoke();
            }
        }
        // Střední tlačítko - rychlé přesunutí
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            InventorySlot slot = InventorySystem.Instance.GetSlot(slotIndex);
            if (slot != null && !slot.IsEmpty())
            {
                // Rychlé přesunutí do prvního volného slotu
                int emptySlot = FindEmptySlot();
                if (emptySlot != -1)
                {
                    InventorySystem.Instance.SwapSlots(slotIndex, emptySlot);
                }
            }
        }
    }
    
    private int FindEmptySlot()
    {
        int inventorySize = InventorySystem.Instance.GetInventorySize();
        for (int i = 0; i < inventorySize; i++)
        {
            InventorySlot slot = InventorySystem.Instance.GetSlot(i);
            if (slot != null && slot.IsEmpty())
            {
                return i;
            }
        }
        return -1;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        InventorySlot slot = InventorySystem.Instance.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty()) return;
        
        draggedSlot = this;
        
        // Vytvoř vizuální reprezentaci taženého itemu
        draggedIcon = new GameObject("DraggedIcon");
        draggedIcon.transform.SetParent(transform.root);
        draggedIcon.transform.SetAsLastSibling();
        
        Image iconImage = draggedIcon.AddComponent<Image>();
        iconImage.sprite = slot.item.icon;
        iconImage.raycastTarget = false;
        
        RectTransform rt = draggedIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50, 50);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            draggedIcon.transform.position = Input.mousePosition;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            Destroy(draggedIcon);
        }
        
        // Pokud jsme drag ukončili mimo inventář UI, dropni item
        if (eventData.pointerEnter == null || !IsOverInventoryUI(eventData.pointerEnter))
        {
            InventorySlot slot = InventorySystem.Instance.GetSlot(slotIndex);
            if (slot != null && !slot.IsEmpty())
            {
                DropItem(slot.item, slot.quantity);
            }
        }
        
        draggedSlot = null;
    }
    
    private bool IsOverInventoryUI(GameObject obj)
    {
        // Zkontroluj, zda je pointerEnter součástí inventáře
        if (obj == null) return false;
        
        InventorySlotUI slotUI = obj.GetComponent<InventorySlotUI>();
        if (slotUI != null) return true;
        
        // Zkontroluj rodiče
        Transform parent = obj.transform;
        while (parent != null)
        {
            if (parent.GetComponent<InventoryUI>() != null)
                return true;
            parent = parent.parent;
        }
        
        return false;
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null || draggedSlot == this) return;
        
        // Prohoď sloty
        InventorySystem.Instance.SwapSlots(draggedSlot.slotIndex, this.slotIndex);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        InventorySlot slot = InventorySystem.Instance.GetSlot(slotIndex);
        if (slot != null && !slot.IsEmpty() && ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.ShowTooltip(slot.item, transform.position);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideTooltip();
        }
    }
}
