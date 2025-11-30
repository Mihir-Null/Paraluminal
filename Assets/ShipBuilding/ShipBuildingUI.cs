using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Experimental.AI;
using UnityEngine.SceneManagement;

public class ShipBuildingUI : MonoBehaviour
{
    public SaveLoadUI SaveLoadUI;

    public static string load = "MyShip";

    public Ship currentShip;

    public Vector2 mousePos;
    public Vector2Int mouseGridSq;
    public Transform mouseObj;
    public float sensitivity;
    InputAction mouse;
    InputAction build;
    InputAction remove;

    public bool freeze;

    //should be one for each class of shiptile
    public List<GameObject> tileImages = new List<GameObject>();

    public int currentlySelected;
    //should be one for each type of part that can be added 
    public List<ShipModule> moduleImages = new List<ShipModule>();

    Dictionary<Vector2Int, ShipModule> modules; 
    Dictionary<Vector2Int, ShipGrid> currentS;


    public Ship Export(string name)
    {
        Ship s = new Ship(currentS, name);
        return s;
    }
    public static void LoadShip(string name)
    {
        Debug.Log("Reloading Scene");
        load = name;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name,LoadSceneMode.Single);
    }
    public void LoadShip(Ship ship)
    {
        Debug.Log("Getting Tiles");
        currentShip = ship;
        LoadShip(ship.GetTileDictionary());
    }
    void LoadShip(Dictionary<Vector2Int, ShipGrid> currentShip)
    {
        Debug.Log("Spawning Tiles");

        modules = new Dictionary<Vector2Int, ShipModule>();
        currentS = new Dictionary<Vector2Int, ShipGrid>(currentShip);
        foreach (KeyValuePair<Vector2Int,ShipGrid> keyValuePair in currentS)
        {
            //draw the blank tile
            Vector3 pos = (Vector2)keyValuePair.Key;
            
            Instantiate(tileImages[(int)keyValuePair.Value.Type], pos , Quaternion.identity);
            //draw a picture of the current mod overtop
            if (keyValuePair.Value.shipModule != -1)
            {
                Vector3 modPos = (Vector2)keyValuePair.Key;
                modPos.z = -1;
                modules.Add(keyValuePair.Key, Instantiate(moduleImages[keyValuePair.Value.shipModule],modPos , Quaternion.identity));
            }
            else
            {
                modules.Add(keyValuePair.Key, null);
            }
          

        }

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mouse = InputSystem.actions.FindAction("Point");
        build = InputSystem.actions.FindAction("Attack");
        remove = InputSystem.actions.FindAction("Attack1");

        //MouseLook.SetCursorState(true);

        LoadShip(ShipSerializer.LoadShip(load));

        SaveLoadUI.CallOnStart();
    }

    // Update is called once per frame
    void Update()
    {
        if (!freeze)
        {
            mousePos = Camera.main.ScreenToWorldPoint(mouse.ReadValue<Vector2>());// * sensitivity;
            mouseGridSq = new Vector2Int((int)Mathf.Round(mousePos.x), (int)Mathf.Round(mousePos.y));
            Vector3 mP = (Vector2)mouseGridSq;
            mP.z = -2;
            mouseObj.position = (Vector2)mP;

            if (currentS != null)
            {
                //check if position is valid
                if (currentS.ContainsKey(mouseGridSq))
                {
                    if (build.triggered)
                    {
                        //check if tile is open
                        if (currentS[mouseGridSq].shipModule == -1)
                        {
                            Debug.Log("Add");
                            currentS[mouseGridSq].shipModule = currentlySelected;
                            Vector3 modPos = (Vector2)mouseGridSq;
                            modPos.z = -1;
                            modules[mouseGridSq] = Instantiate(moduleImages[currentlySelected], (Vector2)mouseGridSq, Quaternion.identity).GetComponent<ShipModule>();
                            Debug.Log(currentS[mouseGridSq].shipModule);
                        }
                        else
                        {
                            Debug.Log("Invalid Move");
                        }
                    }
                    else if (remove.triggered)
                    {

                        if (currentS[mouseGridSq].Type != GridTypes.Static && currentS[mouseGridSq].shipModule != -1)
                        {
                            Debug.Log("Remove");
                            Destroy(modules[mouseGridSq].gameObject);
                            currentS[mouseGridSq].shipModule = -1;
                        }
                        else
                        {
                            Debug.Log("Invalid Remove " + currentS[mouseGridSq].Type + " " + currentS[mouseGridSq].shipModule);
                        }
                    }

                }

            }
        }
    }
}
