using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject slotPrefab;
    
    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.E;
    
    private InventorySlotUI[] slotUIs;
    private bool isInventoryOpen = false;
    private Crosshair crosshair;
    
    void Start()
    {
        if (inventoryPanel == null)
        {
            return;
        }
        
        if (InventorySystem.Instance == null)
        {
            return;
        }
        
        inventoryPanel.SetActive(false);
        CreateSlots();
        
        InventorySystem.Instance.OnInventoryChanged += UpdateUI;
        
        // Najdi crosshair
        crosshair = FindFirstObjectByType<Crosshair>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }
    
    private void CreateSlots()
    {
        int inventorySize = InventorySystem.Instance.GetInventorySize();
        slotUIs = new InventorySlotUI[inventorySize];
        
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            slotUI.Initialize(i);
            slotUIs[i] = slotUI;
        }
    }
    
    private void UpdateUI()
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            InventorySlot slot = InventorySystem.Instance.GetSlot(i);
            slotUIs[i].UpdateSlot(slot);
        }
    }
    
    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        Debug.Log($"ToggleInventory voláno! isInventoryOpen = {isInventoryOpen}");
        
        inventoryPanel.SetActive(isInventoryOpen);
        
        // Uvolnit/zamknout kurzor
        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Skryj crosshair
            if (crosshair != null)
                crosshair.SetVisible(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Zobraz crosshair
            if (crosshair != null)
                crosshair.SetVisible(true);
        }
        
        UpdateUI();
    }
    
    void OnDestroy()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= UpdateUI;
        }
    }
}
