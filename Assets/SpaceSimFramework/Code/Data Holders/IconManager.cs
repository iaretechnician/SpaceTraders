using UnityEngine;
using System;
using System.Collections.Generic;

namespace SpaceSimFramework
{
[CreateAssetMenu(menuName = "DataHolders/Icon Manager")]
public class IconManager : ScriptableObject
{
    [Serializable]
    public struct NamedIcon
    {
        public string name;
        public Sprite icon;
    }

    public Sprite Placeholder;
    public NamedIcon[] Wares;
    public NamedIcon[] Equipment;
    public NamedIcon[] Ships;
    public NamedIcon[] Missions;
    public Sprite[] Weapons;
    public NamedIcon[] MarkerIcons;

    private Dictionary<string, Sprite> wareIcons;
    private Dictionary<string, Sprite> equipmentIcons;
    private Dictionary<string, Sprite> shipIcons;
    private Dictionary<string, Sprite> missionIcons;
    private Dictionary<string, Sprite> markerIcons;
    private static IconManager _instance;

    public enum EquipmentIcons { Gun = 0, Turret = 1, Equipment = 2 }

    public static IconManager Instance
    {
        get
        {
            if(_instance == null)
                _instance = Resources.Load<IconManager>("IconManager");

            if (_instance == null)
                Debug.LogError("ERROR: IconManager not found! Asset must be in the Resources folder!");
            return _instance;
        }
    }

    private void init()
    {
        // Create a hashmap for O(1) access
        wareIcons = new Dictionary<string, Sprite>();
        foreach(var pair in Wares)
        {
            wareIcons.Add(pair.name, pair.icon);
        }

        equipmentIcons = new Dictionary<string, Sprite>();
        foreach (var pair in Equipment)
        {
            equipmentIcons.Add(pair.name, pair.icon);
        }

        missionIcons = new Dictionary<string, Sprite>();
        foreach (var pair in Missions)
        {
            missionIcons.Add(pair.name, pair.icon);
        }

        shipIcons = new Dictionary<string, Sprite>();
        foreach (var pair in Ships)
        {
            shipIcons.Add(pair.name, pair.icon);
        }

        markerIcons = new Dictionary<string, Sprite>();
        foreach (var pair in MarkerIcons)
        {
            markerIcons.Add(pair.name, pair.icon);
        }
    }

    public Sprite GetWareIcon(string itemName)
    {
        if (wareIcons == null)
            init();

        return wareIcons.ContainsKey(itemName) && wareIcons[itemName] != null
            ? wareIcons[itemName] : Placeholder;
    }

    public Sprite GetEquipmentIcon(string itemName)
    {
        if (equipmentIcons == null)
            init();

        return equipmentIcons.ContainsKey(itemName) && equipmentIcons[itemName] != null
            ? equipmentIcons[itemName] : Placeholder;
    }

    public Sprite GetMissionIcon(string jobName)
    {
        if (missionIcons == null)
            init();

        return missionIcons.ContainsKey(jobName) && missionIcons[jobName] != null
            ? missionIcons[jobName] : Placeholder;
    }

    public Sprite GetShipIcon(string shipName)
    {
        if (shipIcons == null)
            init();

        return shipIcons.ContainsKey(shipName) && shipIcons[shipName] != null
            ? shipIcons[shipName] : Placeholder;
    }

    public Sprite GetMarkerIcon(string objectTag)
    {
        if (markerIcons == null)
            init();

        // Returning null here is desired behaviour due to different placeholder marker
        return markerIcons.ContainsKey(objectTag) && markerIcons[objectTag] != null
            ? markerIcons[objectTag] : null;
    }

    public Sprite GetWeaponIcon(int index)
    {
        return Weapons[index];
    }
}
}