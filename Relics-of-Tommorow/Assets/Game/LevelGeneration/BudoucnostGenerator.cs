using UnityEngine;

/// <summary>
/// Generátor pro Budoucnost - futuristické sci-fi prostory
/// </summary>
public class BudoucnostGenerator : RoomBasedGenerator
{
    [Header("Budoucnost Specific")]
    [SerializeField] private Color cyberWallColor = new Color(0.1f, 0.2f, 0.3f);
    [SerializeField] private Color cyberFloorColor = new Color(0.05f, 0.15f, 0.25f);
    [SerializeField] private Color neonBlueColor = new Color(0f, 0.7f, 1f);
    [SerializeField] private Color neonPinkColor = new Color(1f, 0f, 0.7f);
    [SerializeField] private Color energyColor = new Color(0f, 1f, 1f);
    
    [Header("Futuristic Elements")]
    [SerializeField] private int hologramProjectorsCount = 2;
    [SerializeField] private int energyPanelsCount = 2;
    [SerializeField] private int consolesCount = 1;
    [SerializeField] private int levitatingObjectsCount = 1;
    [SerializeField] private int dataStreamsCount = 2;
    // Počet futuristických pilířů můžete přidat později
    // private int futuristicPillarsCount = 4;
    
    [Header("Advanced Sci-Fi Elements")]
    [SerializeField] private int dronesCount = 1;
    [SerializeField] private int securityRobotsCount = 1;
    [SerializeField] private int energyBarriersCount = 0;
    [SerializeField] private int teleportPortalsCount = 1;
    [SerializeField] private int gravityPlatformsCount = 1;
    [SerializeField] private int laserGridsCount = 0;
    [SerializeField] private int aiTerminalsCount = 1;
    
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
        
        float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
        GameObject roomParent = room.floor.transform.parent.gameObject;
        
        // Futuristické sloupy s energií
        CreateFuturisticPillars(room, roomParent.transform, roomSize);
        
        // Holografické projektory
        CreateHologramProjectors(room, roomParent.transform, roomSize);
        
        // Energetické panely
        CreateEnergyPanels(room, roomParent.transform, roomSize);
        
        // Konzole a terminály
        CreateConsoles(room, roomParent.transform, roomSize);
        
        // Levitující objekty
        CreateLevitatingObjects(room, roomParent.transform, roomSize);
        
        // Datové streamy
        CreateDataStreams(room, roomParent.transform, roomSize);
        
        // Pokročilé sci-fi prvky
        CreateDrones(room, roomParent.transform, roomSize);
        CreateSecurityRobots(room, roomParent.transform, roomSize);
        CreateEnergyBarriers(room, roomParent.transform, roomSize);
        CreateGravityPlatforms(room, roomParent.transform, roomSize);
        CreateLaserGrids(room, roomParent.transform, roomSize);
        CreateAITerminals(room, roomParent.transform, roomSize);
        
        // Teleportační portály (pouze v některých místnostech)
        if (Random.value > 0.6f)
        {
            CreateTeleportPortals(room, roomParent.transform, roomSize);
        }
    }
        
    protected void CreateFuturisticPillars(Room room, Transform parent, float roomSize)
    {
        GameObject pillarsParent = new GameObject("FuturisticPillars");
        pillarsParent.transform.parent = parent;
        
        float offset = roomSize / 2f - 2f;
        
        // Sloupy v rozích a středech stran
        int pillarsPerSide = 2;
        float spacing = (roomSize - 4f) / (pillarsPerSide - 1);
        
        for (int x = 0; x < pillarsPerSide; x++)
        {
            for (int z = 0; z < pillarsPerSide; z++)
            {
                Vector3 position = room.center + new Vector3(
                    -offset + x * spacing,
                    0f,
                    -offset + z * spacing
                );
                
                CreateFuturisticPillar(position, pillarsParent.transform);
            }
        }
    }
    
    protected void CreateFuturisticPillar(Vector3 position, Transform parent)
    {
        GameObject pillar = new GameObject("FuturisticPillar");
        pillar.transform.parent = parent;
        pillar.transform.position = position;
        
        // Hlavní hexagonální tělo
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "PillarShaft";
        shaft.transform.parent = pillar.transform;
        shaft.transform.localPosition = new Vector3(0, 2.5f, 0);
        shaft.transform.localScale = new Vector3(0.5f, 2.5f, 0.5f);
        
        Renderer shaftRenderer = shaft.GetComponent<Renderer>();
        shaftRenderer.material.color = cyberWallColor * 1.3f;
        shaftRenderer.material.EnableKeyword("_EMISSION");
        shaftRenderer.material.SetColor("_EmissionColor", neonBlueColor * 0.2f);
        
        // Energetické kroužky
        for (int i = 0; i < 4; i++)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "EnergyRing";
            ring.transform.parent = pillar.transform;
            ring.transform.localPosition = new Vector3(0, 0.7f + i * 1.2f, 0);
            ring.transform.localScale = new Vector3(0.55f, 0.05f, 0.55f);
            
            Renderer ringRenderer = ring.GetComponent<Renderer>();
            ringRenderer.material = new Material(Shader.Find("Unlit/Color"));
            ringRenderer.material.color = energyColor;
            ringRenderer.material.EnableKeyword("_EMISSION");
            ringRenderer.material.SetColor("_EmissionColor", energyColor * 2f);
            
            Destroy(ring.GetComponent<Collider>());
            
            // Přidat bodové světlo u každého kroužku
            GameObject lightObj = new GameObject("RingLight");
            lightObj.transform.parent = ring.transform;
            lightObj.transform.localPosition = Vector3.zero;
            
            Light pointLight = lightObj.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = energyColor;
            pointLight.intensity = 2f;
            pointLight.range = 5f;
        }
        
        // Holografická projekce na vrcholu
        GameObject holoTop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        holoTop.name = "HologramTop";
        holoTop.transform.parent = pillar.transform;
        holoTop.transform.localPosition = new Vector3(0, 5.2f, 0);
        holoTop.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        
        Renderer holoRenderer = holoTop.GetComponent<Renderer>();
        holoRenderer.material = new Material(Shader.Find("Unlit/Color"));
        holoRenderer.material.color = new Color(neonBlueColor.r, neonBlueColor.g, neonBlueColor.b, 0.5f);
        holoRenderer.material.EnableKeyword("_EMISSION");
        holoRenderer.material.SetColor("_EmissionColor", neonBlueColor * 3f);
        
        Destroy(holoTop.GetComponent<Collider>());
    }
    
    protected void CreateHologramProjectors(Room room, Transform parent, float roomSize)
    {
        GameObject projectorsParent = new GameObject("HologramProjectors");
        projectorsParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < hologramProjectorsCount; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.5f, halfSize * 0.5f),
                0f,
                Random.Range(-halfSize * 0.5f, halfSize * 0.5f)
            );
            
            CreateHologramProjector(position, projectorsParent.transform);
        }
    }
    
    protected void CreateHologramProjector(Vector3 position, Transform parent)
    {
        GameObject projector = new GameObject("HologramProjector");
        projector.transform.parent = parent;
        projector.transform.position = position;
        projector.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        
        // Základna projektoru
        GameObject base1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        base1.name = "ProjectorBase";
        base1.transform.parent = projector.transform;
        base1.transform.localPosition = new Vector3(0, 0.1f, 0);
        base1.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        base1.GetComponent<Renderer>().material.color = cyberWallColor * 1.5f;
        
        // Tělo projektoru
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "ProjectorBody";
        body.transform.parent = projector.transform;
        body.transform.localPosition = new Vector3(0, 0.4f, 0);
        body.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        
        Renderer bodyRenderer = body.GetComponent<Renderer>();
        bodyRenderer.material.color = cyberWallColor * 2f;
        bodyRenderer.material.EnableKeyword("_EMISSION");
        bodyRenderer.material.SetColor("_EmissionColor", neonPinkColor * 0.5f);
        
        // Hologram nad projektorem
        GameObject hologram = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hologram.name = "Hologram";
        hologram.transform.parent = projector.transform;
        hologram.transform.localPosition = new Vector3(0, 1.5f, 0);
        hologram.transform.localScale = new Vector3(0.6f, 0.8f, 0.1f);
        hologram.transform.localRotation = Quaternion.Euler(0, 45, 0);
        
        Renderer holoRenderer = hologram.GetComponent<Renderer>();
        holoRenderer.material = new Material(Shader.Find("Unlit/Color"));
        
        Color holoColor = Random.value > 0.5f ? neonBlueColor : neonPinkColor;
        holoRenderer.material.color = new Color(holoColor.r, holoColor.g, holoColor.b, 0.4f);
        holoRenderer.material.EnableKeyword("_EMISSION");
        holoRenderer.material.SetColor("_EmissionColor", holoColor * 2f);
        
        Destroy(hologram.GetComponent<Collider>());
        
        // Světlo projektoru
        GameObject lightObj = new GameObject("HoloLight");
        lightObj.transform.parent = projector.transform;
        lightObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        
        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = holoColor;
        pointLight.intensity = 3f;
        pointLight.range = 6f;
    }
    
    protected void CreateEnergyPanels(Room room, Transform parent, float roomSize)
    {
        GameObject panelsParent = new GameObject("EnergyPanels");
        panelsParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        // Panely na stěnách
        for (int i = 0; i < energyPanelsCount; i++)
        {
            int side = Random.Range(0, 4);
            Vector3 position = room.center;
            Quaternion rotation = Quaternion.identity;
            
            float offset = Random.Range(-halfSize * 0.6f, halfSize * 0.6f);
            float height = Random.Range(1f, 3.5f);
            
            switch (side)
            {
                case 0: // North
                    position += new Vector3(offset, height, halfSize - 0.2f);
                    rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case 1: // South
                    position += new Vector3(offset, height, -halfSize + 0.2f);
                    break;
                case 2: // East
                    position += new Vector3(halfSize - 0.2f, height, offset);
                    rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case 3: // West
                    position += new Vector3(-halfSize + 0.2f, height, offset);
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
            }
            
            CreateEnergyPanel(position, rotation, panelsParent.transform);
        }
    }
    
    protected void CreateEnergyPanel(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject panel = new GameObject("EnergyPanel");
        panel.transform.parent = parent;
        panel.transform.position = position;
        panel.transform.rotation = rotation;
        
        // Rám panelu
        GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.name = "PanelFrame";
        frame.transform.parent = panel.transform;
        frame.transform.localPosition = Vector3.zero;
        frame.transform.localScale = new Vector3(1f, 1.5f, 0.1f);
        frame.GetComponent<Renderer>().material.color = cyberWallColor * 2f;
        
        // Energetický štít/zobrazení
        GameObject energy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        energy.name = "EnergyDisplay";
        energy.transform.parent = panel.transform;
        energy.transform.localPosition = new Vector3(0, 0, -0.08f);
        energy.transform.localScale = new Vector3(0.9f, 1.4f, 0.02f);
        
        Renderer energyRenderer = energy.GetComponent<Renderer>();
        energyRenderer.material = new Material(Shader.Find("Unlit/Color"));
        energyRenderer.material.color = energyColor;
        energyRenderer.material.EnableKeyword("_EMISSION");
        energyRenderer.material.SetColor("_EmissionColor", energyColor * 3f);
        
        Destroy(energy.GetComponent<Collider>());
        
        // Přidat hexagonální mřížku (energetický vzor)
        for (int i = 0; i < 4; i++)
        {
            GameObject hex = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hex.name = "HexPattern";
            hex.transform.parent = panel.transform;
            hex.transform.localPosition = new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.5f, 0.5f),
                -0.1f
            );
            hex.transform.localScale = new Vector3(0.15f, 0.01f, 0.15f);
            
            Renderer hexRenderer = hex.GetComponent<Renderer>();
            hexRenderer.material = new Material(Shader.Find("Unlit/Color"));
            hexRenderer.material.color = neonBlueColor;
            hexRenderer.material.EnableKeyword("_EMISSION");
            hexRenderer.material.SetColor("_EmissionColor", neonBlueColor * 2f);
            
            Destroy(hex.GetComponent<Collider>());
        }
    }
    
    protected void CreateConsoles(Room room, Transform parent, float roomSize)
    {
        GameObject consolesParent = new GameObject("Consoles");
        consolesParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < consolesCount; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.5f, halfSize * 0.5f),
                0f,
                Random.Range(-halfSize * 0.5f, halfSize * 0.5f)
            );
            
            CreateConsole(position, consolesParent.transform);
        }
    }
    
    protected void CreateConsole(Vector3 position, Transform parent)
    {
        GameObject console = new GameObject("Console");
        console.transform.parent = parent;
        console.transform.position = position;
        console.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        
        // Základna konzole
        GameObject base1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        base1.name = "ConsoleBase";
        base1.transform.parent = console.transform;
        base1.transform.localPosition = new Vector3(0, 0.5f, 0);
        base1.transform.localScale = new Vector3(1f, 1f, 0.6f);
        base1.GetComponent<Renderer>().material.color = cyberWallColor * 1.8f;
        
        // Nakloněná plocha s displejem
        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        surface.name = "ConsoleSurface";
        surface.transform.parent = console.transform;
        surface.transform.localPosition = new Vector3(0, 1.05f, -0.1f);
        surface.transform.localScale = new Vector3(0.9f, 0.1f, 0.5f);
        surface.transform.localRotation = Quaternion.Euler(-30, 0, 0);
        surface.GetComponent<Renderer>().material.color = cyberWallColor * 2f;
        
        // Displej
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.name = "Display";
        screen.transform.parent = console.transform;
        screen.transform.localPosition = new Vector3(0, 1.08f, -0.1f);
        screen.transform.localScale = new Vector3(0.85f, 0.02f, 0.45f);
        screen.transform.localRotation = Quaternion.Euler(-30, 0, 0);
        
        Renderer screenRenderer = screen.GetComponent<Renderer>();
        screenRenderer.material = new Material(Shader.Find("Unlit/Color"));
        
        Color screenColor = Random.value > 0.5f ? neonBlueColor : energyColor;
        screenRenderer.material.color = screenColor;
        screenRenderer.material.EnableKeyword("_EMISSION");
        screenRenderer.material.SetColor("_EmissionColor", screenColor * 2f);
        
        Destroy(screen.GetComponent<Collider>());
        
        // Tlačítka a kontrolky
        for (int i = 0; i < 6; i++)
        {
            GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            button.name = "Button";
            button.transform.parent = console.transform;
            button.transform.localPosition = new Vector3(
                -0.3f + (i % 3) * 0.3f,
                1.02f,
                0.15f + (i / 3) * 0.15f
            );
            button.transform.localScale = new Vector3(0.06f, 0.02f, 0.06f);
            
            Color buttonColor = i % 2 == 0 ? neonBlueColor : neonPinkColor;
            Renderer buttonRenderer = button.GetComponent<Renderer>();
            buttonRenderer.material = new Material(Shader.Find("Unlit/Color"));
            buttonRenderer.material.color = buttonColor;
            buttonRenderer.material.EnableKeyword("_EMISSION");
            buttonRenderer.material.SetColor("_EmissionColor", buttonColor * 1.5f);
            
            Destroy(button.GetComponent<Collider>());
        }
    }
    
    protected void CreateLevitatingObjects(Room room, Transform parent, float roomSize)
    {
        GameObject levitatingParent = new GameObject("LevitatingObjects");
        levitatingParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < levitatingObjectsCount; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.5f, halfSize * 0.5f),
                Random.Range(1.5f, 3.5f),
                Random.Range(-halfSize * 0.5f, halfSize * 0.5f)
            );
            
            CreateLevitatingObject(position, levitatingParent.transform);
        }
    }
    
    protected void CreateLevitatingObject(Vector3 position, Transform parent)
    {
        GameObject levObj = new GameObject("LevitatingObject");
        levObj.transform.parent = parent;
        levObj.transform.position = position;
        levObj.transform.rotation = Quaternion.Euler(
            Random.Range(0f, 360f),
            Random.Range(0f, 360f),
            Random.Range(0f, 360f)
        );
        
        // Krystal nebo geometrický tvar
        PrimitiveType[] shapes = { PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Capsule };
        GameObject shape = GameObject.CreatePrimitive(shapes[Random.Range(0, shapes.Length)]);
        shape.name = "FloatingShape";
        shape.transform.parent = levObj.transform;
        shape.transform.localPosition = Vector3.zero;
        shape.transform.localScale = Vector3.one * Random.Range(0.3f, 0.6f);
        
        Renderer shapeRenderer = shape.GetComponent<Renderer>();
        shapeRenderer.material = new Material(Shader.Find("Unlit/Color"));
        
        Color objColor = Random.value > 0.5f ? energyColor : neonPinkColor;
        shapeRenderer.material.color = new Color(objColor.r, objColor.g, objColor.b, 0.6f);
        shapeRenderer.material.EnableKeyword("_EMISSION");
        shapeRenderer.material.SetColor("_EmissionColor", objColor * 2f);
        
        // Energetické částice kolem objektu
        /*ParticleSystem ps = levObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 2f;
        main.startSpeed = 0.3f;
        main.startSize = 0.1f;
        main.startColor = objColor;
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 15f;
        
        var shape2 = ps.shape;
        shape2.shapeType = ParticleSystemShapeType.Sphere;
        shape2.radius = 0.5f;
        
        // Velocity module (konstantní mód)
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = false;
        velocity.x = new ParticleSystem.MinMaxCurve(0f);
        velocity.y = new ParticleSystem.MinMaxCurve(0f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalY = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f);*/
        
        // Bodové světlo
        GameObject lightObj = new GameObject("LevLight");
        lightObj.transform.parent = levObj.transform;
        lightObj.transform.localPosition = Vector3.zero;
        
        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = objColor;
        pointLight.intensity = 2f;
        pointLight.range = 4f;
    }
    
    protected void CreateDataStreams(Room room, Transform parent, float roomSize)
    {
        GameObject streamsParent = new GameObject("DataStreams");
        streamsParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        // Vertikální datové proudy
        for (int i = 0; i < dataStreamsCount; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.7f, halfSize * 0.7f),
                0f,
                Random.Range(-halfSize * 0.7f, halfSize * 0.7f)
            );
            
            CreateDataStream(position, streamsParent.transform);
        }
    }
    
    protected void CreateDataStream(Vector3 position, Transform parent)
    {
        GameObject stream = new GameObject("DataStream");
        stream.transform.parent = parent;
        stream.transform.position = position;
        
        // Svislý paprsek světla/dat
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = "DataBeam";
        beam.transform.parent = stream.transform;
        beam.transform.localPosition = new Vector3(0, 2.5f, 0);
        beam.transform.localScale = new Vector3(0.08f, 2.5f, 0.08f);
        
        Renderer beamRenderer = beam.GetComponent<Renderer>();
        beamRenderer.material = new Material(Shader.Find("Unlit/Color"));
        
        Color streamColor = Random.value > 0.5f ? neonBlueColor : neonPinkColor;
        beamRenderer.material.color = new Color(streamColor.r, streamColor.g, streamColor.b, 0.4f);
        beamRenderer.material.EnableKeyword("_EMISSION");
        beamRenderer.material.SetColor("_EmissionColor", streamColor * 4f);
        
        Destroy(beam.GetComponent<Collider>());
        
        /*// Částice dat proudící nahoru
        ParticleSystem ps = stream.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 3f;
        main.startSpeed = 2f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = streamColor;
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 30f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.radius = 0.05f;
        shape.angle = 0f;
        
        // Velocity module (konstantní mód)
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = false;
        velocity.x = new ParticleSystem.MinMaxCurve(0f);
        velocity.y = new ParticleSystem.MinMaxCurve(0f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalY = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f);
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = streamColor;
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", streamColor * 2f);*/
    }
    
    // ========== POKROČILÉ SCI-FI PRVKY ==========
    
    protected void CreateDrones(Room room, Transform parent, float roomSize)
    {
        GameObject dronesParent = new GameObject("FlyingDrones");
        dronesParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < dronesCount; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.6f, halfSize * 0.6f),
                Random.Range(2.5f, 4.5f),
                Random.Range(-halfSize * 0.6f, halfSize * 0.6f)
            );
            
            CreateDrone(position, dronesParent.transform);
        }
    }
    
    protected void CreateDrone(Vector3 position, Transform parent)
    {
        GameObject drone = new GameObject("SecurityDrone");
        drone.transform.parent = parent;
        drone.transform.position = position;
        drone.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        
        // Tělo dronu - centrální sféra
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "DroneBody";
        body.transform.parent = drone.transform;
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
        
        Renderer bodyRenderer = body.GetComponent<Renderer>();
        bodyRenderer.material.color = cyberWallColor * 2.5f;
        bodyRenderer.material.EnableKeyword("_EMISSION");
        bodyRenderer.material.SetColor("_EmissionColor", neonBlueColor * 0.3f);
        
        // Rotory/Antigravitační generátory (4 ramena)
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f;
            Vector3 armPosition = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * 0.5f,
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad) * 0.5f
            );
            
            // Rameno
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arm.name = "DroneArm";
            arm.transform.parent = drone.transform;
            arm.transform.localPosition = armPosition * 0.5f;
            arm.transform.localRotation = Quaternion.Euler(0, 0, 90);
            arm.transform.localScale = new Vector3(0.05f, 0.25f, 0.05f);
            arm.GetComponent<Renderer>().material.color = cyberWallColor * 2f;
            
            // Rotor
            GameObject rotor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rotor.name = "DroneRotor";
            rotor.transform.parent = drone.transform;
            rotor.transform.localPosition = armPosition;
            rotor.transform.localScale = new Vector3(0.25f, 0.02f, 0.25f);
            
            Renderer rotorRenderer = rotor.GetComponent<Renderer>();
            rotorRenderer.material = new Material(Shader.Find("Unlit/Color"));
            rotorRenderer.material.color = new Color(energyColor.r, energyColor.g, energyColor.b, 0.6f);
            rotorRenderer.material.EnableKeyword("_EMISSION");
            rotorRenderer.material.SetColor("_EmissionColor", energyColor * 2f);
            
            Destroy(rotor.GetComponent<Collider>());
            
            // Světlo na konci ramene
            GameObject light = new GameObject("RotorLight");
            light.transform.parent = drone.transform;
            light.transform.localPosition = armPosition;
            
            Light pointLight = light.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = energyColor;
            pointLight.intensity = 1.5f;
            pointLight.range = 3f;
        }
        
        // Skenující laser
        GameObject scanner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        scanner.name = "ScannerLens";
        scanner.transform.parent = drone.transform;
        scanner.transform.localPosition = new Vector3(0, -0.2f, 0);
        scanner.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        
        Renderer scannerRenderer = scanner.GetComponent<Renderer>();
        scannerRenderer.material = new Material(Shader.Find("Unlit/Color"));
        scannerRenderer.material.color = neonPinkColor;
        scannerRenderer.material.EnableKeyword("_EMISSION");
        scannerRenderer.material.SetColor("_EmissionColor", neonPinkColor * 4f);
        
        Destroy(scanner.GetComponent<Collider>());
        
        // Bodové světlo skeneru
        GameObject scannerLight = new GameObject("ScannerLight");
        scannerLight.transform.parent = scanner.transform;
        scannerLight.transform.localPosition = Vector3.zero;
        
        Light scanLight = scannerLight.AddComponent<Light>();
        scanLight.type = LightType.Spot;
        scanLight.color = neonPinkColor;
        scanLight.intensity = 3f;
        scanLight.range = 8f;
        scanLight.spotAngle = 30f;
        scanLight.transform.localRotation = Quaternion.Euler(90, 0, 0);
        
        // Energetické částice kolem dronu
        ParticleSystem ps = drone.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 1f;
        main.startSpeed = 0.5f;
        main.startSize = 0.08f;
        main.startColor = energyColor;
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 10f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        /*// Velocity module (konstantní mód)
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = false;
        velocity.x = new ParticleSystem.MinMaxCurve(0f);
        velocity.y = new ParticleSystem.MinMaxCurve(0f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalY = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f);*/
    }
    
    protected void CreateSecurityRobots(Room room, Transform parent, float roomSize)
    {
        GameObject robotsParent = new GameObject("SecurityRobots");
        robotsParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < securityRobotsCount; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.5f, halfSize * 0.5f),
                0f,
                Random.Range(-halfSize * 0.5f, halfSize * 0.5f)
            );
            
            CreateSecurityRobot(position, robotsParent.transform);
        }
    }
    
    protected void CreateSecurityRobot(Vector3 position, Transform parent)
    {
        GameObject robot = new GameObject("SecurityRobot");
        robot.transform.parent = parent;
        robot.transform.position = position;
        robot.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        
        // Tělo robota
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "RobotBody";
        body.transform.parent = robot.transform;
        body.transform.localPosition = new Vector3(0, 1.2f, 0);
        body.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);
        
        Renderer bodyRenderer = body.GetComponent<Renderer>();
        bodyRenderer.material.color = cyberWallColor * 2f;
        bodyRenderer.material.EnableKeyword("_EMISSION");
        bodyRenderer.material.SetColor("_EmissionColor", neonBlueColor * 0.2f);
        
        // Hlava robota
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "RobotHead";
        head.transform.parent = robot.transform;
        head.transform.localPosition = new Vector3(0, 2.3f, 0);
        head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        head.GetComponent<Renderer>().material.color = cyberWallColor * 2.2f;
        
        // Oko/Skener
        GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.name = "RobotEye";
        eye.transform.parent = robot.transform;
        eye.transform.localPosition = new Vector3(0, 2.3f, 0.32f);
        eye.transform.localScale = new Vector3(0.25f, 0.25f, 0.15f);
        
        Renderer eyeRenderer = eye.GetComponent<Renderer>();
        eyeRenderer.material = new Material(Shader.Find("Unlit/Color"));
        eyeRenderer.material.color = neonPinkColor;
        eyeRenderer.material.EnableKeyword("_EMISSION");
        eyeRenderer.material.SetColor("_EmissionColor", neonPinkColor * 5f);
        
        Destroy(eye.GetComponent<Collider>());
        
        // Bodové světlo oka
        GameObject eyeLight = new GameObject("EyeLight");
        eyeLight.transform.parent = eye.transform;
        eyeLight.transform.localPosition = Vector3.zero;
        
        Light spotLight = eyeLight.AddComponent<Light>();
        spotLight.type = LightType.Spot;
        spotLight.color = neonPinkColor;
        spotLight.intensity = 4f;
        spotLight.range = 10f;
        spotLight.spotAngle = 45f;
        
        // Ramena/Zbraně
        for (int i = 0; i < 2; i++)
        {
            float xOffset = i == 0 ? -0.5f : 0.5f;
            
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arm.name = "RobotArm";
            arm.transform.parent = robot.transform;
            arm.transform.localPosition = new Vector3(xOffset, 1.2f, 0);
            arm.transform.localRotation = Quaternion.Euler(0, 0, 90);
            arm.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);
            arm.GetComponent<Renderer>().material.color = cyberWallColor * 1.8f;
            
            // Zbraň/Nástroj
            GameObject weapon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            weapon.name = "Weapon";
            weapon.transform.parent = robot.transform;
            weapon.transform.localPosition = new Vector3(xOffset * 1.2f, 1.2f, 0);
            weapon.transform.localScale = new Vector3(0.15f, 0.15f, 0.4f);
            
            Renderer weaponRenderer = weapon.GetComponent<Renderer>();
            weaponRenderer.material.color = cyberWallColor * 2.5f;
            weaponRenderer.material.EnableKeyword("_EMISSION");
            weaponRenderer.material.SetColor("_EmissionColor", energyColor * 0.5f);
        }
        
        // Energetické panely na těle
        for (int i = 0; i < 3; i++)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "EnergyPanel";
            panel.transform.parent = robot.transform;
            panel.transform.localPosition = new Vector3(0, 0.6f + i * 0.4f, 0.42f);
            panel.transform.localScale = new Vector3(0.3f, 0.15f, 0.02f);
            
            Renderer panelRenderer = panel.GetComponent<Renderer>();
            panelRenderer.material = new Material(Shader.Find("Unlit/Color"));
            panelRenderer.material.color = energyColor;
            panelRenderer.material.EnableKeyword("_EMISSION");
            panelRenderer.material.SetColor("_EmissionColor", energyColor * 2f);
            
            Destroy(panel.GetComponent<Collider>());
        }
    }
    
    protected void CreateEnergyBarriers(Room room, Transform parent, float roomSize)
    {
        GameObject barriersParent = new GameObject("EnergyBarriers");
        barriersParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < energyBarriersCount; i++)
        {
            // Bariéry na strategických místech
            int side = Random.Range(0, 4);
            Vector3 position = room.center;
            Quaternion rotation = Quaternion.identity;
            
            float offset = Random.Range(-halfSize * 0.4f, halfSize * 0.4f);
            
            switch (side)
            {
                case 0: // North
                    position += new Vector3(offset, 0f, halfSize * 0.5f);
                    rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case 1: // South
                    position += new Vector3(offset, 0f, -halfSize * 0.5f);
                    rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case 2: // East
                    position += new Vector3(halfSize * 0.5f, 0f, offset);
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case 3: // West
                    position += new Vector3(-halfSize * 0.5f, 0f, offset);
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
            }
            
            CreateEnergyBarrier(position, rotation, barriersParent.transform);
        }
    }
    
    protected void CreateEnergyBarrier(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject barrier = new GameObject("EnergyBarrier");
        barrier.transform.parent = parent;
        barrier.transform.position = position;
        barrier.transform.rotation = rotation;
        
        // Generátory na stranách
        for (int i = 0; i < 2; i++)
        {
            float xOffset = i == 0 ? -1.5f : 1.5f;
            
            GameObject generator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            generator.name = "BarrierGenerator";
            generator.transform.parent = barrier.transform;
            generator.transform.localPosition = new Vector3(xOffset, 1.5f, 0);
            generator.transform.localScale = new Vector3(0.3f, 1.5f, 0.3f);
            
            Renderer genRenderer = generator.GetComponent<Renderer>();
            genRenderer.material.color = cyberWallColor * 2f;
            genRenderer.material.EnableKeyword("_EMISSION");
            genRenderer.material.SetColor("_EmissionColor", energyColor * 0.3f);
            
            // Vrchol generátoru
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            top.name = "GeneratorTop";
            top.transform.parent = generator.transform;
            top.transform.localPosition = new Vector3(0, 0.7f, 0);
            top.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            
            Renderer topRenderer = top.GetComponent<Renderer>();
            topRenderer.material = new Material(Shader.Find("Unlit/Color"));
            topRenderer.material.color = energyColor;
            topRenderer.material.EnableKeyword("_EMISSION");
            topRenderer.material.SetColor("_EmissionColor", energyColor * 3f);
            
            Destroy(top.GetComponent<Collider>());
            
            // Bodové světlo
            GameObject light = new GameObject("GenLight");
            light.transform.parent = top.transform;
            light.transform.localPosition = Vector3.zero;
            
            Light pointLight = light.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = energyColor;
            pointLight.intensity = 4f;
            pointLight.range = 8f;
        }
        
        // Energetická clona
        GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shield.name = "EnergyShield";
        shield.transform.parent = barrier.transform;
        shield.transform.localPosition = new Vector3(0, 1.5f, 0);
        shield.transform.localScale = new Vector3(3f, 3f, 0.1f);
        
        Renderer shieldRenderer = shield.GetComponent<Renderer>();
        shieldRenderer.material = new Material(Shader.Find("Unlit/Color"));
        shieldRenderer.material.color = new Color(energyColor.r, energyColor.g, energyColor.b, 0.3f);
        shieldRenderer.material.EnableKeyword("_EMISSION");
        shieldRenderer.material.SetColor("_EmissionColor", energyColor * 2f);
        
        // Collider pro bariéru
        BoxCollider barrierCollider = shield.GetComponent<BoxCollider>();
        barrierCollider.isTrigger = true;
        
        /*// Energetické částice
        ParticleSystem ps = shield.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 2f;
        main.startSpeed = 0.5f;
        main.startSize = 0.1f;
        main.startColor = energyColor;
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 50f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(3f, 3f, 0.1f);
        
        // Velocity module (konstantní mód)
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = false;
        velocity.x = new ParticleSystem.MinMaxCurve(0f);
        velocity.y = new ParticleSystem.MinMaxCurve(0f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalY = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f);*/
    }
    
    protected void CreateTeleportPortals(Room room, Transform parent, float roomSize)
    {
        GameObject portalsParent = new GameObject("TeleportPortals");
        portalsParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < teleportPortalsCount; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.4f, halfSize * 0.4f),
                0f,
                Random.Range(-halfSize * 0.4f, halfSize * 0.4f)
            );
            
            // Střídavě modré a růžové portály
            Color portalColor = i % 2 == 0 ? neonBlueColor : neonPinkColor;
            CreateTeleportPortal(position, portalColor, portalsParent.transform);
        }
    }
    
    protected void CreateTeleportPortal(Vector3 position, Color portalColor, Transform parent)
    {
        GameObject portal = new GameObject("TeleportPortal");
        portal.transform.parent = parent;
        portal.transform.position = position;
        portal.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        
        // Základna portálu
        GameObject base1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        base1.name = "PortalBase";
        base1.transform.parent = portal.transform;
        base1.transform.localPosition = new Vector3(0, 0.1f, 0);
        base1.transform.localScale = new Vector3(2f, 0.1f, 2f);
        base1.GetComponent<Renderer>().material.color = cyberWallColor * 2f;
        
        // Kroužky portálu
        for (int i = 0; i < 3; i++)
        {
            GameObject ring = new GameObject("PortalRing");
            ring.transform.parent = portal.transform;
            ring.transform.localPosition = new Vector3(0, 1.5f, 0);
            ring.transform.localRotation = Quaternion.Euler(0, i * 120f, 0);
            
            // Dva sloupy pro kroužek
            for (int j = 0; j < 2; j++)
            {
                float angle = j * 180f;
                Vector3 pillarPos = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * (1.2f - i * 0.15f),
                    0.5f + i * 0.3f,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * (1.2f - i * 0.15f)
                );
                
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "RingPillar";
                pillar.transform.parent = ring.transform;
                pillar.transform.localPosition = pillarPos;
                pillar.transform.localScale = new Vector3(0.15f, 1f, 0.15f);
                
                Renderer pillarRenderer = pillar.GetComponent<Renderer>();
                pillarRenderer.material.color = cyberWallColor * 2.5f;
                pillarRenderer.material.EnableKeyword("_EMISSION");
                pillarRenderer.material.SetColor("_EmissionColor", portalColor * (0.5f + i * 0.3f));
            }
        }
        
        // Portálové okno
        GameObject portalWindow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        portalWindow.name = "PortalWindow";
        portalWindow.transform.parent = portal.transform;
        portalWindow.transform.localPosition = new Vector3(0, 1.5f, 0);
        portalWindow.transform.localRotation = Quaternion.Euler(90, 0, 0);
        portalWindow.transform.localScale = new Vector3(1.5f, 0.05f, 1.5f);
        
        Renderer windowRenderer = portalWindow.GetComponent<Renderer>();
        windowRenderer.material = new Material(Shader.Find("Unlit/Color"));
        windowRenderer.material.color = new Color(portalColor.r, portalColor.g, portalColor.b, 0.5f);
        windowRenderer.material.EnableKeyword("_EMISSION");
        windowRenderer.material.SetColor("_EmissionColor", portalColor * 4f);
        
        // Collider je trigger
        Collider windowCollider = portalWindow.GetComponent<Collider>();
        windowCollider.isTrigger = true;
        
        // Světelný kruh
        GameObject light = new GameObject("PortalLight");
        light.transform.parent = portal.transform;
        light.transform.localPosition = new Vector3(0, 1.5f, 0);
        
        Light pointLight = light.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = portalColor;
        pointLight.intensity = 6f;
        pointLight.range = 10f;
        
        /*// Spirálové částice
        ParticleSystem ps = portal.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 3f;
        main.startSpeed = 2f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startColor = portalColor;
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 40f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.7f;
        shape.position = new Vector3(0, 1.5f, 0);
        
        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(0f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0f);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(0f);
        velocityOverLifetime.orbitalX = new ParticleSystem.MinMaxCurve(2f);
        velocityOverLifetime.orbitalY = new ParticleSystem.MinMaxCurve(2f);
        velocityOverLifetime.orbitalZ = new ParticleSystem.MinMaxCurve(0f);*/
    }
    
    protected void CreateGravityPlatforms(Room room, Transform parent, float roomSize)
    {
        GameObject platformsParent = new GameObject("GravityPlatforms");
        platformsParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < gravityPlatformsCount; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.5f, halfSize * 0.5f),
                Random.Range(0.5f, 1.5f),
                Random.Range(-halfSize * 0.5f, halfSize * 0.5f)
            );
            
            CreateGravityPlatform(position, platformsParent.transform);
        }
    }
    
    protected void CreateGravityPlatform(Vector3 position, Transform parent)
    {
        GameObject platform = new GameObject("GravityPlatform");
        platform.transform.parent = parent;
        platform.transform.position = position;
        
        // Hlavní platforma
        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        surface.name = "PlatformSurface";
        surface.transform.parent = platform.transform;
        surface.transform.localPosition = Vector3.zero;
        surface.transform.localScale = new Vector3(2f, 0.1f, 2f);
        
        Renderer surfaceRenderer = surface.GetComponent<Renderer>();
        surfaceRenderer.material.color = cyberWallColor * 2f;
        surfaceRenderer.material.EnableKeyword("_EMISSION");
        surfaceRenderer.material.SetColor("_EmissionColor", neonBlueColor * 0.3f);
        
        // Gravitační generátory pod platformou
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f + 45f;
            Vector3 genPos = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * 0.7f,
                -0.3f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * 0.7f
            );
            
            GameObject generator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            generator.name = "GravityGenerator";
            generator.transform.parent = platform.transform;
            generator.transform.localPosition = genPos;
            generator.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            
            Renderer genRenderer = generator.GetComponent<Renderer>();
            genRenderer.material = new Material(Shader.Find("Unlit/Color"));
            genRenderer.material.color = neonPinkColor;
            genRenderer.material.EnableKeyword("_EMISSION");
            genRenderer.material.SetColor("_EmissionColor", neonPinkColor * 3f);
            
            Destroy(generator.GetComponent<Collider>());
            
            // Světlo generátoru
            GameObject light = new GameObject("GenLight");
            light.transform.parent = generator.transform;
            light.transform.localPosition = Vector3.zero;
            
            Light pointLight = light.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = neonPinkColor;
            pointLight.intensity = 2f;
            pointLight.range = 4f;
        }
        
        // Gravitační pole (energetické vlny)
        for (int i = 0; i < 3; i++)
        {
            GameObject wave = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wave.name = "GravityWave";
            wave.transform.parent = platform.transform;
            wave.transform.localPosition = new Vector3(0, -0.1f - i * 0.15f, 0);
            wave.transform.localScale = new Vector3(2.2f + i * 0.3f, 0.02f, 2.2f + i * 0.3f);
            
            Renderer waveRenderer = wave.GetComponent<Renderer>();
            waveRenderer.material = new Material(Shader.Find("Unlit/Color"));
            Color waveColor = new Color(neonBlueColor.r, neonBlueColor.g, neonBlueColor.b, 0.2f - i * 0.05f);
            waveRenderer.material.color = waveColor;
            waveRenderer.material.EnableKeyword("_EMISSION");
            waveRenderer.material.SetColor("_EmissionColor", neonBlueColor * (1f - i * 0.3f));
            
            Destroy(wave.GetComponent<Collider>());
        }
        
        // Částice gravitačního pole
        /*ParticleSystem ps = platform.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 2f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(-1f, 1f);
        main.startSize = 0.1f;
        main.startColor = neonBlueColor;
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.5f; // Antigravitace
        
        var emission = ps.emission;
        emission.rateOverTime = 25f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.radius = 1f;
        shape.position = new Vector3(0, -0.5f, 0);
        
        // Velocity module (konstantní mód)
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = false;
        velocity.x = new ParticleSystem.MinMaxCurve(0f);
        velocity.y = new ParticleSystem.MinMaxCurve(0f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalY = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f);*/
    }
    
    protected void CreateLaserGrids(Room room, Transform parent, float roomSize)
    {
        GameObject lasersParent = new GameObject("LaserGrids");
        lasersParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < laserGridsCount; i++)
        {
            // Lasery na strategických místech
            int side = Random.Range(0, 4);
            Vector3 position = room.center;
            Quaternion rotation = Quaternion.identity;
            
            float offset = Random.Range(-halfSize * 0.4f, halfSize * 0.4f);
            
            switch (side)
            {
                case 0: // North
                    position += new Vector3(offset, 0f, halfSize * 0.6f);
                    rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case 1: // South
                    position += new Vector3(offset, 0f, -halfSize * 0.6f);
                    rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case 2: // East
                    position += new Vector3(halfSize * 0.6f, 0f, offset);
                    rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case 3: // West
                    position += new Vector3(-halfSize * 0.6f, 0f, offset);
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
            }
            
            CreateLaserGrid(position, rotation, lasersParent.transform);
        }
    }
    
    protected void CreateLaserGrid(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject laserGrid = new GameObject("LaserGrid");
        laserGrid.transform.parent = parent;
        laserGrid.transform.position = position;
        laserGrid.transform.rotation = rotation;
        
        // Emitory laseru
        for (int i = 0; i < 2; i++)
        {
            float xOffset = i == 0 ? -1f : 1f;
            
            GameObject emitter = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            emitter.name = "LaserEmitter";
            emitter.transform.parent = laserGrid.transform;
            emitter.transform.localPosition = new Vector3(xOffset, 1.5f, 0);
            emitter.transform.localScale = new Vector3(0.2f, 1.5f, 0.2f);
            
            Renderer emitterRenderer = emitter.GetComponent<Renderer>();
            emitterRenderer.material.color = cyberWallColor * 2.5f;
            emitterRenderer.material.EnableKeyword("_EMISSION");
            emitterRenderer.material.SetColor("_EmissionColor", neonPinkColor * 0.3f);
            
            // Čočka laseru
            GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lens.name = "LaserLens";
            lens.transform.parent = emitter.transform;
            lens.transform.localPosition = new Vector3(0, 0.7f, 0);
            lens.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            
            Renderer lensRenderer = lens.GetComponent<Renderer>();
            lensRenderer.material = new Material(Shader.Find("Unlit/Color"));
            lensRenderer.material.color = neonPinkColor;
            lensRenderer.material.EnableKeyword("_EMISSION");
            lensRenderer.material.SetColor("_EmissionColor", neonPinkColor * 5f);
            
            Destroy(lens.GetComponent<Collider>());
        }
        
        // Laserové paprsky (horizontální mřížka)
        for (int i = 0; i < 4; i++)
        {
            float yPos = 0.5f + i * 0.6f;
            
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = "LaserBeam";
            beam.transform.parent = laserGrid.transform;
            beam.transform.localPosition = new Vector3(0, yPos, 0);
            beam.transform.localRotation = Quaternion.Euler(0, 0, 90);
            beam.transform.localScale = new Vector3(0.03f, 1f, 0.03f);
            
            Renderer beamRenderer = beam.GetComponent<Renderer>();
            beamRenderer.material = new Material(Shader.Find("Unlit/Color"));
            beamRenderer.material.color = neonPinkColor;
            beamRenderer.material.EnableKeyword("_EMISSION");
            beamRenderer.material.SetColor("_EmissionColor", neonPinkColor * 6f);
            
            // Collider pro laser
            CapsuleCollider laserCollider = beam.GetComponent<CapsuleCollider>();
            laserCollider.isTrigger = true;
            
            // Světelný efekt laseru
            GameObject beamLight = new GameObject("BeamLight");
            beamLight.transform.parent = beam.transform;
            beamLight.transform.localPosition = Vector3.zero;
            
            Light spotLight = beamLight.AddComponent<Light>();
            spotLight.type = LightType.Spot;
            spotLight.color = neonPinkColor;
            spotLight.intensity = 2f;
            spotLight.range = 4f;
            spotLight.spotAngle = 30f;
        }
    }
    
    protected void CreateAITerminals(Room room, Transform parent, float roomSize)
    {
        GameObject terminalsParent = new GameObject("AITerminals");
        terminalsParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < aiTerminalsCount; i++)
        {
            // Terminály na stěnách
            int side = Random.Range(0, 4);
            Vector3 position = room.center;
            Quaternion rotation = Quaternion.identity;
            
            float offset = Random.Range(-halfSize * 0.5f, halfSize * 0.5f);
            
            switch (side)
            {
                case 0: // North
                    position += new Vector3(offset, 0f, halfSize - 0.3f);
                    rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case 1: // South
                    position += new Vector3(offset, 0f, -halfSize + 0.3f);
                    break;
                case 2: // East
                    position += new Vector3(halfSize - 0.3f, 0f, offset);
                    rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case 3: // West
                    position += new Vector3(-halfSize + 0.3f, 0f, offset);
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
            }
            
            CreateAITerminal(position, rotation, terminalsParent.transform);
        }
    }
    
    protected void CreateAITerminal(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject terminal = new GameObject("AITerminal");
        terminal.transform.parent = parent;
        terminal.transform.position = position;
        terminal.transform.rotation = rotation;
        
        // Hlavní panel terminálu
        GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = "TerminalPanel";
        panel.transform.parent = terminal.transform;
        panel.transform.localPosition = new Vector3(0, 1.5f, 0);
        panel.transform.localScale = new Vector3(1.5f, 2f, 0.1f);
        panel.GetComponent<Renderer>().material.color = cyberWallColor * 2f;
        
        // Hlavní displej
        GameObject mainDisplay = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mainDisplay.name = "MainDisplay";
        mainDisplay.transform.parent = terminal.transform;
        mainDisplay.transform.localPosition = new Vector3(0, 2f, -0.08f);
        mainDisplay.transform.localScale = new Vector3(1.3f, 0.8f, 0.02f);
        
        Renderer displayRenderer = mainDisplay.GetComponent<Renderer>();
        displayRenderer.material = new Material(Shader.Find("Unlit/Color"));
        displayRenderer.material.color = neonBlueColor;
        displayRenderer.material.EnableKeyword("_EMISSION");
        displayRenderer.material.SetColor("_EmissionColor", neonBlueColor * 3f);
        
        Destroy(mainDisplay.GetComponent<Collider>());
        
        // Holografická AI projekce nad terminálem
        GameObject aiHolo = new GameObject("AI_Hologram");
        aiHolo.transform.parent = terminal.transform;
        aiHolo.transform.localPosition = new Vector3(0, 3.2f, -0.5f);
        
        // Holografická hlava/ikona AI
        GameObject holoHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        holoHead.name = "HoloHead";
        holoHead.transform.parent = aiHolo.transform;
        holoHead.transform.localPosition = Vector3.zero;
        holoHead.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        
        Renderer holoRenderer = holoHead.GetComponent<Renderer>();
        holoRenderer.material = new Material(Shader.Find("Unlit/Color"));
        holoRenderer.material.color = new Color(neonBlueColor.r, neonBlueColor.g, neonBlueColor.b, 0.5f);
        holoRenderer.material.EnableKeyword("_EMISSION");
        holoRenderer.material.SetColor("_EmissionColor", neonBlueColor * 4f);
        
        Destroy(holoHead.GetComponent<Collider>());
        
        // Kroužky kolem hologramu
        for (int i = 0; i < 3; i++)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "HoloRing";
            ring.transform.parent = aiHolo.transform;
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localRotation = Quaternion.Euler(90, 0, i * 60f);
            ring.transform.localScale = new Vector3(0.6f + i * 0.1f, 0.02f, 0.6f + i * 0.1f);
            
            Renderer ringRenderer = ring.GetComponent<Renderer>();
            ringRenderer.material = new Material(Shader.Find("Unlit/Color"));
            Color ringColor = new Color(neonBlueColor.r, neonBlueColor.g, neonBlueColor.b, 0.3f);
            ringRenderer.material.color = ringColor;
            ringRenderer.material.EnableKeyword("_EMISSION");
            ringRenderer.material.SetColor("_EmissionColor", neonBlueColor * 2f);
            
            Destroy(ring.GetComponent<Collider>());
        }
        
        // Světlo hologramu
        GameObject holoLight = new GameObject("HoloLight");
        holoLight.transform.parent = aiHolo.transform;
        holoLight.transform.localPosition = Vector3.zero;
        
        Light pointLight = holoLight.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = neonBlueColor;
        pointLight.intensity = 3f;
        pointLight.range = 5f;
        
        // Pod-displeje a ovládací prvky
        for (int i = 0; i < 3; i++)
        {
            GameObject subDisplay = GameObject.CreatePrimitive(PrimitiveType.Cube);
            subDisplay.name = "SubDisplay";
            subDisplay.transform.parent = terminal.transform;
            subDisplay.transform.localPosition = new Vector3(
                -0.5f + i * 0.5f,
                1.2f,
                -0.08f
            );
            subDisplay.transform.localScale = new Vector3(0.3f, 0.25f, 0.02f);
            
            Renderer subRenderer = subDisplay.GetComponent<Renderer>();
            subRenderer.material = new Material(Shader.Find("Unlit/Color"));
            Color subColor = i % 2 == 0 ? energyColor : neonPinkColor;
            subRenderer.material.color = subColor;
            subRenderer.material.EnableKeyword("_EMISSION");
            subRenderer.material.SetColor("_EmissionColor", subColor * 2f);
            
            Destroy(subDisplay.GetComponent<Collider>());
        }
        
        // Klávesnice/ovládací panel
        GameObject keyboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        keyboard.name = "Keyboard";
        keyboard.transform.parent = terminal.transform;
        keyboard.transform.localPosition = new Vector3(0, 0.8f, -0.15f);
        keyboard.transform.localRotation = Quaternion.Euler(-15, 0, 0);
        keyboard.transform.localScale = new Vector3(1.2f, 0.05f, 0.4f);
        keyboard.GetComponent<Renderer>().material.color = cyberWallColor * 1.8f;
        
        // Tlačítka na klávesnici
        for (int i = 0; i < 12; i++)
        {
            GameObject key = GameObject.CreatePrimitive(PrimitiveType.Cube);
            key.name = "Key";
            key.transform.parent = keyboard.transform;
            key.transform.localPosition = new Vector3(
                -0.4f + (i % 4) * 0.25f,
                0.03f,
                -0.12f + (i / 4) * 0.12f
            );
            key.transform.localScale = new Vector3(0.15f, 0.03f, 0.08f);
            
            Color keyColor = Random.value > 0.7f ? neonBlueColor : cyberWallColor * 2.5f;
            Renderer keyRenderer = key.GetComponent<Renderer>();
            keyRenderer.material.color = keyColor;
            if (keyColor == neonBlueColor)
            {
                keyRenderer.material.EnableKeyword("_EMISSION");
                keyRenderer.material.SetColor("_EmissionColor", neonBlueColor * 1.5f);
            }
            
            Destroy(key.GetComponent<Collider>());
        }
        
        /*// Datové částice z terminálu
        ParticleSystem ps = aiHolo.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 2f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSize = 0.05f;
        main.startColor = neonBlueColor;
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 15f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;
        
        // Velocity module (konstantní mód)
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = false;
        velocity.x = new ParticleSystem.MinMaxCurve(0f);
        velocity.y = new ParticleSystem.MinMaxCurve(0f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalY = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f);*/
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
        
        /*ParticleSystem ps = epochParticles.AddComponent<ParticleSystem>();
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
        velocity.x = new ParticleSystem.MinMaxCurve(0f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalY = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f);
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        // Renderer pro holografický efekt
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_EmissionColor", new Color(0f, 2f, 2f));
        renderer.material.EnableKeyword("_EMISSION");
        renderer.renderMode = ParticleSystemRenderMode.Billboard;*/
    }
}
