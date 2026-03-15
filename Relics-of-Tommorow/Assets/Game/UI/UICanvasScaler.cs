using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automaticky nastaví Canvas Scaler pro správné škálování UI na všech rozlišeních
/// Přidej na Canvas objekt ve hře
/// </summary>
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
public class UICanvasScaler : MonoBehaviour
{
    [Header("Reference Resolution")]
    [Tooltip("Referenční rozlišení - UI bude vypadat stejně na všech rozlišeních")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
    
    [Header("Match Settings")]
    [Tooltip("0 = škáluje podle šířky | 1 = škáluje podle výšky | 0.5 = vyvážené")]
    [Range(0f, 1f)]
    [SerializeField] private float matchWidthOrHeight = 0.5f;
    
    void Awake()
    {
        SetupCanvasScaler();
    }
    
    void SetupCanvasScaler()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        
        // Nastavit Canvas Scaler
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = matchWidthOrHeight;
        
        Debug.Log($"UICanvasScaler: Canvas configured for {referenceResolution.x}x{referenceResolution.y}");
    }
    
#if UNITY_EDITOR
    // Automaticky nastavit při přidání do objektu
    void Reset()
    {
        SetupCanvasScaler();
    }
#endif
}
