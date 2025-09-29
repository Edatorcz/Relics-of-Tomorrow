using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float crouchSpeed = 1.5f;
    
    [Header("Character Controller Settings")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float characterRadius = 0.5f;
    [SerializeField] private float stepOffset = 0.3f;
    [SerializeField] private float slopeLimit = 45f;
    [SerializeField] private float skinWidth = 0.08f;
    
    [Header("Camera Integration")]
    [SerializeField] private PlayerCamera playerCamera;
    
    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    
    private Vector3 movement;
    private float currentSpeed;
    private bool isRunning;
    private bool isCrouching;
    private float targetHeight;
    
    void Start()
    {
        SetupCharacterController();
        SetupCamera();
        
        // Nastavení výchozí výšky
        targetHeight = standingHeight;
    }
    
    void SetupCamera()
    {
        // Automatické nalezení PlayerCamera pokud není přiřazena
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<PlayerCamera>();
        }
        
        if (playerCamera == null)
        {
            Debug.Log("PlayerMovement: PlayerCamera not found. Camera integration disabled.");
        }
        else
        {
            Debug.Log("PlayerMovement: PlayerCamera found and linked!");
        }
    }
    
    void SetupCharacterController()
    {
        // Získání Character Controller komponenty pokud není přiřazena
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        
        // Vytvoření Character Controller pokud neexistuje
        if (characterController == null)
        {
            Debug.Log("PlayerMovement: Creating CharacterController component...");
            characterController = gameObject.AddComponent<CharacterController>();
        }
        
        // Konfigurace Character Controller parametrů
        ConfigureCharacterController();
    }
    
    void ConfigureCharacterController()
    {
        if (characterController == null) return;
        
        // Základní nastavení Character Controller
        characterController.height = standingHeight;
        characterController.radius = characterRadius;
        characterController.center = new Vector3(0, standingHeight * 0.5f, 0);
        
        // Nastavení pro pohyb po schodech a sklonech
        characterController.stepOffset = stepOffset;        // Výška schodu, kterou může hráč překonat
        characterController.slopeLimit = slopeLimit;        // Maximální sklon, po kterém může hráč chodit
        
        // Detekce kolizí
        characterController.skinWidth = skinWidth;          // Tloušťka "kůže" pro detekci kolizí
        characterController.minMoveDistance = 0.001f;       // Minimální vzdálenost pro pohyb
        
        Debug.Log("PlayerMovement: CharacterController configured successfully!");
    }
    
    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleCrouch();
    }
    
    void HandleInput()
    {
        // Získání vstupu WASD
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D klávesy
        float vertical = Input.GetAxisRaw("Vertical");     // W/S klávesy
        
        // Vytvoření vektoru pohybu
        movement = new Vector3(horizontal, 0, vertical).normalized;
        
        // Kontrola běhání (Shift)
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        // Kontrola crouchování (Ctrl)
        isCrouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }
    
    void HandleMovement()
    {
        if (characterController == null) return;
        
        // Určení rychlosti na základě stavu hráče
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isRunning)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
        
        // Aplikace pohybu
        Vector3 moveVector = movement * currentSpeed * Time.deltaTime;
        
        // Přidání gravitace (jednoduchá implementace)
        if (!characterController.isGrounded)
        {
            moveVector.y = -9.81f * Time.deltaTime;
        }
        
        // Pohyb hráče
        characterController.Move(moveVector);
    }
    
    void HandleCrouch()
    {
        if (characterController == null) return;
        
        // Nastavení cílové výšky
        targetHeight = isCrouching ? crouchHeight : standingHeight;
        
        // Plynulý přechod výšky
        float newHeight = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        characterController.height = newHeight;
        
        // Úprava středu Character Controlleru pro správné crouchování
        Vector3 center = characterController.center;
        center.y = newHeight * 0.5f;
        characterController.center = center;
    }
    
    // Veřejné metody pro získání stavu hráče (užitečné pro animace či UI)
    public bool IsMoving()
    {
        return movement.magnitude > 0.1f;
    }
    
    public bool IsRunning()
    {
        return isRunning && IsMoving();
    }
    
    public bool IsCrouching()
    {
        return isCrouching;
    }
    
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    public Vector3 GetMovementDirection()
    {
        return movement;
    }
}
