using UnityEngine;

/// <summary>
/// Správa respawnu hráče a přesunu mezi epochami po smrti
/// </summary>
public class RespawnManager : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private Transform[] epochSpawnPoints;
    [SerializeField] private string[] epochNames = { "Pravěk", "Starověk", "Středověk", "Současnost", "Budoucnost" };
    [SerializeField] private float respawnDelay = 2f;
    
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerHealth playerHealth;
    
    private int currentEpochIndex = 0;
    private static RespawnManager instance;
    
    public static RespawnManager Instance => instance;
    
    void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    
    void Start()
    {
        // Najít hráče pokud není přiřazen
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (playerHealth == null && player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
        
        // Přihlásit se k eventu smrti
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied += HandlePlayerDeath;
        }
        
        // Pokud spawn pointy nejsou nastaveny, najít je
        if (epochSpawnPoints == null || epochSpawnPoints.Length == 0)
        {
            FindEpochSpawnPoints();
        }
        
        Debug.Log($"RespawnManager: Initialized with {epochSpawnPoints.Length} spawn points");
    }
    
    void FindEpochSpawnPoints()
    {
        // Hledat objekty s názvem obsahujícím "SpawnPoint" nebo "Spawn"
        GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag("Respawn");
        
        if (spawnObjects.Length > 0)
        {
            epochSpawnPoints = new Transform[spawnObjects.Length];
            for (int i = 0; i < spawnObjects.Length; i++)
            {
                epochSpawnPoints[i] = spawnObjects[i].transform;
            }
            Debug.Log($"RespawnManager: Found {spawnObjects.Length} spawn points with 'Respawn' tag");
        }
        else
        {
            // Fallback - vytvoř default spawn pointy
            CreateDefaultSpawnPoints();
        }
    }
    
    void CreateDefaultSpawnPoints()
    {
        epochSpawnPoints = new Transform[5];
        
        for (int i = 0; i < 5; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_Epoch{i}");
            spawnPoint.transform.position = new Vector3(i * 50f, 0, 0); // 50 jednotek mezi epochami
            epochSpawnPoints[i] = spawnPoint.transform;
        }
        
        Debug.Log("RespawnManager: Created 5 default spawn points");
    }
    
    void HandlePlayerDeath()
    {
        Debug.Log("RespawnManager: Player died! Preparing respawn...");
        Invoke(nameof(RespawnPlayer), respawnDelay);
    }
    
    void RespawnPlayer()
    {
        if (player == null || playerHealth == null) return;
        
        // Přesunout do další epochy
        currentEpochIndex = (currentEpochIndex + 1) % epochSpawnPoints.Length;
        
        // Získat spawn pozici
        Vector3 spawnPosition = GetSpawnPosition();
        
        // Teleportovat hráče
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            player.transform.position = spawnPosition;
            controller.enabled = true;
        }
        else
        {
            player.transform.position = spawnPosition;
        }
        
        // Reset rotace
        player.transform.rotation = Quaternion.identity;
        
        // Respawnout hráče (obnovit health)
        playerHealth.Respawn();
        
        // Zobrazit zprávu
        string epochName = currentEpochIndex < epochNames.Length ? epochNames[currentEpochIndex] : $"Epocha {currentEpochIndex + 1}";
        Debug.Log($"RespawnManager: Player respawned in {epochName} at {spawnPosition}");
        
        // Volitelně - zobrazit UI zprávu
        ShowRespawnMessage(epochName);
    }
    
    Vector3 GetSpawnPosition()
    {
        if (epochSpawnPoints != null && epochSpawnPoints.Length > 0 && currentEpochIndex < epochSpawnPoints.Length)
        {
            Transform spawnPoint = epochSpawnPoints[currentEpochIndex];
            if (spawnPoint != null)
            {
                return spawnPoint.position;
            }
        }
        
        // Fallback - spawn na default pozici
        return new Vector3(currentEpochIndex * 50f, 1f, 0);
    }
    
    void ShowRespawnMessage(string epochName)
    {
        // TODO: Zobrazit UI message
        // Například přes UI Manager nebo Canvas text
        Debug.Log($"=== VSTUPUJEŠ DO EPOCHY: {epochName.ToUpper()} ===");
    }
    
    // Veřejné metody
    public void SetCurrentEpoch(int epochIndex)
    {
        currentEpochIndex = Mathf.Clamp(epochIndex, 0, epochSpawnPoints.Length - 1);
    }
    
    public int GetCurrentEpochIndex()
    {
        return currentEpochIndex;
    }
    
    public string GetCurrentEpochName()
    {
        return currentEpochIndex < epochNames.Length ? epochNames[currentEpochIndex] : $"Epocha {currentEpochIndex + 1}";
    }
    
    public void AddSpawnPoint(Transform spawnPoint)
    {
        if (epochSpawnPoints == null)
        {
            epochSpawnPoints = new Transform[] { spawnPoint };
        }
        else
        {
            Transform[] newArray = new Transform[epochSpawnPoints.Length + 1];
            epochSpawnPoints.CopyTo(newArray, 0);
            newArray[epochSpawnPoints.Length] = spawnPoint;
            epochSpawnPoints = newArray;
        }
    }
    
    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied -= HandlePlayerDeath;
        }
    }
    
    // Debug vizualizace
    void OnDrawGizmos()
    {
        if (epochSpawnPoints == null) return;
        
        for (int i = 0; i < epochSpawnPoints.Length; i++)
        {
            if (epochSpawnPoints[i] == null) continue;
            
            // Barva podle indexu
            Gizmos.color = i == currentEpochIndex ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(epochSpawnPoints[i].position, 1f);
            
            // Šipka nahoru
            Gizmos.DrawLine(epochSpawnPoints[i].position, epochSpawnPoints[i].position + Vector3.up * 3f);
        }
    }
}
