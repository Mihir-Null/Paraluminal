using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SaveLoadUI : MonoBehaviour
{
    public ShipBuildingUI ShipBuildingUI;

    public GameObject clone;
    List<GameObject> buttonList = new List<GameObject>();

    public Transform parent;

    public int spacing;

    public TMP_InputField saveLoad;

    public void Save()
    {
        Debug.Log("Saving Ship");
        ShipSerializer.SaveShip(ShipBuildingUI.Export(saveLoad.text));
        Load();
    }
    public void Load()
    {
        Debug.Log("Loading Ship");
        if (ShipSerializer.LoadShip(saveLoad.text) != null)
        {
            Debug.Log("Load is valid");
            ShipBuildingUI.LoadShip(saveLoad.text);
        }
        else
        {
            Debug.Log("Invalid Load");
        }
    }


    public void CallOnStart()
    {
        Refresh();
        Debug.Log(saveLoad + " " + saveLoad.text + " " + ShipBuildingUI);
        saveLoad.text = ShipBuildingUI.currentShip.shipName;
    }

    void Refresh()
    {

        buttonList = new List<GameObject>();
        int i = 1;

        foreach (string s in GetAllSavedShipNames())
        {
            GameObject g = Instantiate(clone);
            g.transform.parent = parent;
            g.transform.position = saveLoad.transform.position + new Vector3(0,spacing * i);
            g.transform.GetChild(0).GetComponent<TMP_Text>().text = s;
            i++;
        }
    }

    static List<string> GetAllSavedShipNames()
    {
        List<string> shipNames = new List<string>();
        string saveDir = Application.persistentDataPath;

        if (!Directory.Exists(saveDir))
        {
            Debug.LogWarning("Save directory not found: " + saveDir);
            return shipNames;
        }

        string[] files = Directory.GetFiles(saveDir, "*.json");

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            shipNames.Add(fileName);
        }

        return shipNames;
    }
}
