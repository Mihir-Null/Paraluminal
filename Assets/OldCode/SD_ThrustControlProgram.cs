using System.Collections.Generic;
using TMPro;
using Unity.Mathematics.Geometry;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Dispite the name, this just uses accleration, becuase its basically the same as adding thrust while being easier to do math with
/// </summary>
public class SD_ThrustControlProgram : ScreenData
{
    public Transform ship;

    public Throttle acclerationThrottle;
    public Throttle velocityThrottle;
    public Throttle secondaryAcclerationThrottle;
    public Throttle secondaryVelocityThrottle;

    //this should match values in the throttles 
    public float maxSecondaryAccleration;
    public float minSecondaryAccleration;
    public float maxMainAccleration;
    public float minMainAccleration;


    //The amount of Accleration we want each Acclerationer direction to apply 
    public float fbAccleration { get; private set; }
    public float lrAccleration { get; private set; }
    public float udAccleration { get; private set; }

    public bool velocityLock;

    public Vector3 targetVelocity;
    public float lrVelocity;
    public float udVelocity;

    public bool isFlyByWire;
    public override void RunStart()
    {
        base.RunStart();
        ship = GameObject.Find("PlayerBody").transform;
    }

    public override void RunUpdate()
    {
        base.RunUpdate();

        if (!isFlyByWire)
        {
            acclerationThrottle.gameObject.SetActive(true);
            velocityThrottle.gameObject.SetActive(false);
            secondaryAcclerationThrottle.gameObject.SetActive(true);
            secondaryVelocityThrottle.gameObject.SetActive(false);

            acclerationThrottle.ThrottleLogic();
            secondaryAcclerationThrottle.ThrottleLogic();

            fbAccleration = acclerationThrottle.currentValue;
        }
        else
        {
            acclerationThrottle.gameObject.SetActive(false);
            velocityThrottle.gameObject.SetActive(true);
            secondaryAcclerationThrottle.gameObject.SetActive(false);
            secondaryVelocityThrottle.gameObject.SetActive(true);

            velocityThrottle.ThrottleLogic();
            secondaryVelocityThrottle.ThrottleLogic();

            if (!velocityLock)
            {
                targetVelocity = (ship.transform.forward * velocityThrottle.currentValue) +
                                 (ship.transform.right * lrVelocity) +
                                 (ship.transform.up * udVelocity);
            }
        }
    }
    public void Control_ToggleFlyByWire()
    {
        if (isFlyByWire)
        {
            csb.LogMess("Manual Control Enabled");
            isFlyByWire = false;
        }
        else if (!isFlyByWire)
        {
            csb.LogMess("Fly By Wire Mode Activated");
            isFlyByWire = true;
        }
    }
    public void Control_FowardBack(int direction)
    {
        if (isFlyByWire)
        {
            Control_FowardBack(direction, acclerationThrottle);
        }
        else
        {
            Control_FowardBack(direction, velocityThrottle);
        }
    }
    void Control_FowardBack(int direction, Throttle t)
    {
        t.Control_IncreaseDecreaseThrottle(direction);
    }
    public void Control_Secondary_DirectionalMovement(int direction)
    {
        if (!isFlyByWire)
        {
            //left
            if (direction == 1)
            {
                lrAccleration = -secondaryAcclerationThrottle.currentValue;
            }
            //right
            else if (direction == 2)
            {
                lrAccleration = secondaryAcclerationThrottle.currentValue;
            }
            else if (direction == 0)
            {
                lrAccleration = 0;
            }
            //up
            if (direction == 1)
            {
                udAccleration = secondaryAcclerationThrottle.currentValue;
            }
            //down
            else if (direction == 2)
            {
                udAccleration = -secondaryAcclerationThrottle.currentValue;
            }
            else if (direction == 0)
            {
                udAccleration = 0;
            }
        }
        else
        {
            //left
            if (direction == 1)
            {
                lrVelocity = -10;//-secondaryVelocityThrottle.currentValue;
            }
            //right
            else if (direction == 2)
            {
                lrVelocity = 10;// secondaryVelocityThrottle.currentValue;
            }
            else if (direction == 0)
            {
                lrVelocity = 0;
            }
            //up
            if (direction == 1)
            {
                udVelocity = 10;// secondaryVelocityThrottle.currentValue;
            }
            //down
            else if (direction == 2)
            {
                udVelocity = -10;//-secondaryVelocityThrottle.currentValue;
            }
            else if (direction == 0)
            {
                udVelocity = 0;
            }
        }
    }
    public void Control_Secondary_ShiftThrottle(int direction)
    {
        if (isFlyByWire)
        {
            secondaryAcclerationThrottle.Control_IncreaseDecreaseThrottle(direction);
        }
    }
    public void Control_ToggleLockVelocity()
    {
        if (isFlyByWire)
        {
            velocityLock = !velocityLock;
        }
    }
    /// <summary>
    /// Returns the required velocity in each relative direction required to achive desiredVelocity
    /// Note: currentVelocity and desiredVelocity are not relative to the angle of the player
    /// </summary>
    /// <param name="currentVelocity"></param>
    /// <param name="desiredVelocity"></param>
    /// <param name="transformForward"></param>
    /// <param name="transformUp"></param>
    /// <param name="transformDown"></param>
    /// <returns></returns>
    public static Vector3 FlyByWire_RequiredRelAccleration(Vector3 currentVelocity, Vector3 desiredVelocity, Vector3 transformForward, Vector3 transformUp, Vector3 transformRight)
    {
        Vector3 requiredChange = desiredVelocity - currentVelocity;

        //dot products 
        float f = Vector3.Dot(requiredChange, transformForward);
        float u = Vector3.Dot(requiredChange, transformUp);
        float r = Vector3.Dot(requiredChange, transformRight);

        Debug.Log(f);

        return new Vector3(r, u, f);
    }




    /// <summary>
    /// This loop simulates the logic of a fly-by-wire flight control computer, 
    /// under the rules of the ship it is allowed to instantly change the secondary Acclerationers to any valid level of Accleration, 
    /// but like the player the control computer cannot instantly change the main Acclerationers 
    /// </summary>
    /// <param name="desiredVelocity"></param>
    /// <param name="currentVelocity"></param>
    /// <param name="currentMass"></param>
    /// <param name="transformForward"></param>
    /// <param name="transformUp"></param>
    /// <param name="transformRight"></param>
    public void FlyByWire_UpdateLoop_Old(Vector3 currentVelocity, Vector3 transformForward, Vector3 transformUp, Vector3 transformRight, float allowedFThrotDif = 0.1f)
    {
        Vector3 desiredVelocity = targetVelocity;

        Vector3 requiredAccler = FlyByWire_RequiredRelAccleration(currentVelocity, desiredVelocity, transformForward, transformUp, transformRight);

        //deal with x and y axis

        //if Accler is within min and max Accleration, we can match it exaclty  
        if (requiredAccler.x >= minSecondaryAccleration && requiredAccler.x <= maxSecondaryAccleration)
        {
            lrAccleration = requiredAccler.x;
        }
        //if Accler is too negative, apply max neg Accler
        else if (requiredAccler.x < 0)
        {
            lrAccleration = minSecondaryAccleration;
        }
        //if Accler is too large, appy max pos Accler
        else if (requiredAccler.x > 0)
        {
            lrAccleration = maxSecondaryAccleration;
        }
        else
        {
            lrAccleration = 0;
        }

        //if Accler is within min and max Accleration, we can match it exaclty  
        if (requiredAccler.y >= minSecondaryAccleration && requiredAccler.y <= maxSecondaryAccleration)
        {
            udAccleration = requiredAccler.y;
        }
        //if Accler is too negative, apply max neg Accler
        else if (requiredAccler.y < 0)
        {
            udAccleration = minSecondaryAccleration;
        }
        //if Accler is too large, appy max pos Accler
        else if (requiredAccler.y > 0)
        {
            udAccleration = maxSecondaryAccleration;
        }
        else
        {
            udAccleration = 0;
        }

        //if Accler is within min and max Accleration, we can match it exaclty  
        if (requiredAccler.z >= minMainAccleration && requiredAccler.z <= maxMainAccleration)
        {
            fbAccleration = requiredAccler.z;
        }
        //if Accler is too negative, apply max neg Accler
        else if (requiredAccler.z < 0)
        {
            fbAccleration = minMainAccleration;
        }
        //if Accler is too large, appy max pos Accler
        else if (requiredAccler.z > 0)
        {
            fbAccleration = maxMainAccleration;
        }
        else
        {
            fbAccleration = 0;
        }

    }

    public void FlyByWire_UpdateLoop(Vector3 currentVelocity, Vector3 transformForward, Vector3 transformUp, Vector3 transformRight, float allowedFThrotDif = 0.1f)
    {
        // This is our total desired change in velocity
        Vector3 desiredDeltaVelocity = (targetVelocity - currentVelocity);
        //If possible we want to accelerate so that by the next frame (when we reevaluate our thrust priorities) we have reached our desired delta velocity
        Vector3 desiredAcceleration = desiredDeltaVelocity / Time.deltaTime;

        //Unfortunately we can't always have what we want so we calculate what the max acceleration possible is given our current orientation
        float f = Vector3.Dot(desiredAcceleration, transformForward);
        float u = Vector3.Dot(desiredAcceleration, transformUp);
        float r = Vector3.Dot(desiredAcceleration, transformRight);

        float fpercentage = f > 0 ? maxMainAccleration / f : minMainAccleration / f;
        float upercentage = u > 0 ? maxMainAccleration / u : minMainAccleration / u;
        float rpercentage = r > 0 ? maxMainAccleration / r : minMainAccleration / r;

        float limitingPercentage = Mathf.Min(fpercentage, upercentage, rpercentage);
        Vector3 actualAcceleration = limitingPercentage * desiredAcceleration;

        udAccleration = actualAcceleration.y;
        lrAccleration = actualAcceleration.x;
        fbAccleration = actualAcceleration.z;




    }
}
