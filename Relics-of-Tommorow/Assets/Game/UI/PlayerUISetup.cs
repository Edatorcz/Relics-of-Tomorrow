using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automaticky nastaví UI pro hráče (Health bar a Stamina bar)
/// Přiřaď tento script na Canvas nebo UI parent objekt
/// </summary>
public class PlayerUISetup : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private bool autoCreateUI = true;
    
    [Header("Health Bar Settings")]
    [SerializeField] private Vector2 healthBarPosition = new Vector2(-220f, -20f);
    [SerializeField] private Vector2 healthBarSize = new Vector2(200f, 20f);
    
    [Header("Stamina Bar Settings")]
    [SerializeField] private Vector2 staminaBarPosition = new Vector2(-220f, -45f);
    [SerializeField] private Vector2 staminaBarSize = new Vector2(200f, 15f);
    
    private GameObject healthBarObj;
    private GameObject staminaBarObj;
    
    void Start()
    {
        if (autoCreateUI)
        {
            CreatePlayerUI();
        }
    }
    
    void CreatePlayerUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("PlayerUISetup: Canvas component not found!");
            return;
        }
        
        // Vytvoř Stamina Bar
        CreateStaminaBar();
        
        Debug.Log("PlayerUISetup: Player UI created successfully!");
    }
    
    void CreateStaminaBar()
    {
        // Zkontroluj jestli už existuje
        StaminaBarUI existing = FindFirstObjectByType<StaminaBarUI>();
        if (existing != null)
        {
            Debug.Log("PlayerUISetup: StaminaBarUI already exists");
            return;
        }
        
        // Vytvoř nový GameObject
        staminaBarObj = new GameObject("StaminaBar");
        staminaBarObj.transform.SetParent(transform, false);
        
        // Přidej StaminaBarUI komponentu
        StaminaBarUI staminaBar = staminaBarObj.AddComponent<StaminaBarUI>();
        staminaBar.SetPosition(staminaBarPosition);
        staminaBar.SetSize(staminaBarSize);
        
        Debug.Log("PlayerUISetup: StaminaBar created");
    }
}
