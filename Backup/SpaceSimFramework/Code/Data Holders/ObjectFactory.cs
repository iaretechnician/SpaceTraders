using SpaceSimFramework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataHolders/ObjectFactory")]
public class ObjectFactory : SingletonScriptableObject<ObjectFactory> {

    public GameObject WaypointPrefab;
    public GameObject JumpGatePrefab;
    public GameObject AsteroidFieldPrefab;
    public GameObject NebulaPrefab;

    public Faction[] Factions;

    public GameObject[] Stations;
    public GameObject[] Ships;
    public WeaponData[] Weapons;
    public Equipment[] Equipment;
    public GameObject[] Wrecks;

    private Dictionary<string, GameObject> shipPrefabs;
    private Dictionary<string, GameObject> stationPrefabs;
    private Dictionary<string, WeaponData> weaponPrefabs;
    private Dictionary<string, Equipment> equipmentPrefabs;

    private void Awake()
    {
        shipPrefabs = new Dictionary<string, GameObject>();
        weaponPrefabs = new Dictionary<string, WeaponData>();

        foreach (GameObject ShipPrefab in Ships)
            shipPrefabs.Add(ShipPrefab.GetComponent<Ship>().ShipModelInfo.ModelName, ShipPrefab);
        foreach (WeaponData WeaponPrefab in Weapons)
            weaponPrefabs.Add(WeaponPrefab.name, WeaponPrefab);
    }

    public GameObject GetShipByName(string shipName)
    {
        if(shipPrefabs == null)
        {
            shipPrefabs = new Dictionary<string, GameObject>();
            foreach (GameObject ShipPrefab in Ships)
                shipPrefabs.Add(ShipPrefab.GetComponent<Ship>().ShipModelInfo.ModelName, ShipPrefab);
        }

        if (shipPrefabs.ContainsKey(shipName))
            return shipPrefabs[shipName];
        else {
            Debug.LogError("Ship " + shipName + " not found in ObjectFactory!");
            return null;
        }
    }

    public GameObject GetStationByName(string stationName)
    {
        if (stationPrefabs == null)
        {
            stationPrefabs = new Dictionary<string, GameObject>();
            foreach (GameObject StationPrefab in Stations)
                stationPrefabs.Add(StationPrefab.name, StationPrefab);
        }

        if (stationPrefabs.ContainsKey(stationName))
            return stationPrefabs[stationName];
        else
        {
            Debug.LogError("Station " + stationName + " not found in ObjectFactory!");
            return null;
        }
    }

    public WeaponData GetWeaponByName(string weaponName)
    {
        if (weaponName == null || weaponName=="")
            return null;

        if (weaponPrefabs == null)
        {
            weaponPrefabs = new Dictionary<string, WeaponData>();
            foreach (WeaponData WeaponPrefab in Weapons)
                weaponPrefabs.Add(WeaponPrefab.name, WeaponPrefab);
        }

        if (weaponPrefabs.ContainsKey(weaponName))
            return weaponPrefabs[weaponName];
        else
            return null;
    }

    public Equipment GetEquipmentByName(string itemName)
    {
        if (itemName == null || itemName == "")
            return null;

        if (equipmentPrefabs == null)
        {
            equipmentPrefabs = new Dictionary<string, Equipment>();
            foreach (Equipment equipmentPrefab in Equipment) {
                equipmentPrefabs.Add(equipmentPrefab.name, equipmentPrefab);
            }
        }

        if (equipmentPrefabs.ContainsKey(itemName))
            return equipmentPrefabs[itemName];
        else
            return null;
    }

    public Faction GetFactionFromName(string name)
    {
        foreach (Faction f in Factions)
        {
            if (name == f.name)
                return f;
        }

        return null;
    }

    public GameObject GetWreckByName(string name)
    {
        foreach (GameObject wreckPrefab in Wrecks)
        {
            if (name == wreckPrefab.name)
                return wreckPrefab;
        }

        return null;
    }

    /// <summary>
    ///  Static version of the GetFactionFromName function
    /// </summary>
    public static Faction GetFactionFromName(string name, Faction[] factions)
    {
        foreach (Faction f in factions)
        {
            if (name == f.name)
                return f;
        }

        return null;
    }
}
