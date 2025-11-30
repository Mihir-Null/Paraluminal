using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook2 : MonoBehaviour
{
    [SerializeField] Transform cameraRoot;
    [SerializeField] float sensitivity = 1f;

    Vector2 lookInput;
    float pitch;
    bool cursorLocked = true;

    void Start()
    {
        LockCursor();
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;   // keeps it "in the center"
        Cursor.visible   = false;                   // hides cursor
        cursorLocked     = true;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        cursorLocked     = false;
    }

    // InputSystem callback from PlayerInput ("Look" action)
    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    void Update()
    {
        // Example: press Escape to unlock mouse (for menus, etc.)
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (cursorLocked) UnlockCursor();
            else              LockCursor();
        }

        if (!cursorLocked) return;   // don't rotate while cursor free

        Vector2 delta = lookInput * sensitivity;

        // yaw
        transform.Rotate(Vector3.up, delta.x);

        // pitch
        pitch -= delta.y;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
        cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    // When you alt-tab away and back, re-lock
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && cursorLocked)
            LockCursor();
    }
}
