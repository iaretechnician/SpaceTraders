using SpaceSimFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializablePlayerData
{
    // General player info
    public string Name;
    public long Credits;
    public int Rank;
    public int Experience;
    public SerializableVector2 CurrentSector;
    public SerializableVector2[] Kills;
    public float[] Reputation;

    // Mission data
    public SerializableMissionData Mission;

    // Ships
    public List<SerializablePlayerShip> Ships;
}

[Serializable]
public class SerializableMissionData
{
    public string Type;
    public float Duration;
    public string Employer;
    public int Payout;
    public float TimeStarted;
    public SerializableVector2 Sector;
    public List<KeyValuePair<String, object>> JobData;

    public object GetData(string key)
    {
        foreach(var pair in JobData)
        {
            if (pair.Key == key)
                return pair.Value;
        }

        return null;
    }

    public static SerializableMissionData FromMission()
    {
        if (MissionControl.CurrentJob == null)
            return null;

        SerializableMissionData data = new SerializableMissionData();

        data = new SerializableMissionData();
        data.Type = MissionControl.CurrentJob.Type.ToString();
        data.Duration = MissionControl.CurrentJob.GetRemainingTime();
        data.Employer = MissionControl.CurrentJob.Employer.name;
        data.Payout = MissionControl.CurrentJob.Payout;
        data.TimeStarted = MissionControl.CurrentJob.TimeStarted;
        data.Sector = MissionControl.CurrentJob.Sector;

        data.JobData = new List<KeyValuePair<String, object>>();
        if (MissionControl.CurrentJob.Type == Mission.JobType.Patrol)
        {
            data.JobData.Add(new KeyValuePair<string, object>(
                "KillCount",
                new SerializableVector2(((Patrol)MissionControl.CurrentJob).Kills.x, ((Patrol)MissionControl.CurrentJob).Kills.y))
            );
        }
        else if (MissionControl.CurrentJob.Type == Mission.JobType.CargoDelivery)
        {
            data.JobData.Add(new KeyValuePair<string, object>(
                "StationID", ((CargoDelivery)MissionControl.CurrentJob).StationID));
            data.JobData.Add(new KeyValuePair<string, object>(
                "Ware", ((CargoDelivery)MissionControl.CurrentJob).Ware));
            data.JobData.Add(new KeyValuePair<string, object>(
                "Amount", ((CargoDelivery)MissionControl.CurrentJob).Amount));
        }
        else if (MissionControl.CurrentJob.Type == Mission.JobType.Courier)
        {
            data.JobData.Add(new KeyValuePair<string, object>(
                "StationID", ((Courier)MissionControl.CurrentJob).StationID));
            data.JobData.Add(new KeyValuePair<string, object>(
                "Ware", ((Courier)MissionControl.CurrentJob).Ware));
            data.JobData.Add(new KeyValuePair<string, object>(
                "Amount", ((Courier)MissionControl.CurrentJob).Amount));
        }

        return data;
    }
}

[Serializable]
public class SerializablePlayerShip
{
    public string Model;
    public bool IsPlayerShip;
    public SerializableVector2 Sector;
    public string StationDocked;
    public float Armor;
    public SerializableVector3 Position;
    public SerializableVector3 Rotation;

    // Weapons
    public string[] Guns;
    public string[] Turrets;

    // Cargo
    public List<SerializableCargoItem> Cargo;

    // Equipment
    public string[] Equipment;

    /// <summary>
    /// Returns the serializable representation of ship data. The nextSector variable is passed because
    /// if the player ship jumps, the game is saved and the player's position needs to be saved as the next system.
    /// </summary>
    public static SerializablePlayerShip FromShip(Ship ship, Vector2 nextSector)
    {
        SerializablePlayerShip data = new SerializablePlayerShip();

        data.Model = ship.ShipModelInfo.ModelName;
        data.IsPlayerShip = ship == Ship.PlayerShip;
        data.StationDocked = ship.StationDocked;
        data.Armor = ship.Armor;
        data.Position = ship.transform.position;
        data.Rotation = ship.transform.rotation.eulerAngles;
        // Save the sector of this ship as "nextSector" if player ship just jumped
        data.Sector = data.IsPlayerShip && nextSector != SectorNavigation.UNSET_SECTOR ? nextSector : SectorNavigation.CurrentSector;

        // Add weapons
        int w_i = 0;
        data.Guns = new string[ship.Equipment.Guns.Count];
        foreach (GunHardpoint hardpoint in ship.Equipment.Guns)
        {
            data.Guns[w_i++] = (hardpoint.mountedWeapon != null) ? hardpoint.mountedWeapon.name : "";
        }
        w_i = 0;
        data.Turrets = new string[ship.Equipment.Turrets.Count];
        foreach (GunHardpoint hardpoint in ship.Equipment.Turrets)
        {
            data.Turrets[w_i++] = (hardpoint.mountedWeapon != null) ? hardpoint.mountedWeapon.name : "";
        }

        // Add equipment
        data.Equipment = new String[ship.Equipment.MountedEquipment.Count];
        w_i = 0;
        foreach (Equipment item in ship.Equipment.MountedEquipment)
        {
            data.Equipment[w_i++] = item.name;
        }

        // Add cargo
        data.Cargo = new List<SerializableCargoItem>();
        foreach (HoldItem cargoItem in ship.ShipCargo.CargoContents)
        {
            SerializableCargoItem cargoModel = new SerializableCargoItem();
            cargoModel.Type = cargoItem.cargoType.ToString();
            cargoModel.Item = cargoItem.itemName;
            cargoModel.Amount = cargoItem.amount;
            data.Cargo.Add(cargoModel);
        }

        return data;
    }

    public static SerializablePlayerShip FromOOSShip(Player.ShipDescriptor OOSShip)
    {
        SerializablePlayerShip shipModel = new SerializablePlayerShip();
        shipModel.Model = OOSShip.ModelName;
        shipModel.IsPlayerShip = false;
        shipModel.Sector = OOSShip.Sector;
        shipModel.StationDocked = OOSShip.StationDocked;
        shipModel.Armor = OOSShip.Armor;
        shipModel.Position = new SerializableVector3(OOSShip.Position);
        shipModel.Rotation = new SerializableVector3(OOSShip.Position);

        // Add weapons
        int w_i = 0;
        shipModel.Guns = new string[OOSShip.Guns.Length];
        foreach (WeaponData weapon in OOSShip.Guns)
        {
            shipModel.Guns[w_i++] = (weapon != null) ? weapon.name : "";
        }
        w_i = 0;
        shipModel.Turrets = new string[OOSShip.Turrets.Length];
        foreach (WeaponData weapon in OOSShip.Turrets)
        {
            shipModel.Turrets[w_i++] = (weapon != null) ? weapon.name : "";
        }

        // Add equipment
        if(OOSShip.MountedEquipment != null)
        {
            shipModel.Equipment = new String[OOSShip.MountedEquipment.Length];
            w_i = 0;
            foreach (Equipment item in OOSShip.MountedEquipment)
            {
                shipModel.Equipment[w_i++] = (item != null) ? item.name : "";
            }
        }
        else
        {
            shipModel.Equipment = new String[0];
        }

        // Add cargo
        shipModel.Cargo = new List<SerializableCargoItem>();
        foreach (HoldItem cargoItem in OOSShip.CargoItems)
        {
            if (cargoItem == null) continue;
            SerializableCargoItem cargoModel = new SerializableCargoItem();
            cargoModel.Type = cargoItem.cargoType.ToString();
            cargoModel.Item = cargoItem.itemName;
            cargoModel.Amount = cargoItem.amount;
            shipModel.Cargo.Add(cargoModel);
        }

        return shipModel;
    }
}

[Serializable]
public struct SerializableCargoItem
{
    public string Type;
    public string Item;
    public int Amount;
}

[Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }

    public SerializableVector3(Vector3 rValue)
    {
        x = rValue.x;
        y = rValue.y;
        z = rValue.z;
    }

    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}]", x, y, z);
    }

    public static implicit operator Vector3(SerializableVector3 rValue)
    {
        return new Vector3(rValue.x, rValue.y, rValue.z);
    }

    public static implicit operator SerializableVector3(Vector3 rValue)
    {
        return new SerializableVector3(rValue.x, rValue.y, rValue.z);
    }
}

[Serializable]
public struct SerializableVector2
{
    public float x;
    public float y;

    public SerializableVector2(float rX, float rY)
    {
        x = rX;
        y = rY;
    }

    public override string ToString()
    {
        return String.Format("[{0}, {1}]", x, y);
    }

    public static implicit operator Vector2(SerializableVector2 rValue)
    {
        return new Vector2(rValue.x, rValue.y);
    }

    public static implicit operator SerializableVector2(Vector2 rValue)
    {
        return new SerializableVector2(rValue.x, rValue.y);
    }
}

[Serializable]
public struct SerializableColor
{
    public float r;
    public float g;
    public float b;
    public float a;

    public SerializableColor(float r, float g, float b, float a)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}, {3}]", r, g, b, a);
    }

    public static implicit operator Color(SerializableColor rValue)
    {
        return new Color(rValue.r, rValue.g, rValue.b, rValue.a);
    }

    public static implicit operator SerializableColor(Color rValue)
    {
        return new SerializableColor(rValue.r, rValue.g, rValue.b, rValue.a);
    }
}

[Serializable]
public class SerializableKeyValuePair<K, V>
{
    public SerializableKeyValuePair(K key, V value)
    {
        Key = key;
        Value = value;
    }

    public K Key { get; set; }
    public V Value { get; set; }
}