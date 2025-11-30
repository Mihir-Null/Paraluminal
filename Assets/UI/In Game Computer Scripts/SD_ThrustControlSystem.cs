using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SD_ThrustControlSystem : ScreenData
{
    public Vector3 maxThrust;
    public Vector3 minThrust;
    public Vector3 changeVelocitySpeed;
    public Vector3 targetedRelVelocity;
   public Vector3 relVel;
    public Vector3 outputAccleration { get;private set; }
    public bool velocityLocked;

    public Rigidbody ship;


    public RectTransform center;
    public float scale;
    public RectTransform desiredVelocityIm;
    public RectTransform currentVelocityIm;
    public TMP_Text dv_x, dv_y, dv_z,cv_x,cv_y,cv_z,a_x,a_y,a_z;
    public TMP_Text speed;

    public override void RunUpdate()
    {




      

        base.RunUpdate();

        outputAccleration = FlyByWire_UpdateLoop(ship.linearVelocity, ship.transform.forward, ship.transform.up, ship.transform.right, targetedRelVelocity, maxThrust, minThrust);

        DrawUI(ship.linearVelocity);
    }

   void DrawUI(Vector3 currentVelocity)
    {
        speed.text = currentVelocity.magnitude.ToString();


     
        

        void SetText(TMP_Text text, float value)
        {
            text.text = Mathf.Round(value).ToString();
        }
        void Set(TMP_Text x, TMP_Text y, TMP_Text z, Vector3 dv, RectTransform rectTransform)
        {
            SetText(x, dv.x);
            SetText(y, dv.y);
            SetText(z, dv.z);





            float fx = HMath.posLogic(50,-50, dv.x) * scale;
            float fy = HMath.posLogic(50,-50, dv.y);
            float fz = HMath.posLogic(50,-50, dv.z) * scale;
            
            Color yColor = (fy > 0 ? Color.Lerp(Color.white,Color.blue,fy) : fy < 0 ? Color.Lerp(Color.white, Color.orange,-fy) : Color.white);



            Vector3 pos = center.localPosition + new Vector3(fx,fz,0);
            rectTransform.localPosition = pos;
            rectTransform.GetComponent<Image>().color = yColor;

        }
        Set(dv_x, dv_y, dv_z, FlyByWire_RequiredRelAccleration(Vector3.zero,targetedRelVelocity,ship.transform.forward, ship.transform.up, ship.transform.right), desiredVelocityIm);
        Set(cv_x, cv_y, cv_z, FlyByWire_RequiredRelAccleration(Vector3.zero, currentVelocity, ship.transform.forward, ship.transform.up, ship.transform.right), currentVelocityIm);
    }


    public void Control(InputAction foward,InputAction back, InputAction left, InputAction right, InputAction up, InputAction down)
    {
        if (!velocityLocked)
        {
            int Direction(InputAction positive, InputAction negative)
            {
                if (positive.IsPressed())
                {
                    Debug.Log("A");
                    return 1;
                }
                else if (negative.IsPressed())
                {
                    Debug.Log("B");
                    return -1;
                }
                Debug.Log("C");
                return 0;
            }

            relVel += HMath.VectorMult(new Vector3(Direction(right, left), Direction(up, down), Direction(foward, back)), changeVelocitySpeed);// * (float)RelBody.ReadOnly_deltaTau;
            targetedRelVelocity = (ship.transform.forward * relVel.z) + (ship.transform.up * relVel.y) + (ship.transform.right * relVel.x);
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



  
    const float arbritaryScaleFactor = 5;

    public static Vector3 FlyByWire_UpdateLoop(Vector3 currentVelocity, Vector3 transformForward, Vector3 transformUp, Vector3 transformRight, Vector3 desiredVelocity, Vector3 maxAccleration, Vector3 minAccleration) {


        //Vector3 desiredVelocity = (transformForward * targetedRelVelocity.z) + (transformUp * targetedRelVelocity.y) + (transformRight * targetedRelVelocity.x);

        Debug.Log("DV " + desiredVelocity);
        // This is our total desired change in velocity
        Vector3 desiredDeltaVelocity = (desiredVelocity - currentVelocity);
        Debug.Log("DDV " + desiredVelocity);
        //If possible we want to accelerate so that by the next frame (when we reevaluate our thrust priorities) we have reached our desired delta velocity
        Vector3 desiredAcceleration = desiredDeltaVelocity / arbritaryScaleFactor;

        //Unfortunately we can't always have what we want so we calculate what the max acceleration possible is given our current orientation
        float f = Vector3.Dot(desiredAcceleration, transformForward);
        float u = Vector3.Dot(desiredAcceleration, transformUp);
        float r = Vector3.Dot(desiredAcceleration, transformRight);

        Debug.Log(f + " " + u + " " + r);

        float fpercentage = Mathf.Abs( f > 0 ? maxAccleration.z / f : minAccleration.z / f );
      
        float upercentage = Mathf.Abs( u > 0 ? maxAccleration.y / u : minAccleration.y / u );
        float rpercentage = Mathf.Abs( r > 0 ? maxAccleration.x / r : maxAccleration.x / r );

        Debug.Log(fpercentage + " " + upercentage + " " + rpercentage);

        float limitingPercentage = Mathf.Min(fpercentage, upercentage, rpercentage);

        Debug.Log("PP" + limitingPercentage);

        //THis may somehow bite us in the ass in the future 5/28/2025 --Sam
        if ( limitingPercentage > 100)
        {
            limitingPercentage = 100;
        }
        Vector3 actualAcceleration = limitingPercentage * desiredAcceleration;

        Debug.Log("aa " + actualAcceleration);
        return actualAcceleration;


    }



    /// <summary>
    /// OLD DO NOT USE
    /// </summary>
    /// <param name="currentVelocity"></param>
    /// <param name="transformForward"></param>
    /// <param name="transformUp"></param>
    /// <param name="transformRight"></param>
    /// <param name="desiredVelocity"></param>
    /// <param name="maxAccleration"></param>
    /// <param name="minAccleration"></param>
    /// <param name="allowedFThrotDif"></param>
    /// <returns></returns>
    public static Vector3 OLDFlyByWire_UpdateLoop(Vector3 currentVelocity, Vector3 transformForward, Vector3 transformUp, Vector3 transformRight, Vector3 desiredVelocity, Vector3 maxAccleration, Vector3 minAccleration, float allowedFThrotDif = 0.1f)
    {
        Vector3 applyAccleration = new Vector3();




        Vector3 requiredAccler = FlyByWire_RequiredRelAccleration(currentVelocity, desiredVelocity, transformForward, transformUp, transformRight);



        float Direction(float required, float min, float max)
        {
            //if Accler is within min and max Accleration, we can match it exaclty  
            if (required >= min && required <= max)
            {
                return required;
            }
            //if Accler is too negative, apply max neg Accler
            else if (required < 0)
            {
                return min;
            }
            //if Accler is too large, appy max pos Accler
            else if (required > 0)
            {
                return max;
            }
            else
            {
                return 0;
            }
        }

        applyAccleration.x = Direction(requiredAccler.x, minAccleration.x, maxAccleration.x);
        applyAccleration.y = Direction(requiredAccler.y, minAccleration.y, maxAccleration.y);
        applyAccleration.z = Direction(requiredAccler.z, minAccleration.z, maxAccleration.z);


        return applyAccleration;

    }

}
