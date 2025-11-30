using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


[Serializable]
public class Ship
{
    public string shipName;
    public List<SerializableTile> tileList;

    public Ship(Dictionary<Vector2Int, ShipGrid> tiles, string name)
    {
        shipName = name;
        tileList = new List<SerializableTile>();
        foreach (var kvp in tiles)
        {
            tileList.Add(new SerializableTile(kvp.Key, kvp.Value));
        }
    }

    public Dictionary<Vector2Int, ShipGrid> GetTileDictionary()
    {
        var dict = new Dictionary<Vector2Int, ShipGrid>();
        foreach (var tile in tileList)
        {
            dict[tile.position] = new ShipGrid(tile.gridType, tile.prefabAttached);
        }
        return dict;
    }
}

[Serializable]
public class SerializableTile
{
    public Vector2Int position;
    public GridTypes gridType;
    public int prefabAttached;

    public SerializableTile(Vector2Int pos, ShipGrid grid)
    {
        position = pos;
        gridType = grid.Type;
        prefabAttached = grid.shipModule;
    }
}

[Serializable]
public class ShipGrid
{
    public GridTypes Type;
    public int shipModule;

    public ShipGrid(GridTypes type, int shipModule)
    {
        this.Type = type;
        this.shipModule  = shipModule;
    }
}

public enum GridTypes
{
    Small,
    Medium,
    Large,
    HardpointS,
    HardpointM,
    HardpointL,
    Static
}
public static class ShipSerializer
{
    private static string GetShipPath(string shipName)
    {
        return Path.Combine(Application.persistentDataPath, $"{shipName}.json");
    }

    public static void SaveShip(Ship ship)
    {
        string json = JsonUtility.ToJson(ship, true);
        File.WriteAllText(GetShipPath(ship.shipName), json);
        Debug.Log($"Ship '{ship.shipName}' saved to {GetShipPath(ship.shipName)}");
    }

    public static Ship LoadShip(string shipName)
    {
        string path = GetShipPath(shipName);
        if (!File.Exists(path))
        {
            Debug.LogError($"Ship file '{path}' not found.");
            return null;
        }

        string json = File.ReadAllText(path);
        Ship loadedShip = JsonUtility.FromJson<Ship>(json);
        return new Ship(loadedShip.GetTileDictionary(), loadedShip.shipName);
    }

    public static void SaveExample()
    {
        var tiles = new Dictionary<Vector2Int, ShipGrid>
        {
            [new Vector2Int(0, 0)] = new ShipGrid(GridTypes.Small,-1),
            [new Vector2Int(1, 0)] = new ShipGrid(GridTypes.Medium,-1)
        };

        var ship = new Ship(tiles, "MyShip");
        ShipSerializer.SaveShip(ship);
    }

    public static Dictionary<Vector2Int,ShipGrid> LoadExample()
    {
        SaveExample();
        Ship ship = ShipSerializer.LoadShip("MyShip");
        if (ship != null)
        {
            Debug.Log("Loaded ship: " + ship.shipName);
            return ship.GetTileDictionary();
        }
        return null;
    }
}