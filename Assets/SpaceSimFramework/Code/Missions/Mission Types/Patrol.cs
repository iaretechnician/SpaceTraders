using UnityEngine;

namespace SpaceSimFramework
{
public class Patrol : Mission {

    public Vector2 Kills;

    // Constructors
    public Patrol(Faction employer) : base(employer)
    {
        Employer = employer;
        _type = JobType.Patrol;
    }

    public Patrol(Faction employer, int payout, float timestamp, Vector2 sector) 
        : base(employer, payout, timestamp, sector)
    {
        _type = JobType.Patrol;
        Employer = employer;
        Payout = payout;
        TimeStarted = timestamp;
        Sector = sector;
    }

    public override void OnMissionStarted()
    {
        base.OnMissionStarted();

        // Spawn a target ship in current sector, regardless
        SectorNavigation.Instance.GetJumpgates()[0].
            GetComponent<ShipSpawner>().SpawnMissionTarget(ObjectFactory.Instance.Factions[1]);    // TODO - spawns Enemy faction ships

    }

    public override void RegisterKill(Ship kill)
    {
        if (kill.gameObject.GetComponent<Ship>().ShipModelInfo.ExternalDocking)
        {
            Kills.y++;
        }
        else
        {
            Kills.x++;
        }
        ConsoleOutput.PostMessage("Well done! Nice kill!");
        MissionUI.Instance.SetDescriptionText("Kills: "+(Kills.x + Kills.y));

    }

    public override void OnTimeRanOut()
    {
        int pay = (int)(Payout * Kills.x + Payout * Kills.y * 2);
        ConsoleOutput.PostMessage("Mission completed! Your payment of " + pay + " credits will be transferred now.", Color.blue);
        Progression.MissionCompleted();
        Player.Instance.Credits += pay;
        MissionUI.Instance.panel.SetActive(false);
        MissionControl.CurrentJob = null;
        TextFlash.ShowYellowText("Mission completed successfully!");
    }

    protected override string GetStartingMessage()
    {
        return "Mission accepted! You have " + Duration / 60 + " minutes to destroy hostile ships.";
    }

    public override bool GenerateMissionData()
    {
        base.GenerateMissionData();

        Kills = Vector2.zero;

        return true;
    }
}
}