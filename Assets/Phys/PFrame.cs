using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class PFrame : MonoBehaviour
{
    [SerializeField] bool damping;
    [SerializeField] float inDamp;
    [SerializeField] RelativisticBodyC reference;
    public PAccel playerAcceleration;
    private Vector3 lookDirection;
    private Vector2 moveInput;
    public float Fmax = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerAcceleration.alpha = 0.0f;
        playerAcceleration.direction = Vector3.right;
        lookDirection = Camera.main.transform.forward;
    }

    void FixedUpdate()
    {
        // In case we want to make sure the accel direction is always oriented towards current input
        // lookDirection = Camera.main.transform.forward;
        // Vector3 moveInput3 = moveInput;
        // playerAcceleration.direction = (Fmax * ChangeOfBasis(moveInput3, lookDirection).normalized).normalized;
        // playerAcceleration.alpha = playerAcceleration.direction.magnitude;
        // Debug.Log($"Player Reports: alpha = {playerAcceleration.alpha} , direction = {playerAcceleration.direction}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // public static Vector3 ChangeOfBasis(Vector3 v, Vector3 newX)
    // {
    //     // Step 1: normalize the given axis
    //     Vector3 x = newX.normalized;

    //     // Step 2: pick a non-parallel temp vector
    //     Vector3 temp = Mathf.Abs(Vector3.Dot(x, Vector3.up)) < 0.99f 
    //                ? Vector3.up 
    //                : Vector3.right;

    //     // Step 3: build orthonormal basis
    //     Vector3 z = Vector3.Normalize(Vector3.Cross(x, temp));
    //     Vector3 y = Vector3.Cross(z, x);

    //     // Step 4: project v into this basis
    //     return new Vector3(
    //         Vector3.Dot(v, x),
    //         Vector3.Dot(v, y),
    //         Vector3.Dot(v, z)
    //     );
    // }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        ChangeAcceleration();
        
    }

    public void LateUpdate()
    {
        // if(damping && moveInput.magnitude <= 1e-2)
        // {
        //     Vector3 playerVelocity = Quaternion.FromToRotation(reference.GetLocalBasis(), Vector3.right) * reference.GetLocalVelocity();
        //     playerAcceleration.alpha = inDamp * math.abs(playerVelocity.magnitude);
        // }
    }

    private void ChangeAcceleration()
    {
        lookDirection = Camera.main.transform.forward;
        Vector3 moveInput3 = new Vector3(moveInput.x, 0, moveInput.y);
        Quaternion R = Quaternion.FromToRotation(Vector3.forward, lookDirection);
        if (!damping || moveInput.magnitude >= 1e-2)
        {
            //FOR DAMPING NEED BODIES TO REPORT IN GLOBAL COORDS!!!!
            playerAcceleration.alpha = moveInput.normalized.magnitude * Fmax;
            if(moveInput.magnitude > 1e-2){
                playerAcceleration.direction = R * moveInput3;
            }
        }
        else
        {
            // playerAcceleration.direction = -1 * reference.GetGlobalVelocity();
            // playerAcceleration.alpha = inDamp;

            Vector3 vLocal = reference.GetLocalVelocity();

        if (vLocal.sqrMagnitude < 1e-6f)
        {
            playerAcceleration.alpha = 0f;
            playerAcceleration.direction = Vector3.right; // dummy but safe
        }
        else
        {
            playerAcceleration.direction = -vLocal.normalized;  
            playerAcceleration.alpha = inDamp * vLocal.magnitude; 
        }
        }
        Debug.Log($"Player Reports: alpha = {playerAcceleration.alpha} , direction = {playerAcceleration.direction}");

        // foreach (RelativisticBody rb in FindObjectsOfType<RelativisticBody>())
        //     {
        //         rb.DoSomething();
        //     }
    }
}
