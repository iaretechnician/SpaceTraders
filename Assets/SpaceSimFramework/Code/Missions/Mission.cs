using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Generic mission data class with all mission properties shared by all mission types.
/// These are: mission type, employer faction, payout in credits, sector in which mission
/// can be completed, mission time limit and time started.
/// </summary>
public abstract class Mission {

    public enum JobType { 
        Patrol, CargoDelivery, Courier, Assassinate, Mining
    }

    public Faction Employer;
    public JobType Type {
        get { return _type; }
    }
    protected JobType _type;
    public int Payout;
    public Vector2 Sector;

    // Duration and time
    public float TimeStarted;
    public float Duration;

    // Constructors
    public Mission(Faction employer)
    {
        Employer = employer;
    }

    public Mission(Faction employer, int payout, float timestamp, Vector2 sector)
    {
        Employer = employer;
        Payout = payout;
        TimeStarted = timestamp;
        Sector = sector;
    }

    /// <summary>
    /// Invoked to initialize mission specific data when the mission is accepted by player.
    /// </summary>
    public virtual void OnMissionStarted()
    {
        ConsoleOutput.PostMessage(GetStartingMessage(), Color.blue);

        if (SectorNavigation.CurrentSector != Sector)
            ConsoleOutput.PostMessage("Proceed to the " + Sector + " sector to complete your assignment!", Color.blue);

        MissionUI.Instance.panel.SetActive(true);
        MissionUI.Instance.SetDescriptionText(GetStartingMessage());
        MissionUI.Instance.Populate(this);
    }

    /// <summary>
    /// Called in an update loop to update mission timer and other info.
    /// </summary>
    public virtual void OnMissionUpdate()
    {
        if (Time.time - TimeStarted > Duration)
        {
            OnTimeRanOut();
        }
    }

    public float GetRemainingTime()
    {
        // Remaining = total - elapsed
        return Duration - (Time.time - TimeStarted);
    }

    /// <summary>
    /// If this mission has a time limit this should terminate it.
    /// </summary>
    public abstract void OnTimeRanOut();

    /// <summary>
    /// Some missions may not be about kills but shooting down a friendly ship should result in
    /// mission failure.
    /// </summary>
    /// <param name="kill">What you've blown up</param>
    public abstract void RegisterKill(Ship kill);

    protected abstract string GetStartingMessage();

    /// <summary>
    /// Populates the mission properties with randomly generated data.
    /// </summary>
    /// <returns>Whether mission was created</returns>
    public virtual bool GenerateMissionData() {
        // Give random payout 
        Payout =  (int)(Random.Range(0.5f, 5f) * 10000f);        // [5 000, 50 000] credits
        Duration = Random.Range(3, 8) * 60;
        Sector = SectorNavigation.CurrentSector;

        return true;
    }
}
}