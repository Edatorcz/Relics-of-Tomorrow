using UnityEngine;

/// <summary>
/// Generátor pro Současnost - moderní prostory
/// </summary>
public class SoucasnostGenerator : RoomBasedGenerator
{
    [Header("Současnost Specific")]
    [SerializeField] private Color modernWallColor = new Color(0.9f, 0.9f, 0.95f);
    [SerializeField] private Color modernFloorColor = new Color(0.7f, 0.7f, 0.75f);
    
    protected override Color GetWallColor() => modernWallColor;
    protected override Color GetFloorColor() => modernFloorColor;
    
    protected override void CreateRoomStructure(Room room)
    {
        base.CreateRoomStructure(room);
        
        // Přidat moderní předměty (bedny, kontejnery)
        GameObject propsParent = new GameObject("ModernProps");
        propsParent.transform.parent = room.floor.transform.parent;
        
        for (int i = 0; i < 4; i++)
        {
            GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.transform.position = GetRandomPositionInRoom(room);
            crate.transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);
            crate.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            crate.transform.parent = propsParent.transform;
            crate.GetComponent<Renderer>().material.color = new Color(0.6f, 0.5f, 0.4f);
        }
    }
}
