using UnityEngine;
using UnityEngine.InputSystem;

public class SD_RCP : ScreenData
{
    public Transform screenCenter;
    public Transform mouse;
    public Transform vMouse;

    public Vector3 lastMouse;
    public Vector2 currentScreenMouse;
    public Vector2 currentVirtualMouse;

    public float minMouseMove;
    public float maxDist;
    public float minDist;
    public float mouseSpeed;
    public float centerReturnSpeed;
    public float sensitivity;
 





    public override void RunStart()
    {
        currentVirtualMouse = screenCenter.position;
        currentScreenMouse = screenCenter.position;
        base.RunStart();
    }


    public void Control(InputAction mouse)
    {
        float mouseX = mouse.ReadValue<Vector2>().x * sensitivity;
        float mouseY = mouse.ReadValue<Vector2>().y * sensitivity;
        Debug.Log(mouseX + " " + mouseY);
        RotLogic(new Vector3(mouseX, mouseY, 0));
    }



    Vector2 RotLogic(Vector3 mousePosition)
    {
        if ((mousePosition - lastMouse).magnitude < minMouseMove)
        {
            currentScreenMouse -= (currentScreenMouse - (Vector2)screenCenter.position).normalized * centerReturnSpeed * Time.fixedDeltaTime;

        }
        else
        {
            lastMouse = currentScreenMouse;
            currentScreenMouse = mousePosition;
        }
        float dist = Vector2.Distance(currentScreenMouse, currentVirtualMouse);

        if (dist > minDist)
        {
            currentVirtualMouse += (currentScreenMouse - currentVirtualMouse).normalized * mouseSpeed * Time.fixedDeltaTime;
            float distmax = Vector2.Distance(currentVirtualMouse, screenCenter.position);
            if (distmax > maxDist)
            {
                currentVirtualMouse = (Vector2)screenCenter.position + (currentVirtualMouse - (Vector2)screenCenter.position).normalized * maxDist;
            }
        }
        mouse.position = currentScreenMouse;
        vMouse.position = currentVirtualMouse;

        Vector2 temp = (currentVirtualMouse - (Vector2)screenCenter.position) / maxDist;
        return new Vector2(-temp.y, temp.x);
    }
}
