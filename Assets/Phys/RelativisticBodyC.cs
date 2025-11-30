using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Relativistic body evolving in a Rindler-like accelerating frame,
/// using a 4th-order Yoshida-style drift–kick integrator in Rindler time T,
/// with explicit speed of light c and frame acceleration aFrame.
///
/// Assumptions:
/// - Position in meters, time in seconds.
/// - c in meters/second, aFrame in m/s^2.
/// - Proper accelerations aProp* in m/s^2 in the local accelerating frame.
/// </summary>
[DisallowMultipleComponent]
public class RelativisticBodyC : MonoBehaviour
{
    [Header("Fundamental constants")]
    [Tooltip("Speed of light in scene units (m/s).")]
    public static float c = 299792458f;

    [Header("Frame Parameters")]
    [Tooltip("Rindler frame acceleration magnitude (background 'gravity' along +X_local) in m/s^2.")]
    public PAccel playerAcceleration;
    public float aFrame = 0.0f;   // 'a' in the metric (1 + a X)^2
    [Header("Acceleration Direction")]
    [Tooltip("World-space direction that defines +X in the accelerating frame (normalized internally).")]
    public Vector3 accelerationDirectionWorld = Vector3.right;
    private Vector3 prevAccelerationDirectionWorld = Vector3.right;

    [Header("Ship Proper Acceleration (local frame, m/s^2)")]
    [Tooltip("Proper acceleration along local +X in the accelerating frame.")]
    public float properAccelX = 0.0f;  // a^X_prop
    [Tooltip("Proper acceleration along local +Y in the accelerating frame.")]
    public float properAccelY = 0.0f;  // a^Y_prop
    [Tooltip("Proper acceleration along local +Z in the accelerating frame.")]
    public float properAccelZ = 0.0f;  // a^Z_prop

    [Header("Time / Integration")]
    [Tooltip("Use Unity's fixedDeltaTime as Rindler time step ΔT (seconds).")]
    public bool useFixedDeltaTime = true;
    [Tooltip("If not using fixedDeltaTime, use this custom ΔT in seconds (Rindler time T).")]
    public float customDeltaT = 0.02f;

    [Header("Debug / State (Read-Only)")]
    [SerializeField] private float T;        // Rindler coordinate time for this body (s)
    [SerializeField] private float tau;      // Proper time of the body (s)
    [SerializeField] private Vector3 xLocal; // (X,Y,Z) in accelerating frame (m)
    [SerializeField] private Vector3 vLocal; // (vX,vY,vZ) = dX/dT in accelerating frame (m/s)

    // Yoshida 4th-order coefficients (drift c_i, kick d_i)
    static readonly float c1 =  0.5153528374311229364f;
    static readonly float c2 = -0.085782019412973646f;
    static readonly float c3 =  0.4415830236164665242f;
    static readonly float c4 =  0.1288461583653841854f;

    static readonly float d1 =  0.1344961992774310892f;
    static readonly float d2 = -0.2248198030794208058f;
    static readonly float d3 =  0.7563200005156682911f;
    static readonly float d4 =  0.3340036032863214255f;

    // Optional: origin of the Rindler frame in world coordinates
    public Vector3 rindlerOriginWorld = Vector3.zero;

    void Awake()
    {
        // Initialize local state from current transform
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

    //Debug.Log($"Object Reports: alpha = {playerAcceleration.alpha} , direction = {playerAcceleration.direction}");
    Debug.Log($"Object Reports: velocity = {GetGlobalVelocity()}");
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
    /// One 4th-order Yoshida-style drift–kick step in Rindler time T
    /// operating on the (3+1) d/dT equations with explicit c.
    /// </summary>
    private void IntegrateStepYoshida(float dt)
    {
        void Drift(float cCoef)
        {
            float h = cCoef * dt;

            // dX/dT = vX, etc. (units: m/s * s = m)
            xLocal += vLocal * h;

            // dτ/dT = sqrt( (1 + a X / c^2)^2 - v^2 / c^2 )
            float X = xLocal.x;
            float v2 = vLocal.sqrMagnitude;
            float onePlusAX = 1.0f + (aFrame * X) / (c * c);
            float inside = onePlusAX * onePlusAX - v2 / (c * c);
            if (inside < 0f) inside = 0f; // numerical guard
            float dtau_dT = Mathf.Sqrt(inside);
            tau += dtau_dT * h;
        }

        void Kick(float dCoef)
        {
            float h = dCoef * dt;
            Vector3 aSpatial = ComputeSpatialAccelRindler(xLocal, vLocal);
            // dv/dT has units m/s^2 * s = m/s
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
    /// Compute dv/dT (3D spatial acceleration in the accelerating frame)
    /// using the 3+1 Rindler d/dT equations with explicit c:
    ///
    /// Let v^2 = vX^2 + vY^2 + vZ^2.
    /// Let u = 1 + aFrame * X / c^2 (dimensionless).
    ///
    /// dvX/dT =
    ///   aPropX * ( u^2 - v^2 / c^2 )
    ///   + 2 (aFrame / c^2) * vX^2 / u
    ///   - aFrame * u
    ///
    /// dvY/dT =
    ///   aPropY * ( u^2 - v^2 / c^2 )
    ///   + 2 (aFrame / c^2) * vX vY / u
    ///
    /// dvZ/dT =
    ///   aPropZ * ( u^2 - v^2 / c^2 )
    ///   + 2 (aFrame / c^2) * vX vZ / u
    ///
    /// All terms have units of m/s^2.
    /// We set aTprop = 0 here for simplicity.
    /// </summary>
    private Vector3 ComputeSpatialAccelRindler(Vector3 x, Vector3 v)
    {
        float X = x.x;
        float vX = v.x;
        float vY = v.y;
        float vZ = v.z;

        float v2 = v.sqrMagnitude;

        // u = 1 + a X / c^2 (dimensionless)
        float u = 1.0f + (aFrame * X) / (c * c);
        float invU = 1.0f / Mathf.Max(1e-6f, u);

        Vector3 improperAcceleration = new Vector3(properAccelX,properAccelY,properAccelZ);
        improperAcceleration = AccelerationRotation() * improperAcceleration;
        float aPropX = improperAcceleration.x;
        float aPropY = improperAcceleration.y;
        float aPropZ = improperAcceleration.z;

        // Factor (dimensionless)
        float factor = u * u - v2 / (c * c);

        // All dv/dT in m/s^2
        float dvX = aPropX * factor
                    + 2.0f * (aFrame / (c * c)) * vX * vX * invU
                    - aFrame * u;

        float dvY = aPropY * factor
                    + 2.0f * (aFrame / (c * c)) * vX * vY * invU;

        float dvZ = aPropZ * factor
                    + 2.0f * (aFrame / (c * c)) * vX * vZ * invU;

        return new Vector3(dvX, dvY, dvZ);
    }

    private Quaternion AccelerationRotation()
    {
        return Quaternion.Inverse(Quaternion.FromToRotation(accelerationDirectionWorld, Vector3.right));
    }

    // Optional public getters
    public float GetRindlerTime() => T;
    public float GetProperTime() => tau;
    public Vector3 GetLocalPosition() => xLocal;
    public Vector3 GetLocalVelocity() => vLocal;
    public Vector3 GetLocalBasis() => accelerationDirectionWorld;
    public Quaternion GlobalBasis() => Quaternion.FromToRotation(Vector3.right, accelerationDirectionWorld);
    public Vector3 GetGlobalVelocity() => GlobalBasis() * vLocal;
}
