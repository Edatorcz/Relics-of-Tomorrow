using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Kruhový indikátor bloku kolem kurzoru myši
/// Zobrazuje zbývající block staminu
/// </summary>
public class BlockIndicatorUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private Image blockCircle;
    [SerializeField] private Image blockFill;
    
    [Header("Settings")]
    [SerializeField] private float circleRadius = 40f;
    [SerializeField] private Color fullStaminaColor = Color.cyan;
    [SerializeField] private Color lowStaminaColor = Color.red;
    [SerializeField] private float lowStaminaThreshold = 0.3f;
    
    private RectTransform circleRect;
    private Canvas canvas;
    
    void Start()
    {
        // Najít PlayerCombat pokud není přiřazen
        if (playerCombat == null)
        {
            playerCombat = FindFirstObjectByType<PlayerCombat>();
        }
        
        // Setup UI
        SetupBlockIndicator();
        
        // Skrýt na začátku
        if (blockCircle != null) blockCircle.gameObject.SetActive(false);
    }
    
    void SetupBlockIndicator()
    {
        canvas = GetComponentInParent<Canvas>();
        
        // Pokud komponenty nejsou přiřazeny, vytvořit je
        if (blockCircle == null)
        {
            GameObject circleObj = new GameObject("BlockCircle");
            circleObj.transform.SetParent(transform, false);
            blockCircle = circleObj.AddComponent<Image>();
            blockCircle.type = Image.Type.Simple;
            blockCircle.color = new Color(1, 1, 1, 0.3f);
            
            circleRect = circleObj.GetComponent<RectTransform>();
            circleRect.sizeDelta = new Vector2(circleRadius * 2, circleRadius * 2);
        }
        else
        {
            circleRect = blockCircle.GetComponent<RectTransform>();
        }
        
        if (blockFill == null)
        {
            GameObject fillObj = new GameObject("BlockFill");
            fillObj.transform.SetParent(blockCircle.transform, false);
            blockFill = fillObj.AddComponent<Image>();
            blockFill.type = Image.Type.Filled;
            blockFill.fillMethod = Image.FillMethod.Radial360;
            blockFill.fillOrigin = (int)Image.Origin360.Top;
            blockFill.fillClockwise = false;
            
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
        }
    }
    
    void Update()
    {
        if (playerCombat == null || blockCircle == null) return;
        
        // Zobrazit pouze při blokování
        bool isBlocking = playerCombat.IsBlocking;
        blockCircle.gameObject.SetActive(isBlocking);
        
        if (isBlocking)
        {
            UpdateIndicatorPosition();
            UpdateIndicatorFill();
        }
    }
    
    void UpdateIndicatorPosition()
    {
        if (circleRect == null || canvas == null) return;
        
        // Umístit na pozici myši
        Vector2 mousePosition = Input.mousePosition;
        
        // Konvertovat screen space na canvas space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            mousePosition,
            canvas.worldCamera,
            out Vector2 localPoint
        );
        
        circleRect.anchoredPosition = localPoint;
    }
    
    void UpdateIndicatorFill()
    {
        if (blockFill == null || playerCombat == null) return;
        
        float staminaPercent = playerCombat.BlockStaminaPercent;
        blockFill.fillAmount = staminaPercent;
        
        // Změnit barvu podle staminy
        if (staminaPercent <= lowStaminaThreshold)
        {
            blockFill.color = lowStaminaColor;
        }
        else
        {
            blockFill.color = Color.Lerp(lowStaminaColor, fullStaminaColor, 
                (staminaPercent - lowStaminaThreshold) / (1f - lowStaminaThreshold));
        }
    }
    
    // Veřejné metody pro konfiguraci
    public void SetCircleRadius(float radius)
    {
        circleRadius = radius;
        if (circleRect != null)
        {
            circleRect.sizeDelta = new Vector2(radius * 2, radius * 2);
        }
    }
    
    public void SetColors(Color full, Color low)
    {
        fullStaminaColor = full;
        lowStaminaColor = low;
    }
}
