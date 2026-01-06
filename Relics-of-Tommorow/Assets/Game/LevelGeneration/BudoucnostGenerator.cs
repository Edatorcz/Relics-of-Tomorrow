using UnityEngine;

/// <summary>
/// Generátor pro Budoucnost - futuristické místnosti
/// </summary>
public class BudoucnostGenerator : RoomBasedGenerator
{
    [Header("Budoucnost Specific")]
    [SerializeField] private Color cyberWallColor = new Color(0.1f, 0.2f, 0.3f);
    [SerializeField] private Color cyberFloorColor = new Color(0.05f, 0.15f, 0.25f);
    
    // Vizuální efekty
    private Color ambientLightColor;
    private float ambientIntensity;
    private Color directionalLightColor;
    private float directionalLightIntensity;
    private Vector3 directionalLightRotation;
    private Color fogColor;
    private float fogDensity;
    private GameObject epochLighting;
    
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
    
    public override void GenerateLevel()
    {
        base.GenerateLevel();
        ApplyVisualEffects();
    }
    
    protected void ApplyVisualEffects()
    {
        // Neonové kybernetické osvětlení
        ambientLightColor = new Color(0.2f, 0.4f, 0.6f);
        ambientIntensity = 0.4f;
        directionalLightColor = new Color(0.5f, 0.7f, 1f);
        directionalLightIntensity = 0.9f;
        directionalLightRotation = new Vector3(40, 120, 0);
        
        // Modro-fialová kybernetická mlha
        fogColor = new Color(0.2f, 0.3f, 0.5f);
        fogDensity = 0.025f;
        
        SetupLighting();
        SetupFog();
        CreateNeonLights();
        CreateHologramParticles();
    }
    
    protected void SetupLighting()
    {
        epochLighting = new GameObject("EpochLighting");
        epochLighting.transform.parent = transform;
        
        RenderSettings.ambientLight = ambientLightColor;
        RenderSettings.ambientIntensity = ambientIntensity;
        
        GameObject lightObj = new GameObject("DirectionalLight");
        lightObj.transform.parent = epochLighting.transform;
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
    
    protected void CreateNeonLights()
    {
        // Přidat bodová neonová světla pro kybernetický efekt
        for (int i = 0; i < 3; i++)
        {
            GameObject lightObj = new GameObject($"NeonLight_{i}");
            lightObj.transform.parent = epochLighting.transform;
            lightObj.transform.position = new Vector3(
                Random.Range(-baseRoomSize * 0.3f, baseRoomSize * 0.3f),
                Random.Range(2f, 4f),
                Random.Range(-baseRoomSize * 0.3f, baseRoomSize * 0.3f)
            );
            
            Light pointLight = lightObj.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = new Color(0f, Random.Range(0.5f, 1f), Random.Range(0.8f, 1f));
            pointLight.intensity = 3f;
            pointLight.range = 10f;
        }
    }
    
    protected void CreateHologramParticles()
    {
        // Holografické částice
        GameObject epochParticles = new GameObject("HologramParticles");
        epochParticles.transform.parent = transform;
        epochParticles.transform.position = Vector3.up * 2f;
        
        ParticleSystem ps = epochParticles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 8f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.8f, 1.5f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0f, 0.9f, 1f, 0.6f),
            new Color(0.6f, 0f, 1f, 0.6f)
        );
        main.maxParticles = 120;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 15f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(baseRoomSize, 8f, baseRoomSize);
        
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        // Renderer pro holografický efekt
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_EmissionColor", new Color(0f, 2f, 2f));
        renderer.material.EnableKeyword("_EMISSION");
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }
}
