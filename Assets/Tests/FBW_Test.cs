using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Testing whether or not I fucked up the fly by wire code
/// </summary>
public class FBW_Test : MonoBehaviour
{
    public ComputerScreenBase Screen;
    public SD_ThrustControlSystem thrustControlProgram;
    public Rigidbody rb;


    public bool rand;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (thrustControlProgram == null)
        {
            thrustControlProgram = (SD_ThrustControlSystem)Screen.screens[Screen.currentScreen];
            thrustControlProgram.ship = rb;
        }
        
       
        rb.AddForce(thrustControlProgram.outputAccleration, ForceMode.Acceleration);

        
    }
}
