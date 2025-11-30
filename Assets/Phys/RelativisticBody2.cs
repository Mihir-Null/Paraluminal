using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Relativistic body evolving in a Rindler-like accelerating frame,
/// using a 4th-order Yoshida-style drift–kick integrator in Rindler time T,
/// with explicit speed of light c and frame acceleration physicsState.playerAcceleration.magnitude.
///
/// Assumptions:
/// - Position in meters, time in seconds.
/// - c in meters/second, physicsState.playerAcceleration.magnitude in m/s^2.
/// - Proper accelerations aProp* in m/s^2 in the local accelerating frame.
/// </summary>
[DisallowMultipleComponent]
public class RelativisticBody2 : MonoBehaviour
{

    [Header("Frame Parameters")]
    [Tooltip("Global Physics state modified by player inputs")]
    public GlobalPhysics physicsState;

    [Tooltip("Proper acceleration in player/world frame")]
    public Vector3 properAcceleration;

    [Header("Time / Integration")]
    [Tooltip("Use Unity's fixedDeltaTime as Rindler time step ΔT (seconds).")]
    public bool useFixedDeltaTime = true;
    [Tooltip("If not using fixedDeltaTime, use this custom ΔT in seconds (Rindler time T).")]
    public float customDeltaT = 0.02f;

    [Header("Mass Shadow")]
    [Tooltip("Simulation of Lightspeed lag")]
    [SerializeField] private MassShadow shadow;

    [Header("Debug / State (Read-Only)")]
    [SerializeField] bool debug = false;
    [SerializeField] private float T;        // Rindler coordinate time for this body (s)
    [SerializeField] private float tau;      // Proper time of the body (s)
    [SerializeField] private Vector3 xGlobal; // (X,Y,Z) in accelerating frame (m)
    [SerializeField] private Vector3 vGlobal; // (vX,vY,vZ) = dX/dT in accelerating frame (m/s)
    [SerializeField] private Vector3 gamma;

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
    private Vector3 rindlerOriginWorld = Vector3.zero;

    void Awake()
    {
        // Initialize local state from current transform
        Vector3 worldPos = transform.position - rindlerOriginWorld;
        xGlobal = worldPos;

        vGlobal = Vector3.zero; // start at rest in local frame
        T = 0f;
        tau = 0f;

        var massShadow = this.transform.parent.gameObject.GetComponent<MassShadow>();

        if(massShadow != null){shadow = massShadow;}
    }

    void FixedUpdate()
    {
    // calculate current engine timestep
    float dt = useFixedDeltaTime ? Time.fixedDeltaTime : customDeltaT;
    if (dt <= 0f) return;

    // 3. Integrate in local Rindler frame (still just along +X_local)
    IntegrateStepYoshida(dt, xGlobal - rindlerOriginWorld, vGlobal, ref xGlobal, ref vGlobal);
    xGlobal += rindlerOriginWorld;
    // 3.5 change transform.localScale based on current tangent plane velocity
    float invC2 = 1 / (physicsState.c * physicsState.c);
    for(int i = 0; i < 3; i++)
        {
            gamma[i] = math.sqrt(1.0f - (vGlobal[i] * vGlobal[i] * invC2));
        }
    transform.localScale = gamma;


    // 4. Map local position back to world space
    Vector3 newWorldPos = rindlerOriginWorld + xGlobal;
    transform.position = newWorldPos;

    T += dt;
    }

    public void LateUpdate()
    {
        if(shadow != null)
        {
            shadow.history.Push(xGlobal, vGlobal, T, transform.rotation);
        }
    }

    /// <summary>
    /// One 4th-order Yoshida-style drift–kick step in Rindler time T
    /// operating on the (3+1) d/dT equations with explicit c.
    /// </summary>
    private void IntegrateStepYoshida(float dt, Vector3 x, Vector3 v, ref Vector3 xG, ref Vector3 vG)
    {
        Quaternion Local2World = Quaternion.FromToRotation(Vector3.right, physicsState.playerAcceleration.normalized);
        Vector3 xLocal = Quaternion.Inverse(Local2World) * x;
        Vector3 vLocal =  Quaternion.Inverse(Local2World) * v;


        void Drift(float cCoef)
        {
            float h = cCoef * dt;

            // dX/dT = vX, etc. (units: m/s * s = m)
            xLocal += vLocal * h;

            // dτ/dT = sqrt( (1 + a X / c^2)^2 - v^2 / c^2 )
            float X = xLocal.x;
            float v2 = vLocal.sqrMagnitude;
            float onePlusAX = 1.0f + (physicsState.playerAcceleration.magnitude * X) / (physicsState.c * physicsState.c);
            float inside = onePlusAX * onePlusAX - v2 / (physicsState.c * physicsState.c);
            if (inside < 0f) inside = 0f; // numerical guard
            float dtau_dT = Mathf.Sqrt(inside);
            tau += dtau_dT * h;
        }

        void Kick(float dCoef)
        {
            float h = dCoef * dt;
            Vector3 aSpatial = ComputeSpatialAccelRindler(xLocal, vLocal, Local2World);
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

        xG = Local2World * xLocal;
        vG = Local2World * vLocal;
    }

    /// <summary>
    /// Compute dv/dT (3D spatial acceleration in the accelerating frame)
    /// using the 3+1 Rindler d/dT equations with explicit c:
    ///
    /// Let v^2 = vX^2 + vY^2 + vZ^2.
    /// Let u = 1 + physicsState.playerAcceleration.magnitude * X / c^2 (dimensionless).
    ///
    /// dvX/dT =
    ///   aPropX * ( u^2 - v^2 / c^2 )
    ///   + 2 (physicsState.playerAcceleration.magnitude / c^2) * vX^2 / u
    ///   - physicsState.playerAcceleration.magnitude * u
    ///
    /// dvY/dT =
    ///   aPropY * ( u^2 - v^2 / c^2 )
    ///   + 2 (physicsState.playerAcceleration.magnitude / c^2) * vX vY / u
    ///
    /// dvZ/dT =
    ///   aPropZ * ( u^2 - v^2 / c^2 )
    ///   + 2 (physicsState.playerAcceleration.magnitude / c^2) * vX vZ / u
    ///
    /// All terms have units of m/s^2.
    /// We set aTprop = 0 here for simplicity.
    /// </summary>
    private Vector3 ComputeSpatialAccelRindler(Vector3 xLocal, Vector3 vLocal, Quaternion basis)
    {
        float X = xLocal.x;
        float vX = vLocal.x;
        float vY = vLocal.y;
        float vZ = vLocal.z;

        float v2 = vLocal.sqrMagnitude;

        // u = 1 + a X / c^2 (dimensionless)
        float u = 1.0f + (physicsState.playerAcceleration.magnitude * X) / (physicsState.c * physicsState.c);
        float invU = 1.0f / Mathf.Max(1e-6f, u);

        Vector3 improperAcceleration =  Quaternion.Inverse(basis) * properAcceleration;
        float aPropX = improperAcceleration.x;
        float aPropY = improperAcceleration.y;
        float aPropZ = improperAcceleration.z;

        // Factor (dimensionless)
        float factor = u * u - v2 / (physicsState.c * physicsState.c);

        // All dv/dT in m/s^2
        float dvX = aPropX * factor
                    + 2.0f * (physicsState.playerAcceleration.magnitude / (physicsState.c * physicsState.c)) * vX * vX * invU
                    - physicsState.playerAcceleration.magnitude * u;

        float dvY = aPropY * factor
                    + 2.0f * (physicsState.playerAcceleration.magnitude / (physicsState.c * physicsState.c)) * vX * vY * invU;

        float dvZ = aPropZ * factor
                    + 2.0f * (physicsState.playerAcceleration.magnitude / (physicsState.c * physicsState.c)) * vX * vZ * invU;

        return new Vector3(dvX, dvY, dvZ);
    }

    // Optional public getters
    public float GetRindlerTime() => T;
    public float GetProperTime() => tau;
    public Vector3 GetPosition() => xGlobal;
    public Vector3 GetVelocity() => vGlobal;
    public void SetAcceleration(Vector3 accel)
    {
        this.properAcceleration = accel;
    }

    // naive implementation, can significantly speed this up via bracketing 
}
