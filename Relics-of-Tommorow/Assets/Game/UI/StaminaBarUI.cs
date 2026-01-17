using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI zobrazující staminu pro blokování
/// Zobrazuje se trvale vedle health baru
/// </summary>
public class StaminaBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private Image staminaFill;
    [SerializeField] private Image staminaBackground;
    
    [Header("Settings")]
    [SerializeField] private Color fullStaminaColor = new Color(0.3f, 0.8f, 1f); // Světle modrá
    [SerializeField] private Color lowStaminaColor = Color.red;
    [SerializeField] private float lowStaminaThreshold = 0.3f;
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    
    [Header("Layout")]
    [SerializeField] private Vector2 barSize = new Vector2(200f, 20f);
    [SerializeField] private Vector2 barPosition = new Vector2(-220f, -40f); // Pod health bar
    
    private RectTransform rectTransform;
    private Canvas canvas;
    
    void Start()
    {
        // Najít PlayerCombat pokud není přiřazen
        if (playerCombat == null)
        {
            playerCombat = FindFirstObjectByType<PlayerCombat>();
            if (playerCombat == null)
            {
                Debug.LogError("StaminaBarUI: PlayerCombat not found!");
                enabled = false;
                return;
            }
        }
        
        // Setup UI
        SetupStaminaBar();
    }
    
    void SetupStaminaBar()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        
        // Nastavit pozici a velikost
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(1, 1); // Pravý horní roh
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.anchoredPosition = barPosition;
            rectTransform.sizeDelta = barSize;
        }
        
        // Vytvořit background pokud neexistuje
        if (staminaBackground == null)
        {
            GameObject bgObj = new GameObject("StaminaBackground");
            bgObj.transform.SetParent(transform, false);
            
            staminaBackground = bgObj.AddComponent<Image>();
            staminaBackground.color = backgroundColor;
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
        }
        
        // Vytvořit fill pokud neexistuje
        if (staminaFill == null)
        {
            GameObject fillObj = new GameObject("StaminaFill");
            fillObj.transform.SetParent(staminaBackground.transform, false);
            
            staminaFill = fillObj.AddComponent<Image>();
            staminaFill.color = fullStaminaColor;
            staminaFill.type = Image.Type.Filled;
            staminaFill.fillMethod = Image.FillMethod.Horizontal;
            staminaFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
        }
        
        Debug.Log("StaminaBarUI: Setup complete!");
    }
    
    void Update()
    {
        UpdateStaminaBar();
    }
    
    void UpdateStaminaBar()
    {
        if (staminaFill == null || playerCombat == null) return;
        
        // Získat stamina percent
        float staminaPercent = playerCombat.BlockStaminaPercent;
        staminaFill.fillAmount = staminaPercent;
        
        // Změnit barvu podle staminy
        if (staminaPercent <= lowStaminaThreshold)
        {
            staminaFill.color = Color.Lerp(lowStaminaColor, fullStaminaColor, 
                staminaPercent / lowStaminaThreshold);
        }
        else
        {
            staminaFill.color = fullStaminaColor;
        }
    }
    
    // Veřejné metody pro konfiguraci
    public void SetColors(Color full, Color low)
    {
        fullStaminaColor = full;
        lowStaminaColor = low;
    }
    
    public void SetSize(Vector2 size)
    {
        barSize = size;
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = size;
        }
    }
    
    public void SetPosition(Vector2 position)
    {
        barPosition = position;
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = position;
        }
    }
}
