using System.Collections.Generic;
using UnityEngine;

public class SM_FiresProjectile : SModule
{
    (Projectile,int)[] ammoStores = new (Projectile, int)[10];

    /// <summary>
    /// In most places in the code, we have used velocity / accleration
    /// but here, since missiles / bullets could have different masses that should be reflected in their launch speed
    /// </summary>
    public float launchForce;
    public float spawnDistance;

    public RelBody ship;
    protected override void OnStart()
    {
        ammoStores = new (Projectile,int)[2];
        ammoStores[0] = (GameObject.Find("TEST1").GetComponent<Projectile>(),69);
        ammoStores[1] = (GameObject.Find("TEST2").GetComponent<Projectile>(), 420);
        base.OnStart();
    }
    public (Projectile, int)[] GetAmmoStores()
    {
        return ammoStores;
    }

    public int GetLaunchStatus()
    {
        return 0;
    }
    public bool Fire(int ammoType)
    {
        if (ammoStores[ammoType].Item2 > 0)
        {

            Instantiate(ammoStores[ammoType].Item1, transform.position + (transform.forward * spawnDistance), transform.rotation).GetComponent<Projectile>().Launch(ship,transform.forward * launchForce);
            ammoStores[ammoType].Item2--;
            return true;
        }
        return false;
    }
}
