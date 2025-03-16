using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class HunterPlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Transform cameraTransform;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float gamepadSensitivity = 200f;
    [SerializeField] private float maxLookAngle = 85f;
    [SerializeField] private float deadzone = 0.1f;
    [SerializeField] private float mouseSmoothing = 0.1f;

    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isGrounded;
    private Vector2 moveInput; // For gamepad movement
    private Vector2 keyboardMoveInput; // For keyboard movement
    private Vector2 gamepadLookInput;
    private Vector2 mouseLookInput;
    private Vector2 smoothMouseLookInput;
    private bool jumpPressed;
    private bool usingGamepad;

    private InputAction moveAction; // Gamepad movement
    private InputAction jumpAction;
    private InputAction gamepadLookAction;

    void Awake()
    {
        // --- Gamepad Movement Setup ---
        moveAction = new InputAction("Move", InputActionType.Value, "<Gamepad>/leftStick");

        // --- Shared Jump Setup ---
        jumpAction = new InputAction("Jump", InputActionType.Button, "<Gamepad>/buttonSouth");
        jumpAction.AddBinding("<Keyboard>/space");

        // --- Gamepad Look Setup ---
        gamepadLookAction = new InputAction("GamepadLook", InputActionType.Value, "<Gamepad>/rightStick");

        // Callbacks
        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => moveInput = Vector2.zero;
        gamepadLookAction.performed += ctx => gamepadLookInput = ctx.ReadValue<Vector2>();
        gamepadLookAction.canceled += ctx => gamepadLookInput = Vector2.zero;
        jumpAction.performed += ctx => jumpPressed = true;
        jumpAction.canceled += ctx => jumpPressed = false;
    }

    void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        gamepadLookAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        gamepadLookAction.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = transform.Find("Hunter_Camera");
        if (cameraTransform == null) Debug.LogError("Camera not found!");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        smoothMouseLookInput = Vector2.zero;
    }

    void Update()
    {
        usingGamepad = Gamepad.current != null && (moveInput != Vector2.zero || gamepadLookInput != Vector2.zero);
        Debug.Log($"Using Gamepad: {usingGamepad}");

        HandleMovement();
        HandleLook();
    }

    // --- Movement Logic ---
    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        Vector2 adjustedMoveInput;

        if (usingGamepad)
        {
            // --- Gamepad Movement ---
            adjustedMoveInput = moveInput;
            if (adjustedMoveInput.magnitude < deadzone) adjustedMoveInput = Vector2.zero;
            Debug.Log($"Gamepad Move Input: {adjustedMoveInput}");
        }
        else
        {
            // --- Keyboard Movement ---
            keyboardMoveInput = new Vector2(
                Keyboard.current.dKey.isPressed ? 1f : (Keyboard.current.aKey.isPressed ? -1f : 0f),
                Keyboard.current.wKey.isPressed ? 1f : (Keyboard.current.sKey.isPressed ? -1f : 0f)
            );
            adjustedMoveInput = keyboardMoveInput;
            if (adjustedMoveInput.magnitude < deadzone) adjustedMoveInput = Vector2.zero;
            Debug.Log($"Keyboard Move Input: {adjustedMoveInput}");
        }

        Vector3 move = (transform.right * adjustedMoveInput.x + transform.forward * adjustedMoveInput.y).normalized * walkSpeed;
        controller.Move(move * Time.deltaTime);

        if (isGrounded && jumpPressed)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log("Jump triggered!");
            jumpPressed = false;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // --- Look Logic ---
    void HandleLook()
    {
        Vector2 lookInput;

        if (usingGamepad)
        {
            // --- Gamepad Look ---
            lookInput = gamepadLookInput;
            if (lookInput.magnitude < deadzone) lookInput = Vector2.zero;
            Debug.Log($"Gamepad Look Input: {lookInput}");
            lookInput *= gamepadSensitivity * Time.deltaTime;
        }
        else
        {
            // --- Mouse Look ---
            mouseLookInput = Mouse.current.delta.ReadValue();
            smoothMouseLookInput = Vector2.Lerp(smoothMouseLookInput, mouseLookInput, mouseSmoothing);
            lookInput = smoothMouseLookInput;
            if (lookInput.magnitude < deadzone) lookInput = Vector2.zero;
            Debug.Log($"Mouse Look Input: {lookInput}");
            lookInput *= mouseSensitivity * 0.001f;
        }

        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x);
    }

    void OnDestroy()
    {
        moveAction.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled -= ctx => moveInput = Vector2.zero;
        gamepadLookAction.performed -= ctx => gamepadLookInput = ctx.ReadValue<Vector2>();
        gamepadLookAction.canceled -= ctx => gamepadLookInput = Vector2.zero;
        jumpAction.performed -= ctx => jumpPressed = true;
        jumpAction.canceled -= ctx => jumpPressed = false;

        moveAction.Dispose();
        gamepadLookAction.Dispose();
        jumpAction.Dispose();
    }
}