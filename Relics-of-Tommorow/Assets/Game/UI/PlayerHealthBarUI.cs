using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Canvas-based health bar pro hráče - funguje na všech rozlišeních
/// Umístí se vpravo nahoře vedle Stamina baru
/// </summary>
public class PlayerHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthFill;
    [SerializeField] private Image healthBackground;
    [SerializeField] private TMP_Text healthText;
    
    [Header("Settings")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private bool showHealthText = true;
    
    [Header("Layout")]
    [SerializeField] private Vector2 barSize = new Vector2(200f, 20f);
    [SerializeField] private Vector2 barPosition = new Vector2(-220f, -20f); // Pravý horní roh
    
    private RectTransform rectTransform;
    
    void Start()
    {
        // Najít PlayerHealth pokud není přiřazen
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogError("PlayerHealthBarUI: PlayerHealth not found!");
                enabled = false;
                return;
            }
        }
        
        // Přihlásit se k event
        playerHealth.OnHealthChanged += OnHealthChanged;
        
        // Setup UI
        SetupHealthBar();
        
        // Aktualizovat hodnoty
        OnHealthChanged(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
    }
    
    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
        }
    }
    
    void SetupHealthBar()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Nastavit pozici a velikost - pravý horní roh
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.anchoredPosition = barPosition;
            rectTransform.sizeDelta = barSize;
        }
        
        // Vytvořit background pokud neexistuje
        if (healthBackground == null)
        {
            GameObject bgObj = new GameObject("HealthBackground");
            bgObj.transform.SetParent(transform, false);
            
            healthBackground = bgObj.AddComponent<Image>();
            healthBackground.color = backgroundColor;
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
        }
        
        // Vytvořit fill pokud neexistuje
        if (healthFill == null)
        {
            GameObject fillObj = new GameObject("HealthFill");
            fillObj.transform.SetParent(healthBackground.transform, false);
            
            healthFill = fillObj.AddComponent<Image>();
            healthFill.color = fullHealthColor;
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
            healthFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
        }
        
        // Vytvořit text pokud neexistuje
        if (healthText == null && showHealthText)
        {
            GameObject textObj = new GameObject("HealthText");
            textObj.transform.SetParent(transform, false);
            
            healthText = textObj.AddComponent<TMP_Text>();
            healthText.alignment = TextAlignmentOptions.Center;
            healthText.fontSize = 14;
            healthText.color = Color.white;
            healthText.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
        
        Debug.Log("PlayerHealthBarUI: Setup complete!");
    }
    
    void OnHealthChanged(float currentHealth, float maxHealth)
    {
        if (healthFill == null) return;
        
        // Aktualizovat fill amount
        float healthPercent = currentHealth / maxHealth;
        healthFill.fillAmount = healthPercent;
        
        // Změnit barvu podle health
        if (healthPercent <= lowHealthThreshold)
        {
            healthFill.color = Color.Lerp(lowHealthColor, fullHealthColor, 
                healthPercent / lowHealthThreshold);
        }
        else
        {
            healthFill.color = fullHealthColor;
        }
        
        // Aktualizovat text
        if (healthText != null && showHealthText)
        {
            healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
        }
    }
    
    // Veřejné metody
    public void SetColors(Color full, Color low)
    {
        fullHealthColor = full;
        lowHealthColor = low;
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
