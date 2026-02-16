using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI zobrazení aktivních artefaktů hráče
/// Zobrazuje ikony a názvy artefaktů v pravém horním rohu
/// </summary>
public class ArtifactUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject artifactSlotPrefab;
    [SerializeField] private Transform artifactContainer;
    
    [Header("Settings")]
    [SerializeField] private Vector2 slotSize = new Vector2(80f, 80f);
    
    private Dictionary<ArtifactData, GameObject> activeSlots = new Dictionary<ArtifactData, GameObject>();
    
    void Start()
    {
        // Přihlásit se k eventům ArtifactManageru
        if (ArtifactManager.Instance != null)
        {
            ArtifactManager.Instance.OnArtifactActivated += OnArtifactActivated;
            ArtifactManager.Instance.OnArtifactDeactivated += OnArtifactDeactivated;
            ArtifactManager.Instance.OnAllArtifactsCleared += OnAllArtifactsCleared;
        }
    }
    
    void OnDestroy()
    {
        if (ArtifactManager.Instance != null)
        {
            ArtifactManager.Instance.OnArtifactActivated -= OnArtifactActivated;
            ArtifactManager.Instance.OnArtifactDeactivated -= OnArtifactDeactivated;
            ArtifactManager.Instance.OnAllArtifactsCleared -= OnAllArtifactsCleared;
        }
    }
    
    void OnArtifactActivated(ArtifactData artifact)
    {
        if (artifact == null) return;
        
        // Vytvořit slot pro artefakt
        GameObject slot = CreateArtifactSlot(artifact);
        activeSlots[artifact] = slot;
    }
    
    void OnArtifactDeactivated(ArtifactData artifact)
    {
        if (activeSlots.ContainsKey(artifact))
        {
            Destroy(activeSlots[artifact]);
            activeSlots.Remove(artifact);
        }
    }
    
    void OnAllArtifactsCleared()
    {
        // Smazat všechny sloty
        foreach (var slot in activeSlots.Values)
        {
            if (slot != null)
                Destroy(slot);
        }
        activeSlots.Clear();
    }
    
    GameObject CreateArtifactSlot(ArtifactData artifact)
    {
        GameObject slot;
        
        if (artifactSlotPrefab != null)
        {
            slot = Instantiate(artifactSlotPrefab, artifactContainer);
        }
        else
        {
            slot = CreateDefaultSlot();
        }
        
        // Nastavit ikonu
        Image icon = slot.GetComponentInChildren<Image>();
        if (icon != null && artifact.icon != null)
        {
            icon.sprite = artifact.icon;
        }
        
        // Nastavit název
        TextMeshProUGUI nameText = slot.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = artifact.artifactName;
        }
        
        // Tooltip
        ArtifactTooltip tooltip = slot.GetComponent<ArtifactTooltip>();
        if (tooltip == null)
            tooltip = slot.AddComponent<ArtifactTooltip>();
        tooltip.SetArtifact(artifact);
        
        return slot;
    }
    
    GameObject CreateDefaultSlot()
    {
        GameObject slot = new GameObject("ArtifactSlot");
        slot.transform.SetParent(artifactContainer);
        
        // Background
        Image bg = slot.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        RectTransform bgRect = slot.GetComponent<RectTransform>();
        bgRect.sizeDelta = slotSize;
        
        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(slot.transform);
        Image icon = iconObj.AddComponent<Image>();
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = slotSize * 0.8f;
        iconRect.anchoredPosition = Vector2.zero;
        
        // Name text
        GameObject textObj = new GameObject("Name");
        textObj.transform.SetParent(slot.transform);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 12;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(slotSize.x, 20);
        textRect.anchoredPosition = new Vector2(0, -slotSize.y * 0.5f - 10);
        
        return slot;
    }
}

/// <summary>
/// Tooltip pro zobrazení info o artefaktu při najetí myší
/// </summary>
public class ArtifactTooltip : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    private ArtifactData artifact;
    private GameObject tooltipObject;
    
    public void SetArtifact(ArtifactData data)
    {
        artifact = data;
    }
    
    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (artifact == null) return;
        ShowTooltip();
    }
    
    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        HideTooltip();
    }
    
    void ShowTooltip()
    {
        // TODO: Vytvořit tooltip panel s popisem artefaktu
        Debug.Log($"Artifact: {artifact.artifactName}\n{artifact.description}");
    }
    
    void HideTooltip()
    {
        if (tooltipObject != null)
            Destroy(tooltipObject);
    }
}
