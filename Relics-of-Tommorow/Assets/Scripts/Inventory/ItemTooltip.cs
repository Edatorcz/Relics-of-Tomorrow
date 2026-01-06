using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private Image backgroundImage;
    
    [Header("Settings")]
    [SerializeField] private Vector2 offset = new Vector2(10, -10);
    [SerializeField] private float padding = 10f;
    
    private RectTransform tooltipRect;
    private Canvas canvas;
    
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
        
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        HideTooltip();
    }
    
    public void ShowTooltip(ItemData item, Vector2 position)
    {
        if (item == null)
        {
            HideTooltip();
            return;
        }
        
        tooltipPanel.SetActive(true);
        
        // Nastavit texty
        itemNameText.text = item.itemName;
        itemDescriptionText.text = item.description;
        itemTypeText.text = $"Typ: {item.itemType}";
        
        // Barva podle typu
        Color typeColor = GetColorForItemType(item.itemType);
        itemNameText.color = typeColor;
        
        // Přizpůsobit velikost
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        
        // Nastavit pozici
        Vector2 tooltipPosition = position + offset;
        
        // Ujistit se, že tooltip je na obrazovce
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            
            // Kontrola pravého okraje
            if (tooltipPosition.x + tooltipRect.rect.width + padding > canvasRect.rect.width)
            {
                tooltipPosition.x = position.x - tooltipRect.rect.width - offset.x;
            }
            
            // Kontrola levého okraje
            if (tooltipPosition.x < padding)
            {
                tooltipPosition.x = padding;
            }
            
            // Kontrola spodního okraje
            if (tooltipPosition.y - tooltipRect.rect.height < padding)
            {
                tooltipPosition.y = position.y + tooltipRect.rect.height - offset.y;
            }
            
            // Kontrola horního okraje
            if (tooltipPosition.y > canvasRect.rect.height - padding)
            {
                tooltipPosition.y = canvasRect.rect.height - padding;
            }
        }
        
        tooltipRect.position = tooltipPosition;
    }
    
    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
    
    private Color GetColorForItemType(ItemData.ItemType type)
    {
        switch (type)
        {
            case ItemData.ItemType.Material:
                return new Color(0.8f, 0.8f, 0.8f); // Šedá
            case ItemData.ItemType.Weapon:
                return new Color(1f, 0.3f, 0.3f); // Červená
            case ItemData.ItemType.Tool:
                return new Color(0.3f, 0.8f, 1f); // Modrá
            case ItemData.ItemType.Consumable:
                return new Color(0.3f, 1f, 0.3f); // Zelená
            case ItemData.ItemType.Quest:
                return new Color(1f, 1f, 0.3f); // Žlutá
            default:
                return Color.white;
        }
    }
}
