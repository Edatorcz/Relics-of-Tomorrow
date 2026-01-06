using UnityEngine;

/// <summary>
/// Generátor pro Středověk - hrady a věže
/// </summary>
public class StredovekGenerator : RoomBasedGenerator
{
    [Header("Středověk Specific")]
    [SerializeField] private Color castleWallColor = new Color(0.5f, 0.5f, 0.55f);
    [SerializeField] private Color castleFloorColor = new Color(0.4f, 0.4f, 0.45f);
    
    // Vizuální efekty
    private Color ambientLightColor;
    private float ambientIntensity;
    private Color directionalLightColor;
    private float directionalLightIntensity;
    private Vector3 directionalLightRotation;
    private Color fogColor;
    private float fogDensity;
    
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
    
    public override void GenerateLevel()
    {
        base.GenerateLevel();
        ApplyVisualEffects();
    }
    
    protected void ApplyVisualEffects()
    {
        // Šedé oblačné osvětlení
        ambientLightColor = new Color(0.6f, 0.65f, 0.7f);
        ambientIntensity = 0.5f;
        directionalLightColor = new Color(0.8f, 0.85f, 0.9f);
        directionalLightIntensity = 0.8f;
        directionalLightRotation = new Vector3(55, -60, 0);
        
        // Mlhavá atmosféra
        fogColor = new Color(0.7f, 0.75f, 0.8f);
        fogDensity = 0.02f;
        
        SetupLighting();
        SetupFog();
        CreateWeatherParticles();
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
    
    protected void CreateWeatherParticles()
    {
        // Déšť nebo sníh
        GameObject epochParticles = new GameObject("WeatherParticles");
        epochParticles.transform.parent = transform;
        epochParticles.transform.position = Vector3.up * 10f;
        
        ParticleSystem ps = epochParticles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 4f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 7f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
        main.startColor = new Color(0.85f, 0.9f, 0.95f, 0.7f);
        main.maxParticles = 300;
        main.gravityModifier = 1.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 80f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(baseRoomSize * 1.5f, 1f, baseRoomSize * 1.5f);
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = new Color(0.9f, 0.95f, 1f, 0.8f);
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 2f;
    }
}
