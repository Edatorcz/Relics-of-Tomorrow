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
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpSpeed = 5f;
    
    [Header("Roll/Dodge Settings")]
    [SerializeField] private float rollSpeed = 10f;
    [SerializeField] private float rollDuration = 0.5f;
    [SerializeField] private float rollCooldown = 1f;
    [SerializeField] private float rollInvulnerabilityDuration = 0.5f;
    
    [Header("Freeze Settings")]
    [SerializeField] private bool freezeOnStart = true; // Zmrazit hráče na začátku
    private bool isFrozen = false;
    
    private Vector3 movement;
    private float currentSpeed;
    private bool isRunning;
    private bool isCrouching;
    private float targetHeight;
    
    // Jump variables
    private float verticalVelocity = 0f;
    
    // Roll variables
    private bool isRolling = false;
    private float rollTimer = 0f;
    private Vector3 rollDirection;
    private float lastRollTime = -999f;
    private PlayerHealth playerHealth;
    
    void Start()
    {
        SetupCharacterController();
        SetupCamera();
        
        // Nastavení výchozí výšky
        targetHeight = standingHeight;
        
        // RESET všeho
        movement = Vector3.zero;
        
        // Získání reference na PlayerHealth
        playerHealth = GetComponent<PlayerHealth>();
        
        // Freeze hráče na začátku pokud je to zapnuté
        if (freezeOnStart)
        {
            isFrozen = true;
            verticalVelocity = 0f; // Nulová gravitační rychlost
            Debug.Log("PlayerMovement: Hráč je zmrazený - zmáčkni levé tlačítko myši pro odmčení");
        }
        
        Debug.Log("PlayerMovement: Initialized with SimpleMove mode");
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
        // ODSTRANIT RIGIDBODY pokud existuje - způsobuje konflikt s CharacterController
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.LogWarning("PlayerMovement: Removing Rigidbody - conflicts with CharacterController!");
            DestroyImmediate(rb);
        }
        
        // ODSTRANIT běžné collidery - CharacterController má svůj vlastní
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            if (!(col is CharacterController))
            {
                Debug.LogWarning($"PlayerMovement: Removing {col.GetType().Name} - conflicts with CharacterController!");
                DestroyImmediate(col);
            }
        }
        
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
        characterController.minMoveDistance = 0f;           // 0 = žádné sklouzávání!
        
        Debug.Log("PlayerMovement: CharacterController configured successfully!");
    }
    
    void Update()
    {
        // Kontrola unfreeze - levé tlačítko myši
        if (isFrozen)
        {
            if (Input.GetMouseButtonDown(0)) // 0 = levé tlačítko myši
            {
                isFrozen = false;
                verticalVelocity = 0f; // Nulová gravitace na začátek
                Debug.Log("PlayerMovement: Hráč odmčen!");
            }
            // Při freeze stále aplikovat gravitaci, aby hráč spadl na zem
            if (characterController != null && characterController.enabled)
            {
                // Gravitace i když je frozen - aby hráč spadl na podlahu
                if (characterController.isGrounded)
                {
                    verticalVelocity = -2f;
                }
                else
                {
                    verticalVelocity -= 20f * Time.deltaTime;
                }
                
                Vector3 gravityMove = new Vector3(0, verticalVelocity, 0);
                characterController.Move(gravityMove * Time.deltaTime);
            }
            return; // Blokovat input a horizontální pohyb
        }
        
        // Roll - jen když není frozen
        if (isRolling)
        {
            HandleRoll();
        }
        else
        {
            HandleInput();
            HandleCrouch();
            
            // DŮLEŽITÉ: Pohyb voláme až po Input, aby se movement nastavil správně
            if (characterController != null && characterController.enabled)
            {
                SimpleMove();
            }
        }
    }
    
    // POHYB S GRAVITACÍ A SKOKEM
    void SimpleMove()
    {
        // Určení rychlosti
        float speed = walkSpeed;
        if (isRunning) speed = runSpeed;
        if (isCrouching) speed = crouchSpeed;
        
        // Horizontální pohyb - RELATIVNĚ KE KAMEŘE!
        Vector3 moveDirection = Vector3.zero;
        if (movement.magnitude > 0.01f)
        {
            // Získat směr kamery (bez Y rotace)
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;
            
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            // Pohyb relativně ke kameře
            moveDirection = cameraForward * movement.z + cameraRight * movement.x;
            moveDirection = moveDirection.normalized * speed;
        }
        
        // Gravitace
        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = -2f; // Přitlačit k zemi
        }
        else
        {
            verticalVelocity -= 20f * Time.deltaTime; // Gravitace
        }
        
        // Finlní pohyb
        moveDirection.y = verticalVelocity;
        characterController.Move(moveDirection * Time.deltaTime);
    }
    
    void HandleInput()
    {
        // Blokovat input když je hráč frozen
        if (isFrozen)
        {
            movement = Vector3.zero;
            isRunning = false;
            return;
        }
        
        // Získání vstupu WASD
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D klávesy
        float vertical = Input.GetAxisRaw("Vertical");     // W/S klávesy
        
        // Vytvoření vektoru pohybu - JEN pokud je nějaký input
        if (Mathf.Abs(horizontal) < 0.01f && Mathf.Abs(vertical) < 0.01f)
        {
            movement = Vector3.zero; // Žádný pohyb!
        }
        else
        {
            movement = new Vector3(horizontal, 0, vertical).normalized;
        }
        
        // Kontrola běhání (Shift)
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        // ROLL - dvojité zmáčknutí Shift při běhání NEBO stisk CTRL při běhání
        if (Input.GetKeyDown(KeyCode.LeftControl) && isRunning && movement.magnitude > 0.1f && !isRolling)
        {
            TryRoll();
            return;
        }
        
        // Kontrola crouchování (Ctrl) - jen pokud NEběžíme
        isCrouching = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !isRunning;
        
        // Skok - SPACE
        if (Input.GetKeyDown(KeyCode.Space) && characterController.isGrounded)
        {
            verticalVelocity = jumpSpeed;
        }
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
    
    // === ROLL/DODGE SYSTEM ===
    
    private void TryRoll()
    {
        // Kontrola cooldownu
        if (Time.time - lastRollTime < rollCooldown)
            return;
        
        // Začít roll
        StartRoll();
    }
    
    private void StartRoll()
    {
        isRolling = true;
        rollTimer = rollDuration;
        lastRollTime = Time.time;
        
        // Snížit výšku během rollu (crouch)
        if (characterController != null)
        {
            characterController.height = crouchHeight;
            characterController.center = new Vector3(0, crouchHeight * 0.5f, 0);
        }
        
        // Směr rollu = směr pohybu (kam se hýbeš WASD) relativní ke kameře
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // Roll ve směru pohybu
        rollDirection = (cameraForward * movement.z + cameraRight * movement.x).normalized;
        
        // Aktivovat invulnerabilitu
        if (playerHealth != null)
        {
            playerHealth.SetInvulnerable(true, rollInvulnerabilityDuration);
        }
        
        Debug.Log($"PlayerMovement: Rolling! Direction: {rollDirection}, Movement: {movement}");
    }
    
    private void HandleRoll()
    {
        rollTimer -= Time.deltaTime;
        
        if (rollTimer <= 0f)
        {
            EndRoll();
            return;
        }
        
        // Pohyb během rollu - rychlý pohyb ve směru rollu
        if (characterController != null && characterController.enabled)
        {
            Vector3 rollMovement = rollDirection * rollSpeed;
            rollMovement.y = -2f; // Držet na zemi
            characterController.Move(rollMovement * Time.deltaTime);
        }
    }
    
    private void EndRoll()
    {
        isRolling = false;
        
        // Vrátit normální výšku
        if (characterController != null)
        {
            characterController.height = standingHeight;
            characterController.center = new Vector3(0, standingHeight * 0.5f, 0);
        }
        
        Debug.Log("PlayerMovement: Roll ended");
    }
    
    public bool IsRolling()
    {
        return isRolling;
    }
}
