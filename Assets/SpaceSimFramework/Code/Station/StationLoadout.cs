using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Contains specifications of a certain station type
/// </summary>
[CreateAssetMenu(menuName = "DataHolders/StationLoadout")]
public class StationLoadout: ScriptableObject
{
    [Tooltip("Station type name")]
    public string ModelName;
    [Tooltip("Owner faction of the station")]
    public Faction faction;

    [Tooltip("Equipment items for sale.")]
    public Equipment[] EquipmentForSale;
    [Tooltip("Weapons for sale.")]
    public WeaponData[] WeaponsForSale;

    [Tooltip("Is the cargo dealer available on this station type")]
    public bool HasCargoDealer = true;

    [Tooltip("Ships for sale on this station. If empty or null, no ship dealer on station")]
    public GameObject[] ShipsForSale;


    /// <summary>
    /// Gives a specific loadout to the specified ship
    /// </summary>
    /// <param name="loadout">The loadout to be applied on the station</param>
    /// <param name="station">The station to receive the loadout</param>
    public static void ApplyLoadoutToStation(StationLoadout loadout, Station station)
    {
        if (loadout == null)
            return;

        if (station.Loadout.ModelName != loadout.ModelName)
        {
            Debug.LogWarning("Warning: Trying to apply " + loadout.ModelName +
                " loadout to " + station.Loadout.ModelName);
            return;
        }

        station.faction = loadout.faction;

        StationDealer dealer = station.GetComponent<StationDealer>();

        station.HasCargoDealer = loadout.HasCargoDealer;

        dealer.EquipmentForSale = loadout.EquipmentForSale;
        dealer.WeaponsForSale = loadout.WeaponsForSale;

        station.HasShipDealer = loadout.ShipsForSale.Length > 0;
        if (station.HasShipDealer)
            dealer.ShipsForSale = loadout.ShipsForSale;

    }

    public static StationLoadout GetLoadoutByName(string name)
    {
        return Resources.Load<StationLoadout>("Stations/"+name);
    }
}
}