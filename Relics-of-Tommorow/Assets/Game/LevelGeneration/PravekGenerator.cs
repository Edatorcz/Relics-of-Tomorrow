using UnityEngine;

/// <summary>
/// Generátor pro Pravěk - místnosti z kamene
/// </summary>
public class PravekGenerator : RoomBasedGenerator
{
    [Header("Pravek Specific")]
    [SerializeField] private Color caveWallColor = new Color(0.4f, 0.3f, 0.2f);
    [SerializeField] private Color caveFloorColor = new Color(0.3f, 0.25f, 0.2f);
    
    protected override Color GetWallColor() => caveWallColor;
    protected override Color GetFloorColor() => caveFloorColor;
    
    // Odstraněno automatické generování sphere kamenů - nyní se používají prefaby
}
