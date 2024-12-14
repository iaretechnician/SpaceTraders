using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Controls the job that is currently active for the player. Dictates creation of the job
/// as well handling each player kill.
/// </summary>
public class MissionControl : MonoBehaviour {

    public static Mission CurrentJob;

    private void Update()
    {
        if (CurrentJob == null)
            return;

        CurrentJob.OnMissionUpdate();
    }

    public static Mission GetNewMission(Faction employer)
    {
        Mission job = null;
        int mission_i = Random.Range(0, 4);
        if (mission_i == 0)
        {
            job = new Patrol(employer);
        }
        if (mission_i == 1)
        {
            job = new Assassination(employer);
        }
        if (mission_i == 2)
        {
            job = new CargoDelivery(employer);
        }
        if (mission_i == 3)
        {
            job = new Courier(employer);
        }

        // If job creation failed, try again (sooner or later it will succeed) 
        if(!job.GenerateMissionData())
            job = GetNewMission(employer);

        return job;
    }

    public static void RegisterKill(Ship kill)
    {
        if (CurrentJob == null)
            return;

        if (CurrentJob.Employer.RelationWith(kill.faction) > 0)
        {
            // Friendly ship killed, abort
            ConsoleOutput.PostMessage("Mission failed! You have destroyed a friendly ship!", Color.blue);
            CurrentJob = null;
            MissionUI.Instance.panel.SetActive(false);
        }
        else
        {
            CurrentJob.RegisterKill(kill);
        }
    }



}
}