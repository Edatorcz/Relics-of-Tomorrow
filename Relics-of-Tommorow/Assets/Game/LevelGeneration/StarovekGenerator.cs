using UnityEngine;

/// <summary>
/// Generátor pro Starověk - monumentální chrámy a paláce
/// </summary>
public class StarovekGenerator : RoomBasedGenerator
{
    [Header("Starověk Specific")]
    [SerializeField] private Color stoneWallColor = new Color(0.7f, 0.6f, 0.5f);
    [SerializeField] private Color stoneFloorColor = new Color(0.6f, 0.55f, 0.5f);
    [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f);
    
    [Header("Ancient Decorations")]
    // Počet monumentálních pilířů můžete přidat později
    // private int monumentalPillarsCount = 8;
    [SerializeField] private int hieroglyphsCount = 12;
    [SerializeField] private int statuesCount = 4;
    [SerializeField] private int oilLampsCount = 8;
    [SerializeField] private float ruinedPillarChance = 0.3f; // 30% šance na rozpadlý sloup
    
    [Header("Weather System")]
    [SerializeField] private float weatherCheckInterval = 5f; // Kontrolovat počasí každých 5s
    [SerializeField] private float sandstormStartChance = 0.15f; // 15% šance na začátek bouře
    [SerializeField] private float sandstormEndChance = 0.3f; // 30% šance na konec bouře (vyšší = kratší bouře)
    [SerializeField] private float sandstormMovementMultiplier = 0.5f; // 50% rychlosti při bouři
    
    // Weather system
    private ParticleSystem sandParticleSystem;
    private bool isSandstorm = false;
    private float weatherTimer = 0f;
    
    // Vizuální efekty
    private Color ambientLightColor;
    private float ambientIntensity;
    private Color directionalLightColor;
    private float directionalLightIntensity;
    private Vector3 directionalLightRotation;
    private Color fogColor;
    private float fogDensity;
    private GameObject epochLighting;
    
    protected override Color GetWallColor() => stoneWallColor;
    protected override Color GetFloorColor() => stoneFloorColor;
    
    protected override void CreateRoomStructure(Room room)
    {
        base.CreateRoomStructure(room);
        
        float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
        GameObject roomParent = room.floor.transform.parent.gameObject;
        
        // Monumentální sloupy po obvodu
        CreateMonumentalPillars(room, roomParent.transform, roomSize);
        
        // Hieroglyfy a reliéfy na zdech
        CreateHieroglyphs(room, roomParent.transform, roomSize);
        
        // Písečné duny v rozích
        CreateSandDunes(room, roomParent.transform, roomSize);
        
        // Zlaté ozdoby
        CreateGoldenDecorations(room, roomParent.transform, roomSize);
    }
    
    protected void CreateMonumentalPillars(Room room, Transform parent, float roomSize)
    {
        GameObject pillarParent = new GameObject("MonumentalPillars");
        pillarParent.transform.parent = parent;
        
        float offset = roomSize / 2f - 2.5f;
        
        // Sloupy v rozích
        Vector3[] cornerPositions = new Vector3[]
        {
            room.center + new Vector3(offset, 0f, offset),
            room.center + new Vector3(offset, 0f, -offset),
            room.center + new Vector3(-offset, 0f, offset),
            room.center + new Vector3(-offset, 0f, -offset)
        };
        
        foreach (Vector3 pos in cornerPositions)
        {
            CreateEgyptianPillar(pos, pillarParent.transform);
        }
        
        // Další sloupy podél stěn
        int pillarsPerWall = 2;
        float spacing = roomSize / (pillarsPerWall + 1);
        
        // Severní a jižní strana
        for (int i = 1; i <= pillarsPerWall; i++)
        {
            float x = -roomSize / 2f + spacing * i;
            CreateEgyptianPillar(room.center + new Vector3(x, 0f, offset), pillarParent.transform);
            CreateEgyptianPillar(room.center + new Vector3(x, 0f, -offset), pillarParent.transform);
        }
        
        // Východní a západní strana
        for (int i = 1; i <= pillarsPerWall; i++)
        {
            float z = -roomSize / 2f + spacing * i;
            CreateEgyptianPillar(room.center + new Vector3(offset, 0f, z), pillarParent.transform);
            CreateEgyptianPillar(room.center + new Vector3(-offset, 0f, z), pillarParent.transform);
        }
    }
    
    protected void CreateEgyptianPillar(Vector3 position, Transform parent)
    {
        GameObject pillar = new GameObject("EgyptianPillar");
        pillar.transform.parent = parent;
        pillar.transform.position = position;
        
        // Náhodně rozhodnout, zda bude sloup rozpadlý
        bool isRuined = Random.value < ruinedPillarChance;
        
        if (isRuined)
        {
            CreateRuinedPillar(pillar.transform);
        }
        else
        {
            CreateIntactPillar(pillar.transform);
        }
    }
    
    protected void CreateIntactPillar(Transform parent)
    {
        // Základna sloupu
        GameObject base1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        base1.name = "Base";
        base1.transform.parent = parent;
        base1.transform.localPosition = new Vector3(0, 0.3f, 0);
        base1.transform.localScale = new Vector3(1.2f, 0.6f, 1.2f);
        base1.GetComponent<Renderer>().material.color = stoneWallColor;
        
        // Hlavní tělo sloupu
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Shaft";
        shaft.transform.parent = parent;
        shaft.transform.localPosition = new Vector3(0, 3.5f, 0);
        shaft.transform.localScale = new Vector3(0.8f, 3f, 0.8f);
        shaft.GetComponent<Renderer>().material.color = new Color(0.75f, 0.65f, 0.55f);
        
        // Hlavice sloupu
        GameObject capital = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        capital.name = "Capital";
        capital.transform.parent = parent;
        capital.transform.localPosition = new Vector3(0, 6.7f, 0);
        capital.transform.localScale = new Vector3(1.1f, 0.4f, 1.1f);
        capital.GetComponent<Renderer>().material.color = goldColor * 0.7f;
        
        // Zlatá ozdoba na vrcholu
        GameObject decoration = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        decoration.name = "GoldDecoration";
        decoration.transform.parent = parent;
        decoration.transform.localPosition = new Vector3(0, 7.2f, 0);
        decoration.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        decoration.GetComponent<Renderer>().material.color = goldColor;
    }
    
    protected void CreateRuinedPillar(Transform parent)
    {
        // Základna - vždy přítomná, ale poškozená
        GameObject base1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        base1.name = "RuinedBase";
        base1.transform.parent = parent;
        base1.transform.localPosition = new Vector3(0, 0.3f, 0);
        base1.transform.localScale = new Vector3(1.2f, 0.6f, 1.2f);
        base1.transform.rotation = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(0f, 360f), Random.Range(-5f, 5f));
        base1.GetComponent<Renderer>().material.color = stoneWallColor * 0.8f; // Tmavší - opotřebovaná
        
        // Rozpadlé části sloupu - náhodná výška
        float remainingHeight = Random.Range(1f, 4f); // Sloup zbyl jen z 1-4m
        int segments = Random.Range(2, 4); // 2-3 rozpadlé segmenty
        
        for (int i = 0; i < segments; i++)
        {
            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            segment.name = $"RuinedSegment_{i}";
            segment.transform.parent = parent;
            
            float segmentHeight = remainingHeight / segments;
            float yPos = 0.6f + (segmentHeight * i) + (segmentHeight / 2f);
            
            segment.transform.localPosition = new Vector3(
                Random.Range(-0.1f, 0.1f),
                yPos,
                Random.Range(-0.1f, 0.1f)
            );
            segment.transform.localScale = new Vector3(0.8f, segmentHeight / 2f, 0.8f);
            segment.transform.rotation = Quaternion.Euler(
                Random.Range(-8f, 8f),
                Random.Range(0f, 360f),
                Random.Range(-8f, 8f)
            );
            
            // Opotřebovaný kámen
            segment.GetComponent<Renderer>().material.color = new Color(
                0.65f + Random.Range(-0.1f, 0.1f),
                0.55f + Random.Range(-0.1f, 0.1f),
                0.45f + Random.Range(-0.1f, 0.1f)
            );
        }
        
        // Padlé kusy kolem sloupu
        int fallenPieces = Random.Range(2, 4);
        for (int i = 0; i < fallenPieces; i++)
        {
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = "FallenPiece";
            piece.transform.parent = parent;
            piece.transform.localPosition = new Vector3(
                Random.Range(-1.5f, 1.5f),
                Random.Range(0.1f, 0.3f),
                Random.Range(-1.5f, 1.5f)
            );
            piece.transform.localScale = new Vector3(
                Random.Range(0.3f, 0.8f),
                Random.Range(0.3f, 0.6f),
                Random.Range(0.3f, 0.8f)
            );
            piece.transform.rotation = Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            );
            piece.GetComponent<Renderer>().material.color = stoneWallColor * 0.75f;
        }
        
        // Trosky a prach
        GameObject rubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rubble.name = "Rubble";
        rubble.transform.parent = parent;
        rubble.transform.localPosition = new Vector3(0, 0.1f, 0);
        rubble.transform.localScale = new Vector3(1.5f, 0.2f, 1.5f);
        rubble.GetComponent<Renderer>().material.color = new Color(0.6f, 0.5f, 0.4f);
    }
    
    protected void CreateHieroglyphs(Room room, Transform parent, float roomSize)
    {
        GameObject hieroglyphParent = new GameObject("Hieroglyphs");
        hieroglyphParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < hieroglyphsCount; i++)
        {
            // Náhodná stěna (0=N, 1=S, 2=E, 3=W)
            int side = Random.Range(0, 4);
            Vector3 position = room.center;
            Quaternion rotation = Quaternion.identity;
            
            float height = Random.Range(1.5f, 4f);
            float offset = Random.Range(-halfSize * 0.6f, halfSize * 0.6f);
            
            switch (side)
            {
                case 0: // North
                    position += new Vector3(offset, height, halfSize - 0.5f);
                    rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case 1: // South
                    position += new Vector3(offset, height, -halfSize + 0.5f);
                    rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case 2: // East
                    position += new Vector3(halfSize - 0.5f, height, offset);
                    rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case 3: // West
                    position += new Vector3(-halfSize + 0.5f, height, offset);
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
            }
            
            CreateHieroglyph(position, rotation, hieroglyphParent.transform);
        }
    }
    
    protected void CreateHieroglyph(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject hieroglyph = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hieroglyph.name = "Hieroglyph";
        hieroglyph.transform.parent = parent;
        hieroglyph.transform.position = position;
        hieroglyph.transform.rotation = rotation;
        hieroglyph.transform.localScale = new Vector3(1.5f, 2f, 0.1f);
        
        // Tmavší kámen pro kontrast
        hieroglyph.GetComponent<Renderer>().material.color = new Color(0.5f, 0.4f, 0.3f);
        
        // Přidat zlaté detaily jako "symboly"
        for (int i = 0; i < 3; i++)
        {
            GameObject symbol = GameObject.CreatePrimitive(PrimitiveType.Cube);
            symbol.name = "Symbol";
            symbol.transform.parent = hieroglyph.transform;
            symbol.transform.localPosition = new Vector3(
                Random.Range(-0.4f, 0.4f),
                Random.Range(-0.7f, 0.7f),
                -0.05f
            );
            symbol.transform.localScale = new Vector3(0.15f, 0.15f, 0.02f);
            symbol.GetComponent<Renderer>().material.color = goldColor * 0.8f;
        }
    }
    
    protected void CreateSandDunes(Room room, Transform parent, float roomSize)
    {
        GameObject dunesParent = new GameObject("SandDunes");
        dunesParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        // Duny v rozích
        Vector3[] dunePositions = new Vector3[]
        {
            room.center + new Vector3(halfSize * 0.8f, 0f, halfSize * 0.8f),
            room.center + new Vector3(halfSize * 0.8f, 0f, -halfSize * 0.8f),
            room.center + new Vector3(-halfSize * 0.8f, 0f, halfSize * 0.8f),
            room.center + new Vector3(-halfSize * 0.8f, 0f, -halfSize * 0.8f)
        };
        
        foreach (Vector3 pos in dunePositions)
        {
            GameObject dune = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dune.name = "SandDune";
            dune.transform.parent = dunesParent.transform;
            dune.transform.position = pos + Vector3.up * 0.3f;
            dune.transform.localScale = new Vector3(
                Random.Range(2f, 3f),
                Random.Range(0.5f, 0.8f),
                Random.Range(2f, 3f)
            );
            dune.GetComponent<Renderer>().material.color = new Color(0.9f, 0.8f, 0.6f);
        }
    }
    
    protected void CreateGoldenDecorations(Room room, Transform parent, float roomSize)
    {
        GameObject decorationsParent = new GameObject("GoldenDecorations");
        decorationsParent.transform.parent = parent;
        
        // Zlaté sochy/obelisky
        for (int i = 0; i < statuesCount; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-roomSize * 0.35f, roomSize * 0.35f),
                0f,
                Random.Range(-roomSize * 0.35f, roomSize * 0.35f)
            );
            
            CreateStatue(position, decorationsParent.transform);
        }
        
        // Oltář/svatyně ve středu některých místností
        if (Random.value > 0.5f)
        {
            CreateAltar(room.center, decorationsParent.transform);
        }
    }
    
    protected void CreateStatue(Vector3 position, Transform parent)
    {
        GameObject statue = new GameObject("Statue");
        statue.transform.parent = parent;
        statue.transform.position = position;
        
        // Podstavec
        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pedestal.name = "Pedestal";
        pedestal.transform.parent = statue.transform;
        pedestal.transform.localPosition = new Vector3(0, 0.5f, 0);
        pedestal.transform.localScale = new Vector3(1f, 1f, 1f);
        pedestal.GetComponent<Renderer>().material.color = stoneWallColor;
        
        // Tělo sochy
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "StatueBody";
        body.transform.parent = statue.transform;
        body.transform.localPosition = new Vector3(0, 2f, 0);
        body.transform.localScale = new Vector3(0.6f, 1.5f, 0.6f);
        body.GetComponent<Renderer>().material.color = goldColor * 0.6f;
        
        // Hlava
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "StatueHead";
        head.transform.parent = statue.transform;
        head.transform.localPosition = new Vector3(0, 3.5f, 0);
        head.transform.localScale = new Vector3(0.5f, 0.6f, 0.5f);
        head.GetComponent<Renderer>().material.color = goldColor * 0.6f;
    }
    
    protected void CreateAltar(Vector3 position, Transform parent)
    {
        GameObject altar = new GameObject("Altar");
        altar.transform.parent = parent;
        altar.transform.position = position + Vector3.up * 0.5f;
        
        // Hlavní část oltáře
        GameObject altarBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        altarBase.name = "AltarBase";
        altarBase.transform.parent = altar.transform;
        altarBase.transform.localPosition = Vector3.zero;
        altarBase.transform.localScale = new Vector3(2f, 1f, 1.5f);
        altarBase.GetComponent<Renderer>().material.color = stoneWallColor;
        
        // Zlaté nádoby na oltáři
        for (int i = -1; i <= 1; i++)
        {
            GameObject vessel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            vessel.name = "GoldenVessel";
            vessel.transform.parent = altar.transform;
            vessel.transform.localPosition = new Vector3(i * 0.5f, 0.7f, 0);
            vessel.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
            vessel.GetComponent<Renderer>().material.color = goldColor;
        }
    }

    public override void GenerateLevel()
    {
        base.GenerateLevel();
        ApplyVisualEffects();
        CreateOilLamps();
    }
    
    protected void CreateOilLamps()
    {
        GameObject lampsParent = new GameObject("OilLamps");
        lampsParent.transform.parent = transform;
        
        foreach (Room room in rooms)
        {
            float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
            float halfSize = roomSize / 2f;
            
            for (int i = 0; i < oilLampsCount; i++)
            {
                float angle = (360f / oilLampsCount) * i + Random.Range(-10f, 10f);
                float distance = halfSize * Random.Range(0.7f, 0.85f);
                
                Vector3 position = room.center + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                    Random.Range(2f, 3f),
                    Mathf.Sin(angle * Mathf.Deg2Rad) * distance
                );
                
                CreateOilLamp(position, lampsParent.transform);
            }
        }
    }
    
    protected void CreateOilLamp(Vector3 position, Transform parent)
    {
        GameObject lamp = new GameObject("OilLamp");
        lamp.transform.parent = parent;
        lamp.transform.position = position;
        
        // Bronzová nádoba
        GameObject vessel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        vessel.name = "LampVessel";
        vessel.transform.parent = lamp.transform;
        vessel.transform.localPosition = Vector3.zero;
        vessel.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);
        vessel.GetComponent<Renderer>().material.color = new Color(0.8f, 0.5f, 0.2f);
        
        // Plamen
        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "Flame";
        flame.transform.parent = lamp.transform;
        flame.transform.localPosition = new Vector3(0, 0.3f, 0);
        flame.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
        
        Renderer flameRenderer = flame.GetComponent<Renderer>();
        flameRenderer.material = new Material(Shader.Find("Unlit/Color"));
        flameRenderer.material.color = new Color(1f, 0.7f, 0.3f);
        
        // Teplé světlo
        GameObject lightObj = new GameObject("LampLight");
        lightObj.transform.parent = lamp.transform;
        lightObj.transform.localPosition = new Vector3(0, 0.3f, 0);
        
        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = new Color(1f, 0.8f, 0.5f);
        pointLight.intensity = 2.5f;
        pointLight.range = 7f;
        pointLight.shadows = LightShadows.Soft;
        
        // Částice plamene
        ParticleSystem ps = flame.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.8f;
        main.startSpeed = 0.5f;
        main.startSize = 0.15f;
        main.startColor = new Color(1f, 0.8f, 0.4f, 0.8f);
        main.maxParticles = 15;
        
        var emission = ps.emission;
        emission.rateOverTime = 20f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;
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
        
        // Přidat zlatavé bodové světlo pro mystickou atmosféru
        GameObject ambientLightObj = new GameObject("AmbientGlow");
        ambientLightObj.transform.parent = epochLighting.transform;
        ambientLightObj.transform.position = Vector3.up * 5f;
        
        Light ambientGlow = ambientLightObj.AddComponent<Light>();
        ambientGlow.type = LightType.Point;
        ambientGlow.color = goldColor;
        ambientGlow.intensity = 1.5f;
        ambientGlow.range = baseRoomSize * 1.5f;
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
        // Písečné částice ve vzduchu - najít střed levelu
        Vector3 levelCenter = Vector3.zero;
        if (rooms != null && rooms.Count > 0)
        {
            // Spočítat průměr pozic všech místností
            foreach (Room room in rooms)
            {
                levelCenter += room.center;
            }
            levelCenter /= rooms.Count;
        }
        
        GameObject epochParticles = new GameObject("SandParticles");
        epochParticles.transform.parent = null; // Odstranit parenta, aby byl ve World space
        epochParticles.transform.position = levelCenter + Vector3.up * 5f; // Výše nad středem levelu
        
        ParticleSystem ps = epochParticles.AddComponent<ParticleSystem>();
        sandParticleSystem = ps; // Uložit referenci pro weather systém
        
        var main = ps.main;
        main.startLifetime = 12f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
        main.startColor = new Color(0.9f, 0.8f, 0.6f, 0.8f); // Zvýšená průhlednost
        main.maxParticles = 150;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true; // Zajistit že částice běží kontinuálně
        main.playOnAwake = true;
        
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 12f;
        
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        // Větší oblast pokrytí
        float coverageSize = baseRoomSize * Mathf.Max(3, rooms.Count);
        shape.scale = new Vector3(coverageSize, 10f, coverageSize);
        
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = new Color(0.95f, 0.85f, 0.7f, 0.8f);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // Spustit particle systém
        ps.Play();
        
        Debug.Log($"Sand particles created at {epochParticles.transform.position}, coverage: {coverageSize}");
    }
    
    private void Update()
    {
        if (sandParticleSystem == null) return;
        
        UpdateWeatherSystem();
    }
    
    protected void UpdateWeatherSystem()
    {
        weatherTimer += Time.deltaTime;
        
        // Pravidelně kontrolovat, zda se má změnit počasí
        if (weatherTimer >= weatherCheckInterval)
        {
            weatherTimer = 0f;
            
            if (!isSandstorm)
            {
                // Náhodná šance na spuštění bouře
                if (Random.value < sandstormStartChance)
                {
                    StartSandstorm();
                }
            }
            else
            {
                // Náhodná šance na ukončení bouře
                if (Random.value < sandstormEndChance)
                {
                    EndSandstorm();
                }
            }
        }
    }
    
    protected void StartSandstorm()
    {
        isSandstorm = true;
        Debug.Log("Písečná bouře začíná!");
        
        // Zvýšit intenzitu částic
        var emission = sandParticleSystem.emission;
        emission.rateOverTime = 80f; // Z 12 na 80
        
        var main = sandParticleSystem.main;
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f); // Rychlejší částice
        main.maxParticles = 500; // Více částic
        
        var velocity = sandParticleSystem.velocityOverLifetime;
        velocity.x = new ParticleSystem.MinMaxCurve(2f, 5f); // Silnější vítr
        velocity.y = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f); // Žádný pohyb v Z
        
        // Snížit viditelnost - hustší mlha
        RenderSettings.fogDensity = 0.08f; // Z 0.015f na 0.08f
        RenderSettings.fogColor = new Color(0.7f, 0.6f, 0.4f); // Hnědavější mlha
        
        // Snížit intenzitu světla
        if (epochLighting != null)
        {
            Light dirLight = epochLighting.GetComponentInChildren<Light>();
            if (dirLight != null)
            {
                dirLight.intensity = 0.4f; // Z 1.2f na 0.4f
            }
        }
        
        RenderSettings.ambientIntensity = 0.3f; // Z 0.6f na 0.3f
        
        // Aplikovat snížení rychlosti na hráče a nepřátele
        ApplyMovementSpeedModifier(sandstormMovementMultiplier);
    }
    
    protected void EndSandstorm()
    {
        isSandstorm = false;
        Debug.Log("Písečná bouře končí...");
        
        // Vrátit normální intenzitu částic
        var emission = sandParticleSystem.emission;
        emission.rateOverTime = 12f;
        
        var main = sandParticleSystem.main;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.maxParticles = 150;
        
        var velocity = sandParticleSystem.velocityOverLifetime;
        velocity.x = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f); // Žádný pohyb v Z
        
        // Vrátit normální viditelnost
        RenderSettings.fogDensity = 0.015f;
        RenderSettings.fogColor = new Color(0.9f, 0.8f, 0.6f);
        
        // Vrátit normální osvětlení
        if (epochLighting != null)
        {
            Light dirLight = epochLighting.GetComponentInChildren<Light>();
            if (dirLight != null)
            {
                dirLight.intensity = 1.2f;
            }
        }
        
        RenderSettings.ambientIntensity = 0.6f;
        
        // Vrátit normální rychlost pohybu
        ApplyMovementSpeedModifier(1f);
    }
    
    protected void ApplyMovementSpeedModifier(float multiplier)
    {
        // Najít všechny objekty s pohybem a aplikovat modifikátor
        
        // Hráč
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Předpokládáme že má CharacterController nebo podobný komponent
            var controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                // Uložit originální rychlost a aplikovat modifikátor
                // Toto by mělo být implementováno v PlayerMovement skriptu
                player.SendMessage("SetWeatherSpeedModifier", multiplier, SendMessageOptions.DontRequireReceiver);
            }
        }
        
        // Nepřátelé
        EnemyBase[] enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (EnemyBase enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.SendMessage("SetWeatherSpeedModifier", multiplier, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    
    // Getter pro zjištění, zda právě probíhá bouře (pro jiné skripty)
    public bool IsSandstorm()
    {
        return isSandstorm;
    }
    
    public float GetMovementSpeedMultiplier()
    {
        return isSandstorm ? sandstormMovementMultiplier : 1f;
    }
}