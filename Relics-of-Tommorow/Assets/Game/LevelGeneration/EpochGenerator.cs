using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Základní třída pro generování epoch s procedurálním terénem
/// </summary>
public abstract class EpochGenerator : MonoBehaviour
{
    [Header("Terrain Generation")]
    [SerializeField] protected bool useProceduralTerrain = true;
    [SerializeField] protected int terrainWidth = 50;
    [SerializeField] protected int terrainLength = 50;
    [SerializeField] protected float terrainHeight = 10f;
    [SerializeField] protected float noiseScale = 20f;
    [SerializeField] protected AnimationCurve heightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("OR Use Prefab")]
    [SerializeField] protected GameObject floorPrefab;
    
    [Header("Environment Prefabs")]
    [SerializeField] protected GameObject[] decorationPrefabs;
    [SerializeField] protected int decorationCount = 20;
    
    [Header("Enemy Settings")]
    [SerializeField] protected bool spawnEnemies = true; // Povolit/zakázat spawning enemáků
    [SerializeField] protected GameObject[] enemyPrefabs;
    [SerializeField] protected int enemyCount = 5;
    [SerializeField] protected float minEnemySpacing = 5f;
    
    [Header("Player Spawn")]
    [SerializeField] protected bool spawnPlayer = true; // Povolit/zakázat spawning hráče
    [SerializeField] protected Vector3 playerSpawnPosition = new Vector3(0, 1, 0);
    
    [Header("Visual Effects")]
    [SerializeField] protected Color ambientLightColor = Color.gray;
    [SerializeField] protected float ambientIntensity = 0.5f;
    [SerializeField] protected Color directionalLightColor = Color.white;
    [SerializeField] protected float directionalLightIntensity = 1f;
    [SerializeField] protected Vector3 directionalLightRotation = new Vector3(50, -30, 0);
    [SerializeField] protected bool useFog = false;
    [SerializeField] protected Color fogColor = Color.gray;
    [SerializeField] protected float fogDensity = 0.01f;
    [SerializeField] protected GameObject particleSystemPrefab;
    
    protected List<Vector3> spawnedPositions = new List<Vector3>();
    protected Terrain generatedTerrain;
    protected GameObject epochLighting;
    protected GameObject epochParticles;
    
    void Start()
    {
        GenerateLevel();
    }
    
    public virtual void GenerateLevel()
    {
        ClearLevel();
        GenerateFloor();
        GenerateDecorations();
        SpawnEnemies();
        SetupPlayerSpawn();
        SetupVisualEffects();
    }
    
    protected virtual void ClearLevel()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        spawnedPositions.Clear();
        generatedTerrain = null;
        
        // Vyčistit starý lighting a částice
        if (epochLighting != null)
            Destroy(epochLighting);
        if (epochParticles != null)
            Destroy(epochParticles);
    }
    
    protected virtual void GenerateFloor()
    {
        if (useProceduralTerrain)
        {
            GenerateProceduralTerrain();
        }
        else if (floorPrefab != null)
        {
            GameObject floor = Instantiate(floorPrefab, Vector3.zero, Quaternion.identity, transform);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(terrainWidth / 10f, 1f, terrainLength / 10f);
        }
    }
    
    protected virtual void GenerateProceduralTerrain()
    {
        // Vytvořit TerrainData
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = 513;
        terrainData.size = new Vector3(terrainWidth, terrainHeight, terrainLength);
        
        // Vygenerovat heightmap pomocí Perlin noise
        float[,] heights = GenerateHeights();
        terrainData.SetHeights(0, 0, heights);
        
        // Vytvořit Terrain GameObject
        GameObject terrainObj = Terrain.CreateTerrainGameObject(terrainData);
        terrainObj.name = "ProceduralTerrain";
        terrainObj.transform.parent = transform;
        terrainObj.transform.position = new Vector3(-terrainWidth / 2f, 0, -terrainLength / 2f);
        
        generatedTerrain = terrainObj.GetComponent<Terrain>();
        
        // Nastavit layer na Ignore Raycast aby nekombatové raycasty neblokoval
        terrainObj.layer = LayerMask.NameToLayer("Ignore Raycast");
    }
    
    protected virtual float[,] GenerateHeights()
    {
        int resolution = 513;
        float[,] heights = new float[resolution, resolution];
        
        float offsetX = Random.Range(0f, 9999f);
        float offsetZ = Random.Range(0f, 9999f);
        
        for (int x = 0; x < resolution; x++)
        {
            for (int z = 0; z < resolution; z++)
            {
                float xCoord = (float)x / resolution * noiseScale + offsetX;
                float zCoord = (float)z / resolution * noiseScale + offsetZ;
                
                float sample = Mathf.PerlinNoise(xCoord, zCoord);
                heights[x, z] = heightCurve.Evaluate(sample);
            }
        }
        
        return heights;
    }
    
    protected virtual void GenerateDecorations()
    {
        if (decorationPrefabs == null || decorationPrefabs.Length == 0)
        {
            Debug.LogWarning($"{GetType().Name}: No decoration prefabs assigned!");
            return;
        }
        
        GameObject decorationParent = new GameObject("Decorations");
        decorationParent.transform.parent = transform;
        
        for (int i = 0; i < decorationCount; i++)
        {
            Vector3 position = GetRandomPositionOnTerrain();
            GameObject prefab = decorationPrefabs[Random.Range(0, decorationPrefabs.Length)];
            
            if (prefab != null)
            {
                GameObject decoration = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), decorationParent.transform);
                decoration.name = $"{prefab.name}_{i}";
            }
        }
    }
    
    protected virtual void SpawnEnemies()
    {
        if (!spawnEnemies)
        {
            Debug.Log($"{GetType().Name}: Spawn enemáků je vypnutý");
            return;
        }
        
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"{GetType().Name}: No enemy prefabs assigned!");
            return;
        }
        
        GameObject enemyParent = new GameObject("Enemies");
        enemyParent.transform.parent = transform;
        
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 position = GetRandomEnemyPosition();
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            
            if (prefab != null)
            {
                GameObject enemy = Instantiate(prefab, position, Quaternion.identity, enemyParent.transform);
                enemy.name = $"{prefab.name}_{i}";
                spawnedPositions.Add(position);
            }
        }
    }
    
    protected virtual void SetupPlayerSpawn()
    {
        if (!spawnPlayer)
        {
            Debug.Log($"{GetType().Name}: Spawn hráče je vypnutý");
            return;
        }
        
        // Najít RespawnManager a nastavit spawn point
        RespawnManager respawnManager = FindFirstObjectByType<RespawnManager>();
        if (respawnManager != null)
        {
            // Pokud je spawn vypnutý, vypnout i respawn
            if (!spawnPlayer)
            {
                // Přístup k enableRespawn by vyžadoval public property, zatím jen log
                Debug.Log($"{GetType().Name}: Player spawn je vypnutý, respawn by měl být také vypnutý v RespawnManager");
            }
            
            Debug.Log($"{GetType().Name}: Player spawn set to {playerSpawnPosition}");
        }
    }
    
    protected Vector3 GetRandomPositionOnTerrain()
    {
        float x = Random.Range(-terrainWidth / 2f, terrainWidth / 2f);
        float z = Random.Range(-terrainLength / 2f, terrainLength / 2f);
        float y = GetTerrainHeight(x, z);
        
        return new Vector3(x, y, z);
    }
    
    protected float GetTerrainHeight(float worldX, float worldZ)
    {
        if (generatedTerrain != null)
        {
            // Převést world pozici na terrain lokální pozici
            Vector3 terrainPos = generatedTerrain.transform.position;
            float relativeX = worldX - terrainPos.x;
            float relativeZ = worldZ - terrainPos.z;
            
            // Získat výšku z terrain
            return generatedTerrain.SampleHeight(new Vector3(worldX, 0, worldZ));
        }
        
        return 0f;
    }
    
    protected Vector3 GetRandomEnemyPosition()
    {
        Vector3 position;
        int attempts = 0;
        
        do
        {
            position = GetRandomPositionOnTerrain();
            position.y += 1f; // Trochu nad terénem
            attempts++;
            
            if (attempts > 100)
            {
                Debug.LogWarning($"{GetType().Name}: Could not find valid enemy position after 100 attempts!");
                break;
            }
        }
        while (!IsValidEnemyPosition(position));
        
        return position;
    }
    
    protected bool IsValidEnemyPosition(Vector3 position)
    {
        // Zkontrolovat vzdálenost od player spawnu
        if (Vector3.Distance(position, playerSpawnPosition) < minEnemySpacing * 2f)
            return false;
        
        // Zkontrolovat vzdálenost od ostatních enemáků
        foreach (Vector3 spawnedPos in spawnedPositions)
        {
            if (Vector3.Distance(position, spawnedPos) < minEnemySpacing)
                return false;
        }
        
        return true;
    }
    
    // Veřejná metoda pro regeneraci v editoru
    [ContextMenu("Regenerate Level")]
    public void RegenerateLevel()
    {
        GenerateLevel();
    }
    
    /// <summary>
    /// Nastavení vizuálních efektů pro epochu (osvětlení, částice, mlha)
    /// </summary>
    protected virtual void SetupVisualEffects()
    {
        SetupLighting();
        SetupFog();
        SetupParticles();
    }
    
    /// <summary>
    /// Vytvoří a nakonfiguruje osvětlení pro epochu
    /// </summary>
    protected virtual void SetupLighting()
    {
        // Vytvořit parent pro lighting
        epochLighting = new GameObject("EpochLighting");
        epochLighting.transform.parent = transform;
        
        // Nastavit ambient light
        RenderSettings.ambientLight = ambientLightColor;
        RenderSettings.ambientIntensity = ambientIntensity;
        
        // Vytvořit directional light
        GameObject lightObj = new GameObject("DirectionalLight");
        lightObj.transform.parent = epochLighting.transform;
        lightObj.transform.rotation = Quaternion.Euler(directionalLightRotation);
        
        Light dirLight = lightObj.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = directionalLightColor;
        dirLight.intensity = directionalLightIntensity;
        dirLight.shadows = LightShadows.Soft;
    }
    
    /// <summary>
    /// Nastavení mlhy pro atmosféru
    /// </summary>
    protected virtual void SetupFog()
    {
        RenderSettings.fog = useFog;
        if (useFog)
        {
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = fogDensity;
        }
    }
    
    /// <summary>
    /// Vytvoří částicový systém pro epochu (prach, déšť, sníh, atd.)
    /// </summary>
    protected virtual void SetupParticles()
    {
        if (particleSystemPrefab != null)
        {
            epochParticles = Instantiate(particleSystemPrefab, Vector3.zero, Quaternion.identity, transform);
            epochParticles.name = "EpochParticles";
        }
        else
        {
            // Vytvořit základní částicový systém, pokud není prefab
            CreateDefaultParticleSystem();
        }
    }
    
    /// <summary>
    /// Vytvoří základní částicový systém
    /// </summary>
    protected virtual void CreateDefaultParticleSystem()
    {
        // Override v jednotlivých epochách pro specifické částice
    }
}
