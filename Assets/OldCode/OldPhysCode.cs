using UnityEngine;

public class OldPhysCode : MonoBehaviour
{
    //For reference only


    /*
    Rigidbody unityPhys;


    //locked phys (Intended for orbital objects), means that position and velocity are not updated here. 
    //It is assumed that they are being manually set somewhere else, orignally intended for objects using the Orbital Script
    public bool lockedPhys;

    static List<Collider> objs = new List<Collider>();

    static void ProcessCollisions()
    {
        //shitty O(n^2) bullshit 
        for (int i = 0; i < objs.Count; i++)
        {
            for (int ii = i + 1; ii < objs.Count; ii++)
            {
                Collider colliderA = objs[i];
                Collider colliderB = objs[ii];

                Debug.Log(colliderA.name + " " + colliderB.name);

                if (colliderA != colliderB)
                {
                    bool isOverlapping = Physics.ComputePenetration(
                    colliderA, colliderA.transform.position, colliderA.transform.rotation,
                     colliderB, colliderB.transform.position, colliderB.transform.rotation,
                    out Vector3 direction, out float distance);

                    if (isOverlapping)
                    {
                        colliderA.gameObject.GetComponent<Phys2>().position.Add(new LVector(direction * distance));

                        Phys2 a, b;
                        a = colliderA.GetComponent<Phys2>();
                        b = colliderB.GetComponent<Phys2>();

                        LVector Va, Vb;
                        double Ma, Mb;
                        double elast = 1;

                        Va = new LVector(a.velocity);
                        Ma = a.mass;

                        Vb = new LVector(b.velocity);
                        Mb = b.mass;

                        LVector MATH(double e, LVector VA, LVector VB, double MA, double MB)
                        {
                            LVector dist = LVector.Subtract(VB, VA);
                            dist.Mult(Mb);
                            dist.Mult(e);
                            LVector Pa = LVector.Mult(VA, MA);
                            LVector Pb = LVector.Mult(VB, MB);
                            dist.Add(Pa);
                            dist.Add(Pb);
                            dist.Mult(1 / (MA + MB));
                            return dist;
                        }

                        a.velocity = MATH(elast, Va, Vb, Ma, Mb);

                        b.velocity = MATH(elast, Vb, Va, Mb, Ma);
                    }

                }
            }
        }

    }
    public bool runProcessCollisions;

    public Phys2[] gravApply;
    public void SetAngularDrag(float angularDrag)
    {
        unityPhys.angularDrag = angularDrag;
    }


    static double timeStep = 0.1f;

    //when adding collision velocity to our velocity, mult by this
    static float velocityConverison = 2;

    //this point is considered 0,0,0 when translating into unity space
    static public LVector referencePoint { get; set; }// = new LVector(0,0,0);


    /// <summary>
    /// how many Meters is 1 unity unit equvilent to?
    /// positions are divided by this number 
    /// 
    /// THIS IS THE FINAL SCALE FOR THIS BUILD. DO NOT CHANGE
    /// 
    /// </summary>
    static public float scale = 5000;
    static public float scaleRadius = 5000;
    //Create a new vector multiplied by the timestep
    static LVector MultDT(LVector vec)
    {
        LVector nnew = LVector.Mult(vec, timeStep);
        return nnew;
    }

    /// <summary>
    /// Position in KM
    /// </summary>
    public LVector position { private set; get; }
    public void SetPosition(LVector v)
    {
        position = new LVector(v);
    }
    public string positionV;
    /// <summary>
    /// Velocity in km/s
    /// </summary>
    public LVector velocity { private set; get; }
    public void SetVelocityZero()
    {
        velocity = new LVector();
    }
    public LVector GetMomentum()
    {
        return LVector.Mult(velocity, mass);
    }
    public string velocityV;
    /// <summary>
    /// In Kgs
    /// </summary>
    public double mass;

    /// <summary>
    /// DO NOT USE THIS UNLESS YOU KNOW WHAT THE FUCK YOU ARE DOING. 
    /// </summary>
    public void SetVelocityDANGER(LVector newVelocity)
    {
        velocity = newVelocity;
    }
    public void AddAccleration(LVector accleration, bool instant = false)
    {
        if (instant)
        {
            velocity.Add(accleration);
        }
        else
        {
            velocity.Add(MultDT(accleration));
        }

    }
    public void AddForce(Vector3 force)
    {
        AddForce(new LVector(force));
    }
    public void AddForce(LVector force, bool instant = false)
    {
        LVector nforce = new LVector(force);

        nforce = MultDT(nforce);

        nforce.Mult(1 / mass);
        AddAccleration(nforce, instant);
    }
    public void AddForceUntilZero(LVector force)
    {
        int isNegative(double d, double epl)
        {
            if (d < -epl)
            {
                return -1;
            }
            else if (d > epl)
            {
                return 1;
            }
            return 0;
        }

        //get sign of each 
        int x, y, z;
        x = isNegative(velocity.X(), 0.1);
        y = isNegative(velocity.Y(), 0.1);
        z = isNegative(velocity.Z(), 0.1);

        //add force normally 
        AddForce(force);

        int nx, ny, nz;
        nx = isNegative(velocity.X(), 0.1);
        ny = isNegative(velocity.Y(), 0.1);
        nz = isNegative(velocity.Z(), 0.1);

        double vx, vy, vz;
        vx = velocity.X();
        vy = velocity.Y();
        vz = velocity.Z();

        //if the direction of anything changed, set that velocity to zero instead 
        if (nx != x)
        {
            vx = 0;
        }
        else if (ny != y)
        {
            vy = 0;
        }
        else if (nz != z)
        {
            vz = 0;
        }
    }

    public void AddTorque(Vector3 torque)
    {
        unityPhys.AddRelativeTorque(torque);
    }

    public static double GravForceEquation(double mass1, double mass2, double distance)
    {
        const double g = 6.67E-11;
        distance = distance * distance;
        double massTotal = mass1 * mass2;
        double innerTotal = massTotal / distance;
        innerTotal *= g;

        Debug.Log("Inner " + innerTotal);
        return innerTotal;
    }
    public static LVector GravForceVector(Phys2 objectA, Phys2 objectB)
    {
        LVector dist = LVector.Subtract(objectB.position, objectA.position);
        //Note distance is already in meters, so this code is no longer needed 
        //dist.Mult(1000);

        Debug.Log("DIST: " + dist.ToString());
        double d = GravForceEquation(objectA.mass, objectB.mass, LVector.Magnitude(dist));

        dist = LVector.Normalized(dist);
        Debug.Log("DIST NORM: " + dist);
        dist.Mult(d);
        Debug.Log("Dist FINAL " + dist);
        return dist;
    }
    public static LVector ApplyGravity(LVector velocity, Phys2[] phys, Phys2 tthis)
    {
        LVector acceleration = new LVector();
        foreach (Phys2 phy in phys)
        {


            LVector force = GravForceVector(tthis, phy);
            force.Mult(1 / tthis.mass); // a = F / m
            acceleration.Add(force);
        }
        // Apply acceleration
        LVector newVelocity = new LVector(velocity);
        newVelocity.Add(MultDT(acceleration)); // v += a * dt
        return newVelocity;



        LVector vec = new LVector(velocity);
        foreach (Phys2 phy in phys)
        {
            //vec.Add(Phys2.AddAccleration(GravForceVector(tthis,phy),tthis.mass, new LVector(velocity)));
        }
        return vec;
    }



    void UpdatePosition()
    {
        LVector lastPos = new LVector(position);
        position.Add(MultDT(velocity));
        //LVector vec = LVector.Subtract(lastPos, position);
        //unityPhys.AddForce(,ForceMode.)



    }

    public static Vector3 ConvertToWorldSpace(LVector position)
    {
        LVector relPos = LVector.Subtract(position, referencePoint);
        relPos.Mult(1 / scale);
        return relPos.ToVector3();
    }

    private void Start()
    {


        objs.Add(gameObject.GetComponent<Collider>());
        unityPhys = GetComponent<Rigidbody>();
        unityPhys.constraints = RigidbodyConstraints.None;
        position = new LVector(transform.position);
        velocity = new LVector();

        transform.localScale /= scaleRadius;
    }
    private void OnDestroy()
    {
        objs.Remove(gameObject.GetComponent<Collider>());
    }
    private void FixedUpdate()
    {
        if (referencePoint == null)
        {
            Debug.LogWarning("Null Reference Point");
            return;
        }


        if (runProcessCollisions)
        {
            ProcessCollisions();
        }

        if (!lockedPhys)
        {
            velocity = ApplyGravity(velocity, gravApply, this);
            LVector lastPos = new LVector(position);

            UpdatePosition();
        }
        unityPhys.MovePosition(ConvertToWorldSpace(position));

        positionV = position.ToString();
        velocityV = velocity.ToString();


    }

    */
}
