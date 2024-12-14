using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
public class StationContentGenerator : MonoBehaviour
{
    public class StationWares
    {
        public string StationID;

        public Dictionary<string, int> WaresForSale;
    }
    //private static Dictionary<Vector2, StationWares[]> _sectorStations;

    /// <summary>
    /// Ensures station dealers are populated with appropriate wares
    /// </summary>
    /// <param name="stations">List of stations in the current sector</param>
    public static void OnSectorChanged(Vector2 currentSector, List<GameObject> stations)
    {
        GenerateSectorData(currentSector, stations);     
    }

    private static void GenerateSectorData(Vector2 currentSector, List<GameObject> stations)
    {     
        for (int i = 0; i < stations.Count; i++)
        {
            stations[i].GetComponent<StationDealer>().GenerateStationData();
        }
    }

}
}