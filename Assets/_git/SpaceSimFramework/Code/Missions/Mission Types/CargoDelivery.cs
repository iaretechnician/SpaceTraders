using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Bring ware to station at which the mission was taken
/// </summary>
public class CargoDelivery : Mission
{
    public GameObject Station;
    public string StationID;
    public string Ware;
    public int Amount;

    // Constructors
    public CargoDelivery(Faction employer) : base(employer)
    {
        Employer = employer;
        _type = JobType.CargoDelivery;
    }

    public CargoDelivery(Faction employer, int payout, float timestamp, Vector2 sector)
        : base(employer, payout, timestamp, sector)
    {
        _type = JobType.CargoDelivery;
        Employer = employer;
        Payout = payout;
        TimeStarted = timestamp;
        Sector = sector;
    }

    public override void OnTimeRanOut()
    {
        if(Amount > 0)
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
                " minutes to bring "+Amount+" of "+Ware+" to "+Station.name;
    }

    public override bool GenerateMissionData()
    {
        // Get payout, duration and sector
        base.GenerateMissionData();

        // Get player ship's station
        if (Ship.PlayerShip.StationDocked == null || Ship.PlayerShip.StationDocked == "none")
            Debug.LogError("CargoDelivery mission taken but player ship is not docked!");

        foreach (GameObject st in SectorNavigation.Stations)
            if (Ship.PlayerShip.StationDocked == st.GetComponent<Station>().ID)
            {
                Station = st;
                StationID = st.GetComponent<Station>().ID;
                break;
            }

        // Get ware to bring, not one of station sold wares
        var wares = Station.GetComponent<StationDealer>().WarePrices;
        while (true)
        {
            Ware = Commodities.Instance.CommodityTypes[Random.Range(0, Commodities.Instance.NumberOfWares)].Name;
            if (!wares.ContainsKey(Ware))
                break;
        }

        // Get amount
        Amount = (int)(Ship.PlayerShip.ShipCargo.CargoSize * Mathf.Clamp(Random.value, 0.1f, 1.0f) * 1.2f);

        return true;
    }
}
}