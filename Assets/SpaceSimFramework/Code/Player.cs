using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Contains player data at runtime, including a list of in-sector (spawned) and 
/// out-of-sector (stored) ships. 
/// </summary>
public class Player : Singleton<Player> {

    public string Name;
    public int Credits;
    // <Faction, (Fighter Kills, Capship Kills)>
    public Dictionary<Faction, Vector2> Kills;

    public List<GameObject> Ships;
    public Faction PlayerFaction;
    public List<ShipDescriptor> OOSShips
    {
        get
        {
            if (_oosShips == null)
                _oosShips = new List<ShipDescriptor>();
            return _oosShips;
        }
    }
    private List<ShipDescriptor> _oosShips;

    // Keeps your original reputation 
    private Dictionary<Faction, float> playerRelationsBackup = null;

    /// <summary>
    /// Stores data for player owned ships in other (not player) sectors.
    /// Out-of-sector ships are not simulated.
    /// </summary>
    public class ShipDescriptor
    {
        public string ModelName;
        public Vector2 Sector;
        public string StationDocked;
        public Vector3 Position;
        public Quaternion Rotation;
        public float Armor;
        public WeaponData[] Guns;
        public WeaponData[] Turrets;
        public Equipment[] MountedEquipment;
        public HoldItem[] CargoItems;
    }

    void Start () {

        var ships = GameObject.FindGameObjectsWithTag("Ship");

        Ships = new List<GameObject>();
        foreach (GameObject ship in ships)
        {
            if (ship.GetComponent<Ship>().faction == PlayerFaction)
                Ships.Add(ship);
        }

        if (Kills == null)
        {
            Kills = new Dictionary<Faction, Vector2>();
            foreach (Faction f in ObjectFactory.Instance.Factions)
            {
                Kills.Add(f, Vector2.zero);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                SaveGame.Save(SectorNavigation.UNSET_SECTOR);
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                DisplayFactionRelations();
            }
        }
    }

    #region faction relations

    public float factionPenalty = 0.2f, dampeningFactor = 4f;

    /// <summary>
    /// Decrease rep with faction.
    /// </summary>
    public float AddFactionPenalty(Faction otherfaction)
    {
        float rep = PlayerFaction.RelationWith(otherfaction);

        SetTemporaryFactionHostility(otherfaction);

        rep = Mathf.Clamp(rep - factionPenalty, -1, 1);
        if (playerRelationsBackup != null)
        {
            playerRelationsBackup[otherfaction] = Mathf.Clamp(playerRelationsBackup[otherfaction] - factionPenalty, -1, 1);
        }

        for (int i=0; i<ObjectFactory.Instance.Factions.Length; i++)
        {
            Faction currFac = ObjectFactory.Instance.Factions[i];
            if (currFac == PlayerFaction || currFac == otherfaction)
                continue;

            float relationWithOtherFaction = otherfaction.RelationWith(currFac);
            float repChange = (factionPenalty * -relationWithOtherFaction) / dampeningFactor;
            currFac.cache[PlayerFaction] = Mathf.Clamp(currFac.RelationWith(PlayerFaction) + repChange, -1, 1);
            PlayerFaction.cache[currFac] = Mathf.Clamp(PlayerFaction.RelationWith(currFac) + repChange, -1, 1);
            if(playerRelationsBackup != null)
            {
                playerRelationsBackup[currFac] = Mathf.Clamp(playerRelationsBackup[currFac] + repChange, -1, 1);
            }
        }

        //DisplayFactionRelations();
        return rep;
    }


    public void SetTemporaryFactionHostility(Faction otherfaction)
    {
        // Copy relations, but dont overwrite 
        if(playerRelationsBackup == null)
        {
            playerRelationsBackup = new Dictionary<Faction, float>();
            foreach (Faction f in ObjectFactory.Instance.Factions)
            {
                playerRelationsBackup.Add(f, PlayerFaction.RelationWith(f));
            }
        }

        otherfaction.cache[PlayerFaction] = PlayerFaction.cache[otherfaction] = -1;

        for (int i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
        {
            Faction currFac = ObjectFactory.Instance.Factions[i];
            if (currFac == PlayerFaction || currFac == otherfaction)
                continue;

            float relationWithOtherFaction = otherfaction.RelationWith(currFac);
            if(relationWithOtherFaction > 0) {
                // Turn it hostile too
                currFac.cache[PlayerFaction] = -1;
                PlayerFaction.cache[currFac] = -1;
            }
        }

    }

    /// <summary>
    /// Returns the space-separated faction relation coeffiecients for game saving
    /// </summary>
    /// <returns></returns>
    public string GetReputationString()
    {
        string rep = "";

        if (playerRelationsBackup == null)
        {
            for (int i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
            {
                Faction f = ObjectFactory.Instance.Factions[i];

                if (f != Player.Instance.PlayerFaction)
                    rep += PlayerFaction.RelationWith(f) + " ";
            }
        }
        else
        {
            for (int i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
            {
                Faction f = ObjectFactory.Instance.Factions[i];

                if (f != Player.Instance.PlayerFaction)
                    rep += playerRelationsBackup[f] + " ";
            }
        }

        return rep;
    }

    /// <summary>
    /// Returns the list of faction relation coeffiecients for game saving
    /// </summary>
    /// <returns></returns>
    public float[] GetReputations()
    {
        float[] rep = new float[ObjectFactory.Instance.Factions.Length-1];

        if (playerRelationsBackup == null)
        {
            for (int i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
            {
                Faction f = ObjectFactory.Instance.Factions[i];

                if (f != Player.Instance.PlayerFaction)
                    rep[i] = PlayerFaction.RelationWith(f);
            }
        }
        else
        {
            for (int i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
            {
                Faction f = ObjectFactory.Instance.Factions[i];

                if (f != Player.Instance.PlayerFaction)
                    rep[i] = PlayerFaction.RelationWith(f);
            }
        }

        return rep;
    }

    /// <summary>
    /// Debug function.
    /// </summary>
    private void DisplayFactionRelations()
    {
        string relationsText = "";

        foreach (Faction otherFaction in ObjectFactory.Instance.Factions)
        {
            relationsText += otherFaction.name+" = "+PlayerFaction.RelationWith(otherFaction)+"; ";
        }

        //Debug.Log(relationsText);
        ConsoleOutput.PostMessage(relationsText);
    }

    #endregion faction relations
}
}