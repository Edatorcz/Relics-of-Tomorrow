using UnityEngine;

/// <summary>
/// Generátor pro Současnost - moderní prostory
/// </summary>
public class SoucasnostGenerator : RoomBasedGenerator
{
    [Header("Současnost Specific")]
    [SerializeField] private Color modernWallColor = new Color(0.9f, 0.9f, 0.95f);
    [SerializeField] private Color modernFloorColor = new Color(0.7f, 0.7f, 0.75f);
    
    // Vizuální efekty
    private Color ambientLightColor;
    private float ambientIntensity;
    private Color directionalLightColor;
    private float directionalLightIntensity;
    private Vector3 directionalLightRotation;
    private Color fogColor;
    private float fogDensity;
    
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
    
    public override void GenerateLevel()
    {
        base.GenerateLevel();
        ApplyVisualEffects();
    }
    
    protected void ApplyVisualEffects()
    {
        // Neutrální denní světlo
        ambientLightColor = new Color(0.8f, 0.85f, 0.9f);
        ambientIntensity = 0.7f;
        directionalLightColor = Color.white;
        directionalLightIntensity = 1f;
        directionalLightRotation = new Vector3(50, -30, 0);
        
        // Mírná atmosféra
        fogColor = new Color(0.85f, 0.9f, 0.95f);
        fogDensity = 0.008f;
        
        SetupLighting();
        SetupFog();
        CreateAirParticles();
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
    
    protected void CreateAirParticles()
    {
        // Lehké vzdušné částice (prach ve světle)
        GameObject epochParticles = new GameObject("AirParticles");
        epochParticles.transform.parent = transform;
        epochParticles.transform.position = Vector3.up * 3f;
        
        ParticleSystem ps = epochParticles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 15f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        main.startColor = new Color(1f, 1f, 1f, 0.3f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 8f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(baseRoomSize, 5f, baseRoomSize);
        
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = new Color(1f, 1f, 1f, 0.4f);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }
}
