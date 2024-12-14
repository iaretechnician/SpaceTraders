using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

namespace SpaceSimFramework
{
public class LoadGame {

    public static Vector2 GetCurrentSavedSector()
    {
        BinaryFormatter formatter = new BinaryFormatter();

        if (ProfileMenuController.PLAYER_PROFILE == "undefined")
        {
            Debug.LogWarning("Player profile is not set! Please use the Main Menu to start the game");
        }

        string AUTOSAVE_FILE = Utils.PROFILE_FOLDER + "/Autosave";

        if (!File.Exists(AUTOSAVE_FILE))
        {
            Debug.Log("Autosave doesn't exist, starting new game!");
            return Vector2.zero;
        }
        else
        {
            FileStream stream = new FileStream(AUTOSAVE_FILE, FileMode.Open);

            SerializablePlayerData data = formatter.Deserialize(stream) as SerializablePlayerData;
            stream.Close();

            return data.CurrentSector;
        }
    }

    public static void LoadAutosave()
    {
        BinaryFormatter formatter = new BinaryFormatter();

        if (ProfileMenuController.PLAYER_PROFILE == "undefined")
        {
            Debug.LogWarning("Tried to load autosave but profile was not found!");
        }

        string AUTOSAVE_FILE = Utils.PROFILE_FOLDER + "/Autosave";
        if (!File.Exists(AUTOSAVE_FILE))
        {
            Debug.Log("Autosave file doesn't exist!");
            return;
        }

        FileStream stream = new FileStream(AUTOSAVE_FILE, FileMode.Open);

        SerializablePlayerData data = formatter.Deserialize(stream) as SerializablePlayerData;
        stream.Close();

        Player.Instance.Name = data.Name;
        Progression.Level = data.Rank;
        Progression.Experience = data.Experience;
        Player.Instance.Credits = (int)data.Credits;
        SectorNavigation.ChangeSector(data.CurrentSector, false);

        // Parse and set reputation
        Faction PF = Player.Instance.PlayerFaction;
        int r = 0;
        for (int i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
        {
            if (ObjectFactory.Instance.Factions[i] != Player.Instance.PlayerFaction)
            {
                ObjectFactory.Instance.Factions[i].cache[PF] = data.Reputation[r];
                PF.cache[ObjectFactory.Instance.Factions[i]] = data.Reputation[r++];
            }
        }

        Player.Instance.Kills = new Dictionary<Faction, Vector2>();
        for (int i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
        {
            Player.Instance.Kills.Add(ObjectFactory.Instance.Factions[i], data.Kills[i]);
        }

        // Mission loading
        if(data.Mission != null)
        {
            // Generic mission data
            Faction employer = ObjectFactory.Instance.GetFactionFromName(data.Mission.Employer);
            int payout = data.Mission.Payout;
            float timestamp = data.Mission.TimeStarted;
            Vector2 sector = data.Mission.Sector;

            if (data.Mission.Type == Mission.JobType.Assassinate.ToString())
            {
                MissionControl.CurrentJob = new Assassination(employer, payout, timestamp, sector);
            }
            else if (data.Mission.Type == Mission.JobType.Patrol.ToString())
            {
                MissionControl.CurrentJob = new Patrol(employer, payout, timestamp, sector);

                SerializableVector2 killCount = (SerializableVector2)data.Mission.GetData("KillCount");
                ((Patrol)MissionControl.CurrentJob).Kills = new Vector2(killCount.x, killCount.y);
            }
            else if (data.Mission.Type == Mission.JobType.CargoDelivery.ToString())
            {
                MissionControl.CurrentJob = new CargoDelivery(employer, payout, timestamp, sector);

                CargoDelivery job = ((CargoDelivery)MissionControl.CurrentJob);
                job.StationID = (string)data.Mission.GetData("StationID");
                job.Station = sector == SectorNavigation.CurrentSector ? SectorNavigation.GetStationByID(job.StationID) : null;
                job.Amount = (int)data.Mission.GetData("Amount");
                job.Ware = (string)data.Mission.GetData("Ware");
            }
            else if (data.Mission.Type == Mission.JobType.Courier.ToString())
            {
                MissionControl.CurrentJob = new Courier(employer, payout, timestamp, sector);

                Courier job = ((Courier)MissionControl.CurrentJob);
                job.StationID = (string)data.Mission.GetData("StationID");
                job.Station = sector == SectorNavigation.CurrentSector ? SectorNavigation.GetStationByID(job.StationID) : null;
                job.Amount = (int)data.Mission.GetData("Amount");
                job.Ware = (string)data.Mission.GetData("Ware");
            }
            MissionControl.CurrentJob.Duration = data.Mission.Duration;
        }

        GameObject shipObj;
        Vector3 position;
        Quaternion rotation;
        Player.ShipDescriptor shipOOS;

        // Load Ships
        foreach (var shipModel in data.Ships)
        {
            // Spawn ship in-sector
            if (shipModel.Sector == SectorNavigation.CurrentSector)
            {
                position = shipModel.Position;
                rotation = Quaternion.Euler(shipModel.Rotation);
                shipObj = GameObject.Instantiate(ObjectFactory.Instance.GetShipByName(shipModel.Model),
                    position, rotation, Player.Instance.transform);

                Ship ship = shipObj.GetComponent<Ship>();
                ship.faction = Player.Instance.PlayerFaction;
                ship.Armor = shipModel.Armor;
                ship.IsPlayerControlled = shipModel.IsPlayerShip;
                DockShipToStaton(ship, shipModel.StationDocked);

                // Weapons
                GunHardpoint hardpoint;
                int w_i = 0;
                foreach (string weaponName in shipModel.Guns)
                {
                    hardpoint = ship.Equipment.Guns[w_i];
                    hardpoint.SetWeapon(ObjectFactory.Instance.GetWeaponByName(weaponName));
                    w_i++;
                }
                w_i = 0;
                foreach (string weaponName in shipModel.Turrets)
                {
                    hardpoint = ship.Equipment.Turrets[w_i];
                    hardpoint.SetWeapon(ObjectFactory.Instance.GetWeaponByName(weaponName));
                    w_i++;
                }

                // Equipment
                int item_i = 0;
                foreach (string itemName in shipModel.Equipment)
                {
                    ship.Equipment.MountEquipmentItem(ObjectFactory.Instance.GetEquipmentByName(itemName));
                    item_i++;
                }

                // Cargo
                ship.ShipCargo.RemoveCargo();
                foreach (SerializableCargoItem cargoItem in shipModel.Cargo)
                {
                    ship.ShipCargo.AddWare((HoldItem.CargoType)Enum.Parse(typeof(HoldItem.CargoType), cargoItem.Type),
                        cargoItem.Item, cargoItem.Amount);
                }

                if (ship.IsPlayerControlled)
                {
                    Camera.main.GetComponent<CameraController>().SetTargetShip(ship);
                    Ship.PlayerShip = ship;
                    if (ship.transform.position == Vector3.zero && SectorNavigation.PreviousSector != null)
                    {
                        foreach (GameObject jg in GameObject.FindGameObjectsWithTag("Jumpgate"))
                        {
                            if (jg.GetComponent<Jumpgate>().NextSector == SectorNavigation.PreviousSector)
                            {
                                ship.transform.position = jg.GetComponent<Jumpgate>().SpawnPos.position;
                                break;
                            }

                        }
                    }
                    EquipmentIconUI.Instance.SetIconsForShip(ship);
                }
            }
            else
            {
                // Spawn ship out-of-sector
                shipOOS = new Player.ShipDescriptor();

                shipOOS.Armor = shipModel.Armor;
                shipOOS.ModelName = shipModel.Model;
                shipOOS.Sector = shipModel.Sector;
                shipOOS.StationDocked = shipModel.StationDocked;
                shipOOS.Position = shipModel.Position;
                shipOOS.Rotation = Quaternion.Euler(shipModel.Rotation);
                
                int w_i = 0;
                shipOOS.Guns = new WeaponData[shipModel.Guns.Length];
                foreach (string weaponName in shipModel.Guns)
                {
                    shipOOS.Guns[w_i++] = ObjectFactory.Instance.GetWeaponByName(weaponName);
                }
                w_i = 0;
                shipOOS.Turrets = new WeaponData[shipModel.Turrets.Length];
                foreach (string weaponName in shipModel.Turrets)
                {
                    shipOOS.Turrets[w_i++] = ObjectFactory.Instance.GetWeaponByName(weaponName);
                }

                shipOOS.MountedEquipment = new Equipment[shipModel.Equipment.Length];
                int item_i = 0;
                foreach (string eqItem in shipModel.Equipment)
                {
                    shipOOS.MountedEquipment[item_i++] = ObjectFactory.Instance.GetEquipmentByName(eqItem);
                }

                shipOOS.CargoItems = new HoldItem[shipModel.Cargo.Count];
                int cargo_i = 0;
                foreach (SerializableCargoItem cargoitem in shipModel.Cargo)
                {
                    shipOOS.CargoItems[cargo_i++] = new HoldItem(cargoitem.Type, cargoitem.Item, cargoitem.Amount);
                }

                Player.Instance.OOSShips.Add(shipOOS);
            }

        }

    }

    /// <summary>
    /// Spawns the ship as docked to the station
    /// </summary>
    /// <param name="ship"></param>
    /// <param name="stationID"></param>
    private static void DockShipToStaton(Ship ship, string stationID)
    {
        ship.StationDocked = stationID;
        if (stationID == "none")
            return;

        // Find station with name
        var dockables = GameObject.FindGameObjectsWithTag("Station");
        foreach(GameObject obj in dockables)
        {
            if(obj.name == stationID)
            {
                // If station is found, dock ship to station
                obj.GetComponent<Station>().ForceDockShip(ship);
            }
        }
    }

    public static void LoadPlayerKnowledge()
    {
        Dictionary<SerializableVector2, SerializableSectorData> data = 
            (Dictionary<SerializableVector2, SerializableSectorData>)Utils.LoadBinaryFile(Utils.KNOWLEDGE_FILE);
        if (data == null)
        {
            Debug.LogWarning("Tried to load knowledge from "+Utils.KNOWLEDGE_FILE+" but file was not found!");
            data = new Dictionary<SerializableVector2, SerializableSectorData>();
        }

        UniverseMap.Knowledge = data;
    }

    private static void ParseReputation(string rep)
    {
        string[] relations = rep.Split(' ');
        int r = 0;
        Faction PF = Player.Instance.PlayerFaction;
        for (int i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
        {
            if (ObjectFactory.Instance.Factions[i] != Player.Instance.PlayerFaction)
            {
                ObjectFactory.Instance.Factions[i].cache[PF] = float.Parse(relations[r]);
                PF.cache[ObjectFactory.Instance.Factions[i]] = float.Parse(relations[r++]);
            }
        }
    }

    private static void ParseKills(string kills)
    {
        string[] relations = kills.Split(' ');
        int fighterKills = 0, capKills = 0;

        Player.Instance.Kills = new Dictionary<Faction, Vector2>();
        for (int i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
        {
            fighterKills = Int32.Parse(relations[i].Split('-')[0]);
            capKills = Int32.Parse(relations[i].Split('-')[1]);
            Player.Instance.Kills.Add(ObjectFactory.Instance.Factions[i], new Vector2(fighterKills, capKills));
        }
    }

}
}