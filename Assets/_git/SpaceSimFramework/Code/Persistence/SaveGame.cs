using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System;

namespace SpaceSimFramework
{
public class SaveGame
{    

    public static void Save(Vector2 nextSector)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = null;

        if (ProfileMenuController.PLAYER_PROFILE == "undefined")
        {
            Debug.LogWarning("Player profile is not set! Please use the Main Menu to start the game");
        }

      
        if(!Directory.Exists(Utils.PROFILE_FOLDER))
        {
            Directory.CreateDirectory(Utils.PROFILE_FOLDER);
        }

        // Save progress
        try
        {
            stream = new FileStream(Utils.PROFILE_FOLDER + "/Autosave", FileMode.OpenOrCreate);
            SerializablePlayerData data = GetPlayerData(nextSector);
            formatter.Serialize(stream, data);
            stream.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("Error while saving player data! (" + e.Message + ")\n" + e.StackTrace);
        }
        finally
        {
            if (stream != null)  stream.Close();
        }

        // Save knowledge
        try
        {
            stream = new FileStream(Utils.KNOWLEDGE_FILE, FileMode.OpenOrCreate);
            formatter.Serialize(stream, UniverseMap.Knowledge);
            stream.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("Error while saving player knowledge! (" + e.Message + ")\n" + e.StackTrace);
        }
        finally
        {
            if (stream != null)  stream.Close();
        }
    }

    public static void SaveAndJump(Vector2 nextSector)
    {
        Save(nextSector);
    }

    private static SerializablePlayerData GetPlayerData(SerializableVector2 nextSector)
    {
        SerializablePlayerData data = new SerializablePlayerData();

        // General player info
        data.Name = Player.Instance.Name;
        data.Credits = Player.Instance.Credits;
        data.Rank = Progression.Level;
        data.Experience = Progression.Experience;
        data.CurrentSector = SectorNavigation.CurrentSector;
        data.Kills = GetKillData();
        data.Reputation = Player.Instance.GetReputations();

        // Mission data
        if(MissionControl.CurrentJob != null) {
            data.Mission = SerializableMissionData.FromMission();
        }

        data.Ships = new List<SerializablePlayerShip>();
        foreach (GameObject shipObj in Player.Instance.Ships)
        {
            Ship ship = shipObj.GetComponent<Ship>();

            RemoveEquipmentModifiers(ship);
            SerializablePlayerShip shipModel = SerializablePlayerShip.FromShip(ship, nextSector);
            ReturnEquipmentModifiers(ship);
            data.Ships.Add(shipModel);
        }

        foreach (Player.ShipDescriptor OOSShip in Player.Instance.OOSShips)
        {
            SerializablePlayerShip shipModel = SerializablePlayerShip.FromOOSShip(OOSShip);
            data.Ships.Add(shipModel);
        }

        if(nextSector != SectorNavigation.UNSET_SECTOR)
            data.CurrentSector = nextSector;

        return data;
    }

    private static SerializableVector2[] GetKillData()
    {
        SerializableVector2[] kills = new SerializableVector2[ObjectFactory.Instance.Factions.Length];
        for (int i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
        {
            Faction f = ObjectFactory.Instance.Factions[i];
            kills[i] = new SerializableVector2(Player.Instance.Kills[f].x, Player.Instance.Kills[f].y);
        }
        return kills;
    }

    /// <summary>
    /// Remove effects of mounted equipment before saving to prevent tampering with saving and loading
    /// </summary>
    private static void RemoveEquipmentModifiers(Ship ship)
    {
        var eq = ship.Equipment.MountedEquipment;
        foreach (var item in eq)
            item.RemoveItem(ship);
    }

    /// <summary>
    /// Return effects of mounted equipment after saving in case game was saved manually (scene continues)
    /// </summary>
    private static void ReturnEquipmentModifiers(Ship ship)
    {
        foreach (var item in ship.Equipment.MountedEquipment)
            item.InitItem(ship);
    }

}
}