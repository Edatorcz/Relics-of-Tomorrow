using UnityEngine;

/// <summary>
/// Pomocný script pro rychlé nastavení Player systému s kamerou
/// Spustí se automaticky při přidání na GameObject
/// </summary>
[System.Serializable]
public class PlayerSetup : MonoBehaviour
{
    [Header("Auto Setup Options")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createCameraIfMissing = true;
    [SerializeField] private string cameraObjectName = "PlayerCamera";
    
    [Header("Camera Setup")]
    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0, 1.6f, 0);
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupPlayer();
        }
    }
    
    [ContextMenu("Setup Player System")]
    public void SetupPlayer()
    {
        Debug.Log("PlayerSetup: Starting automatic player setup...");
        
        // Ujistit se, že máme PlayerMovement
        SetupPlayerMovement();
        
        // Nastavit kameru
        if (createCameraIfMissing)
        {
            SetupPlayerCamera();
        }
        
        Debug.Log("PlayerSetup: Player setup completed!");
    }
    
    void SetupPlayerMovement()
    {
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            playerMovement = gameObject.AddComponent<PlayerMovement>();
            Debug.Log("PlayerSetup: Added PlayerMovement component");
        }
    }
    
    void SetupPlayerCamera()
    {
        // Najít existující kameru
        PlayerCamera existingCamera = GetComponentInChildren<PlayerCamera>();
        if (existingCamera != null)
        {
            Debug.Log("PlayerSetup: PlayerCamera already exists");
            return;
        }
        
        // Vytvořit nový camera GameObject
        GameObject cameraObject = new GameObject(cameraObjectName);
        cameraObject.transform.SetParent(transform);
        cameraObject.transform.localPosition = cameraLocalPosition;
        cameraObject.transform.localRotation = Quaternion.identity;
        
        // Přidat Camera komponentu
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 60f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 1000f;
        
        // Přidat PlayerCamera script
        PlayerCamera playerCamera = cameraObject.AddComponent<PlayerCamera>();
        
        // Nastavit tag MainCamera pokud není žádná jiná hlavní kamera
        if (Camera.main == null)
        {
            cameraObject.tag = "MainCamera";
        }
        
        Debug.Log($"PlayerSetup: Created PlayerCamera at {cameraLocalPosition}");
    }
    
    // Veřejná metoda pro manuální nastavení pozice kamery
    public void SetCameraPosition(Vector3 localPosition)
    {
        cameraLocalPosition = localPosition;
        
        PlayerCamera camera = GetComponentInChildren<PlayerCamera>();
        if (camera != null)
        {
            camera.transform.localPosition = localPosition;
        }
    }
}
