using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
        // Tlačítko Q pro vyhození itemu z vybraného slotu
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropSelectedItem();
        }
        
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
        
        // Použití vybraného itemu pomocí pravého tlačítka myši (pro artefakty)
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Debug.Log("HotbarUI: Stisknuto pravé tlačítko myši!");
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
            Debug.LogWarning("HotbarUI.UseSelectedItem(): InventorySystem je NULL!");
            return;
        }
        
        int inventoryIndex = hotbarStartIndex + selectedSlotIndex;
        InventorySlot slot = InventorySystem.Instance.GetSlot(inventoryIndex);
        
        if (slot == null || slot.IsEmpty())
        {
            Debug.Log("HotbarUI.UseSelectedItem(): Slot je prázdný");
            return;
        }
        
        ItemData item = slot.item;
        Debug.Log($"HotbarUI.UseSelectedItem(): Pokouším se použít '{item.itemName}' typu {item.itemType}");
        
        // Použití artefaktu
        if (item.itemType == ItemData.ItemType.Artifact)
        {
            if (item.artifactData == null)
            {
                Debug.LogError($"HotbarUI.UseSelectedItem(): Artefakt '{item.itemName}' nemá přiřazený ArtifactData!");
                return;
            }
            
            Debug.Log($"HotbarUI.UseSelectedItem(): Aktivuji artefakt '{item.artifactData.artifactName}'");
            Debug.Log($"ArtifactData - effectType: {item.artifactData.effectType}, effectValue: {item.artifactData.effectValue}");
            
            // Aktivovat přes ArtifactManager
            if (ArtifactManager.Instance != null)
            {
                Debug.Log($"HotbarUI: Volám ArtifactManager.Instance.ActivateArtifact()");
                ArtifactManager.Instance.ActivateArtifact(item.artifactData);
                
                // Odebrat item z inventáře
                slot.RemoveItem(1);
                InventorySystem.Instance.OnInventoryChanged?.Invoke();
                
                Debug.Log($"HotbarUI.UseSelectedItem(): Artefakt '{item.artifactData.artifactName}' aktivován!");
            }
            else
            {
                Debug.LogError("HotbarUI.UseSelectedItem(): ArtifactManager.Instance nenalezen!");
            }
        }
        else if (item.itemType == ItemData.ItemType.Consumable)
        {
            // TODO: Implementovat consumable logiku
            Debug.Log($"HotbarUI.UseSelectedItem(): Consumable '{item.itemName}' použit");
        }
        else
        {
            Debug.Log($"HotbarUI.UseSelectedItem(): Item typu {item.itemType} nelze použít");
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
    
    void OnDestroy()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= UpdateHotbar;
        }
    }
    
    /// <summary>
    /// Vyhodí vybraný item z hotbaru na zem
    /// </summary>
    private void DropSelectedItem()
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("HotbarUI.DropSelectedItem(): InventorySystem není k dispozici");
            return;
        }
        
        int inventoryIndex = hotbarStartIndex + selectedSlotIndex;
        InventorySlot slot = InventorySystem.Instance.GetSlot(inventoryIndex);
        
        if (slot == null || slot.IsEmpty())
        {
            Debug.Log($"HotbarUI.DropSelectedItem(): Slot {inventoryIndex} je prázdný");
            return;
        }
        
        // Získat data itemu před odebráním
        ItemData itemToDrop = slot.item;
        int amountToDrop = slot.quantity;
        
        // Odebrat item z inventáře
        slot.RemoveItem(amountToDrop);
        InventorySystem.Instance.OnInventoryChanged?.Invoke();
        
        // Spawn itemu na zemi před hráčem
        SpawnDroppedItem(itemToDrop, amountToDrop);
        
        Debug.Log($"HotbarUI.DropSelectedItem(): Vyhozeno {amountToDrop}x '{itemToDrop.itemName}'");
    }
    
    /// <summary>
    /// Vytvoří item na zemi před hráčem
    /// </summary>
    private void SpawnDroppedItem(ItemData itemData, int amount)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("HotbarUI.SpawnDroppedItem(): Nelze najít hráče");
            return;
        }
        
        // Získat kameru hráče pro správný směr hodu
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        Vector3 throwDirection = playerCamera != null ? playerCamera.transform.forward : player.transform.forward;
        
        // Pozice před hráčem
        Vector3 dropPosition = player.transform.position + Vector3.up * 1.5f + throwDirection * 1f;
        
        // Vytvoř GameObject pro dropped item
        GameObject droppedObj;
        
        // Použít worldModelPrefab pokud existuje
        if (itemData.worldModelPrefab != null)
        {
            droppedObj = Instantiate(itemData.worldModelPrefab);
            droppedObj.transform.position = dropPosition;
            droppedObj.name = $"Dropped_{itemData.itemName}";
            
            // Odstranit "(Clone)" ze všech child objektů
            foreach (Transform child in droppedObj.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.Contains("(Clone)"))
                {
                    child.name = child.name.Replace("(Clone)", "");
                }
            }
            
            // Pokud prefab už má ArtifactPickup, nastav mu itemData a odstraň všechny ostatní
            ArtifactPickup existingPickup = droppedObj.GetComponent<ArtifactPickup>();
            
            // IMPORTANT: Odstranit VŠECHNY child ArtifactPickup komponenty aby se předešlo duplikaci
            ArtifactPickup[] childPickups = droppedObj.GetComponentsInChildren<ArtifactPickup>();
            foreach (ArtifactPickup childPickup in childPickups)
            {
                if (childPickup != existingPickup)
                {
                    Destroy(childPickup);
                }
            }
            
            if (existingPickup != null)
            {
                existingPickup.SetItemData(itemData);
            }
        }
        else
        {
            // Fallback na jednoduchý cube
            droppedObj = new GameObject($"Dropped_{itemData.itemName}");
            droppedObj.transform.position = dropPosition;
            
            // Přidat vizuální reprezentaci (mesh nebo sprite)
            MeshRenderer renderer = droppedObj.AddComponent<MeshRenderer>();
            MeshFilter filter = droppedObj.AddComponent<MeshFilter>();
            
            // Použít základní cube mesh
            filter.mesh = CreateCubeMesh();
            
            // Nastavit materiál s ikonou itemu (pokud existuje)
            Material mat = new Material(Shader.Find("Standard"));
            if (itemData.icon != null)
            {
                mat.mainTexture = itemData.icon.texture;
            }
            else
            {
                mat.color = Color.white;
            }
            renderer.material = mat;
        }
        
        // Přidat collider pokud neexistuje
        BoxCollider col = droppedObj.GetComponent<BoxCollider>();
        if (col == null)
        {
            col = droppedObj.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(0.5f, 0.5f, 0.5f);
        }
        
        // Přidat rigidbody pokud neexistuje
        Rigidbody rb = droppedObj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = droppedObj.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            rb.useGravity = true;
        }
        
        // Přidat pickup komponentu podle typu itemu (jen pokud neexistuje)
        if (itemData.itemType == ItemData.ItemType.Artifact)
        {
            ArtifactPickup pickup = droppedObj.GetComponent<ArtifactPickup>();
            if (pickup == null)
            {
                pickup = droppedObj.AddComponent<ArtifactPickup>();
                pickup.SetItemData(itemData);
                Debug.Log($"HotbarUI.SpawnDroppedItem(): Přidán ArtifactPickup na droppedObj");
            }
            else
            {
                // Pickup už existuje (z worldModelPrefab), jen se ujisti že má správná data
                Debug.Log($"HotbarUI.SpawnDroppedItem(): ArtifactPickup již existuje, data již nastavena");
            }
        }
        else
        {
            // Pro ostatní itemy můžeme použít obecný ItemPickup (pokud existuje)
            // TODO: Implementovat ItemPickup pro non-artifact itemy
            Debug.Log($"HotbarUI.SpawnDroppedItem(): Item typu {itemData.itemType} zatím nemá pickup komponentu");
        }
        
        // Hodit item směrem, jakým hráč koučá
        Vector3 throwForce = (throwDirection + Vector3.up * 0.3f) * 8f; // Silnější hod
        rb.AddForce(throwForce, ForceMode.Impulse);
        
        Debug.Log($"HotbarUI.SpawnDroppedItem(): Vytvořen dropped item na pozici {dropPosition}");
    }
    
    /// <summary>
    /// Vytvoří základní cube mesh
    /// </summary>
    private Mesh CreateCubeMesh()
    {
        Mesh mesh = new Mesh();
        
        // Jednoduché cube vertices
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.25f, -0.25f, -0.25f),
            new Vector3(0.25f, -0.25f, -0.25f),
            new Vector3(0.25f, 0.25f, -0.25f),
            new Vector3(-0.25f, 0.25f, -0.25f),
            new Vector3(-0.25f, -0.25f, 0.25f),
            new Vector3(0.25f, -0.25f, 0.25f),
            new Vector3(0.25f, 0.25f, 0.25f),
            new Vector3(-0.25f, 0.25f, 0.25f)
        };
        
        int[] triangles = new int[]
        {
            0, 2, 1, 0, 3, 2,  // Front
            1, 2, 6, 1, 6, 5,  // Right
            5, 6, 7, 5, 7, 4,  // Back
            4, 7, 3, 4, 3, 0,  // Left
            3, 7, 6, 3, 6, 2,  // Top
            4, 0, 1, 4, 1, 5   // Bottom
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        return mesh;
    }
}
