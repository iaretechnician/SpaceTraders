using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Keeps track of the player's progression and experience.
/// </summary>
public class Progression : MonoBehaviour
{
    public static int Level = 0;
    public static int[] LevelExperienceReq = { 1000, 2000, 4500, 8000, 14000, 22000 };
    public static int Experience = 0;

    public static void RegisterKill(Ship ship)
    {
        if (ship.faction == Player.Instance.PlayerFaction)
            return;

        if (ship.ShipModelInfo.ExternalDocking)
           AddExperience(800);
        else
           AddExperience(400);
    }

    public static void MissionCompleted()
    {
        AddExperience(500);
    }

    private static void AddExperience(int amount)
    {
        Experience += amount;

        if (Level < LevelExperienceReq.Length && Experience > LevelExperienceReq[Level])
        {
            Level++;
            TextFlash.ShowYellowText("You have advanced to level " + Level + "!");
        }
    }
}
}