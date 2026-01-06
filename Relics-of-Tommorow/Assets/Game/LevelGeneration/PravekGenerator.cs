using UnityEngine;
using Unity.AI.Navigation;

/// <summary>
/// Generátor pro Pravěk - místnosti z kamene
/// </summary>
public class PravekGenerator : RoomBasedGenerator
{
    [Header("Pravek Specific")]
    [SerializeField] private Color caveWallColor = new Color(0.4f, 0.3f, 0.2f);
    [SerializeField] private Color caveFloorColor = new Color(0.3f, 0.25f, 0.2f);
    
    [Header("Cave Shape Settings")]
    [SerializeField] private int stalactiteCount = 5; // Počet stalaktitů (sníženo)
    [SerializeField] private int stalagmiteCount = 8; // Počet stalagmitů (sníženo)
    [SerializeField] private float rockFormationDensity = 0.2f; // Hustota kamenných formací
    [SerializeField] private int torchesPerRoom = 6; // Počet pochodní na místnost
    [SerializeField] private float ceilingHeight = 15f; // Výška stropu (zvýšeno pro prostor)
    
    // Vizuální efekty
    private Color ambientLightColor;
    private float ambientIntensity;
    private Color directionalLightColor;
    private float directionalLightIntensity;
    private Vector3 directionalLightRotation;
    private Color fogColor;
    private float fogDensity;
    
    protected override Color GetWallColor() => caveWallColor;
    protected override Color GetFloorColor() => caveFloorColor;
    
    // Override pro asymetrické stěny
    protected override void CreateRoomStructure(Room room)
    {
        // Vytvořit organickou asymetrickou místnost místo čtvercové
        CreateOrganicCaveRoom(room);
        
        // Přidat asymetrické prvky ke stěnám
        AddAsymmetricWallFeatures(room);
    }
    
    /// <summary>
    /// Vytvoří organickou jeskynní místnost s nepravidelným tvarem
    /// </summary>
    protected void CreateOrganicCaveRoom(Room room)
    {
        GameObject roomParent = new GameObject($"Room_{room.roomNumber}");
        roomParent.transform.parent = transform;
        roomParent.transform.position = room.center;
        
        float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
        
        // PODLAHA - Rovný choditelný terrain
        GameObject floorParent = new GameObject("Floor");
        floorParent.transform.parent = roomParent.transform;
        
        // Hlavní podlaha - CUBE místo Plane pro spolehlivý BoxCollider
        GameObject mainFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mainFloor.name = "MainFloor";
        mainFloor.transform.parent = floorParent.transform;
        // Cube 0.5m tlustý, vrchní plocha je na room.center
        mainFloor.transform.position = room.center + new Vector3(0, -0.25f, 0);
        mainFloor.transform.localScale = new Vector3(roomSize, 0.5f, roomSize);
        mainFloor.GetComponent<Renderer>().material.color = GetFloorColor();
        
        // BoxCollider je automaticky na Cube - jen zajištění že je enabled
        BoxCollider floorCollider = mainFloor.GetComponent<BoxCollider>();
        if (floorCollider != null)
        {
            floorCollider.enabled = true;
            floorCollider.isTrigger = false;
        }
        
        // Nastavit správný layer pro NavMesh
        mainFloor.layer = LayerMask.NameToLayer("Default");
        
        // Přidat NavMeshModifier pro označení jako walkable pro NavMesh
        NavMeshModifier modifier = mainFloor.GetComponent<NavMeshModifier>();
        if (modifier == null)
        {
            modifier = mainFloor.AddComponent<NavMeshModifier>();
        }
        modifier.overrideArea = true;
        modifier.area = 0; // 0 = Walkable area
        
        room.floor = floorParent;
        
        // STĚNY - Organické zaoblené stěny
        CreateOrganicWalls(room, roomParent.transform);
        
        // STROP - Asymetrický organický strop
        CreateOrganicCeiling(room, roomParent.transform);
    }
    
    /// <summary>
    /// Vytvoří organické stěny pro nepravidelnou místnost
    /// </summary>
    protected void CreateOrganicWalls(Room room, Transform parent)
    {
        GameObject wallParent = new GameObject("Walls");
        wallParent.transform.parent = parent;
        
        float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
        int wallSegments = 40; // Zvýšeno z 20 na 40 pro hustší stěny
        float angleStep = 360f / wallSegments;
        
        // Vytvoříme více "vrstev" kamenů ve výšce - od podlahy ke stropu
        int heightLayers = 6; // 6 vrstev kamenů ve výšce pro plné pokrytí až ke stropu
        
        for (int layer = 0; layer < heightLayers; layer++)
        {
            // Výška pro tuto vrstvu
            float layerHeight = (ceilingHeight / heightLayers) * layer + ceilingHeight / (heightLayers * 2);
            
            for (int i = 0; i < wallSegments; i++)
            {
                float angle = i * angleStep;
                
                // Stěny JEN na samém obvodu místnosti - volný prostor uprostřed
                float distance = roomSize * 0.5f; // Pevná vzdálenost na okraji
                
                Vector3 wallPosition = room.center + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                    layerHeight,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * distance
                );
                
                // KONTROLA: Nekládat stěnu tam, kde je průchod do jiné místnosti
                bool isTooCloseToConnection = false;
                foreach (Room connectedRoom in room.connectedRooms)
                {
                    Vector3 directionToConnection = (connectedRoom.center - room.center).normalized;
                    Vector3 directionToWall = (wallPosition - room.center).normalized;
                    
                    // Pokud je wall segment ve směru průchodu (úhel < 30°), přeskoč ho
                    float dot = Vector3.Dot(directionToConnection, directionToWall);
                    if (dot > 0.85f) // cos(30°) ≈ 0.866
                    {
                        isTooCloseToConnection = true;
                        break;
                    }
                }
                
                if (isTooCloseToConnection)
                {
                    continue; // Přeskočit tento segment - je v průchodu
                }
                
                // Větší kameny pro lepší pokrytí
                GameObject wallSegment = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                wallSegment.name = $"WallSegment_L{layer}_{i}";
                wallSegment.transform.parent = wallParent.transform;
                wallSegment.transform.position = wallPosition;
                wallSegment.transform.localScale = new Vector3(
                    Random.Range(4f, 6f),  // Větší pro lepší pokrytí
                    Random.Range(3f, 5f),  // Vysoké pro překrytí mezi vrstvami
                    Random.Range(4f, 6f)   // Větší pro lepší pokrytí
                );
                
                Renderer renderer = wallSegment.GetComponent<Renderer>();
                renderer.material.color = new Color(
                    GetWallColor().r * Random.Range(0.9f, 1.1f),
                    GetWallColor().g * Random.Range(0.9f, 1.1f),
                    GetWallColor().b * Random.Range(0.9f, 1.1f)
                );
            }
        }
    }
    
    /// <summary>
    /// Vytvoří asymetrický organický strop jako v ASTRONEERu
    /// </summary>
    protected void CreateOrganicCeiling(Room room, Transform parent)
    {
        GameObject ceilingParent = new GameObject("Ceiling");
        ceilingParent.transform.parent = parent;
        
        float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
        
        // Místo jedné velké sféry vytvoříme mnoho menších kamenů pro nepravidelný strop
        int ceilingRocks = Random.Range(25, 35); // Více kamenů pro nepravidelnost
        
        for (int i = 0; i < ceilingRocks; i++)
        {
            // Náhodná pozice JEN na okraji - ne uprostřed místnosti
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(roomSize * 0.35f, roomSize * 0.5f); // Jen na obvodu
            
            Vector3 rockPosition = room.center + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                ceilingHeight + Random.Range(-0.5f, 0.5f), // Malý výchyl ve výšce
                Mathf.Sin(angle * Mathf.Deg2Rad) * distance
            );
            
            GameObject ceilingRock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ceilingRock.name = $"CeilingRock_{i}";
            ceilingRock.transform.parent = ceilingParent.transform;
            ceilingRock.transform.position = rockPosition;
            
            // Různě velké kameny pro organický vzhled
            float scale = Random.Range(3f, 6f);
            ceilingRock.transform.localScale = new Vector3(
                scale * Random.Range(0.8f, 1.2f),
                scale * Random.Range(0.6f, 1.0f),
                scale * Random.Range(0.8f, 1.2f)
            );
            
            // Náhodná rotace
            ceilingRock.transform.rotation = Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            );
            
            Renderer renderer = ceilingRock.GetComponent<Renderer>();
            renderer.material.color = new Color(
                0.35f * Random.Range(0.85f, 1.15f),
                0.3f * Random.Range(0.85f, 1.15f),
                0.25f * Random.Range(0.85f, 1.15f)
            );
            
            // ODSTRANIT SphereCollider ze stropu - nepotřebujeme kolize
            SphereCollider rockCollider = ceilingRock.GetComponent<SphereCollider>();
            if (rockCollider != null)
            {
                DestroyImmediate(rockCollider);
            }
        }
    }
    
    protected void AddAsymmetricWallFeatures(Room room)
    {
        float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
        float halfSize = roomSize / 2f;
        
        GameObject wallParent = new GameObject("AsymmetricWalls");
        wallParent.transform.parent = room.floor.transform.parent;
        
        // Přidat MALÉ výčnělky na stěny - jen na okraji, ne do středu
        int wallFeatures = Random.Range(3, 6); // Sníženo
        for (int i = 0; i < wallFeatures; i++)
        {
            // Náhodná strana (0=N, 1=S, 2=E, 3=W)
            int side = Random.Range(0, 4);
            Vector3 position = room.center;
            Vector3 scale;
            
            float wallHeight = Random.Range(2f, 5f); // Nižší
            float width = Random.Range(0.5f, 1.5f); // Menší
            float depth = Random.Range(0.2f, 0.4f); // Tenčí
            
            switch (side)
            {
                case 0: // North - na OKRAJI
                    position += new Vector3(Random.Range(-halfSize * 0.6f, halfSize * 0.6f), wallHeight / 2f, halfSize * 0.95f);
                    scale = new Vector3(width, wallHeight, depth);
                    break;
                case 1: // South - na OKRAJI
                    position += new Vector3(Random.Range(-halfSize * 0.6f, halfSize * 0.6f), wallHeight / 2f, -halfSize * 0.95f);
                    scale = new Vector3(width, wallHeight, depth);
                    break;
                case 2: // East - na OKRAJI
                    position += new Vector3(halfSize * 0.95f, wallHeight / 2f, Random.Range(-halfSize * 0.6f, halfSize * 0.6f));
                    scale = new Vector3(depth, wallHeight, width);
                    break;
                default: // West - na OKRAJI
                    position += new Vector3(-halfSize * 0.95f, wallHeight / 2f, Random.Range(-halfSize * 0.6f, halfSize * 0.6f));
                    scale = new Vector3(depth, wallHeight, width);
                    break;
            }
            
            GameObject wallFeature = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallFeature.name = "WallFeature";
            wallFeature.transform.parent = wallParent.transform;
            wallFeature.transform.position = position;
            wallFeature.transform.localScale = scale;
            
            Renderer renderer = wallFeature.GetComponent<Renderer>();
            renderer.material.color = new Color(
                Random.Range(0.35f, 0.5f),
                Random.Range(0.25f, 0.4f),
                Random.Range(0.2f, 0.35f)
            );
        }
    }
    
    // Odstraněno automatické generování sphere kamenů - nyní se používají prefaby
    
    public override void GenerateLevel()
    {
        base.GenerateLevel();
        ApplyVisualEffects();
        CreateCaveFormations();
    }
    
    /// <summary>
    /// Vytvoří organické jeskynní formace (stalaktity, stalagmity, skály)
    /// </summary>
    protected void CreateCaveFormations()
    {
        GameObject formationsParent = new GameObject("CaveFormations");
        formationsParent.transform.parent = transform;
        
        // Projít všechny místnosti a přidat do nich formace
        foreach (Room room in rooms)
        {
            CreateRoomCaveFormations(room, formationsParent.transform);
        }
    }
    
    protected void CreateRoomCaveFormations(Room room, Transform parent)
    {
        float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
        float halfSize = roomSize / 2f;
        
        // Pochodně/ohně po obvodu místnosti
        CreateTorches(room, parent);
        CreateTorches(room, parent);
        
        // Stalaktity (visí ze stropu)
        int stalactitesForRoom = Random.Range(stalactiteCount / 2, stalactiteCount);
        for (int i = 0; i < stalactitesForRoom; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.8f, halfSize * 0.8f),
                2.8f, // Těsně pod stropem
                Random.Range(-halfSize * 0.8f, halfSize * 0.8f)
            );
            
            CreateStalactite(position, parent);
        }
        
        // Stalagmity (rostou ze země)
        int stalagmitesForRoom = Random.Range(stalagmiteCount / 2, stalagmiteCount);
        for (int i = 0; i < stalagmitesForRoom; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.7f, halfSize * 0.7f),
                0f,
                Random.Range(-halfSize * 0.7f, halfSize * 0.7f)
            );
            
            CreateStalagmite(position, parent);
        }
        
        // Kamenné výčnělky na stěnách
        int rockFormations = Mathf.RoundToInt(roomSize * rockFormationDensity);
        for (int i = 0; i < rockFormations; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize, halfSize),
                Random.Range(0.5f, 2f),
                Random.Range(-halfSize, halfSize)
            );
            
            CreateRockFormation(position, parent);
        }
    }
    
    protected void CreateStalactite(Vector3 position, Transform parent)
    {
        GameObject stalactite = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stalactite.name = "Stalactite";
        stalactite.transform.parent = parent;
        stalactite.transform.position = position;
        
        // Náhodná velikost
        float height = Random.Range(0.5f, 2.5f);
        float width = Random.Range(0.15f, 0.4f);
        stalactite.transform.localScale = new Vector3(width, height, width);
        
        // Mírná rotace pro realističnost
        stalactite.transform.rotation = Quaternion.Euler(
            Random.Range(-5f, 5f),
            Random.Range(0f, 360f),
            Random.Range(-5f, 5f)
        );
        
        // Barva kamene
        Renderer renderer = stalactite.GetComponent<Renderer>();
        renderer.material.color = new Color(
            Random.Range(0.3f, 0.5f),
            Random.Range(0.25f, 0.4f),
            Random.Range(0.2f, 0.35f)
        );
    }
    
    protected void CreateStalagmite(Vector3 position, Transform parent)
    {
        GameObject stalagmite = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stalagmite.name = "Stalagmite";
        stalagmite.transform.parent = parent;
        stalagmite.transform.position = position + Vector3.up * 0.5f;
        
        // Náhodná velikost - užší nahoře než dole pro efekt stalagmitu
        float height = Random.Range(0.5f, 2f);
        float widthBottom = Random.Range(0.3f, 0.6f);
        float widthTop = widthBottom * Random.Range(0.3f, 0.6f); // Užší nahoře
        
        // Pro kuželový efekt použijeme průměrnou šířku
        float avgWidth = (widthBottom + widthTop) / 2f;
        stalagmite.transform.localScale = new Vector3(avgWidth, height, avgWidth);
        
        // Mírná rotace
        stalagmite.transform.rotation = Quaternion.Euler(
            Random.Range(-3f, 3f),
            Random.Range(0f, 360f),
            Random.Range(-3f, 3f)
        );
        
        // Barva kamene
        Renderer renderer = stalagmite.GetComponent<Renderer>();
        renderer.material.color = new Color(
            Random.Range(0.35f, 0.5f),
            Random.Range(0.3f, 0.45f),
            Random.Range(0.25f, 0.4f)
        );
    }
    
    protected void CreateRockFormation(Vector3 position, Transform parent)
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = "RockFormation";
        rock.transform.parent = parent;
        rock.transform.position = position;
        
        // Náhodná deformovaná velikost
        rock.transform.localScale = new Vector3(
            Random.Range(0.4f, 1.2f),
            Random.Range(0.3f, 0.8f),
            Random.Range(0.4f, 1f)
        );
        
        rock.transform.rotation = Quaternion.Euler(
            Random.Range(0f, 360f),
            Random.Range(0f, 360f),
            Random.Range(0f, 360f)
        );
        
        // Barva kamene s variací
        Renderer renderer = rock.GetComponent<Renderer>();
        renderer.material.color = new Color(
            Random.Range(0.4f, 0.55f),
            Random.Range(0.35f, 0.5f),
            Random.Range(0.3f, 0.45f)
        );
    }
    
    protected void ApplyVisualEffects()
    {
        // Tmavé jeskynní osvětlení
        ambientLightColor = new Color(0.3f, 0.25f, 0.2f);
        ambientIntensity = 0.3f;
        directionalLightColor = new Color(1f, 0.9f, 0.7f);
        directionalLightIntensity = 0.5f;
        directionalLightRotation = new Vector3(45, -30, 0);
        
        // Hnědavá mlha pro jeskyní atmosféru
        fogColor = new Color(0.4f, 0.3f, 0.25f);
        fogDensity = 0.03f;
        
        // Volat setup metody
        SetupLighting();
        SetupFog();
        CreateCaveParticles();
    }
    
    protected void SetupLighting()
    {
        // Nastavit ambient light
        RenderSettings.ambientLight = ambientLightColor;
        RenderSettings.ambientIntensity = ambientIntensity;
        
        // Vytvořit directional light
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
    
    protected void CreateCaveParticles()
    {
        // Prach a mlha v jeskyni
        GameObject epochParticles = new GameObject("CaveParticles");
        epochParticles.transform.parent = transform;
        epochParticles.transform.position = Vector3.zero;
        
        ParticleSystem ps = epochParticles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 15f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.8f, 2f);
        main.startColor = new Color(0.5f, 0.4f, 0.3f, 0.4f);
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 15f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(baseRoomSize * 1.5f, 8f, baseRoomSize * 1.5f);
        
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
        
        // Nastavení rendereru pro lepší vzhled
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = new Color(0.6f, 0.5f, 0.4f, 0.5f);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }
    
    /// <summary>
    /// Rozmístí pochodně/ohně po místnosti pro osvětlení
    /// </summary>
    protected void CreateTorches(Room room, Transform parent)
    {
        float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < torchesPerRoom; i++)
        {
            // Pozice poblíž stěn
            float angle = (360f / torchesPerRoom) * i + Random.Range(-15f, 15f);
            float distance = halfSize * Random.Range(0.7f, 0.85f);
            
            Vector3 position = room.center + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                Random.Range(1f, 2f),
                Mathf.Sin(angle * Mathf.Deg2Rad) * distance
            );
            
            CreateTorch(position, parent);
        }
    }
    
    /// <summary>
    /// Vytvoří jednu pochodeň s ohněm a světlem
    /// </summary>
    protected void CreateTorch(Vector3 position, Transform parent)
    {
        GameObject torchParent = new GameObject("Torch");
        torchParent.transform.parent = parent;
        torchParent.transform.position = position;
        
        // Dřevěná tyč pochodně
        GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stick.name = "TorchStick";
        stick.transform.parent = torchParent.transform;
        stick.transform.localPosition = Vector3.zero;
        stick.transform.localScale = new Vector3(0.08f, 0.5f, 0.08f);
        stick.GetComponent<Renderer>().material.color = new Color(0.3f, 0.2f, 0.1f);
        
        // "Oheň" na vrcholu
        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "Flame";
        flame.transform.parent = torchParent.transform;
        flame.transform.localPosition = new Vector3(0, 0.6f, 0);
        flame.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
        
        Renderer flameRenderer = flame.GetComponent<Renderer>();
        flameRenderer.material = new Material(Shader.Find("Unlit/Color"));
        flameRenderer.material.color = new Color(1f, 0.5f, 0.1f);
        
        // Point Light pro osvětlení
        GameObject lightObj = new GameObject("TorchLight");
        lightObj.transform.parent = torchParent.transform;
        lightObj.transform.localPosition = new Vector3(0, 0.6f, 0);
        
        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = new Color(1f, 0.7f, 0.4f);
        pointLight.intensity = 3f;
        pointLight.range = 8f;
        pointLight.shadows = LightShadows.Soft;
        
        // Částice ohně
        ParticleSystem ps = flame.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 1f;
        main.startSize = 0.2f;
        main.startColor = new Color(1f, 0.7f, 0.3f, 0.8f);
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 30f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.8f, 0.3f), 0f),
                new GradientColorKey(new Color(1f, 0.3f, 0f), 0.5f),
                new GradientColorKey(new Color(0.3f, 0.3f, 0.3f), 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.5f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
        
        var particleRenderer = ps.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
    }
}
