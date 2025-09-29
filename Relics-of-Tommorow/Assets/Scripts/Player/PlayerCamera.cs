using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private Camera playerCamera;
    
    [Header("Mouse Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2f;
    
    [Header("Camera Limits")]
    [SerializeField] private float maxLookUp = 80f;
    [SerializeField] private float maxLookDown = 80f;
    
    [Header("Crouch Camera Settings")]
    [SerializeField] private float standingCameraHeight = 1.6f;
    [SerializeField] private float crouchCameraHeight = 0.8f;
    [SerializeField] private float cameraTransitionSpeed = 8f;
    
    // Privátní proměnné
    private float mouseX;
    private float mouseY;
    private float xRotation = 0f;
    private Vector3 currentVelocity;
    private float targetCameraHeight;
    private PlayerMovement playerMovement;
    
    void Start()
    {
        SetupCamera();
    }
    
    void SetupCamera()
    {
        // Získání PlayerMovement komponenty
        playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
        }
        
        // Automatické nalezení Player Body pokud není přiřazeno
        if (playerBody == null)
        {
            playerBody = transform.parent;
            if (playerBody == null)
            {
                Debug.LogError("PlayerCamera: Player body not found! Please assign playerBody in inspector.");
            }
        }
        
        // Automatické nalezení kamery
        if (playerCamera == null)
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }
        
        // Nastavení výchozí pozice kamery
        targetCameraHeight = standingCameraHeight;
        transform.localPosition = new Vector3(0, targetCameraHeight, 0);
        
        // Uzamknutí kurzoru na střed obrazovky
        Cursor.lockState = CursorLockMode.Locked;
        
        Debug.Log("PlayerCamera: Camera setup completed!");
    }
    
    void Update()
    {
        HandleMouseLook();
        HandleCrouchCamera();
        
        // ESC pro odemknutí kurzoru (pro testování)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }
    }
    
    void HandleMouseLook()
    {
        // Získání vstupu myši
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Rotace hráče doleva/doprava (Y osa)
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
        
        // Rotace kamery nahoru/dolů (X osa)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookUp, maxLookDown);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    
    void HandleCrouchCamera()
    {
        if (playerMovement == null) return;
        
        // Určení cílové výšky kamery podle stavu hráče
        targetCameraHeight = playerMovement.IsCrouching() ? crouchCameraHeight : standingCameraHeight;
        
        // Plynulý přechod výšky kamery
        Vector3 targetPosition = new Vector3(0, targetCameraHeight, 0);
        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition, 
            targetPosition, 
            ref currentVelocity, 
            1f / cameraTransitionSpeed
        );
    }
    
    void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Debug.Log("Cursor unlocked");
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Debug.Log("Cursor locked");
        }
    }
    
    // Veřejné metody
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }
    
    public void SetCameraHeight(float standingHeight, float crouchHeight)
    {
        standingCameraHeight = standingHeight;
        crouchCameraHeight = crouchHeight;
    }
    
    public bool IsLookingUp()
    {
        return xRotation < -10f;
    }
    
    public bool IsLookingDown()
    {
        return xRotation > 10f;
    }
    
    public float GetVerticalLookAngle()
    {
        return xRotation;
    }
}
