using System.Collections.Generic;
using UnityEngine;
using StationWares = SpaceSimFramework.StationContentGenerator.StationWares;

namespace SpaceSimFramework
{
public class StationDealer : MonoBehaviour {
    [HideInInspector]
    public Dictionary<string, int> WarePrices;
    [HideInInspector]
    public WeaponData[] WeaponsForSale;
    [HideInInspector]
    public Equipment[] EquipmentForSale;
    [HideInInspector]
    public GameObject[] ShipsForSale;

    /// <summary>
    /// Generate cargo wares sold by the station
    /// </summary>
    /// <returns>A class containing all station data</returns>
    public StationWares GenerateStationData()
    {
        StationWares stationWares = new StationWares
        {
            StationID = GetComponent<Station>().ID
        };

        // Two wares will be unavailable at each station - this is for the CargoDelivery mission (check the GetMissionData method)
        Vector2 unavailableWareIndices = new Vector2(Random.Range(0, Commodities.Instance.NumberOfWares), Random.Range(0, Commodities.Instance.NumberOfWares));
        int price;
        stationWares.WaresForSale = new Dictionary<string, int>();

        for (int i = 0; i < Commodities.Instance.CommodityTypes.Count; i ++)
        {
            if (unavailableWareIndices.x == i || unavailableWareIndices.y == i)
                continue;

            price = Random.Range(Commodities.Instance.CommodityTypes[i].MinPrice, Commodities.Instance.CommodityTypes[i].MaxPrice);

            stationWares.WaresForSale.Add(Commodities.Instance.CommodityTypes[i].Name, price);
        }
        WarePrices = stationWares.WaresForSale;

        return stationWares;
    }
}
}