using UnityEngine;

/// <summary>
/// Generátor pro Současnost - moderní průmyslové a kancelářské prostory
/// </summary>
public class SoucasnostGenerator : RoomBasedGenerator
{
    [Header("Současnost Specific")]
    [SerializeField] private Color modernWallColor = new Color(0.9f, 0.9f, 0.95f);
    [SerializeField] private Color modernFloorColor = new Color(0.7f, 0.7f, 0.75f);
    [SerializeField] private Color concreteColor = new Color(0.6f, 0.6f, 0.65f);
    [SerializeField] private Color metalColor = new Color(0.5f, 0.55f, 0.6f);
    
    [Header("Modern Elements")]
    // Počet betonových pilířů - můžete přidat později
    // private int concretePillarsCount = 4;
    [SerializeField] private int officeDesksCount = 3;
    [SerializeField] private int computersCount = 4;
    [SerializeField] private int ventShaftsCount = 2;
    [SerializeField] private int electricalBoxesCount = 3;
    [SerializeField] private int neonLightsCount = 5;
    
    // Vizuální efekty
    private Color ambientLightColor;
    private float ambientIntensity;
    private Color directionalLightColor;
    private float directionalLightIntensity;
    private Vector3 directionalLightRotation;
    private Color fogColor;
    private float fogDensity;
    private GameObject epochLighting;
    
    protected override Color GetWallColor() => modernWallColor;
    protected override Color GetFloorColor() => modernFloorColor;
    
    protected override void CreateRoomStructure(Room room)
    {
        base.CreateRoomStructure(room);
        
        float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
        GameObject roomParent = room.floor.transform.parent.gameObject;
        
        // Betonové sloupy
        CreateConcretePillars(room, roomParent.transform, roomSize);
        
        // Kancelářský nábytek
        CreateOfficeFurniture(room, roomParent.transform, roomSize);
        
        // Počítače a technika
        CreateComputers(room, roomParent.transform, roomSize);
        
        // Ventilační šachty
        CreateVentilationShafts(room, roomParent.transform, roomSize);
        
        // Elektrické rozvaděče a kabeláž
        CreateElectricalBoxes(room, roomParent.transform, roomSize);
        
        // Skleněné přepážky
        CreateGlassPanels(room, roomParent.transform, roomSize);
    }
    
    protected void CreateConcretePillars(Room room, Transform parent, float roomSize)
    {
        GameObject pillarsParent = new GameObject("ConcretePillars");
        pillarsParent.transform.parent = parent;
        
        float offset = roomSize / 2f - 2f;
        
        // Sloupy v pravidelné mřížce
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
                
                CreateConcretePillar(position, pillarsParent.transform);
            }
        }
    }
    
    protected void CreateConcretePillar(Vector3 position, Transform parent)
    {
        GameObject pillar = new GameObject("ConcretePillar");
        pillar.transform.parent = parent;
        pillar.transform.position = position;
        
        // Hlavní betonový sloup
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.name = "PillarShaft";
        shaft.transform.parent = pillar.transform;
        shaft.transform.localPosition = new Vector3(0, 2.5f, 0);
        shaft.transform.localScale = new Vector3(0.6f, 5f, 0.6f);
        shaft.GetComponent<Renderer>().material.color = concreteColor;
        
        // Kovová základna
        GameObject base1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        base1.name = "MetalBase";
        base1.transform.parent = pillar.transform;
        base1.transform.localPosition = new Vector3(0, 0.1f, 0);
        base1.transform.localScale = new Vector3(0.8f, 0.2f, 0.8f);
        base1.GetComponent<Renderer>().material.color = metalColor;
        
        // Kovová hlavice
        GameObject capital = GameObject.CreatePrimitive(PrimitiveType.Cube);
        capital.name = "MetalCapital";
        capital.transform.parent = pillar.transform;
        capital.transform.localPosition = new Vector3(0, 4.9f, 0);
        capital.transform.localScale = new Vector3(0.8f, 0.2f, 0.8f);
        capital.GetComponent<Renderer>().material.color = metalColor;
    }
    
    protected void CreateOfficeFurniture(Room room, Transform parent, float roomSize)
    {
        GameObject furnitureParent = new GameObject("OfficeFurniture");
        furnitureParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        // Rozmístit kancelářské stoly
        for (int i = 0; i < officeDesksCount; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.6f, halfSize * 0.6f),
                0f,
                Random.Range(-halfSize * 0.6f, halfSize * 0.6f)
            );
            
            CreateOfficeDesk(position, furnitureParent.transform);
        }
        
        // Přidat kovové skříně
        for (int i = 0; i < 4; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.75f, halfSize * 0.75f),
                0f,
                Random.Range(-halfSize * 0.75f, halfSize * 0.75f)
            );
            
            CreateFilingCabinet(position, furnitureParent.transform);
        }
    }
    
    protected void CreateOfficeDesk(Vector3 position, Transform parent)
    {
        GameObject desk = new GameObject("OfficeDesk");
        desk.transform.parent = parent;
        desk.transform.position = position;
        desk.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        
        // Deska stolu (moderní design)
        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
        top.name = "DeskTop";
        top.transform.parent = desk.transform;
        top.transform.localPosition = new Vector3(0, 0.75f, 0);
        top.transform.localScale = new Vector3(1.8f, 0.08f, 1f);
        top.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.25f);
        
        // Kovové nohy
        Vector3[] legPositions = new Vector3[]
        {
            new Vector3(0.8f, 0.375f, 0.45f),
            new Vector3(0.8f, 0.375f, -0.45f),
            new Vector3(-0.8f, 0.375f, 0.45f),
            new Vector3(-0.8f, 0.375f, -0.45f)
        };
        
        foreach (Vector3 legPos in legPositions)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = "DeskLeg";
            leg.transform.parent = desk.transform;
            leg.transform.localPosition = legPos;
            leg.transform.localScale = new Vector3(0.06f, 0.75f, 0.06f);
            leg.GetComponent<Renderer>().material.color = metalColor;
        }
        
        // Kancelářská židle
        GameObject chair = new GameObject("OfficeChair");
        chair.transform.parent = desk.transform;
        chair.transform.localPosition = new Vector3(0, 0, -0.8f);
        
        // Sedák
        GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seat.name = "Seat";
        seat.transform.parent = chair.transform;
        seat.transform.localPosition = new Vector3(0, 0.5f, 0);
        seat.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        seat.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.15f);
        
        // Opěradlo
        GameObject backrest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backrest.name = "Backrest";
        backrest.transform.parent = chair.transform;
        backrest.transform.localPosition = new Vector3(0, 0.85f, 0.2f);
        backrest.transform.localScale = new Vector3(0.5f, 0.7f, 0.1f);
        backrest.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.15f);
    }
    
    protected void CreateFilingCabinet(Vector3 position, Transform parent)
    {
        GameObject cabinet = new GameObject("FilingCabinet");
        cabinet.transform.parent = parent;
        cabinet.transform.position = position;
        cabinet.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        
        // Tělo skříně
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "CabinetBody";
        body.transform.parent = cabinet.transform;
        body.transform.localPosition = new Vector3(0, 0.9f, 0);
        body.transform.localScale = new Vector3(0.8f, 1.8f, 0.6f);
        body.GetComponent<Renderer>().material.color = metalColor * 1.1f;
        
        // Zásuvky (čáry na přední straně)
        for (int i = 0; i < 3; i++)
        {
            GameObject drawer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            drawer.name = "Drawer";
            drawer.transform.parent = cabinet.transform;
            drawer.transform.localPosition = new Vector3(0, 0.3f + i * 0.6f, 0.31f);
            drawer.transform.localScale = new Vector3(0.75f, 0.05f, 0.02f);
            drawer.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.25f);
            
            // Odstranit collider
            Destroy(drawer.GetComponent<Collider>());
        }
    }
    
    protected void CreateComputers(Room room, Transform parent, float roomSize)
    {
        GameObject computersParent = new GameObject("Computers");
        computersParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < computersCount; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.6f, halfSize * 0.6f),
                0.75f, // Na stolech
                Random.Range(-halfSize * 0.6f, halfSize * 0.6f)
            );
            
            CreateComputer(position, computersParent.transform);
        }
    }
    
    protected void CreateComputer(Vector3 position, Transform parent)
    {
        GameObject computer = new GameObject("Computer");
        computer.transform.parent = parent;
        computer.transform.position = position;
        computer.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        
        // Monitor
        GameObject monitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        monitor.name = "Monitor";
        monitor.transform.parent = computer.transform;
        monitor.transform.localPosition = new Vector3(0, 0.3f, 0);
        monitor.transform.localScale = new Vector3(0.5f, 0.4f, 0.05f);
        monitor.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.15f);
        
        // Obrazovka (svítící)
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.name = "Screen";
        screen.transform.parent = computer.transform;
        screen.transform.localPosition = new Vector3(0, 0.3f, -0.03f);
        screen.transform.localScale = new Vector3(0.45f, 0.35f, 0.01f);
        
        Renderer screenRenderer = screen.GetComponent<Renderer>();
        screenRenderer.material = new Material(Shader.Find("Unlit/Color"));
        screenRenderer.material.color = new Color(0.2f, 0.5f, 0.7f);
        screenRenderer.material.EnableKeyword("_EMISSION");
        screenRenderer.material.SetColor("_EmissionColor", new Color(0.2f, 0.5f, 0.7f) * 0.5f);
        
        // Odstranit collider z obrazovky
        Destroy(screen.GetComponent<Collider>());
        
        // Podstavec
        GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stand.name = "Stand";
        stand.transform.parent = computer.transform;
        stand.transform.localPosition = new Vector3(0, 0.05f, 0);
        stand.transform.localScale = new Vector3(0.1f, 0.1f, 0.15f);
        stand.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.25f);
        
        // Počítačová skříň
        GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tower.name = "Tower";
        tower.transform.parent = computer.transform;
        tower.transform.localPosition = new Vector3(-0.4f, -0.25f, 0);
        tower.transform.localScale = new Vector3(0.2f, 0.5f, 0.4f);
        tower.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.2f);
        
        // LED kontrolka
        GameObject led = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        led.name = "LED";
        led.transform.parent = computer.transform;
        led.transform.localPosition = new Vector3(-0.3f, -0.15f, 0.21f);
        led.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
        
        Renderer ledRenderer = led.GetComponent<Renderer>();
        ledRenderer.material = new Material(Shader.Find("Unlit/Color"));
        ledRenderer.material.color = new Color(0f, 1f, 0f);
        ledRenderer.material.EnableKeyword("_EMISSION");
        ledRenderer.material.SetColor("_EmissionColor", new Color(0f, 1f, 0f));
        
        Destroy(led.GetComponent<Collider>());
    }
    
    protected void CreateVentilationShafts(Room room, Transform parent, float roomSize)
    {
        GameObject ventsParent = new GameObject("VentilationShafts");
        ventsParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < ventShaftsCount; i++)
        {
            int side = Random.Range(0, 4);
            Vector3 position = room.center;
            Quaternion rotation = Quaternion.identity;
            
            float offset = Random.Range(-halfSize * 0.5f, halfSize * 0.5f);
            float height = Random.Range(3.5f, 4.5f);
            
            switch (side)
            {
                case 0: // North
                    position += new Vector3(offset, height, halfSize - 0.3f);
                    rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case 1: // South
                    position += new Vector3(offset, height, -halfSize + 0.3f);
                    break;
                case 2: // East
                    position += new Vector3(halfSize - 0.3f, height, offset);
                    rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case 3: // West
                    position += new Vector3(-halfSize + 0.3f, height, offset);
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
            }
            
            CreateVentShaft(position, rotation, ventsParent.transform);
        }
    }
    
    protected void CreateVentShaft(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject vent = new GameObject("VentShaft");
        vent.transform.parent = parent;
        vent.transform.position = position;
        vent.transform.rotation = rotation;
        
        // Kovový kryt
        GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cover.name = "VentCover";
        cover.transform.parent = vent.transform;
        cover.transform.localPosition = Vector3.zero;
        cover.transform.localScale = new Vector3(0.8f, 0.6f, 0.2f);
        cover.GetComponent<Renderer>().material.color = metalColor * 0.9f;
        
        // Mřížka (pruhy)
        for (int i = 0; i < 4; i++)
        {
            GameObject slat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slat.name = "VentSlat";
            slat.transform.parent = vent.transform;
            slat.transform.localPosition = new Vector3(0, -0.2f + i * 0.15f, -0.05f);
            slat.transform.localScale = new Vector3(0.7f, 0.03f, 0.05f);
            slat.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.25f);
            
            Destroy(slat.GetComponent<Collider>());
        }
    }
    
    protected void CreateElectricalBoxes(Room room, Transform parent, float roomSize)
    {
        GameObject electricalParent = new GameObject("ElectricalBoxes");
        electricalParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < electricalBoxesCount; i++)
        {
            int side = Random.Range(0, 4);
            Vector3 position = room.center;
            Quaternion rotation = Quaternion.identity;
            
            float offset = Random.Range(-halfSize * 0.6f, halfSize * 0.6f);
            float height = Random.Range(1.2f, 2.5f);
            
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
            
            CreateElectricalBox(position, rotation, electricalParent.transform);
        }
    }
    
    protected void CreateElectricalBox(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject box = new GameObject("ElectricalBox");
        box.transform.parent = parent;
        box.transform.position = position;
        box.transform.rotation = rotation;
        
        // Skříň
        GameObject casing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        casing.name = "BoxCasing";
        casing.transform.parent = box.transform;
        casing.transform.localPosition = Vector3.zero;
        casing.transform.localScale = new Vector3(0.5f, 0.7f, 0.15f);
        casing.GetComponent<Renderer>().material.color = new Color(0.8f, 0.7f, 0.2f); // Žlutá bezpečnostní barva
        
        // Výstražný symbol
        GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Cube);
        warning.name = "WarningSign";
        warning.transform.parent = box.transform;
        warning.transform.localPosition = new Vector3(0, 0.15f, -0.08f);
        warning.transform.localScale = new Vector3(0.25f, 0.25f, 0.01f);
        warning.GetComponent<Renderer>().material.color = new Color(1f, 0.2f, 0f);
        
        Destroy(warning.GetComponent<Collider>());
        
        // Kabeláž vedoucí od boxu
        for (int i = 0; i < 2; i++)
        {
            GameObject cable = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cable.name = "Cable";
            cable.transform.parent = box.transform;
            cable.transform.localPosition = new Vector3(-0.15f + i * 0.3f, -0.5f, 0);
            cable.transform.localScale = new Vector3(0.03f, 0.3f, 0.03f);
            cable.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.15f);
        }
    }
    
    protected void CreateGlassPanels(Room room, Transform parent, float roomSize)
    {
        GameObject panelsParent = new GameObject("GlassPanels");
        panelsParent.transform.parent = parent;
        
        // Několik skleněných přepážek v místnosti
        for (int i = 0; i < 3; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-roomSize * 0.3f, roomSize * 0.3f),
                1.5f,
                Random.Range(-roomSize * 0.3f, roomSize * 0.3f)
            );
            
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "GlassPanel";
            panel.transform.parent = panelsParent.transform;
            panel.transform.position = position;
            panel.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            panel.transform.localScale = new Vector3(2f, 3f, 0.1f);
            
            Renderer renderer = panel.GetComponent<Renderer>();
            renderer.material.color = new Color(0.7f, 0.85f, 0.9f, 0.3f);
            
            // Kovový rám
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame";
            frame.transform.parent = panel.transform;
            frame.transform.localPosition = Vector3.zero;
            frame.transform.localScale = new Vector3(1.02f, 1.02f, 1.5f);
            frame.GetComponent<Renderer>().material.color = metalColor;
            
            Destroy(frame.GetComponent<Collider>());
        }
    }

    public override void GenerateLevel()
    {
        base.GenerateLevel();
        ApplyVisualEffects();
        CreateNeonLighting();
    }
    
    protected void CreateNeonLighting()
    {
        GameObject lightsParent = new GameObject("NeonLights");
        lightsParent.transform.parent = transform;
        
        foreach (Room room in rooms)
        {
            float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
            float halfSize = roomSize / 2f;
            
            // Neonové trubice na stropě
            for (int i = 0; i < neonLightsCount; i++)
            {
                Vector3 position = room.center + new Vector3(
                    Random.Range(-halfSize * 0.7f, halfSize * 0.7f),
                    4.5f,
                    Random.Range(-halfSize * 0.7f, halfSize * 0.7f)
                );
                
                CreateNeonLight(position, lightsParent.transform);
            }
        }
    }
    
    protected void CreateNeonLight(Vector3 position, Transform parent)
    {
        GameObject neon = new GameObject("NeonLight");
        neon.transform.parent = parent;
        neon.transform.position = position;
        neon.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 180f), 0);
        
        // Neonová trubice
        GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tube.name = "NeonTube";
        tube.transform.parent = neon.transform;
        tube.transform.localPosition = Vector3.zero;
        tube.transform.localScale = new Vector3(0.08f, 1f, 0.08f);
        tube.transform.localRotation = Quaternion.Euler(0, 0, 90);
        
        Renderer tubeRenderer = tube.GetComponent<Renderer>();
        tubeRenderer.material = new Material(Shader.Find("Unlit/Color"));
        tubeRenderer.material.color = new Color(1f, 1f, 1f);
        tubeRenderer.material.EnableKeyword("_EMISSION");
        tubeRenderer.material.SetColor("_EmissionColor", new Color(1f, 1f, 1f) * 2f);
        
        // Světlo
        GameObject lightObj = new GameObject("Light");
        lightObj.transform.parent = neon.transform;
        lightObj.transform.localPosition = Vector3.zero;
        
        Light areaLight = lightObj.AddComponent<Light>();
        areaLight.type = LightType.Point;
        areaLight.color = new Color(1f, 1f, 1f);
        areaLight.intensity = 3f;
        areaLight.range = 6f;
        areaLight.shadows = LightShadows.Soft;
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
