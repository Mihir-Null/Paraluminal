using UnityEngine;

public class CursorTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("CursorTest.Start running");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void Update()
    {
        // Keep enforcing in case something else changes it
        if (Cursor.lockState != CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.Locked;
        if (Cursor.visible)
            Cursor.visible = false;
    }
}