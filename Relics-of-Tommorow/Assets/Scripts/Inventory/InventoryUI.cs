using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject slotPrefab;
    
    // Settings - inventář je deaktivován, toggleKey odstraněn
    
    private InventorySlotUI[] slotUIs;
    private bool isInventoryOpen = false;
    private Crosshair crosshair;
    
    void Awake()
    {
        // Automaticky najít Canvas ve scéně pokud nejsme jeho část
        Canvas sceneCanvas = FindFirstObjectByType<Canvas>();
        
        // Pokud inventoryPanel není přiřazen, zkus ho najít
        if (inventoryPanel == null)
        {
            // Zkus najít jako child tohoto objektu
            inventoryPanel = transform.Find("InventoryPanel")?.gameObject;
            
            if (inventoryPanel == null && sceneCanvas != null)
            {
                // Hledej v Canvas
                Transform panel = sceneCanvas.transform.Find("InventoryPanel");
                if (panel != null)
                {
                    inventoryPanel = panel.gameObject;
                }
                else
                {
                    // Zkus hledat rekurzivně v celém Canvas
                    foreach (Transform child in sceneCanvas.GetComponentsInChildren<Transform>(true))
                    {
                        if (child.name.Contains("InventoryPanel"))
                        {
                            inventoryPanel = child.gameObject;
                            break;
                        }
                    }
                }
            }
        }
        
        // Pokud slotsParent není přiřazen, zkus ho najít
        if (slotsParent == null && inventoryPanel != null)
        {
            // Hledej v inventoryPanel
            Transform slots = inventoryPanel.transform.Find("SlotsGrid");
            if (slots == null)
            {
                slots = inventoryPanel.transform.Find("Slots");
            }
            if (slots == null)
            {
                // Hledej rekurzivně
                foreach (Transform child in inventoryPanel.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name.Contains("Slot") && child.name.Contains("Grid") || child.name == "SlotsGrid" || child.name == "Slots")
                    {
                        slots = child;
                        break;
                    }
                }
            }
            slotsParent = slots;
        }
        
        // Pokud slotPrefab není přiřazen, zkus ho najít v Resources
        if (slotPrefab == null)
        {
            slotPrefab = Resources.Load<GameObject>("SlotsPrefab");
            if (slotPrefab == null)
            {
                slotPrefab = Resources.Load<GameObject>("InventorySlot");
            }
        }
        
        // OKAMŽITĚ zavřít inventář při vytvoření
        if (inventoryPanel != null)
        {
            isInventoryOpen = false;
            inventoryPanel.SetActive(false);
        }
        
        // Resetovat kurzor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    /// <summary>
    /// Veřejná metoda pro refresh UI referencí (volat po scene load)
    /// </summary>
    public void RefreshUIReferences()
    {
        Debug.Log("InventoryUI.RefreshUIReferences: Hledám UI prvky ve scéně...");
        
        // Najít Canvas ve scéné
        Canvas sceneCanvas = FindFirstObjectByType<Canvas>();
        
        if (sceneCanvas == null)
        {
            Debug.LogWarning("InventoryUI.RefreshUIReferences: Canvas nenalezen ve scéně! Inventář nebude fungovat.");
            return;
        }
        
        Debug.Log($"InventoryUI.RefreshUIReferences: Canvas nalezen: {sceneCanvas.name}");
        
        // Najít InventoryPanel
        if (inventoryPanel == null)
        {
            Transform panel = sceneCanvas.transform.Find("InventoryPanel");
            if (panel == null)
            {
                // Hledej rekurzivně
                foreach (Transform child in sceneCanvas.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "InventoryPanel" || child.name.Contains("Inventory"))
                    {
                        panel = child;
                        Debug.Log($"InventoryUI.RefreshUIReferences: InventoryPanel nalezen rekurzivně: {panel.name}");
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("InventoryUI.RefreshUIReferences: InventoryPanel nalezen přímo");
            }
            
            if (panel != null)
            {
                inventoryPanel = panel.gameObject;
            }
            else
            {
                Debug.LogWarning($"InventoryUI.RefreshUIReferences: InventoryPanel nenalezen v Canvas '{sceneCanvas.name}'! Zkontroluj hierarchii scény.");
            }
        }
        
        // Najít SlotsParent
        if (inventoryPanel != null)
        {
            Transform slots = inventoryPanel.transform.Find("SlotsGrid");
            if (slots == null)
            {
                slots = inventoryPanel.transform.Find("Slots");
            }
            if (slots == null)
            {
                foreach (Transform child in inventoryPanel.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "SlotsGrid" || child.name == "Slots" || child.name.Contains("Slot"))
                    {
                        slots = child;
                        Debug.Log($"InventoryUI.RefreshUIReferences: SlotsParent nalezen: {slots.name}");
                        break;
                    }
                }
            }
            
            if (slots != null)
            {
                slotsParent = slots;
            }
            else
            {
                Debug.LogWarning($"InventoryUI.RefreshUIReferences: SlotsGrid nenalezen v InventoryPanel! Zkontroluj hierarchii.");
            }
        }
        
        // Najít SlotPrefab
        if (inventoryPanel != null)
        {
            // Hledej SlotsPrefab v children
            Transform prefabTransform = null;
            foreach (Transform child in inventoryPanel.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "SlotsPrefab" || child.name.Contains("SlotPrefab"))
                {
                    prefabTransform = child;
                    Debug.Log($"InventoryUI.RefreshUIReferences: SlotPrefab nalezen: {prefabTransform.name}");
                    break;
                }
            }
            
            if (prefabTransform != null)
            {
                slotPrefab = prefabTransform.gameObject;
            }
            else
            {
                Debug.LogWarning("InventoryUI.RefreshUIReferences: SlotsPrefab nenalezen! Zkontroluj hierarchii.");
            }
        }
        
        // Zavřít inventář
        if (inventoryPanel != null)
        {
            isInventoryOpen = false;
            inventoryPanel.SetActive(false);
        }
        
        // DŮLEŽITÉ: Po refresh referencí znovu vytvořit a aktualizovat sloty
        if (inventoryPanel != null && slotsParent != null && slotPrefab != null && InventorySystem.Instance != null)
        {
            Debug.Log("InventoryUI.RefreshUIReferences: Všechny reference nalezeny, vytvářím sloty...");
            
            // Smazat staré sloty pokud existují
            if (slotUIs != null)
            {
                foreach (var slotUI in slotUIs)
                {
                    if (slotUI != null)
                    {
                        Destroy(slotUI.gameObject);
                    }
                }
            }
            
            // Vytvořit nové sloty
            CreateSlots();
            
            // Aktualizovat UI aby se zobrazily itemy
            UpdateUI();
            
            // Přihlásit se k eventu
            InventorySystem.Instance.OnInventoryChanged -= UpdateUI; // Nejdřív odhlásit
            InventorySystem.Instance.OnInventoryChanged += UpdateUI; // Pak přihlásit
            
            Debug.Log("InventoryUI.RefreshUIReferences: Hotovo!");
        }
        else
        {
            Debug.LogWarning($"InventoryUI.RefreshUIReferences: Chybí reference! Panel={inventoryPanel != null}, SlotsParent={slotsParent != null}, SlotPrefab={slotPrefab != null}, InventorySystem={InventorySystem.Instance != null}");
        }
    }
    
    void Start()
    {
        if (inventoryPanel == null || InventorySystem.Instance == null)
        {
            return;
        }
        
        // Zavřít inventář při startu nové scény (double-check)
        isInventoryOpen = false;
        inventoryPanel.SetActive(false);
        
        // Vytvořit sloty pouze pokud ještě neexistují (RefreshUIReferences je už možná vytvořil)
        if (slotUIs == null || slotUIs.Length == 0)
        {
            CreateSlots();
        }
        
        // Přihlásit se k eventu (pokud už není)
        InventorySystem.Instance.OnInventoryChanged -= UpdateUI;
        InventorySystem.Instance.OnInventoryChanged += UpdateUI;
        
        // Aktualizovat UI
        UpdateUI();
        
        // Najdi crosshair
        crosshair = FindFirstObjectByType<Crosshair>();
    }
    
    void Update()
    {
        // INVENTÁŘ DEAKTIVOVÁN - používáme jen hotbar
        // Inventář se už neotevírá klávesou
        /*
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
        */
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
        if (slotUIs == null || slotUIs.Length == 0 || InventorySystem.Instance == null)
        {
            return;
        }
        
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] == null) continue;
            
            InventorySlot slot = InventorySystem.Instance.GetSlot(i);
            slotUIs[i].UpdateSlot(slot);
        }
    }
    
    public void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            return;
        }
        
        isInventoryOpen = !isInventoryOpen;
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
    
    /// <summary>
    /// Zavřít inventář (pro EpochManager před přechodem)
    /// </summary>
    public void CloseInventory()
    {
        if (isInventoryOpen)
        {
            isInventoryOpen = false;
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (crosshair != null)
                crosshair.SetVisible(true);
        }
    }
    
    void OnDestroy()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= UpdateUI;
        }
    }
}
