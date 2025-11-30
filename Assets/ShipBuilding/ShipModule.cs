using System;
using UnityEngine;
[Serializable]
public class ShipModule : MonoBehaviour
{
    
    public string ModName;
    public string description;
    public GridTypes mainType;
    public GridTypes[] alsoAllowed;


    public static ShipModule New(int code)
    {
        GameObject g = new GameObject();
        g.AddComponent<ShipModule>();
        return g.GetComponent<ShipModule>();
    }
}
