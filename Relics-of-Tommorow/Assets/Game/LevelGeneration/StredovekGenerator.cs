using UnityEngine;

/// <summary>
/// Generátor pro Středověk - hrady a věže
/// </summary>
public class StredovekGenerator : RoomBasedGenerator
{
    [Header("Středověk Specific")]
    [SerializeField] private Color castleWallColor = new Color(0.5f, 0.5f, 0.55f);
    [SerializeField] private Color castleFloorColor = new Color(0.4f, 0.4f, 0.45f);
    
    protected override Color GetWallColor() => castleWallColor;
    protected override Color GetFloorColor() => castleFloorColor;
    
    protected override void CreateRoomStructure(Room room)
    {
        base.CreateRoomStructure(room);
        
        // Přidat hranatné kamenné bloky
        GameObject blockParent = new GameObject("CastleBlocks");
        blockParent.transform.parent = room.floor.transform.parent;
        
        for (int i = 0; i < 2; i++)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.transform.position = GetRandomPositionInRoom(room);
            block.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
            block.transform.parent = blockParent.transform;
            block.GetComponent<Renderer>().material.color = new Color(0.6f, 0.6f, 0.65f);
        }
    }
}
