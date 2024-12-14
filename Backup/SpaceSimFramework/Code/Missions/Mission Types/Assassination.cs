using UnityEngine;

namespace SpaceSimFramework
{
public class Assassination : Mission {

    public GameObject Target;

    // Constructors
    public Assassination(Faction employer) : base(employer)
    {
        Employer = employer;
        _type = JobType.Assassinate;
    }

    public Assassination(Faction employer, int payout, float timestamp, Vector2 sector) 
        : base(employer, payout, timestamp, sector)
    {
        _type = JobType.Assassinate;
        Employer = employer;
        Payout = payout;
        TimeStarted = timestamp;
        Sector = sector;
    }

    public override void OnMissionStarted()
    {
        base.OnMissionStarted();

        // Spawn a target ship in current sector, regardless
        Target = SectorNavigation.Instance.GetJumpgates()[0].
            GetComponent<ShipSpawner>().SpawnMissionTarget(ObjectFactory.Instance.Factions[1]);    // TODO - spawns Enemy faction ships

    }

    public override void OnMissionUpdate()
    {
        base.OnMissionUpdate();

        if (Target == null)
        {
            ConsoleOutput.PostMessage("Mission failed! The target ship was destroyed by someone else!", Color.blue);
            MissionControl.CurrentJob = null;
            MissionUI.Instance.panel.SetActive(false);
            TextFlash.ShowYellowText("Mission failed!");
        }
    }

    public override void OnTimeRanOut()
    {
        ConsoleOutput.PostMessage("Mission failed! Time has run out!", Color.blue);
        MissionControl.CurrentJob = null;
        MissionUI.Instance.panel.SetActive(false);
        TextFlash.ShowYellowText("Mission failed!");
    }

    public override void RegisterKill(Ship kill)
    {
        if (kill.gameObject == Target)
        {
            ConsoleOutput.PostMessage("Mission completed! Your payment of " + Payout + " credits will be transferred now.", Color.blue);
            Progression.MissionCompleted();
            Player.Instance.Credits += Payout;

            MissionUI.Instance.panel.SetActive(false);
            MissionControl.CurrentJob = null;
            TextFlash.ShowYellowText("Mission completed successfully!");

            return;
        }
    }

    protected override string GetStartingMessage()
    {
        return "Mission accepted! You have " + Duration / 60 + " minutes to destroy hostile ships.";
    }


    public override bool GenerateMissionData()
    {
        base.GenerateMissionData();

        return true;
    }
}
}