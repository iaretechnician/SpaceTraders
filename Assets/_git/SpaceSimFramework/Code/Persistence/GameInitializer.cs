using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Invoked when a game scene is started.
/// </summary>
public class GameInitializer : MonoBehaviour
{

    public bool StartNewGame;

    // Loading sequence: 
    // First - load savegame to obtain current sector
    // Second - load the sector data into scene
    // Third - load player's exploration state
    void Awake()
    {
        Universe.Sectors = Universe.LoadUniverse();
        if (!StartNewGame)
        {
            // Read sector that needs to be loaded from savefile
            SectorNavigation.ChangeSector(LoadGame.GetCurrentSavedSector(), false);
            if (Universe.Sectors.ContainsKey(SectorNavigation.CurrentSector))   // Load if exists, create if it doesnt
            {
                SectorLoader.LoadSectorData(Universe.Sectors[SectorNavigation.CurrentSector].Name);
            }
            else
            {
                GenerateRandomSector.GenerateSectorAtPosition(SectorNavigation.CurrentSector, SectorNavigation.PreviousSector);
                SectorSaver.SaveCurrentSectorToFile();
            }

            // Load player saved data
            LoadGame.LoadAutosave();
            if (UniverseMap.Knowledge == null)
            {
                LoadGame.LoadPlayerKnowledge();
            }

            // Start mission if needed
            if (MissionControl.CurrentJob != null)
                MissionControl.CurrentJob.OnMissionStarted();
        }
        else
        {
            UniverseMap.Knowledge = new Dictionary<SerializableVector2, SerializableSectorData>();
            SectorNavigation.ChangeSector(Vector2.zero, false);
            // Generate IDs for initial sector
            foreach (var station in GameObject.FindGameObjectsWithTag("Station"))
            {
                station.GetComponent<Station>().ID = "x0y0_st" + GenerateRandomSector.RandomString(6);
            }
            foreach (var gate in GameObject.FindGameObjectsWithTag("Jumpgate"))
            {
                gate.GetComponent<Jumpgate>().ID = "x0y0_jg" + GenerateRandomSector.RandomString(6);
            }
            foreach (var field in GameObject.FindGameObjectsWithTag("AsteroidField"))
            {
                field.GetComponent<AsteroidField>().ID = "x0y0_f" + GenerateRandomSector.RandomString(6);
            }
            // Save initial sector
            SectorSaver.SaveCurrentSectorToFile();
        }

        // Remove the sectorloader once it has loaded everything
        GameObject.Destroy(gameObject);
    }
}
}