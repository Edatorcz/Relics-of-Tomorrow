using UnityEngine;

/// <summary>
/// Generátor pro Starověk - místnosti s kamennými sloupy
/// </summary>
public class StarovekGenerator : RoomBasedGenerator
{
    [Header("Starověk Specific")]
    [SerializeField] private Color stoneWallColor = new Color(0.7f, 0.6f, 0.5f);
    [SerializeField] private Color stoneFloorColor = new Color(0.6f, 0.55f, 0.5f);
    
    protected override Color GetWallColor() => stoneWallColor;
    protected override Color GetFloorColor() => stoneFloorColor;
    
    protected override void CreateRoomStructure(Room room)
    {
        base.CreateRoomStructure(room);
        
        // Přidat sloupy do rohů místnosti
        GameObject pillarParent = new GameObject("Pillars");
        pillarParent.transform.parent = room.floor.transform.parent;
        
        float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
        float offset = roomSize / 2f - 2f;
        Vector3[] corners = new Vector3[]
        {
            room.center + new Vector3(offset, 2f, offset),
            room.center + new Vector3(offset, 2f, -offset),
            room.center + new Vector3(-offset, 2f, offset),
            room.center + new Vector3(-offset, 2f, -offset)
        };
        
        foreach (Vector3 corner in corners)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.transform.position = corner;
            pillar.transform.localScale = new Vector3(0.5f, 4f, 0.5f);
            pillar.transform.parent = pillarParent.transform;
            pillar.GetComponent<Renderer>().material.color = new Color(0.8f, 0.7f, 0.6f);
        }
    }
}
