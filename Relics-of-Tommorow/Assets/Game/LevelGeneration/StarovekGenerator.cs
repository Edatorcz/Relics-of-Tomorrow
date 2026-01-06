using UnityEngine;

/// <summary>
/// Generátor pro Starověk - místnosti s kamennými sloupy
/// </summary>
public class StarovekGenerator : RoomBasedGenerator
{
    [Header("Starověk Specific")]
    [SerializeField] private Color stoneWallColor = new Color(0.7f, 0.6f, 0.5f);
    [SerializeField] private Color stoneFloorColor = new Color(0.6f, 0.55f, 0.5f);
    
    // Vizuální efekty
    private Color ambientLightColor;
    private float ambientIntensity;
    private Color directionalLightColor;
    private float directionalLightIntensity;
    private Vector3 directionalLightRotation;
    private Color fogColor;
    private float fogDensity;
    
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
    
    public override void GenerateLevel()
    {
        base.GenerateLevel();
        ApplyVisualEffects();
    }
    
    protected void ApplyVisualEffects()
    {
        // Teplé sluneční osvětlení jako ve starém Egyptě
        ambientLightColor = new Color(1f, 0.9f, 0.7f);
        ambientIntensity = 0.6f;
        directionalLightColor = new Color(1f, 0.95f, 0.8f);
        directionalLightIntensity = 1.2f;
        directionalLightRotation = new Vector3(60, -45, 0);
        
        // Zlatavá teplá mlha
        fogColor = new Color(0.9f, 0.8f, 0.6f);
        fogDensity = 0.015f;
        
        SetupLighting();
        SetupFog();
        CreateSandParticles();
    }
    
    protected void SetupLighting()
    {
        RenderSettings.ambientLight = ambientLightColor;
        RenderSettings.ambientIntensity = ambientIntensity;
        
        GameObject lightObj = new GameObject("DirectionalLight");
        lightObj.transform.parent = transform;
        lightObj.transform.rotation = Quaternion.Euler(directionalLightRotation);
        
        Light dirLight = lightObj.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = directionalLightColor;
        dirLight.intensity = directionalLightIntensity;
        dirLight.shadows = LightShadows.Soft;
    }
    
    protected void SetupFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = fogDensity;
    }
    
    protected void CreateSandParticles()
    {
        // Písečné částice ve vzduchu
        GameObject epochParticles = new GameObject("SandParticles");
        epochParticles.transform.parent = transform;
        epochParticles.transform.position = Vector3.up * 2f;
        
        ParticleSystem ps = epochParticles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 12f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
        main.startColor = new Color(0.9f, 0.8f, 0.6f, 0.5f);
        main.maxParticles = 150;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 12f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(baseRoomSize * 1.2f, 6f, baseRoomSize * 1.2f);
        
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = new Color(0.95f, 0.85f, 0.7f, 0.6f);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }
}
