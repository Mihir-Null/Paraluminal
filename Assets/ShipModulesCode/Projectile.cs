using TMPro.EditorUtilities;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public string wepName;

    public float accleration;
    public double acclerateTime;

    RelBody rb;

    private void Start()
    {
        rb = GetComponent <RelBody> ();       
    }

    public void Launch(RelBody parent, Vector3 firingVelocity)
    {
        DVector4 parentVel = parent.velocity;
        rb.velocity = parentVel;
        rb.Rel2Rb();
        rb.AddVelocity(firingVelocity);
    }
    private void Update()
    {
        if (acclerateTime > 0)
        {
            acclerateTime -= rb.deltaTau;
            rb.AccelerateSync(new DVector4(transform.forward * accleration));  
        }
    }

}
