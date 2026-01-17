using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Jednoduchý crosshair uprostřed obrazovky
/// </summary>
public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private CrosshairType crosshairType = CrosshairType.Cross;
    [SerializeField] private Color crosshairColor = Color.white;
    [SerializeField] private float crosshairSize = 20f;
    [SerializeField] private float crosshairThickness = 2f;
    [SerializeField] private float crosshairGap = 5f;
    [SerializeField] private bool dynamicCrosshair = false;
    [SerializeField] private float maxSpread = 30f;
    
    private RectTransform[] crosshairParts;
    private Image[] crosshairImages;
    private Canvas canvas;
    private float currentSpread = 0f;
    
    public enum CrosshairType
    {
        Cross,      // + křížek
        Dot,        // • tečka
        Circle,     // ○ kroužek
        Custom      // vlastní sprite
    }
    
    void Start()
    {
        CreateCrosshair();
    }
    
    void Update()
    {
        if (dynamicCrosshair)
        {
            UpdateDynamicCrosshair();
        }
    }
    
    private void CreateCrosshair()
    {
        // Najdi nebo vytvoř Canvas
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return;
        }
        
        // Vyčisti staré části
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        switch (crosshairType)
        {
            case CrosshairType.Cross:
                CreateCrossCrosshair();
                break;
            case CrosshairType.Dot:
                CreateDotCrosshair();
                break;
            case CrosshairType.Circle:
                CreateCircleCrosshair();
                break;
        }
    }
    
    private void CreateCrossCrosshair()
    {
        crosshairParts = new RectTransform[4]; // Top, Bottom, Left, Right
        crosshairImages = new Image[4];
        
        // Horní část
        crosshairParts[0] = CreateLine("Top", new Vector2(crosshairThickness, crosshairSize), new Vector2(0, crosshairGap + crosshairSize / 2));
        // Dolní část
        crosshairParts[1] = CreateLine("Bottom", new Vector2(crosshairThickness, crosshairSize), new Vector2(0, -(crosshairGap + crosshairSize / 2)));
        // Levá část
        crosshairParts[2] = CreateLine("Left", new Vector2(crosshairSize, crosshairThickness), new Vector2(-(crosshairGap + crosshairSize / 2), 0));
        // Pravá část
        crosshairParts[3] = CreateLine("Right", new Vector2(crosshairSize, crosshairThickness), new Vector2(crosshairGap + crosshairSize / 2, 0));
        
        for (int i = 0; i < 4; i++)
        {
            crosshairImages[i] = crosshairParts[i].GetComponent<Image>();
            crosshairImages[i].color = crosshairColor;
        }
    }
    
    private void CreateDotCrosshair()
    {
        crosshairParts = new RectTransform[1];
        crosshairImages = new Image[1];
        
        crosshairParts[0] = CreateLine("Dot", new Vector2(crosshairThickness * 2, crosshairThickness * 2), Vector2.zero);
        crosshairImages[0] = crosshairParts[0].GetComponent<Image>();
        crosshairImages[0].color = crosshairColor;
    }
    
    private void CreateCircleCrosshair()
    {
        crosshairParts = new RectTransform[1];
        crosshairImages = new Image[1];
        
        GameObject circleObj = new GameObject("Circle");
        circleObj.transform.SetParent(transform);
        
        RectTransform rect = circleObj.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(crosshairSize, crosshairSize);
        
        Image img = circleObj.AddComponent<Image>();
        img.color = crosshairColor;
        
        // Vytvoř kruhovou texturu
        Texture2D circleTex = CreateCircleTexture((int)crosshairSize, (int)crosshairThickness);
        Sprite circleSprite = Sprite.Create(circleTex, new Rect(0, 0, circleTex.width, circleTex.height), new Vector2(0.5f, 0.5f));
        img.sprite = circleSprite;
        
        crosshairParts[0] = rect;
        crosshairImages[0] = img;
    }
    
    private RectTransform CreateLine(string name, Vector2 size, Vector2 position)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(transform);
        
        RectTransform rect = lineObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image img = lineObj.AddComponent<Image>();
        
        return rect;
    }
    
    private Texture2D CreateCircleTexture(int size, int thickness)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        float center = size / 2f;
        float outerRadius = size / 2f;
        float innerRadius = outerRadius - thickness;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                
                if (distance <= outerRadius && distance >= innerRadius)
                {
                    pixels[y * size + x] = Color.white;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
    
    private void UpdateDynamicCrosshair()
    {
        // Dynamický crosshair - může se roztahovat podle pohybu
        // Můžeš to připojit k rychlosti hráče, střelbě, atd.
        
        if (crosshairType == CrosshairType.Cross && crosshairParts != null && crosshairParts.Length == 4)
        {
            // Roztáhni čáry podle spreadu
            float spread = crosshairGap + currentSpread;
            
            crosshairParts[0].anchoredPosition = new Vector2(0, spread + crosshairSize / 2);
            crosshairParts[1].anchoredPosition = new Vector2(0, -(spread + crosshairSize / 2));
            crosshairParts[2].anchoredPosition = new Vector2(-(spread + crosshairSize / 2), 0);
            crosshairParts[3].anchoredPosition = new Vector2(spread + crosshairSize / 2, 0);
        }
    }
    
    /// <summary>
    /// Nastav spread crosshairu (pro dynamický crosshair)
    /// </summary>
    public void SetSpread(float spread)
    {
        currentSpread = Mathf.Clamp(spread, 0, maxSpread);
    }
    
    /// <summary>
    /// Změň barvu crosshairu
    /// </summary>
    public void SetColor(Color color)
    {
        crosshairColor = color;
        if (crosshairImages != null)
        {
            foreach (Image img in crosshairImages)
            {
                if (img != null)
                    img.color = color;
            }
        }
    }
    
    /// <summary>
    /// Zobraz/skryj crosshair
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
    
    // Pro změny v editoru
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            CreateCrosshair();
        }
    }
}
