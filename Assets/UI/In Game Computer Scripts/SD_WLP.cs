using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SD_WLP : ScreenData
{
    public SM_FiresProjectile currentlyLinked;

    public int currentlySelected;
    public TMP_Text[] ammoText;

    public Transform readyLight;

    public Transform selectedDot;

    public override void RunStart()
    {
        currentlyLinked = GameObject.Find("TEST").GetComponent<SM_FiresProjectile>();
        base.RunStart();
   
    }
    void UpdateText()
    {
       (Projectile,int)[] ammo = currentlyLinked.GetAmmoStores();
        Debug.Log("Update text");
        for (int i = 0; i < ammo.Length; i++)
        {
            ammoText[i].text =ammo[i].Item1.wepName + " " + ammo[i].Item2;
            if (i == currentlySelected)
            {
                readyLight.transform.localPosition = new Vector3(readyLight.transform.localPosition.x, ammoText[i].transform.localPosition.y,readyLight.transform.localPosition.z);
                selectedDot.transform.localPosition = new Vector3(selectedDot.transform.localPosition.x, ammoText[i].transform.localPosition.y, selectedDot.transform.localPosition.z);
            }         
        }
    }
    public override void RunUpdate()
    {
        base.RunUpdate();
        UpdateText();
        UpdateLight(currentlyLinked.GetLaunchStatus());
    }
    void UpdateLight(int status)
    {
        if (status == 0)
        {
            readyLight.GetComponent<Image>().color = Color.gray;
        }
        else if (status == 1)
        {

        }
    }
    public void Control_ChangeSelection(bool add)
    {
        if (add)
        {
            currentlySelected++;
        }
        else
        {
            currentlySelected--;
        }
        if (currentlySelected >= currentlyLinked.GetAmmoStores().Length)
        {
            currentlySelected = 0;
        }
        else if (currentlySelected < 0)
        {
            currentlySelected = currentlyLinked.GetAmmoStores().Length - 1;
        }

        UpdateText();
        UpdateLight(currentlyLinked.GetLaunchStatus());
    }
    public void Control_Fire()
    {
        currentlyLinked.Fire(currentlySelected);
    }
}
