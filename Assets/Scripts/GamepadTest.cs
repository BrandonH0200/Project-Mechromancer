using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadTest : MonoBehaviour
{
    void Update()
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;

        if (gamepad.aButton.wasPressedThisFrame)
            Debug.Log("A button pressed!");
        if (gamepad.leftStick.ReadValue() != Vector2.zero)
            Debug.Log($"Left Stick: {gamepad.leftStick.ReadValue()}");
        if (gamepad.rightStick.ReadValue() != Vector2.zero)
            Debug.Log($"Right Stick: {gamepad.rightStick.ReadValue()}");
    }
}