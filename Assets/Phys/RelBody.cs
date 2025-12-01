using UnityEngine;
using System;
using UnityEngine.Rendering.Universal.Internal;

public class RelBody : MonoBehaviour
{
    public const double c = 100;
    public Rigidbody rb;
    //public DVector4 position;
    public DVector4 velocity;
    public double gamma;
    public double rapidity;
    public double tau = 0;
    public double deltaTau;
    public static double ReadOnly_deltaTau { get; private set; }


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Sets the objects 4 velocity in World frame using the current rigidbody velocity
    public void Rb2Rel()
    {
        this.gamma = 1 / Math.Sqrt(1 - Math.Pow((rb.linearVelocity.magnitude / c), 2));
        this.velocity = this.gamma * (new DVector4(this.rb.linearVelocity));
        this.velocity.t = c * this.gamma;
        this.rapidity = 0.5 * Math.Log((1 + rb.linearVelocity.magnitude / c) / (1 - rb.linearVelocity.magnitude / c));
    }
    public void Rel2Rb()
    {
        rb.linearVelocity = this.velocity.ToVector3() / (float)this.gamma;
    }
    public static Vector3 VRel2Scene(DVector4 Velocity4, double gamma)
    {
        return (Velocity4 / gamma).ToVector3();
    }
    
    public static DVector4 Scene2VRel(Vector3 velocity3) {
        double gamma = 1 / Math.Sqrt(1 - ((velocity3.magnitude / c) * (velocity3.magnitude / c)));
        DVector4 velocity = gamma * new DVector4(velocity3);
        velocity.t = c * gamma;
        double rapidity = Math.Atan(velocity3.magnitude / c);
        return velocity;
    }
    
    // Given 4 velocity of B in A's frame and 4 velocity of A in C's frame, "boosts"/converts/returns 4 velocity of B in C's frame
    public static DVector4 BoostToCFrame(DVector4 a_4vel_C, DVector4 b_4vel_A)
    {

        // Step 1: Extract A's 3-velocity in C's frame
        Vector3 vA = new Vector3(
            (float)(a_4vel_C.x / a_4vel_C.t),
            (float)(a_4vel_C.y / a_4vel_C.t),
            (float)(a_4vel_C.z / a_4vel_C.t)
        ) * (float)c;

        float vMag = vA.magnitude;
        if (vMag == 0f)
            return b_4vel_A; // no boost needed

        Vector3 n = vA.normalized; // boost direction
        float beta = vMag / (float)c;
        float gamma = 1.0f / Mathf.Sqrt(1 - beta * beta);

        // Step 2: Apply Lorentz boost to B's 4-velocity in A's frame
        // Boost matrix only affects t and spatial components along boost direction

        Vector3 b_spatial = new Vector3((float)b_4vel_A.x, (float)b_4vel_A.y, (float)b_4vel_A.z);
        float b_parallel = Vector3.Dot(b_spatial, n);
        Vector3 b_perp = b_spatial - b_parallel * n;

        // Boost formulas:
        double t_prime = gamma * (b_4vel_A.t + (beta * b_parallel));
        double b_parallel_prime = gamma * (b_parallel + beta * b_4vel_A.t);

        Vector3 boosted_spatial = b_perp + (float)b_parallel_prime * n;

        DVector4 final = new DVector4(
            t_prime,
            boosted_spatial.x,
            boosted_spatial.y,
            boosted_spatial.z
        );
        return Renormalize(final);
    }
    
    //renormalizes 4 vectors to have minkowski unit norm
    public static DVector4 Renormalize(DVector4 u)
    {
        double norm = u * u;
        double scale = Math.Pow(c,2) / Math.Sqrt(norm);
        u *= scale;
        return u;
    }


    //applies "proper" acceleration alpha (i.e object percieves being accelerated at alpha m/s^2) to a velocity for time deltatau and returns the result
    // Might be mildly borked
    public DVector4 Accelerate(DVector4 velocity, DVector4 alpha, double deltaTau)
    {
        DVector4 uNew = velocity + (alpha * deltaTau);
        return Renormalize(uNew);


    }

    // Adds velocity uPrime to v in world frame
    public static Vector3 RelativisticVelocityAdd(Vector3 uPrime, Vector3 v)
    {
        double c2 = c * c;
        double vMag2 = v.sqrMagnitude;
        double vDotU = Vector3.Dot(v, uPrime);
        double gamma = 1.0 / System.Math.Sqrt(1 - vMag2 / c2);

        // Decompose u' into parallel and perpendicular components w.r.t. v
        Vector3 vNorm = v.normalized;
        Vector3 uParallel = Vector3.Dot(uPrime, vNorm) * vNorm;
        Vector3 uPerp = uPrime - uParallel;

        // Compute transformed parallel component
        Vector3 uNewParallel = (uParallel + v) / (float)(1 + vDotU / c2);

        // Compute transformed perpendicular component
        Vector3 uNewPerp = uPerp / (float)(gamma * (1 + vDotU / c2));

        return uNewParallel + uNewPerp;
    }

    // If the current object sees another object moving at  velocity velocityInFrame this method will set the current oobjects in World velocity, i.e rigidbody velocity to match
    public void AddVelocity(Vector3 velocityInFrame)
    {
        Vector3 newVelocity = RelativisticVelocityAdd(velocityInFrame, rb.linearVelocity);
        rb.linearVelocity = newVelocity;
    }

    // Accelerates the current object by alpha for how long it percieves the current tick to be, this is better used for when the object itself is limited by the acceleration it can supply than us accelerating the object
    // MIght want to change all the syncs so that only 4 velocity is updated and rigidbody velocity changes to match
    public void AccelerateSync(DVector4 alpha)
    {
        AccelerateSync(alpha, this.deltaTau);
    }

    //Accelerates object by alpha (in its own frame) for dt world
    public void AccelerateSync(DVector4 alpha, double deltat)
    {
        deltaTau = deltat;
        Vector3 vSceneOld = rb.linearVelocity;
        this.velocity = Accelerate(this.velocity, alpha, deltaTau);
        this.gamma = this.velocity.t / c;
        this.rapidity = 0.5 * Math.Log((1 + vSceneOld.magnitude / c)
                              / (1 - vSceneOld.magnitude / c));
        Vector3 vSceneNew = VRel2Scene(this.velocity, gamma);
        // rb.velocity = vSceneNew;
        Vector3 coordAccel = (vSceneNew - vSceneOld) / (float)Time.fixedDeltaTime; // m/s^2
        rb.AddForce(coordAccel * rb.mass, ForceMode.Acceleration); //Changed this from forcemode force
    }

    void FixedUpdate()
    {
        this.deltaTau = Time.fixedDeltaTime / gamma;
        tau += deltaTau;

        //this is set to 1 so I can test UI movement stuff
        ReadOnly_deltaTau = 1;
    }


}
