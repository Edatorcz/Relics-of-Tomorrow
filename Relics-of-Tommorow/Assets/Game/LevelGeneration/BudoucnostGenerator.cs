using UnityEngine;

/// <summary>
/// Generátor pro Budoucnost - futuristické místnosti
/// </summary>
public class BudoucnostGenerator : RoomBasedGenerator
{
    [Header("Budoucnost Specific")]
    [SerializeField] private Color cyberWallColor = new Color(0.1f, 0.2f, 0.3f);
    [SerializeField] private Color cyberFloorColor = new Color(0.05f, 0.15f, 0.25f);
    
    protected override Color GetWallColor() => cyberWallColor;
    protected override Color GetFloorColor() => cyberFloorColor;
    
    protected override void CreateRoomStructure(Room room)
    {
        base.CreateRoomStructure(room);
        
        // Přidat svitící panely
        GameObject panelParent = new GameObject("TechPanels");
        panelParent.transform.parent = room.floor.transform.parent;
        
        for (int i = 0; i < 3; i++)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.transform.position = GetRandomPositionInRoom(room) + Vector3.up * 1.5f;
            panel.transform.localScale = new Vector3(0.1f, 2f, 1f);
            panel.transform.parent = panelParent.transform;
            
            // Neonová barva
            Renderer renderer = panel.GetComponent<Renderer>();
            renderer.material.color = new Color(0f, 1f, 1f);
            renderer.material.SetFloat("_Metallic", 1f);
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", new Color(0f, 0.5f, 0.5f));
        }
    }
}
