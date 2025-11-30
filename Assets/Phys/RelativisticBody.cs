using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

/// <summary>
/// Relativistic body evolving in a Rindler-like accelerating frame,
/// using 4th-order drift–kick integration in Rindler time T.
///
/// Assumptions:
/// - Units chosen so that c = 1.
/// - "aFrame" is the constant Rindler frame acceleration magnitude (background "gravity").
/// - Ship's *proper* thrust is applied along local +X in the accelerating frame.
/// - We treat local +X as aligned with a chosen world-space direction each step.
/// </summary>
[DisallowMultipleComponent]
public class RelativisticBody : MonoBehaviour
{
    [Header("Frame Parameters")]
    [Tooltip("Rindler frame acceleration magnitude (background 'gravity' along +X_local).")]

    public PAccel playerAcceleration;
    public float aFrame = 0.0f;   // 'a' in the metric (1 + a X)^2

    [Header("Acceleration Direction")]
    [Tooltip("World-space direction that defines +X in the accelerating frame (normalized internally).")]
    public Vector3 accelerationDirectionWorld = Vector3.right;
    private Vector3 prevAccelerationDirectionWorld = Vector3.right;

    [Header("Ship Proper Acceleration (local frame)")]
    [Tooltip("Proper acceleration along local +X in the accelerating frame.")]
    public float properAccelX = 0.0f;  // a^X_prop

    [Tooltip("Proper acceleration along local +Y in the accelerating frame.")]
    public float properAccelY = 0.0f;  // a^Y_prop
    [Tooltip("Proper acceleration along local +Z in the accelerating frame.")]
    public float properAccelZ = 0.0f;  // a^Z_prop

    [Header("Time / Integration")]
    [Tooltip("Use Unity's fixedDeltaTime as Rindler time step ΔT.")]
    public bool useFixedDeltaTime = true;
    [Tooltip("If not using fixedDeltaTime, use this custom ΔT (in 'T' units).")]
    public float customDeltaT = 0.02f;

    [Header("Debug / State (Read-Only)")]
    [SerializeField] private float T;        // Rindler time for this body (can track or ignore)
    [SerializeField] private float tau;      // Proper time of the body
    [SerializeField] private Vector3 xLocal; // (X,Y,Z) in accelerating frame
    [SerializeField] private Vector3 vLocal; // (vX,vY,vZ) = dX/dT in accelerating frame

    // Yoshida 4th-order symplectic-like coefficients (drift c_i, kick d_i)
    // These are the real Yoshida coefficients; we’re using them as a 4th-order drift–kick scheme.
    static readonly float c1 =  0.5153528374311229364f;
    static readonly float c2 = -0.085782019412973646f;
    static readonly float c3 =  0.4415830236164665242f;
    static readonly float c4 =  0.1288461583653841854f;

    static readonly float d1 =  0.1344961992774310892f;
    static readonly float d2 = -0.2248198030794208058f;
    static readonly float d3 =  0.7563200005156682911f;
    static readonly float d4 =  0.3340036032863214255f;

    // Optional: choose some global origin for the Rindler frame
    public Vector3 rindlerOriginWorld = Vector3.zero;

    void Awake()
    {
        // Initialize local state from current transform
        // Treat world origin as X=Y=Z=0 at T=0.
        // Quaternion R = ComputeLocalFrameRotation();
        Vector3 worldPos = transform.position - rindlerOriginWorld;
        // xLocal = Quaternion.Inverse(R) * worldPos;
        xLocal = worldPos;

        vLocal = Vector3.zero; // start at rest in local frame
        T = 0f;
        tau = 0f;
    }

    void FixedUpdate()
{
    aFrame = playerAcceleration.alpha;
    accelerationDirectionWorld = playerAcceleration.direction.normalized;

    // Don't let the direction go degenerate
    if (accelerationDirectionWorld.sqrMagnitude < 1e-6f)
        accelerationDirectionWorld = prevAccelerationDirectionWorld;

    // Only re-orient when the desired acceleration direction changes
    if (!accelerationDirectionWorld.Equals(prevAccelerationDirectionWorld))
    {
        // R_prev: local +X -> old accel dir
        Quaternion R_prev = Quaternion.FromToRotation(Vector3.right, prevAccelerationDirectionWorld);
        // R_new:  local +X -> new accel dir
        Quaternion R_new  = Quaternion.FromToRotation(Vector3.right, accelerationDirectionWorld);

        // Change of basis: x_new = R_new^-1 * R_prev * x_old
        Quaternion basisChange = Quaternion.Inverse(R_new) * R_prev;

        xLocal = basisChange * xLocal;
        vLocal = basisChange * vLocal;

        // IMPORTANT: remember the new direction
        prevAccelerationDirectionWorld = accelerationDirectionWorld;
    }

    Debug.Log($"Object Reports: alpha = {playerAcceleration.alpha} , direction = {playerAcceleration.direction}");

    float dt = useFixedDeltaTime ? Time.fixedDeltaTime : customDeltaT;
    if (dt <= 0f) return;

    // 3. Integrate in local Rindler frame (still just along +X_local)
    IntegrateStepYoshida(dt);

    // 4. Map local position back to world space
    Quaternion R_world = Quaternion.FromToRotation(Vector3.right, accelerationDirectionWorld);
    Vector3 newWorldPos = rindlerOriginWorld + R_world * xLocal;
    transform.position = newWorldPos;

    T += dt;
}

    /// <summary>
    /// Compute rotation R that maps local +X to the chosen world acceleration direction.
    /// </summary>
    private Quaternion AccelerationRotation()
    {
        return Quaternion.Inverse(Quaternion.FromToRotation(accelerationDirectionWorld, Vector3.right));
    }

    /// <summary>
    /// One 4th-order Yoshida-style drift–kick step in Rindler time T
    /// operating on the (3+1) d/dT equations.
    /// </summary>
    public virtual void IntegrateStepYoshida(float dt)
    {
        // Helper local lambda to do a drift (update xLocal, tau) with current vLocal
        void Drift(float c)
        {
            float h = c * dt;

            // dX/dT = vX, etc.
            xLocal += vLocal * h;

            // dτ/dT = sqrt( (1 + a X)^2 - v^2 )
            float X = xLocal.x;
            float v2 = vLocal.sqrMagnitude;
            float onePlusAX = 1.0f + aFrame * X;
            float inside = onePlusAX * onePlusAX - v2;
            if (inside < 0f) inside = 0f; // numerical guard
            float dtau_dT = Mathf.Sqrt(inside);
            tau += dtau_dT * h;
        }

        // Helper to do a kick (update vLocal) with current xLocal, vLocal
        void Kick(float d)
        {
            float h = d * dt;
            Vector3 aSpatial = ComputeSpatialAccelRindler(xLocal, vLocal);
            vLocal += aSpatial * h;
        }

        // Stage 1
        Drift(c1);
        Kick(d1);

        // Stage 2
        Drift(c2);
        Kick(d2);

        // Stage 3
        Drift(c3);
        Kick(d3);

        // Stage 4
        Drift(c4);
        Kick(d4);
    }

    /// <summary>
    /// Compute dv/dT (spatial 3-acceleration in Rindler time) from the
    /// 3+1 Rindler d/dT equations, given local (X,Y,Z) and (vX,vY,vZ).
    ///
    /// Using the full 3+1 forms:
    ///   v^2 = vX^2 + vY^2 + vZ^2
    ///   onePlusAX = 1 + aFrame * X
    ///
    ///   dvX/dT =
    ///     (aPropX - vX * aTprop) * [(1+aX)^2 - v^2]
    ///     + 2 aFrame vX^2 / (1+aX)
    ///     - aFrame (1+aX)
    ///
    ///   dvY/dT =
    ///     (aPropY - vY * aTprop) * [(1+aX)^2 - v^2]
    ///     + 2 aFrame vX vY / (1+aX)
    ///
    ///   dvZ/dT =
    ///     (aPropZ - vZ * aTprop) * [(1+aX)^2 - v^2]
    ///     + 2 aFrame vX vZ / (1+aX)
    ///
    /// Here we set aTprop = 0 for simplicity (no explicit proper acceleration along time).
    /// </summary>
    private Vector3 ComputeSpatialAccelRindler(Vector3 x, Vector3 v)
    {
        float X = x.x;
        float Y = x.y;
        float Z = x.z;

        float vX = v.x;
        float vY = v.y;
        float vZ = v.z;

        float v2 = v.sqrMagnitude;
        float onePlusAX = 1.0f + aFrame * X;

        // Proper acceleration components in local frame
        Vector3 improperAcceleration = new Vector3(properAccelX,properAccelY,properAccelZ);
        improperAcceleration = AccelerationRotation() * improperAcceleration;
        float aPropX = improperAcceleration.x;
        float aPropY = improperAcceleration.y;
        float aPropZ = improperAcceleration.z;

        // Time component of proper acceleration (for full covariance you’d solve for this
        // via orthogonality a·u = 0; here we approximate with aTprop = 0 for simplicity).
        float aTprop = 0.0f;

        float factor = (onePlusAX * onePlusAX) - v2;

        // dvX/dT
        float dvX = (aPropX - vX * aTprop) * factor
                    + 2.0f * aFrame * vX * vX / Mathf.Max(1e-6f, onePlusAX)
                    - aFrame * onePlusAX;

        // dvY/dT
        float dvY = (aPropY - vY * aTprop) * factor
                    + 2.0f * aFrame * vX * vY / Mathf.Max(1e-6f, onePlusAX);

        // dvZ/dT
        float dvZ = (aPropZ - vZ * aTprop) * factor
                    + 2.0f * aFrame * vX * vZ / Mathf.Max(1e-6f, onePlusAX);

        return new Vector3(dvX, dvY, dvZ);
    }



    // Optional public getters if you want to inspect state elsewhere:
    public float GetRindlerTime() => T;
    public float GetProperTime() => tau;
    public Vector3 GetLocalPosition() => xLocal;
    public Vector3 GetLocalVelocity() => vLocal;
}
