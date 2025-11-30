using UnityEngine;
using UnityEngine.InputSystem;

public class SD_RCP2 : ScreenData
{


    public Transform vMouse;

 public   Vector3 mp;


    public float sensitivity;

    public float centerReturnSpeed;
    public float minMouseMove;
    public float minDist;
    public float maxDist;

    public float maxRotrRate;
    public float zRotrRate;


    public Vector3 rotOutput;
    public override void RunStart()
    {
        SetCursorState(true);
        base.RunStart();
    }

    public float ControlZ(InputAction positive, InputAction negative)
    {
        if (positive.IsPressed())
        {
            return zRotrRate;
        }
        else if (negative.IsPressed())
        {
            return -zRotrRate;
        }
        else
        {
            return 0;
        }
    }
    public void Control(InputAction mouse, InputAction zPos, InputAction zNeg)
    {
        float mouseX = mouse.ReadValue<Vector2>().x * sensitivity;
        float mouseY = mouse.ReadValue<Vector2>().y * sensitivity;
        Debug.Log(mouseX + " " + mouseY);

        if (Mathf.Abs(mouseX) > minMouseMove || Mathf.Abs(mouseY) > minMouseMove)
        {
            mp += new Vector3(mouseX, mouseY);       
            

            if (mp.x > maxDist && mouseX < 0)
            {
                mp.x = maxDist;
            }
            else if (mp.x < -maxDist && mouseX > 0)
            {
                mp.x = -maxDist;
            }

            if (mp.y > maxDist && mouseY < 0)
            {
                mp.y = maxDist;
            }
            else if (mp.y < -maxDist && mouseY > 0)
            {
                mp.y = -maxDist;
            }
        }
        else
        {
            mp -= mp.normalized * centerReturnSpeed;
        }

        mp.z = -0.1f;
        Vector3 mpClamped = new Vector3(Mathf.Clamp(mp.x, -maxDist, maxDist), Mathf.Clamp(mp.y, -maxDist, maxDist), -0.01f);
        vMouse.localPosition = mpClamped;

        //cal forces 
        float x = HMath.posLogic(maxDist, -maxDist, mpClamped.x) * maxRotrRate;
        float y = HMath.posLogic(maxDist, -maxDist, mpClamped.y) * maxRotrRate;

       rotOutput = new Vector3(y, x, ControlZ(zPos,zNeg));

    }




    void SetCursorState(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }
}
