using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Optimalizace výkonu - deaktivuje místnosti a objekty, které hráč nevidí
/// Aktivuje pouze místnosti v blízkosti hráče
/// </summary>
public class RoomOptimizer : MonoBehaviour
{
    public static RoomOptimizer Instance { get; private set; }
    
    [Header("Optimization Settings")]
    [SerializeField] private float activeRoomDistance = 40f; // Vzdálenost pro aktivní místnosti
    [SerializeField] private float updateInterval = 0.5f; // Jak často kontrolovat (v sekundách)
    [SerializeField] private bool enableOptimization = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private int activeRoomsCount = 0;
    [SerializeField] private int totalRoomsCount = 0;
    
    private Transform playerTransform;
    private List<OptimizedRoom> rooms = new List<OptimizedRoom>();
    private float nextUpdateTime = 0f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Najít hráče
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
    
    void Update()
    {
        if (!enableOptimization || playerTransform == null) return;
        
        // Kontrola jen každých X sekund
        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            OptimizeRooms();
        }
    }
    
    /// <summary>
    /// Registrovat místnost pro optimalizaci
    /// </summary>
    public void RegisterRoom(GameObject roomObject, Vector3 roomCenter)
    {
        OptimizedRoom room = new OptimizedRoom
        {
            roomObject = roomObject,
            center = roomCenter,
            isActive = true
        };
        
        rooms.Add(room);
        totalRoomsCount = rooms.Count;
        
        if (showDebugInfo)
        {
            Debug.Log($"RoomOptimizer: Registrována místnost na pozici {roomCenter}, celkem: {totalRoomsCount}");
        }
    }
    
    /// <summary>
    /// Deaktivovat všechny registrované místnosti
    /// </summary>
    public void ClearRooms()
    {
        rooms.Clear();
        totalRoomsCount = 0;
        activeRoomsCount = 0;
    }
    
    /// <summary>
    /// Aktivovat/deaktivovat místnosti podle vzdálenosti od hráče
    /// </summary>
    private void OptimizeRooms()
    {
        if (playerTransform == null) return;
        
        int activeCount = 0;
        Vector3 playerPos = playerTransform.position;
        
        foreach (OptimizedRoom room in rooms)
        {
            if (room.roomObject == null) continue;
            
            // Vypočítat vzdálenost od hráče
            float distance = Vector3.Distance(playerPos, room.center);
            bool shouldBeActive = distance <= activeRoomDistance;
            
            // Aktivovat/deaktivovat pouze pokud se změnil stav
            if (room.isActive != shouldBeActive)
            {
                SetRoomActive(room, shouldBeActive);
            }
            
            if (shouldBeActive)
            {
                activeCount++;
            }
        }
        
        activeRoomsCount = activeCount;
        
        if (showDebugInfo && Time.frameCount % 60 == 0) // Debug každou sekundu
        {
            Debug.Log($"RoomOptimizer: Aktivní místnosti: {activeRoomsCount}/{totalRoomsCount}");
        }
    }
    
    /// <summary>
    /// Aktivovat nebo deaktivovat místnost
    /// </summary>
    private void SetRoomActive(OptimizedRoom room, bool active)
    {
        room.isActive = active;
        
        // Deaktivovat jen určité komponenty, ne celý GameObject (aby se neztratila struktura)
        if (room.roomObject != null)
        {
            // Deaktivovat nepřátele
            Transform enemiesParent = room.roomObject.transform.Find("Enemies");
            if (enemiesParent != null)
            {
                foreach (Transform enemy in enemiesParent)
                {
                    enemy.gameObject.SetActive(active);
                }
            }
            
            // Deaktivovat itemy (kromě když jsou blízko)
            Transform itemsParent = room.roomObject.transform.Find("Items");
            if (itemsParent != null)
            {
                itemsParent.gameObject.SetActive(active);
            }
            
            // Deaktivovat particle systémy
            ParticleSystem[] particles = room.roomObject.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in particles)
            {
                if (active)
                {
                    if (!ps.isPlaying) ps.Play();
                }
                else
                {
                    ps.Stop();
                }
            }
            
            // Deaktivovat světla (kromě hlavních)
            Light[] lights = room.roomObject.GetComponentsInChildren<Light>(true);
            foreach (Light light in lights)
            {
                // Ponechat hlavní světla aktivní vždy
                if (light.type != LightType.Directional)
                {
                    light.enabled = active;
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"RoomOptimizer: Místnost {room.roomObject.name} {(active ? "AKTIVOVÁNA" : "DEAKTIVOVÁNA")}");
        }
    }
    
    /// <summary>
    /// Nastavit vzdálenost pro aktivní místnosti
    /// </summary>
    public void SetActiveDistance(float distance)
    {
        activeRoomDistance = distance;
    }
    
    /// <summary>
    /// Zapnout/vypnout optimalizaci
    /// </summary>
    public void SetOptimizationEnabled(bool enabled)
    {
        enableOptimization = enabled;
        
        if (!enabled)
        {
            // Aktivovat všechny místnosti
            foreach (OptimizedRoom room in rooms)
            {
                if (!room.isActive)
                {
                    SetRoomActive(room, true);
                }
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugInfo || playerTransform == null) return;
        
        // Nakreslit radius aktivních místností
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(playerTransform.position, activeRoomDistance);
        
        // Nakreslit místnosti
        foreach (OptimizedRoom room in rooms)
        {
            if (room.roomObject == null) continue;
            
            Gizmos.color = room.isActive ? Color.green : Color.red;
            Gizmos.DrawWireCube(room.center, Vector3.one * 5f);
        }
    }
    
    // Pomocná třída pro místnost
    [System.Serializable]
    private class OptimizedRoom
    {
        public GameObject roomObject;
        public Vector3 center;
        public bool isActive;
    }
}
