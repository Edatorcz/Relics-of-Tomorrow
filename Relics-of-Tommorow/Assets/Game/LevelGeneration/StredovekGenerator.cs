using UnityEngine;

/// <summary>
/// Generátor pro Středověk - gotické hrady a zbrojnice
/// </summary>
public class StredovekGenerator : RoomBasedGenerator
{
    [Header("Středověk Specific")]
    [SerializeField] private Color castleWallColor = new Color(0.5f, 0.5f, 0.55f);
    [SerializeField] private Color castleFloorColor = new Color(0.4f, 0.4f, 0.45f);
    [SerializeField] private Color woodColor = new Color(0.4f, 0.25f, 0.1f);
    [SerializeField] private Color ironColor = new Color(0.3f, 0.3f, 0.35f);
    
    [Header("Medieval Decorations")]
    // Počet věží můžete přidat později
    // private int towerCount = 4;
    [SerializeField] private int furnitureCount = 8;
    [SerializeField] private int weaponRacksCount = 6;
    [SerializeField] private int bannersCount = 10;
    [SerializeField] private int torchesCount = 12;
    
    // Vizuální efekty
    private Color ambientLightColor;
    private float ambientIntensity;
    private Color directionalLightColor;
    private float directionalLightIntensity;
    private Vector3 directionalLightRotation;
    private Color fogColor;
    private float fogDensity;
    private GameObject epochLighting;
    
    protected override Color GetWallColor() => castleWallColor;
    protected override Color GetFloorColor() => castleFloorColor;
    
    protected override void CreateRoomStructure(Room room)
    {
        base.CreateRoomStructure(room);
        
        float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
        GameObject roomParent = room.floor.transform.parent.gameObject;
        
        // Kamenné zdi s cimbuřím
        CreateCastleWalls(room, roomParent.transform, roomSize);
        
        // Gotické oblouky
        CreateGothicArches(room, roomParent.transform, roomSize);
        
        // Věže v rozích
        CreateTowers(room, roomParent.transform, roomSize);
        
        // Nábytek
        CreateFurniture(room, roomParent.transform, roomSize);
        
        // Zbraně a zbroj
        CreateWeaponDisplays(room, roomParent.transform, roomSize);
        
        // Vlajky a prapory
        CreateBanners(room, roomParent.transform, roomSize);
    }
    
    protected void CreateCastleWalls(Room room, Transform parent, float roomSize)
    {
        GameObject wallsParent = new GameObject("CastleWalls");
        wallsParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        float wallThickness = 1f;
        float wallHeight = 5f;
        
        // Přidat cimbuří na vrchol stěn
        int battlementCount = 8;
        float battlementSpacing = roomSize / battlementCount;
        
        // Severní strana
        for (int i = 0; i < battlementCount; i++)
        {
            float x = -halfSize + battlementSpacing * i + battlementSpacing / 2f;
            if (i % 2 == 0) // Střídavě vyšší a nižší
            {
                CreateBattlement(room.center + new Vector3(x, wallHeight, halfSize - wallThickness / 2f), wallsParent.transform);
            }
        }
        
        // Jižní strana
        for (int i = 0; i < battlementCount; i++)
        {
            float x = -halfSize + battlementSpacing * i + battlementSpacing / 2f;
            if (i % 2 == 0)
            {
                CreateBattlement(room.center + new Vector3(x, wallHeight, -halfSize + wallThickness / 2f), wallsParent.transform);
            }
        }
        
        // Východní strana
        for (int i = 0; i < battlementCount; i++)
        {
            float z = -halfSize + battlementSpacing * i + battlementSpacing / 2f;
            if (i % 2 == 0)
            {
                CreateBattlement(room.center + new Vector3(halfSize - wallThickness / 2f, wallHeight, z), wallsParent.transform);
            }
        }
        
        // Západní strana
        for (int i = 0; i < battlementCount; i++)
        {
            float z = -halfSize + battlementSpacing * i + battlementSpacing / 2f;
            if (i % 2 == 0)
            {
                CreateBattlement(room.center + new Vector3(-halfSize + wallThickness / 2f, wallHeight, z), wallsParent.transform);
            }
        }
    }
    
    protected void CreateBattlement(Vector3 position, Transform parent)
    {
        GameObject battlement = GameObject.CreatePrimitive(PrimitiveType.Cube);
        battlement.name = "Battlement";
        battlement.transform.parent = parent;
        battlement.transform.position = position;
        battlement.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        battlement.GetComponent<Renderer>().material.color = castleWallColor * 1.1f;
    }
    
    protected void CreateGothicArches(Room room, Transform parent, float roomSize)
    {
        GameObject archesParent = new GameObject("GothicArches");
        archesParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        // Oblouky na stěnách
        int archesPerWall = 2;
        float spacing = roomSize / (archesPerWall + 1);
        
        for (int i = 1; i <= archesPerWall; i++)
        {
            float x = -halfSize + spacing * i;
            
            // Severní strana
            CreateGothicArch(room.center + new Vector3(x, 2.5f, halfSize - 0.5f), Quaternion.Euler(0, 180, 0), archesParent.transform);
            
            // Jižní strana
            CreateGothicArch(room.center + new Vector3(x, 2.5f, -halfSize + 0.5f), Quaternion.identity, archesParent.transform);
        }
    }
    
    protected void CreateGothicArch(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject arch = new GameObject("GothicArch");
        arch.transform.parent = parent;
        arch.transform.position = position;
        arch.transform.rotation = rotation;
        
        // Levý sloup
        GameObject leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftPillar.name = "LeftPillar";
        leftPillar.transform.parent = arch.transform;
        leftPillar.transform.localPosition = new Vector3(-1f, 0, 0);
        leftPillar.transform.localScale = new Vector3(0.3f, 2.5f, 0.3f);
        leftPillar.GetComponent<Renderer>().material.color = castleWallColor * 0.9f;
        
        // Pravý sloup
        GameObject rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightPillar.name = "RightPillar";
        rightPillar.transform.parent = arch.transform;
        rightPillar.transform.localPosition = new Vector3(1f, 0, 0);
        rightPillar.transform.localScale = new Vector3(0.3f, 2.5f, 0.3f);
        rightPillar.GetComponent<Renderer>().material.color = castleWallColor * 0.9f;
        
        // Vrchol oblouku (špičatý gotický styl)
        GameObject archTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        archTop.name = "ArchTop";
        archTop.transform.parent = arch.transform;
        archTop.transform.localPosition = new Vector3(0, 2f, 0);
        archTop.transform.localScale = new Vector3(2f, 0.3f, 0.3f);
        archTop.transform.rotation = Quaternion.Euler(0, 0, 0);
        archTop.GetComponent<Renderer>().material.color = castleWallColor * 0.8f;
        
        // Špička
        GameObject peak = GameObject.CreatePrimitive(PrimitiveType.Cube);
        peak.name = "Peak";
        peak.transform.parent = arch.transform;
        peak.transform.localPosition = new Vector3(0, 2.5f, 0);
        peak.transform.localScale = new Vector3(0.4f, 0.6f, 0.3f);
        peak.transform.rotation = Quaternion.Euler(0, 0, 45);
        peak.GetComponent<Renderer>().material.color = castleWallColor * 0.7f;
    }
    
    protected void CreateTowers(Room room, Transform parent, float roomSize)
    {
        GameObject towersParent = new GameObject("Towers");
        towersParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        float towerOffset = halfSize * 0.9f;
        
        // Věže v rozích
        Vector3[] towerPositions = new Vector3[]
        {
            room.center + new Vector3(towerOffset, 0f, towerOffset),
            room.center + new Vector3(towerOffset, 0f, -towerOffset),
            room.center + new Vector3(-towerOffset, 0f, towerOffset),
            room.center + new Vector3(-towerOffset, 0f, -towerOffset)
        };
        
        foreach (Vector3 pos in towerPositions)
        {
            CreateTower(pos, towersParent.transform);
        }
    }
    
    protected void CreateTower(Vector3 position, Transform parent)
    {
        GameObject tower = new GameObject("Tower");
        tower.transform.parent = parent;
        tower.transform.position = position;
        
        // Základna věže
        GameObject base1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        base1.name = "TowerBase";
        base1.transform.parent = tower.transform;
        base1.transform.localPosition = new Vector3(0, 3f, 0);
        base1.transform.localScale = new Vector3(1.5f, 3f, 1.5f);
        base1.GetComponent<Renderer>().material.color = castleWallColor;
        
        // Střecha věže (kužel)
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        roof.name = "TowerRoof";
        roof.transform.parent = tower.transform;
        roof.transform.localPosition = new Vector3(0, 6.5f, 0);
        roof.transform.localScale = new Vector3(1.8f, 0.5f, 1.8f);
        roof.GetComponent<Renderer>().material.color = new Color(0.3f, 0.2f, 0.15f);
        
        // Vlajka na vrcholu
        GameObject flagpole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        flagpole.name = "Flagpole";
        flagpole.transform.parent = tower.transform;
        flagpole.transform.localPosition = new Vector3(0, 8f, 0);
        flagpole.transform.localScale = new Vector3(0.05f, 1.5f, 0.05f);
        flagpole.GetComponent<Renderer>().material.color = woodColor;
        
        // Vlajka
        GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flag.name = "Flag";
        flag.transform.parent = tower.transform;
        flag.transform.localPosition = new Vector3(0.3f, 8.5f, 0);
        flag.transform.localScale = new Vector3(0.6f, 0.4f, 0.05f);
        flag.GetComponent<Renderer>().material.color = new Color(0.8f, 0.1f, 0.1f);
    }
    
    protected void CreateFurniture(Room room, Transform parent, float roomSize)
    {
        GameObject furnitureParent = new GameObject("Furniture");
        furnitureParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        // Stoly
        for (int i = 0; i < furnitureCount / 2; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.6f, halfSize * 0.6f),
                0f,
                Random.Range(-halfSize * 0.6f, halfSize * 0.6f)
            );
            CreateTable(position, furnitureParent.transform);
        }
        
        // Soudky
        for (int i = 0; i < furnitureCount / 2; i++)
        {
            Vector3 position = room.center + new Vector3(
                Random.Range(-halfSize * 0.7f, halfSize * 0.7f),
                0f,
                Random.Range(-halfSize * 0.7f, halfSize * 0.7f)
            );
            CreateBarrel(position, furnitureParent.transform);
        }
    }
    
    protected void CreateTable(Vector3 position, Transform parent)
    {
        GameObject table = new GameObject("Table");
        table.transform.parent = parent;
        table.transform.position = position;
        table.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        
        // Deska stolu
        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
        top.name = "TableTop";
        top.transform.parent = table.transform;
        top.transform.localPosition = new Vector3(0, 0.9f, 0);
        top.transform.localScale = new Vector3(2f, 0.1f, 1f);
        top.GetComponent<Renderer>().material.color = woodColor;
        
        // Nohy stolu
        Vector3[] legPositions = new Vector3[]
        {
            new Vector3(0.8f, 0.45f, 0.4f),
            new Vector3(0.8f, 0.45f, -0.4f),
            new Vector3(-0.8f, 0.45f, 0.4f),
            new Vector3(-0.8f, 0.45f, -0.4f)
        };
        
        foreach (Vector3 legPos in legPositions)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = "TableLeg";
            leg.transform.parent = table.transform;
            leg.transform.localPosition = legPos;
            leg.transform.localScale = new Vector3(0.1f, 0.9f, 0.1f);
            leg.GetComponent<Renderer>().material.color = woodColor * 0.8f;
        }
    }
    
    protected void CreateBarrel(Vector3 position, Transform parent)
    {
        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.name = "Barrel";
        barrel.transform.parent = parent;
        barrel.transform.position = position + Vector3.up * 0.5f;
        barrel.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
        barrel.GetComponent<Renderer>().material.color = woodColor * 1.2f;
        
        // Železné kruhy kolem soudku
        for (int i = 0; i < 3; i++)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "IronRing";
            ring.transform.parent = barrel.transform;
            ring.transform.localPosition = new Vector3(0, -0.3f + i * 0.3f, 0);
            ring.transform.localScale = new Vector3(1.05f, 0.05f, 1.05f);
            ring.GetComponent<Renderer>().material.color = ironColor;
            
            // Odstranit collider z dekorativních kruhů
            Destroy(ring.GetComponent<Collider>());
        }
    }
    
    protected void CreateWeaponDisplays(Room room, Transform parent, float roomSize)
    {
        GameObject weaponsParent = new GameObject("WeaponDisplays");
        weaponsParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < weaponRacksCount; i++)
        {
            // Rozmístit podél stěn
            int side = Random.Range(0, 4);
            Vector3 position = room.center;
            Quaternion rotation = Quaternion.identity;
            
            float offset = Random.Range(-halfSize * 0.6f, halfSize * 0.6f);
            
            switch (side)
            {
                case 0: // North
                    position += new Vector3(offset, 0f, halfSize * 0.8f);
                    rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case 1: // South
                    position += new Vector3(offset, 0f, -halfSize * 0.8f);
                    break;
                case 2: // East
                    position += new Vector3(halfSize * 0.8f, 0f, offset);
                    rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case 3: // West
                    position += new Vector3(-halfSize * 0.8f, 0f, offset);
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
            }
            
            CreateWeaponRack(position, rotation, weaponsParent.transform);
        }
    }
    
    protected void CreateWeaponRack(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject rack = new GameObject("WeaponRack");
        rack.transform.parent = parent;
        rack.transform.position = position;
        rack.transform.rotation = rotation;
        
        // Stojan
        GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stand.name = "Stand";
        stand.transform.parent = rack.transform;
        stand.transform.localPosition = new Vector3(0, 1.5f, 0);
        stand.transform.localScale = new Vector3(1.5f, 3f, 0.2f);
        stand.GetComponent<Renderer>().material.color = woodColor * 0.7f;
        
        // Meče na stojanu
        for (int i = 0; i < 3; i++)
        {
            GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sword.name = "Sword";
            sword.transform.parent = rack.transform;
            sword.transform.localPosition = new Vector3(-0.5f + i * 0.5f, 2f, 0.15f);
            sword.transform.localScale = new Vector3(0.08f, 1f, 0.05f);
            sword.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));
            sword.GetComponent<Renderer>().material.color = ironColor;
        }
        
        // Štít
        GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shield.name = "Shield";
        shield.transform.parent = rack.transform;
        shield.transform.localPosition = new Vector3(0, 1f, 0.15f);
        shield.transform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
        shield.transform.localRotation = Quaternion.Euler(90, 0, 0);
        shield.GetComponent<Renderer>().material.color = new Color(0.6f, 0.4f, 0.2f);
    }
    
    protected void CreateBanners(Room room, Transform parent, float roomSize)
    {
        GameObject bannersParent = new GameObject("Banners");
        bannersParent.transform.parent = parent;
        
        float halfSize = roomSize / 2f;
        
        for (int i = 0; i < bannersCount; i++)
        {
            float angle = (360f / bannersCount) * i;
            float distance = halfSize * 0.85f;
            
            Vector3 position = room.center + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                0f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * distance
            );
            
            CreateBanner(position, bannersParent.transform);
        }
    }
    
    protected void CreateBanner(Vector3 position, Transform parent)
    {
        GameObject banner = new GameObject("Banner");
        banner.transform.parent = parent;
        banner.transform.position = position;
        
        // Tyč
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "BannerPole";
        pole.transform.parent = banner.transform;
        pole.transform.localPosition = new Vector3(0, 2.5f, 0);
        pole.transform.localScale = new Vector3(0.08f, 2.5f, 0.08f);
        pole.GetComponent<Renderer>().material.color = woodColor;
        
        // Látka praporu
        GameObject cloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cloth.name = "BannerCloth";
        cloth.transform.parent = banner.transform;
        cloth.transform.localPosition = new Vector3(0.5f, 4f, 0);
        cloth.transform.localScale = new Vector3(1f, 1.5f, 0.05f);
        
        // Různé barvy pro různé prapory
        Color[] bannerColors = new Color[]
        {
            new Color(0.8f, 0.1f, 0.1f), // Červená
            new Color(0.1f, 0.1f, 0.8f), // Modrá
            new Color(1f, 0.8f, 0f),     // Zlatá
            new Color(0.2f, 0.6f, 0.2f)  // Zelená
        };
        
        cloth.GetComponent<Renderer>().material.color = bannerColors[Random.Range(0, bannerColors.Length)];
    }

    public override void GenerateLevel()
    {
        base.GenerateLevel();
        ApplyVisualEffects();
        CreateWallTorches();
    }
    
    protected void CreateWallTorches()
    {
        GameObject torchesParent = new GameObject("WallTorches");
        torchesParent.transform.parent = transform;
        
        foreach (Room room in rooms)
        {
            float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
            float halfSize = roomSize / 2f;
            
            for (int i = 0; i < torchesCount; i++)
            {
                float angle = (360f / torchesCount) * i;
                float distance = halfSize * 0.85f;
                
                Vector3 position = room.center + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                    Random.Range(2.5f, 3.5f),
                    Mathf.Sin(angle * Mathf.Deg2Rad) * distance
                );
                
                CreateWallTorch(position, torchesParent.transform);
            }
        }
    }
    
    protected void CreateWallTorch(Vector3 position, Transform parent)
    {
        GameObject torch = new GameObject("WallTorch");
        torch.transform.parent = parent;
        torch.transform.position = position;
        
        // Držák pochodně (železný)
        GameObject holder = GameObject.CreatePrimitive(PrimitiveType.Cube);
        holder.name = "TorchHolder";
        holder.transform.parent = torch.transform;
        holder.transform.localPosition = new Vector3(0, -0.2f, 0);
        holder.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
        holder.GetComponent<Renderer>().material.color = ironColor;
        
        // Dřevo pochodně
        GameObject wood = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wood.name = "TorchWood";
        wood.transform.parent = torch.transform;
        wood.transform.localPosition = new Vector3(0, 0.2f, 0);
        wood.transform.localScale = new Vector3(0.08f, 0.3f, 0.08f);
        wood.GetComponent<Renderer>().material.color = woodColor;
        
        // Plamen
        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "Flame";
        flame.transform.parent = torch.transform;
        flame.transform.localPosition = new Vector3(0, 0.6f, 0);
        flame.transform.localScale = new Vector3(0.25f, 0.35f, 0.25f);
        
        Renderer flameRenderer = flame.GetComponent<Renderer>();
        flameRenderer.material = new Material(Shader.Find("Unlit/Color"));
        flameRenderer.material.color = new Color(1f, 0.6f, 0.2f);
        
        // Světlo
        GameObject lightObj = new GameObject("TorchLight");
        lightObj.transform.parent = torch.transform;
        lightObj.transform.localPosition = new Vector3(0, 0.6f, 0);
        
        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = new Color(1f, 0.7f, 0.4f);
        pointLight.intensity = 2.8f;
        pointLight.range = 8f;
        pointLight.shadows = LightShadows.Soft;
        
        // Částice ohně
        ParticleSystem ps = flame.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.6f;
        main.startSpeed = 0.8f;
        main.startSize = 0.2f;
        main.startColor = new Color(1f, 0.7f, 0.3f, 0.9f);
        main.maxParticles = 20;
        
        var emission = ps.emission;
        emission.rateOverTime = 25f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.08f;
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
    
    protected void CreateWeatherParticles()
    {
        // Déšť nebo sníh - najít střed levelu
        Vector3 levelCenter = Vector3.zero;
        if (rooms != null && rooms.Count > 0)
        {
            foreach (Room room in rooms)
            {
                levelCenter += room.center;
            }
            levelCenter /= rooms.Count;
        }
        
        GameObject epochParticles = new GameObject("WeatherParticles");
        epochParticles.transform.parent = null; // World space
        epochParticles.transform.position = levelCenter + Vector3.up * 12f; // Vysoko nad mapou
        
        ParticleSystem ps = epochParticles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 4f;
        main.startSpeed = 0f; // Žádná počáteční rychlost - částice jen padají
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
        main.startColor = new Color(0.85f, 0.9f, 0.95f, 0.7f);
        main.maxParticles = 300;
        main.gravityModifier = 2f; // Gravitace způsobí pád
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        main.playOnAwake = true;
        
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 80f;
        
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        // Pokrýt celý level
        float coverageSize = baseRoomSize * Mathf.Max(3, rooms.Count);
        shape.scale = new Vector3(coverageSize, 1f, coverageSize);
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = new Color(0.9f, 0.95f, 1f, 0.8f);
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 2f;
        renderer.velocityScale = 0.1f;
        
        ps.Play();
        
        Debug.Log($"Weather particles created at {epochParticles.transform.position}, coverage: {coverageSize}");}}
