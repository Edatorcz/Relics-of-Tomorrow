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
    
    void Start()
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogError("InventorySystem nenalezen! Ujisti se, že existuje InventoryManager ve scéně.");
            return;
        }
        
        inventoryPanel.SetActive(false);
        CreateSlots();
        
        InventorySystem.Instance.OnInventoryChanged += UpdateUI;
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
        inventoryPanel.SetActive(isInventoryOpen);
        
        // Uvolnit/zamknout kurzor
        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
