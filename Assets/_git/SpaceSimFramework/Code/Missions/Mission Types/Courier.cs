using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Take cargo from the station on which the mission was taken to another station.
/// </summary>
public class Courier : Mission
{
    public GameObject Station;
    public string StationID;
    public string Ware;
    public int Amount;

    // Constructors
    public Courier(Faction employer) : base(employer)
    {
        Employer = employer;
        _type = JobType.Courier;
    }

    public Courier(Faction employer, int payout, float timestamp, Vector2 sector)
        : base(employer, payout, timestamp, sector)
    {
        _type = JobType.Courier;
        Employer = employer;
        Payout = payout;
        TimeStarted = timestamp;
        Sector = sector;
    }

    private string GetKnownStationIDForSector(Vector2 sectorName)
    {
        // Sector not known?
        if (!UniverseMap.Knowledge.ContainsKey(sectorName))
            return null;
        // No known stations in sector?
        int numOfStations = UniverseMap.Knowledge[sectorName].Stations.Count;
        if (numOfStations == 0)
            return null;
        var stations = UniverseMap.Knowledge[sectorName].Stations;

        // Pick a random station
        return stations[Random.Range(0, numOfStations)].ID;
    }

    public override void OnTimeRanOut()
    {
        if (Amount > 0)
        {
            ConsoleOutput.PostMessage("Mission failed! Time has ran out!", Color.blue);
            MissionUI.Instance.panel.SetActive(false);
            MissionControl.CurrentJob = null;
            TextFlash.ShowYellowText("Mission failed!");
        }
        else
        {
            FinishJob();
        }
    }

    public void FinishJob()
    {
        ConsoleOutput.PostMessage("Mission completed! Your payment of " + Payout + " credits will be transferred now.", Color.blue);
        Progression.MissionCompleted();
        Player.Instance.Credits += Payout;
        MissionUI.Instance.panel.SetActive(false);
        MissionControl.CurrentJob = null;
        TextFlash.ShowYellowText("Mission completed successfully!");
    }

    public override void RegisterKill(Ship kill)
    {
    }

    protected override string GetStartingMessage()
    {
        if (SectorNavigation.CurrentSector != Sector)
            return "Proceed to the " + Sector + " sector to complete your assignment!";
        else
            return "Mission accepted! You have " + Duration / 60 +
                " minutes to bring " + Amount + " of " + Ware + " to " + Station.name;
    }

    public override bool GenerateMissionData()
    {
        // Get payout, duration and sector
        base.GenerateMissionData();

        if (Ship.PlayerShip.StationDocked == "none")
            Debug.LogError("Courier mission taken but player ship is not docked!");

        StationID = GetRandomKnownStation();
        if (StationID == null)
        {   // Mission must be terminated because no suitable destination is found
            return false;
        }

        // Get ware to bring, but we dont know which ones are available on the station
        var wares = Commodities.Instance.CommodityTypes;
        {
            Ware = Commodities.Instance.CommodityTypes[Random.Range(0, Commodities.Instance.NumberOfWares)].Name;
        }

        // Get amount
        Amount = (int)(Ship.PlayerShip.ShipCargo.CargoSize * Mathf.Clamp(Random.value, 0.1f, 1.0f));

        return true;
    }

    /// <summary>
    /// Gets a random station in the close proximity of the player's position
    /// </summary>
    /// <returns></returns>
    private string GetRandomKnownStation()
    {
        // TODO uses only adjacent sectors
        List<GameObject> jumpgates = new List<GameObject>(SectorNavigation.Instance.GetJumpgates());
        string stationID = null;

        int i = 0;
        do
        {
            if (i >= jumpgates.Count)
                return stationID;

            Sector = jumpgates[i].GetComponent<Jumpgate>().NextSector;
            stationID = GetKnownStationIDForSector(Sector);

            i++;
        } while (stationID == null);

        return stationID;
    }
}
}