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
    [SerializeField] protected GameObject[] enemyPrefabs;
    [SerializeField] protected int enemyCount = 5;
    [SerializeField] protected float minEnemySpacing = 5f;
    
    [Header("Player Spawn")]
    [SerializeField] protected Vector3 playerSpawnPosition = new Vector3(0, 1, 0);
    
    protected List<Vector3> spawnedPositions = new List<Vector3>();
    protected Terrain generatedTerrain;
    
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
    }
    
    protected virtual void ClearLevel()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        spawnedPositions.Clear();
        generatedTerrain = null;
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
        // Najít RespawnManager a nastavit spawn point
        RespawnManager respawnManager = FindFirstObjectByType<RespawnManager>();
        if (respawnManager != null)
        {
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
}
