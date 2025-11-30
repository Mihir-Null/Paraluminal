using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class ComputerScreenBase : MonoBehaviour
{

    static List<ComputerScreenBase> activeScreens = new List<ComputerScreenBase>();
    
    /// <summary>
    /// Note: I have no idea what will happen if you get a screen, and then
    /// that screen is deactivated
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static ScreenData FindScreen(string name)
    {
        foreach (ComputerScreenBase screen in activeScreens)
        {
            if (screen.currentScreen == name)
            {
                return screen.screens[screen.currentScreen];
            }
        }
        return null;
    }

   public Dictionary<string, ScreenData> screens = new Dictionary<string, ScreenData>();
    List<string> messages = new List<string>();
    public void LogMess(string message)
    {
        messages.Add(message);
    }


   public string currentScreen = "START";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        activeScreens.Add(this);


        thrustIncrease = InputSystem.actions.FindAction("ThrustIncrease");
        thrustDecrease = InputSystem.actions.FindAction("ThrustDecrease");
        left = InputSystem.actions.FindAction("Left");
        right = InputSystem.actions.FindAction("Right");
        up = InputSystem.actions.FindAction("Up");
        down = InputSystem.actions.FindAction("Down");
        velocityLockToggle = InputSystem.actions.FindAction("VelocityLockToggle");
        flybywireLockToggle = InputSystem.actions.FindAction("FlyByWireToggle");
        RCP_mouseMovement = InputSystem.actions.FindAction("Look");
        zRotPos = InputSystem.actions.FindAction("zRotPos");
        zRotNeg = InputSystem.actions.FindAction("zRotNeg");
        WLP_fire = InputSystem.actions.FindAction("Attack");
        WLP_SwitchWepPos = new NotShittyGetKeyDown(InputSystem.actions.FindAction("Next"));
        WLP_SwitchWepNeg = new NotShittyGetKeyDown(InputSystem.actions.FindAction("Previous"));
        object[] rawObjs = Resources.LoadAll("ComputerScreens");
       
        for (int i = 0; i < rawObjs.Length; i++)
        {
            GameObject s = Instantiate((GameObject)rawObjs[i]);
            s.name = ((GameObject)rawObjs[i]).name;
            s.transform.parent = transform;
            s.transform.localPosition = Vector3.zero;
            s.transform.localRotation = Quaternion.Euler(-90,0,0);


            Debug.Log(s.name);
            screens.Add(s.name, s.GetComponent<ScreenData>());       
        }

        foreach (ScreenData obj in screens.Values)
        {
            obj.transform.localPosition = new Vector3( 0, 0, -0.5f);
            obj.RunStart();
            obj.gameObject.SetActive(false);
        }

        LoadScreen(currentScreen);
    }
    void LoadUnloadScreen(string screen, bool state)
    {
        if (screens.ContainsKey(screen))
        {
            screens[screen].gameObject.SetActive(state);
        }
        else
        {
            Debug.LogWarning(screen + " does not exist as a screen name");
        }
    }
    public void LoadScreen(string screen)
    {
        LoadUnloadScreen(currentScreen, false);
        LoadUnloadScreen(screen, true); 
        currentScreen = screen;
    }

    
    
    // Update is called once per frame
    void Update()
    {
        KeyShortcuts();

        if (screens.ContainsKey(currentScreen))
        {
            screens[currentScreen].RunUpdate();
        }

    }

    InputAction thrustIncrease;
    InputAction thrustDecrease;
    InputAction left,right,up,down;
    InputAction zRotPos, zRotNeg;
    InputAction velocityLockToggle;
    InputAction flybywireLockToggle;
    InputAction RCP_mouseMovement;
    InputAction WLP_fire;
    NotShittyGetKeyDown WLP_SwitchWepPos;
    NotShittyGetKeyDown WLP_SwitchWepNeg;
    /// <summary>
    /// This function holds all key inputs that corespond to an
    /// action within a screen
    /// 
    /// the simpilier way would be to just do this within each screen
    /// but for the sake of future proofing its probably better to centeralize 
    /// them, even though it means the code is a bit (much) dumber
    /// </summary>
    void KeyShortcuts()
    {

        ///all controls for the TCP

        if (screens[currentScreen].GetType() == typeof(SD_ThrustControlSystem))
        {
            SD_ThrustControlSystem tcp = (SD_ThrustControlSystem)screens[currentScreen];

            if (velocityLockToggle.IsPressed())
            {
                tcp.velocityLocked = true;
            }
            else
            {
                tcp.velocityLocked = false;
            }


            tcp.Control(thrustIncrease, thrustDecrease, left, right, up, down);
        }
        else if (screens[currentScreen].GetType() == typeof(SD_RCP2))
        {
            SD_RCP2 rcp = (SD_RCP2)screens[currentScreen];
            rcp.Control(RCP_mouseMovement, zRotPos, zRotNeg);
        }
        else if (screens[currentScreen].GetType() == typeof(SD_WLP))
        {
            SD_WLP wlp = (SD_WLP)screens[currentScreen];

            if (WLP_fire.triggered)
            {
                wlp.Control_Fire();
            }
            if (WLP_SwitchWepPos.GetKeyDown())
            {
                wlp.Control_ChangeSelection(true);
            }
            if (WLP_SwitchWepNeg.GetKeyDown())
            {
                wlp.Control_ChangeSelection(false);
            }
        }
        else
        {
            Debug.Log(screens[currentScreen].GetType().ToString());
        }
    }

    /// <summary>
    /// fuck you unity, '.trigger' sucks 
    /// 
    /// this should only be used in an if statement 
    /// not an if else statement, because it needs to check each frame to
    /// see when the key is released 
    /// </summary>
    class NotShittyGetKeyDown
    {
        bool lastState = false;
        InputAction action;
        public NotShittyGetKeyDown(InputAction action)
        {
            this.action = action;
        }
        public bool GetKeyDown()
        {
           
            if (!lastState && action.IsPressed())
            {
                lastState = true;
                Debug.Log("Key Down");
                return true; 
            }
            else if (lastState && !action.IsPressed())
            {
                Debug.Log("Key Up");
                lastState = false;                
            }
            return false;
        }
    }
}
