using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager pro správu všech nepřátel ve hře
/// Sleduje počty, spawning, events a poskytuje API pro jiné systémy
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Management")]
    [SerializeField] private List<EnemyBase> allEnemies = new List<EnemyBase>();
    [SerializeField] private int maxEnemyCount = 20;
    [SerializeField] private bool autoRegisterEnemies = true;
    
    [Header("Spawning")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnCooldown = 5f;
    [SerializeField] private bool enableAutoSpawn = false;
    
    [Header("Difficulty Scaling")]
    [SerializeField] private float difficultyMultiplier = 1f;
    [SerializeField] private float difficultyIncreaseRate = 0.1f;
    [SerializeField] private float maxDifficultyMultiplier = 3f;
    
    // Events
    public System.Action<EnemyBase> OnEnemySpawned;
    public System.Action<EnemyBase> OnEnemyDied;
    public System.Action<int> OnEnemyCountChanged;
    public System.Action OnAllEnemiesDefeated;
    
    // Privátní proměnné
    private float lastSpawnTime;
    private int totalEnemiesSpawned = 0;
    private int totalEnemiesKilled = 0;
    
    // Singleton pattern (volitelný)
    public static EnemyManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        Initialize();
    }
    
    void Initialize()
    {
        // Automaticky najít všechny nepřátele ve scéně
        if (autoRegisterEnemies)
        {
            RegisterAllEnemiesInScene();
        }
        
    }
    
    void Update()
    {
        // Auto spawning
        if (enableAutoSpawn && Time.time - lastSpawnTime >= spawnCooldown)
        {
            if (allEnemies.Count < maxEnemyCount && enemyPrefabs.Length > 0 && spawnPoints.Length > 0)
            {
                SpawnRandomEnemy();
                lastSpawnTime = Time.time;
            }
        }
        
        // Difficulty scaling
        UpdateDifficulty();
    }
    
    private void RegisterAllEnemiesInScene()
    {
        EnemyBase[] enemiesInScene = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        
        foreach (EnemyBase enemy in enemiesInScene)
        {
            RegisterEnemy(enemy);
        }
    }
    
    public void RegisterEnemy(EnemyBase enemy)
    {
        if (enemy == null || allEnemies.Contains(enemy)) return;
        
        allEnemies.Add(enemy);
        
        // Přihlásit se k events
        enemy.OnDeath += HandleEnemyDeath;
        enemy.OnPlayerDetected += HandlePlayerDetected;
        enemy.OnPlayerLost += HandlePlayerLost;
        
        OnEnemyCountChanged?.Invoke(allEnemies.Count);
        
    }
    
    public void UnregisterEnemy(EnemyBase enemy)
    {
        if (enemy == null || !allEnemies.Contains(enemy)) return;
        
        allEnemies.Remove(enemy);
        
        // Odhlásit events
        enemy.OnDeath -= HandleEnemyDeath;
        enemy.OnPlayerDetected -= HandlePlayerDetected;
        enemy.OnPlayerLost -= HandlePlayerLost;
        
        OnEnemyCountChanged?.Invoke(allEnemies.Count);
        
    }
    
    private void HandleEnemyDeath(EnemyBase deadEnemy)
    {
        totalEnemiesKilled++;
        UnregisterEnemy(deadEnemy);
        
        OnEnemyDied?.Invoke(deadEnemy);
        
        // Kontrola zda jsou všichni nepřátelé poraženi
        if (allEnemies.Count == 0)
        {
            OnAllEnemiesDefeated?.Invoke();
        }
    }
    
    private void HandlePlayerDetected(EnemyBase enemy)
    {
        
        // Můžeme zde implementovat alert systém pro ostatní nepřátele
        AlertNearbyEnemies(enemy.transform.position, 10f);
    }
    
    private void HandlePlayerLost(EnemyBase enemy)
    {
        Debug.Log($"EnemyManager: {enemy.name} lost player");
    }
    
    public void SpawnRandomEnemy()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;
        
        // Náhodný prefab a spawn point
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        SpawnEnemy(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }
    
    public EnemyBase SpawnEnemy(GameObject enemyPrefab, Vector3 position, Quaternion rotation)
    {
        if (enemyPrefab == null) return null;
        
        GameObject spawnedEnemy = Instantiate(enemyPrefab, position, rotation);
        EnemyBase enemyScript = spawnedEnemy.GetComponent<EnemyBase>();
        
        if (enemyScript != null)
        {
            // Aplikovat difficulty scaling
            ApplyDifficultyScaling(enemyScript);
            
            RegisterEnemy(enemyScript);
            totalEnemiesSpawned++;
            
            OnEnemySpawned?.Invoke(enemyScript);
            
        }
        
        return enemyScript;
    }
    
    private void ApplyDifficultyScaling(EnemyBase enemy)
    {
        // Zvýšit health a damage podle difficulty
        enemy.transform.localScale *= (1f + (difficultyMultiplier - 1f) * 0.1f); // Mírné zvětšení
    }
    
    private void UpdateDifficulty()
    {
        // Postupné zvyšování obtížnosti
        float targetDifficulty = 1f + (totalEnemiesKilled * difficultyIncreaseRate);
        targetDifficulty = Mathf.Clamp(targetDifficulty, 1f, maxDifficultyMultiplier);
        
        difficultyMultiplier = Mathf.Lerp(difficultyMultiplier, targetDifficulty, Time.deltaTime * 0.1f);
    }
    
    public void AlertNearbyEnemies(Vector3 alertPosition, float alertRadius)
    {
        foreach (EnemyBase enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead()) continue;
            
            float distance = Vector3.Distance(enemy.transform.position, alertPosition);
            if (distance <= alertRadius)
            {
                // Přepnout nepřítele do chase stavu pokud je v idle/patrol
                if (enemy.GetCurrentState() == EnemyBase.EnemyState.Idle || 
                    enemy.GetCurrentState() == EnemyBase.EnemyState.Patrol)
                {
                    // Toto by vyžadovalo public metodu v EnemyBase
                    Debug.Log($"EnemyManager: Alerted {enemy.name}");
                }
            }
        }
    }
    
    // Public API pro jiné systémy
    public int GetEnemyCount() => allEnemies.Count;
    public int GetTotalEnemiesSpawned() => totalEnemiesSpawned;
    public int GetTotalEnemiesKilled() => totalEnemiesKilled;
    public float GetDifficultyMultiplier() => difficultyMultiplier;
    public List<EnemyBase> GetAllEnemies() => new List<EnemyBase>(allEnemies);
    
    public List<EnemyBase> GetEnemiesInRadius(Vector3 center, float radius)
    {
        List<EnemyBase> enemiesInRadius = new List<EnemyBase>();
        
        foreach (EnemyBase enemy in allEnemies)
        {
            if (enemy != null && !enemy.IsDead() && 
                Vector3.Distance(enemy.transform.position, center) <= radius)
            {
                enemiesInRadius.Add(enemy);
            }
        }
        
        return enemiesInRadius;
    }
    
    public void KillAllEnemies()
    {
        foreach (EnemyBase enemy in allEnemies.ToArray())
        {
            if (enemy != null && !enemy.IsDead())
            {
                enemy.TakeDamage(999999f);
            }
        }
    }
    
    public void SetAutoSpawn(bool enabled)
    {
        enableAutoSpawn = enabled;
    }
    
    // Debug info
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.EndArea();
    }
}
