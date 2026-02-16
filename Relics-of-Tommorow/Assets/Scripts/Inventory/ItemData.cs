using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    
    [Header("3D Model")]
    public GameObject worldModelPrefab; // 3D model pro drop/pickup
    
    [Header("Hand Position (když je equipnutý)")]
    public Vector3 handPositionOffset = new Vector3(0.792094f, -0.86572f, 0.873465f);
    public Vector3 handRotationOffset = new Vector3(-67.944f, -2.678f, 178.563f);
    
    [Header("Stack Settings")]
    public int maxStackSize = 64;
    public bool isStackable = true;
    
    [Header("Item Type")]
    public ItemType itemType;
    
    [Header("Weapon Stats")]
    [Tooltip("Platí pouze pro ItemType.Weapon")]
    public WeaponStats weaponStats;
    
    [Header("Artifact Settings")]
    [Tooltip("Platí pouze pro ItemType.Artifact")]
    public ArtifactData artifactData;  // Reference na artifact data
    
    [Tooltip("Doba držení pravého tlačítka pro aktivaci (v sekundách)")]
    public float activationHoldTime = 1.5f;
    
    public enum ItemType
    {
        Material,
        Weapon,
        Tool,
        Consumable,
        Quest,
        Artifact  // Artefakty - aktivovatelné power-upy
    }
}

[System.Serializable]
public class WeaponStats
{
    [Header("Combat Stats")]
    [Tooltip("Základní poškození zbraně")]
    public float damage = 10f;
    
    [Tooltip("Dosah zbraně v metrech")]
    public float range = 2f;
    
    [Tooltip("Rychlost útoku (útoků za sekundu)")]
    public float attackSpeed = 1f;
    
    [Tooltip("Jak dobře se díky této zbrani lze bránit (0-100)")]
    [Range(0f, 100f)]
    public float defenseValue = 0f;
    
    [Header("Spawn Settings")]
    [Tooltip("Šance na spawn (0-100%). Čím vyšší, tím častější.")]
    [Range(0f, 100f)]
    public float spawnChance = 50f;
    
    [Tooltip("Je toto boss loot? (Spadne pouze po zabití bosse)")]
    public bool isBossLoot = false;
}
