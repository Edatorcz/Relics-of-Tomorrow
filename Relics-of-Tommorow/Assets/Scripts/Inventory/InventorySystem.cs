using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }
    
    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 9; // Jen hotbar (bez hlavního inventáře)
    
    [Header("Debug - Obsah inventáře")]
    [SerializeField] private List<string> debugInventoryContents = new List<string>();
    
    private List<InventorySlot> slots;
    
    public System.Action OnInventoryChanged;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("InventorySystem: První instance vytvořena, inicializuji inventář");
            InitializeInventory();
        }
        else
        {
            // Už existuje instance z předchozí scény
            // Zkontroluj jestli jsme tento objekt nebo jiný
            if (Instance != this)
            {
                Debug.Log("InventorySystem: Duplicitní instance detekována, ničím novou");
                // Jsme duplicitní - znič tento nový
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("InventorySystem: Stejná instance po scene reload");
            }
        }
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"InventorySystem.OnSceneLoaded: Scéna '{scene.name}' načtena. Aktuální počet itemů v inventáři: {GetItemCount()}");
        
        // Vyčistit všechny event subscribers (starý UI byl zničen)
        OnInventoryChanged = null;
        
        // Najít InventoryUI - buď na tomto objektu nebo vytvořit nový
        InventoryUI inventoryUI = GetComponent<InventoryUI>();
        if (inventoryUI == null)
        {
            Debug.Log("InventorySystem.OnSceneLoaded: InventoryUI neexistuje, přidávám nový");
            // Pokud neexistuje, přidej ho
            inventoryUI = gameObject.AddComponent<InventoryUI>();
        }
        else
        {
            Debug.Log("InventorySystem.OnSceneLoaded: InventoryUI již existuje");
        }
        
        // Vynutit refresh UI referencí z nové scény
        if (inventoryUI != null)
        {
            inventoryUI.RefreshUIReferences();
        }
        
        Debug.Log($"InventorySystem.OnSceneLoaded: Dokončeno. Počet itemů po načtení: {GetItemCount()}");
    }
    
    /// <summary>
    /// Pomocná metoda pro debug - vrátí celkový počet itemů
    /// </summary>
    private int GetItemCount()
    {
        if (slots == null) return 0;
        int count = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty())
            {
                count += slot.quantity;
            }
        }
        return count;
    }
    
    private void InitializeInventory()
    {
        // Pokud už slots existují, nemazat je!
        if (slots != null && slots.Count > 0)
        {
            Debug.Log($"InventorySystem: Slots již existují ({slots.Count} slotů), nemazám je!");
            return;
        }
        
        Debug.Log($"InventorySystem: Vytvářím nové slots ({inventorySize})");
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
        
        // PŘEDNOST: Hotbar sloty (0-8) - nejdřív zkus přidat do prázdných hotbar slotů
        for (int i = 0; i < 9 && i < slots.Count; i++)
        {
            if (slots[i].IsEmpty())
            {
                remainingQuantity = slots[i].AddItem(item, remainingQuantity);
                Debug.Log($"InventorySystem: Item '{item.itemName}' přidán do hotbar slotu {i}");
                if (remainingQuantity <= 0)
                {
                    OnInventoryChanged?.Invoke();
                    UpdateDebugDisplay();
                    return true;
                }
            }
        }
        
        // Nejdřív zkus přidat do existujících stacků (v celém inventáři)
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
                        UpdateDebugDisplay();
                        return true;
                    }
                }
            }
        }
        
        // Pak zkus přidat do prázdných slotů ve zbytku inventáře (9+)
        while (remainingQuantity > 0)
        {
            int emptySlotIndex = FindEmptySlotInInventory();
            if (emptySlotIndex == -1)
            {
                OnInventoryChanged?.Invoke();
                UpdateDebugDisplay();
                return false; // Inventář plný
            }
            
            remainingQuantity = slots[emptySlotIndex].AddItem(item, remainingQuantity);
        }
        
        OnInventoryChanged?.Invoke();
        UpdateDebugDisplay();
        return true;
    }
    
    private void UpdateDebugDisplay()
    {
        debugInventoryContents.Clear();
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty())
            {
                debugInventoryContents.Add($"Slot {i}: {slots[i].item.itemName} x{slots[i].quantity}");
            }
        }
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
    
    /// <summary>
    /// Najde prázdný slot v hlavním inventáři (mimo hotbar sloty 0-8)
    /// </summary>
    private int FindEmptySlotInInventory()
    {
        for (int i = 9; i < slots.Count; i++)
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
