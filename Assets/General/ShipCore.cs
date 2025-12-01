using UnityEngine;

public class ShipCore : MonoBehaviour
{

    public SD_ThrustControlSystem thrustControlSystem;
    public SD_RCP2 rotation_Control_Program;
    public Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (thrustControlSystem == null)
        {
            thrustControlSystem = (SD_ThrustControlSystem)ComputerScreenBase.FindScreen("TCS");
            thrustControlSystem.ship = rb;
        }
        if (rotation_Control_Program == null)
        {
            rotation_Control_Program =(SD_RCP2)ComputerScreenBase.FindScreen("RCP");
        }

        if (thrustControlSystem != null && rotation_Control_Program != null) {
            rb.AddForce(thrustControlSystem.outputAccleration, ForceMode.Acceleration);
            rb.AddRelativeTorque(rotation_Control_Program.rotOutput, ForceMode.Acceleration);
        }
    }
}
