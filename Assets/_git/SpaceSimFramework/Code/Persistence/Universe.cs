using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SpaceSimFramework
{
public class Universe : MonoBehaviour
{
    #region static functionality
    public static Dictionary<SerializableVector2, SerializableUniverseSector> Sectors;

    static Universe()
    {
        // Load from binary file upon start
        Sectors = LoadUniverse();
    }
    #endregion static functionality

    public static Dictionary<SerializableVector2, SerializableUniverseSector> LoadUniverse()
    {
        Dictionary<SerializableVector2, SerializableUniverseSector> data =
            (Dictionary<SerializableVector2, SerializableUniverseSector>)Utils.LoadBinaryFile(Utils.UNIVERSE_FILE);
        if (data == null)
        {
            Debug.LogWarning("Tried to load universe from "+ Utils.UNIVERSE_FILE + " but universe file was not found! Creating new universe.");
            data = new Dictionary<SerializableVector2, SerializableUniverseSector>();
        }

        return data;
    }

    public static void SaveUniverse()
    {
        if (!Directory.Exists(Utils.PROFILE_FOLDER))
        {
            Directory.CreateDirectory(Utils.PROFILE_FOLDER);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(Utils.UNIVERSE_FILE, FileMode.OpenOrCreate);
        formatter.Serialize(stream, Universe.Sectors);
        stream.Close();
    }

    public static void AddSector(Vector2 sectorPosition, List<GameObject> jumpgates, string name = "")
    {
        SerializableUniverseSector sectorData = new SerializableUniverseSector(
            name == "" ? "sector_x"+sectorPosition.x+"_y"+sectorPosition.y : name,
            (int)sectorPosition.x,
            (int)sectorPosition.y,
            "Neutral"
            );

        foreach (GameObject gate in jumpgates)
        {
            sectorData.Connections.Add(gate.GetComponent<Jumpgate>().NextSector);
        }

        if (Sectors.ContainsKey(sectorPosition))
        {
            Sectors[sectorPosition] = sectorData;
        }
        else
        {
            Sectors.Add(sectorPosition, sectorData);
        }

        SaveUniverse();
    }

    public static void AddCurrentSector(Vector2 sectorPosition)
    {
        Sectors = LoadUniverse();
        AddSector(
            sectorPosition,
            SectorNavigation.Jumpgates,
            ProfileMenuController.PLAYER_PROFILE + "_x" + sectorPosition.x + "y" + sectorPosition.y
        );        
    }

    public static List<SerializableUniverseSector> GetAdjacentSectors(Vector2 position)
    {
        List<SerializableUniverseSector> adjacent = new List<SerializableUniverseSector>();
        SerializableUniverseSector sector = null;

        for(int i=-1; i<=1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if(i != 0 && j != 0)
                {
                    Sectors.TryGetValue(new Vector2(i, j), out sector);
                    if (sector != null)
                        adjacent.Add(sector);
                }
            }
        }

        return adjacent;
    }
}
}