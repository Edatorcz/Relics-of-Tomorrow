using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Systém pro používání itemů v hotbaru pomocí držení pravého tlačítka myši
/// Speciálně navrženo pro aktivaci artefaktů
/// </summary>
public class ItemUseSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HotbarUI hotbarUI;
    
    [Header("Use Settings")]
    [SerializeField] private KeyCode useKey = KeyCode.Mouse1; // Pravé tlačítko myši
    
    [Header("Visual Feedback")]
    [SerializeField] private Image progressCircle; // Kruhový progress bar
    [SerializeField] private GameObject usePromptUI; // UI prompt "Drž RMB pro použití"
    
    private bool isHoldingUseKey = false;
    private float holdStartTime = 0f;
    private ItemData currentHoldItem = null;
    private float requiredHoldTime = 0f;
    
    void Start()
    {
        if (hotbarUI == null)
        {
            hotbarUI = FindFirstObjectByType<HotbarUI>();
            if (hotbarUI == null)
            {
                Debug.LogError("ItemUseSystem: HotbarUI nebyl nalezen! ItemUseSystem nebude fungovat!");
            }
            else
            {
                Debug.Log("ItemUseSystem: HotbarUI nalezen automaticky");
            }
        }
        
        if (progressCircle != null)
            progressCircle.gameObject.SetActive(false);
        else
            Debug.LogWarning("ItemUseSystem: ProgressCircle není nastaven! Vizuální feedback nebude fungovat.");
            
        if (usePromptUI != null)
            usePromptUI.SetActive(false);
        else
            Debug.LogWarning("ItemUseSystem: UsePromptUI není nastaven!");
            
        Debug.Log("ItemUseSystem: Inicializován - použij pravé tlačítko myši pro aktivaci artefaktů");
    }
    
    void Update()
    {
        HandleItemUse();
        UpdateVisualFeedback();
    }
    
    void HandleItemUse()
    {
        if (hotbarUI == null || InventorySystem.Instance == null)
        {
            if (hotbarUI == null)
                Debug.LogWarning("ItemUseSystem: HotbarUI je NULL!");
            if (InventorySystem.Instance == null)
                Debug.LogWarning("ItemUseSystem: InventorySystem.Instance je NULL!");
            return;
        }
        
        // Získat aktuálně vybraný item
        ItemData selectedItem = hotbarUI.GetSelectedItem();
        
        // Debug log pro kontrolu - jen když se drží RMB
        if (Input.GetKey(useKey))
        {
            if (selectedItem != null)
            {
                if (selectedItem.itemType == ItemData.ItemType.Artifact)
                {
                    if (selectedItem.artifactData != null)
                    {
                        Debug.Log($"ItemUseSystem: Vybraný artefakt: {selectedItem.itemName}");
                    }
                    else
                    {
                        Debug.LogError($"ItemUseSystem: Artefakt '{selectedItem.itemName}' nemá přiřazený ArtifactData!");
                    }
                }
                else
                {
                    Debug.Log($"ItemUseSystem: Item '{selectedItem.itemName}' není artefakt (typ: {selectedItem.itemType})");
                }
            }
            else
            {
                Debug.Log("ItemUseSystem: Žádný item není vybrán v hotbaru");
            }
        }
        
        // Kontrola zda můžeme použít item
        bool canUseItem = selectedItem != null && IsUsableItem(selectedItem);
        
        // Zobrazit/skrýt prompt
        if (usePromptUI != null)
        {
            usePromptUI.SetActive(canUseItem && !isHoldingUseKey);
        }
        
        // Začátek držení
        if (Input.GetKeyDown(useKey))
        {
            if (canUseItem)
            {
                Debug.Log($"ItemUseSystem: RMB stisknuto! Začínám držení itemu '{selectedItem.itemName}'");
                StartHolding(selectedItem);
            }
            else
            {
                if (selectedItem != null)
                {
                    Debug.LogWarning($"ItemUseSystem: Item '{selectedItem.itemName}' nelze použít (canUseItem=false)");
                }
                else
                {
                    Debug.LogWarning("ItemUseSystem: Není vybrán žádný item k použití");
                }
            }
        }
        
        // Během držení
        if (isHoldingUseKey && Input.GetKey(useKey))
        {
            float holdDuration = Time.time - holdStartTime;
            Debug.Log($"ItemUseSystem: Držím RMB - progress: {holdDuration}/{requiredHoldTime}s");
            
            // Kontrola zda se item změnil během držení
            if (selectedItem != currentHoldItem)
            {
                CancelHolding();
                return;
            }
            
            // Dokončeno držení?
            if (holdDuration >= requiredHoldTime)
            {
                UseItem(currentHoldItem);
                CancelHolding();
            }
        }
        
        // Uvolnění tlačítka před dokončením
        if (isHoldingUseKey && Input.GetKeyUp(useKey))
        {
            CancelHolding();
        }
    }
    
    void StartHolding(ItemData item)
    {
        isHoldingUseKey = true;
        holdStartTime = Time.time;
        currentHoldItem = item;
        
        // Určit čas držení podle typu itemu
        if (item.itemType == ItemData.ItemType.Artifact)
        {
            requiredHoldTime = item.activationHoldTime;
        }
        else
        {
            requiredHoldTime = 0.5f; // Default pro ostatní itemy
        }
        
        if (progressCircle != null)
            progressCircle.gameObject.SetActive(true);
        
        Debug.Log($"ItemUseSystem: Začátek držení itemu '{item.itemName}', required time: {requiredHoldTime}s");
    }
    
    void CancelHolding()
    {
        isHoldingUseKey = false;
        currentHoldItem = null;
        
        if (progressCircle != null)
            progressCircle.gameObject.SetActive(false);
        
        Debug.Log("ItemUseSystem: Držení zrušeno");
    }
    
    void UseItem(ItemData item)
    {
        if (item == null) return;
        
        Debug.Log($"ItemUseSystem: Používám item '{item.itemName}' typu {item.itemType}");
        
        switch (item.itemType)
        {
            case ItemData.ItemType.Artifact:
                UseArtifact(item);
                break;
                
            case ItemData.ItemType.Consumable:
                UseConsumable(item);
                break;
                
            default:
                Debug.Log($"ItemUseSystem: Item typu {item.itemType} nemá use funkci");
                break;
        }
    }
    
    void UseArtifact(ItemData item)
    {
        if (item.artifactData == null)
        {
            Debug.LogError($"ItemUseSystem: Artifact item '{item.itemName}' nemá přiřazený ArtifactData!");
            return;
        }
        
        // Aktivovat artefakt přes ArtifactManager
        if (ArtifactManager.Instance != null)
        {
            ArtifactManager.Instance.ActivateArtifact(item.artifactData);
            
            // Odebrat item z inventáře (artefakt je consumed)
            RemoveItemFromHotbar();
            
            // Vizuální a zvukové efekty
            PlayActivationEffects(item);
            
            Debug.Log($"ItemUseSystem: Artefakt '{item.artifactData.artifactName}' byl aktivován!");
        }
        else
        {
            Debug.LogError("ItemUseSystem: ArtifactManager.Instance nenalezen!");
        }
    }
    
    void UseConsumable(ItemData item)
    {
        // TODO: Implementovat consumable logiku (healing potion, atd.)
        Debug.Log($"ItemUseSystem: Použit consumable '{item.itemName}'");
        
        // Odebrat item
        RemoveItemFromHotbar();
    }
    
    void RemoveItemFromHotbar()
    {
        if (hotbarUI == null || InventorySystem.Instance == null)
            return;
        
        int selectedIndex = hotbarUI.GetSelectedSlotIndex();
        InventorySlot slot = InventorySystem.Instance.GetSlot(selectedIndex);
        
        if (slot != null && !slot.IsEmpty())
        {
            slot.RemoveItem(1);
            InventorySystem.Instance.OnInventoryChanged?.Invoke();
        }
    }
    
    void PlayActivationEffects(ItemData item)
    {
        // TODO: Particle efekty, zvuky, screen shake, atd.
        if (item.artifactData != null && item.artifactData.activationSound != null)
        {
            AudioSource.PlayClipAtPoint(item.artifactData.activationSound, Camera.main.transform.position);
        }
    }
    
    void UpdateVisualFeedback()
    {
        if (!isHoldingUseKey || progressCircle == null)
            return;
        
        float holdDuration = Time.time - holdStartTime;
        float progress = Mathf.Clamp01(holdDuration / requiredHoldTime);
        
        progressCircle.fillAmount = progress;
        
        // Změna barvy podle pokroku
        progressCircle.color = Color.Lerp(Color.yellow, Color.green, progress);
    }
    
    bool IsUsableItem(ItemData item)
    {
        if (item == null)
        {
            Debug.Log("ItemUseSystem.IsUsableItem(): Item je NULL");
            return false;
        }
        
        switch (item.itemType)
        {
            case ItemData.ItemType.Artifact:
                bool hasArtifactData = item.artifactData != null;
                if (!hasArtifactData)
                {
                    Debug.LogWarning($"ItemUseSystem.IsUsableItem(): Artefakt '{item.itemName}' nemá přiřazený ArtifactData!");
                }
                return hasArtifactData;
                
            case ItemData.ItemType.Consumable:
                return true;
                
            default:
                Debug.Log($"ItemUseSystem.IsUsableItem(): Item typu {item.itemType} není použitelný");
                return false;
        }
    }
    
    // Public gettery pro UI
    public bool IsHolding() => isHoldingUseKey;
    public float GetHoldProgress() => isHoldingUseKey ? Mathf.Clamp01((Time.time - holdStartTime) / requiredHoldTime) : 0f;
    public ItemData GetCurrentHoldItem() => currentHoldItem;
}
