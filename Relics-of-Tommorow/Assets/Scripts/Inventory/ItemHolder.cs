using UnityEngine;

/// <summary>
/// Drží a zobrazuje aktuálně vybraný item z hotbaru v ruce hráče
/// </summary>
public class ItemHolder : MonoBehaviour
{
    public static ItemHolder Instance { get; private set; }
    
    [Header("Hand Position")]
    [SerializeField] private Transform handTransform; // Pozice kde se drží item (např. child kamery)
    [SerializeField] private Vector3 itemPositionOffset = new Vector3(0.792094f, -0.86572f, 0.873465f); // Nastaveno podle testování
    [SerializeField] private Vector3 itemRotationOffset = new Vector3(-67.944f, -2.678f, 178.563f); // Nastaveno podle testování
    [SerializeField] private Vector3 itemScale = Vector3.one; // Původní velikost world modelu
    
    private GameObject currentHeldItem;
    private ItemData currentItemData;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Pokud není handTransform nastavený, najdi nebo vytvoř ho
        if (handTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam == null) cam = Camera.main;
            
            if (cam != null)
            {
                // Zkus najít existující "Hand" nebo "ItemHolder" objekt
                Transform hand = cam.transform.Find("Hand");
                if (hand == null) hand = cam.transform.Find("ItemHolder");
                
                if (hand == null)
                {
                    // Vytvoř nový GameObject pro ruku jako child kamery
                    GameObject handObj = new GameObject("Hand");
                    handObj.transform.SetParent(cam.transform);
                    handObj.transform.localPosition = Vector3.zero; // Hand na pozici kamery
                    handObj.transform.localRotation = Quaternion.identity;
                    handTransform = handObj.transform;
                    Debug.Log($"ItemHolder: Vytvořen Hand objekt");
                }
                else
                {
                    handTransform = hand;
                    Debug.Log($"ItemHolder: Nalezen existující Hand objekt");
                }
            }
            else
            {
                Debug.LogError("ItemHolder: Nelze najít kameru!");
            }
        }
    }
    
    /// <summary>
    /// Zobrazí item v ruce hráče
    /// </summary>
    public void EquipItem(ItemData itemData)
    {
        // Odstraň současný item
        UnequipItem();
        
        if (itemData == null)
        {
            Debug.Log("ItemHolder: Žádný item k equipnutí");
            return;
        }
        
        if (handTransform == null)
        {
            Debug.LogWarning("ItemHolder: handTransform není nastaven!");
            return;
        }
        
        currentItemData = itemData;
        Debug.Log($"ItemHolder: Equipuji item '{itemData.itemName}'");
        
        // Pokud má item 3D model, použij ho
        if (itemData.worldModelPrefab != null)
        {
            // Pokud má item 3D model, použij ho
            Debug.Log($"ItemHolder: Používám worldModelPrefab pro '{itemData.itemName}'");
            currentHeldItem = Instantiate(itemData.worldModelPrefab, handTransform);
            
            // Odstraň ItemPickup a colliders z world modelu (není to pickup, je to equipped item)
            ItemPickup pickup = currentHeldItem.GetComponent<ItemPickup>();
            if (pickup != null) Destroy(pickup);
            
            Collider[] colliders = currentHeldItem.GetComponents<Collider>();
            foreach (var col in colliders) Destroy(col);
            
            // Zakázat Rigidbody aby se netočil a nespadl
            Rigidbody rb = currentHeldItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Debug.Log($"ItemHolder: Rigidbody nastaven jako kinematic");
            }
            
            // Zakázat MonoBehaviour scripty které by mohly způsobovat pohyb/rotaci
            MonoBehaviour[] scripts = currentHeldItem.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script != null)
                {
                    script.enabled = false;
                    Debug.Log($"ItemHolder: Zakázán script {script.GetType().Name}");
                }
            }
            
            Debug.Log($"ItemHolder: Model vytvořen na pozici {currentHeldItem.transform.position}");
        }
        else if (itemData.icon != null)
        {
            // Jinak vytvoř quad s ikonou
            Debug.Log($"ItemHolder: Vytvářím ikonu pro '{itemData.itemName}' (worldModelPrefab chybí)");
            currentHeldItem = CreateItemFromIcon(itemData.icon);
            currentHeldItem.transform.SetParent(handTransform);
        }
        else
        {
            Debug.LogWarning($"ItemHolder: Item '{itemData.itemName}' nemá ani worldModelPrefab ani icon!");
            return;
        }
        
        // Nastav pozici, rotaci a scale
        // Použij custom hodnoty z ItemData pokud existují, jinak výchozí
        Vector3 finalPosition = itemData.handPositionOffset;
        Vector3 finalRotation = itemData.handRotationOffset;
        
        currentHeldItem.transform.localPosition = finalPosition;
        currentHeldItem.transform.localRotation = Quaternion.Euler(finalRotation);
        // NEMĚŇ scale - ponech původní velikost world modelu
        // currentHeldItem.transform.localScale = itemScale;
        
        Debug.Log($"ItemHolder: Pozice={finalPosition}, Rotace={finalRotation}");
        
        Debug.Log($"ItemHolder: Item scale: {currentHeldItem.transform.localScale}, rotation: {itemRotationOffset}");
        
        // Nastav layer na Ignore Raycast aby nezasahoval do raycasts
        int ignoreRaycastLayer = 2; // Ignore Raycast je standardně layer 2
        SetLayerRecursively(currentHeldItem, ignoreRaycastLayer);
        Debug.Log($"ItemHolder: Layer nastaven na {ignoreRaycastLayer} (Ignore Raycast) aby nebránil raycasts");
        
        // Aktualizuj player stats pokud je to zbraň
        UpdatePlayerStats(itemData);
        
        Debug.Log($"ItemHolder: Item '{itemData.itemName}' equipnutý v ruce");
    }
    
    /// <summary>
    /// Odstraní item z ruky
    /// </summary>
    public void UnequipItem()
    {
        if (currentHeldItem != null)
        {            Destroy(currentHeldItem);
            currentHeldItem = null;
        }
        
        // Reset player stats
        if (currentItemData != null && currentItemData.itemType == ItemData.ItemType.Weapon)
        {
            ResetPlayerStats();
        }
        
        currentItemData = null;
    }
    
    /// <summary>
    /// Aktualizuje stats hráče podle equipnuté zbraně
    /// </summary>
    private void UpdatePlayerStats(ItemData itemData)
    {
        if (itemData.itemType != ItemData.ItemType.Weapon)
        {
            Debug.Log($"ItemHolder: Item '{itemData.itemName}' není zbraň, stats se nemění");
            return;
        }
        
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat == null)
        {
            Debug.LogWarning("ItemHolder: PlayerCombat komponent nebyl nalezen!");
            return;
        }
        
        // Nastav damage a range podle zbraně
        combat.SetAttackDamage(itemData.weaponStats.damage);
        combat.SetAttackRange(itemData.weaponStats.range);
        
        // Nastav cooldown podle attackSpeed (útoků za sekundu)
        // attackSpeed 1 = 1 cooldown, attackSpeed 10 = 0.1 cooldown
        float cooldown = 1f / Mathf.Max(0.1f, itemData.weaponStats.attackSpeed);
        combat.SetAttackCooldown(cooldown);
        
        Debug.Log($"ItemHolder: Player stats aktualizovány - Damage: {itemData.weaponStats.damage}, Range: {itemData.weaponStats.range}, AttackSpeed: {itemData.weaponStats.attackSpeed} (útoků/s), Cooldown: {cooldown}s");
    }
    
    /// <summary>
    /// Resetuje player stats na výchozí hodnoty
    /// </summary>
    private void ResetPlayerStats()
    {
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
        {
            combat.SetAttackDamage(25f); // Výchozí damage
            combat.SetAttackRange(10f); // Výchozí range
            combat.SetAttackCooldown(0.5f); // Výchozí cooldown
            Debug.Log("ItemHolder: Player stats resetovány na výchozí hodnoty");
        }
    }
    
    /// <summary>
    /// Vytvoří 2D quad s ikonou itemu pro zobrazení v ruce
    /// </summary>
    private GameObject CreateItemFromIcon(Sprite icon)
    {
        GameObject iconObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        iconObj.name = "HeldItemIcon";
        
        // Odstraň collider
        Collider col = iconObj.GetComponent<Collider>();
        if (col != null) Destroy(col);
        
        // Vytvoř material s texturou ikony
        MeshRenderer renderer = iconObj.GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Unlit/Transparent"));
        mat.mainTexture = icon.texture;
        renderer.material = mat;
        
        // Nastav velikost podle sprite
        float aspect = (float)icon.texture.width / icon.texture.height;
        iconObj.transform.localScale = new Vector3(aspect * 0.3f, 0.3f, 1f);
        
        return iconObj;
    }
    
    /// <summary>
    /// Nastaví layer rekurzivně pro všechny child objekty
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    
    /// <summary>
    /// Vrátí aktuálně držený item
    /// </summary>
    public ItemData GetCurrentItem()
    {
        return currentItemData;
    }
    
    /// <summary>
    /// Vrátí GameObject aktuálně drženého itemu
    /// </summary>
    public GameObject GetCurrentHeldObject()
    {
        return currentHeldItem;
    }
}
