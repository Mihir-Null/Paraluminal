using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public Transform playerCamera;       // Camera to rotate up/down
    public Transform body;
    public float sensitivity = 2f;       // Mouse sensitivity
     InputAction toggle; // Key to toggle mouse look
     InputAction look;
    public bool mouseLookEnabled = false;
    private float xRotation = 0f;

    void Start()
    {
        toggle = InputSystem.actions.FindAction("Interact");
        look = InputSystem.actions.FindAction("Look");

        // Optional: Hide and lock cursor at start
        SetCursorState(true);
        mouseLookEnabled = true;
    }

    void Update()
    {
        //we are moving cam
        if (mouseLookEnabled)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Ray(transform.position, transform.forward), out hit, 15) && toggle.triggered && hit.transform.tag == "Screen")
            {
                mouseLookEnabled = false;
                SetCursorState(mouseLookEnabled);
            }
        }
        //we are looking at a screen
        else if (!mouseLookEnabled && toggle.triggered)
        {
            mouseLookEnabled = true;
            SetCursorState(mouseLookEnabled);
        }
        // Apply mouse look only if enabled
        if (mouseLookEnabled)
        {
            float mouseX = look.ReadValue<Vector2>().x * sensitivity;
            float mouseY = look.ReadValue<Vector2>().y * sensitivity;

            // Rotate the camera up/down
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent flipping

            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Rotate the body left/right
            body.transform.Rotate(Vector3.up * mouseX);
        }
    }

   public static void SetCursorState(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }
}
