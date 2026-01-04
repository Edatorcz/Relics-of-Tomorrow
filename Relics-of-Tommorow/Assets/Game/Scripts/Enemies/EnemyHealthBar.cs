using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Health bar nad nepřítelem
/// Automaticky se vytvoří a aktualizuje podle EnemyBase health
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyBase enemy;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image healthBarBackground;
    [SerializeField] private Image healthBarFill;
    
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 0);
    [SerializeField] private Vector2 barSize = new Vector2(2f, 0.3f); // Větší bar!
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color healthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private bool alwaysFaceCamera = true;
    
    private Camera mainCamera;
    private float currentHealth;
    private float maxHealth;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Najít EnemyBase pokud není přiřazen
        if (enemy == null)
        {
            enemy = GetComponentInParent<EnemyBase>();
        }
        
        if (enemy == null)
        {
            Debug.LogError("EnemyHealthBar: No EnemyBase found!");
            enabled = false;
            return;
        }
        
        // Setup canvas a UI
        SetupHealthBar();
        
        // Inicializovat health
        UpdateHealth();
    }
    
    void SetupHealthBar()
    {
        // Vytvořit Canvas pokud neexistuje
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("HealthBarCanvas");
            canvasObj.transform.SetParent(transform, false);
            canvasObj.transform.localPosition = offset;
            
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;
            
            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = barSize;
        }
        
        // Vytvořit background
        if (healthBarBackground == null)
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvas.transform, false);
            healthBarBackground = bgObj.AddComponent<Image>();
            healthBarBackground.color = backgroundColor;
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
        }
        
        // Vytvořit fill
        if (healthBarFill == null)
        {
            GameObject fillObj = new GameObject("HealthFill");
            fillObj.transform.SetParent(healthBarBackground.transform, false);
            healthBarFill = fillObj.AddComponent<Image>();
            healthBarFill.color = healthColor;
            healthBarFill.type = Image.Type.Filled;
            healthBarFill.fillMethod = Image.FillMethod.Horizontal;
            
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
        }
    }
    
    void Update()
    {
        UpdateHealth();
        
        // Otočit směrem ke kameře
        if (alwaysFaceCamera && mainCamera != null && canvas != null)
        {
            canvas.transform.LookAt(canvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }
    
    void UpdateHealth()
    {
        if (enemy == null || healthBarFill == null) return;
        
        currentHealth = enemy.GetCurrentHealth();
        maxHealth = enemy.GetMaxHealth();
        
        float healthPercent = maxHealth > 0 ? currentHealth / maxHealth : 0;
        
        // Aktualizovat fill amount
        healthBarFill.fillAmount = healthPercent;
        
        // Změnit barvu podle health
        if (healthPercent <= lowHealthThreshold)
        {
            healthBarFill.color = Color.Lerp(lowHealthColor, healthColor, healthPercent / lowHealthThreshold);
        }
        else
        {
            healthBarFill.color = healthColor;
        }
        
        // Skrýt když je full health
        if (hideWhenFull && canvas != null)
        {
            canvas.gameObject.SetActive(healthPercent < 0.99f);
        }
    }
    
    // Veřejné metody
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        if (canvas != null)
        {
            canvas.transform.localPosition = offset;
        }
    }
    
    public void SetColors(Color health, Color lowHealth)
    {
        healthColor = health;
        lowHealthColor = lowHealth;
    }
    
    public void SetSize(Vector2 size)
    {
        barSize = size;
        if (canvas != null)
        {
            RectTransform rect = canvas.GetComponent<RectTransform>();
            rect.sizeDelta = size;
        }
    }
}
