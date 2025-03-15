using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    private CharacterController controller;
    private Transform cameraTransform;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gamepadSensitivity = 200f;
    [SerializeField] private float maxLookAngle = 85f;
    [SerializeField] private float deadzone = 0.1f; // Added for stick smoothing

    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isGrounded;
    private bool usingGamepad = false;
    private Vector2 smoothMoveInput; // For smoothing movement

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = transform.Find("Main Camera");
        if (cameraTransform == null) Debug.LogError("Camera not found!");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Detect input device (check every frame for responsiveness)
        usingGamepad = Gamepad.current != null && (Gamepad.current.leftStick.ReadValue().magnitude > deadzone || 
                                                   Gamepad.current.rightStick.ReadValue().magnitude > deadzone);
        Debug.Log($"Using Gamepad: {usingGamepad}");

        HandleMovement();
        HandleLook();
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        Vector2 rawMoveInput = Vector2.zero;
        if (usingGamepad && Gamepad.current != null)
        {
            rawMoveInput = Gamepad.current.leftStick.ReadValue();
            if (rawMoveInput.magnitude < deadzone) rawMoveInput = Vector2.zero; // Apply deadzone
            Debug.Log($"Gamepad Move: {rawMoveInput}");
        }
        else
        {
            rawMoveInput = new Vector2(
                Keyboard.current.dKey.isPressed ? 1 : (Keyboard.current.aKey.isPressed ? -1 : 0),
                Keyboard.current.wKey.isPressed ? 1 : (Keyboard.current.sKey.isPressed ? -1 : 0)
            );
            Debug.Log($"Keyboard Move: {rawMoveInput}");
        }

        // Smooth the input
        smoothMoveInput = Vector2.Lerp(smoothMoveInput, rawMoveInput, Time.deltaTime * 10f);
        Vector3 move = (transform.right * smoothMoveInput.x + transform.forward * smoothMoveInput.y).normalized * walkSpeed;
        controller.Move(move * Time.deltaTime);

        if (isGrounded && ((usingGamepad && Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) || 
                          (!usingGamepad && Keyboard.current.spaceKey.wasPressedThisFrame)))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log("Jump!");
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        Vector2 lookInput = Vector2.zero;
        float sensitivity = usingGamepad ? gamepadSensitivity : mouseSensitivity;

        if (usingGamepad && Gamepad.current != null)
        {
            lookInput = Gamepad.current.rightStick.ReadValue();
            if (lookInput.magnitude < deadzone) lookInput = Vector2.zero; // Apply deadzone
            Debug.Log($"Gamepad Look: {lookInput}");
        }
        else
        {
            lookInput = Mouse.current.delta.ReadValue() * 0.1f;
            Debug.Log($"Mouse Look: {lookInput}");
        }

        lookInput *= sensitivity * Time.deltaTime;

        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x);
    }
}