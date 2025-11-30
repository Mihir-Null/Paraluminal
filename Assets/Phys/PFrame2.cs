using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class PFrame2 : MonoBehaviour
{
    [SerializeField] bool damping;
    [SerializeField] float inDamp;
    [SerializeField] RelativisticBody2 reference;
    public GlobalPhysics physicsState;

    [SerializeField] float speedOfLight = 299792458f;
    private Vector3 lookDirection;
    private Vector2 moveInput;
    public float Fmax = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        physicsState.c = speedOfLight;
        physicsState.playerAcceleration = 1e-6f * Vector3.right;
        lookDirection = Camera.main.transform.forward;
    }

    void FixedUpdate()
    {
        ChangeAcceleration();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        
    }

    public void LateUpdate()
    {
        
    }

    private void ChangeAcceleration()
    {
        lookDirection = Camera.main.transform.forward;
        Vector3 moveInput3 = new Vector3(moveInput.x, 0, moveInput.y);
        Quaternion R = Quaternion.FromToRotation(Vector3.forward, lookDirection);
        if (moveInput.magnitude >= 1e-2)
        {
            physicsState.playerAcceleration = Fmax * (R * moveInput3);
        }
        else
        {
            if (damping)
            {
                Vector3 pvel = reference.GetVelocity();
                physicsState.playerAcceleration = -inDamp * pvel;  
            }else
            {
                physicsState.playerAcceleration = 1e-6f * Vector3.right;
            }
        }
    }
}
