using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceSimFramework
{
public class GenerateRandomSector
{
    public static int SectorSize
    {
        get { return _sectorSize; }
    }
    private static int _sectorSize = 3000;

    private const float MINEABLE_FIELD_PROBABILITY = 0.8f;
    private const float FIELD_SPAWN_PROBABILITY = 0.5f;
    private const float NEBULA_SECTOR_PROBABILITY = 0.33f;

    public static void GenerateSectorAtPosition(Vector2 position, Vector2 previousSector)
    {
        Flare[] flares = SectorVisualData.Instance.Flares;
        Material[] skybox = SectorVisualData.Instance.Skybox;
        Color skyboxTint = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 1f);
        Faction[] factions = ObjectFactory.Instance.Factions;

        GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>().flare = flares[Random.Range(0, flares.Length)];

        if (Random.value < NEBULA_SECTOR_PROBABILITY)
        {
            Nebula nebula = GameObject.Instantiate(ObjectFactory.Instance.NebulaPrefab).GetComponent<Nebula>();
            nebula.AmbientLight = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 0.35f);
            nebula.FogEnd = Random.Range(1.5f, 3f)*1000f;
            nebula.FogStart = 150;
            nebula.MaxViewDistance = nebula.FogEnd;
            nebula.NebulaColor = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 0.35f); 
            nebula.Clouds.PuffColor = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 0.35f); 
            nebula.Particles.PuffColor = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 0.35f); 

            if(Random.value < 0.25)
            {
                nebula.Resource = "Helium";
                nebula.YieldPerSecond = Random.Range(1,3);
            }
            else
            {
                nebula.Resource = "";
            }
            nebula.CorrosionDamagePerSecond = 0;
            nebula.IsSensorObscuring = false;
        }

        RenderSettings.skybox = skybox[Random.Range(0, skybox.Length)];
        if (RenderSettings.skybox.HasProperty("_Tint"))
            RenderSettings.skybox.SetColor("_Tint", skyboxTint);
        else if (RenderSettings.skybox.HasProperty("_SkyTint"))
            RenderSettings.skybox.SetColor("_SkyTint", skyboxTint);

        // This fixes the ambient light problem when dynamically changing skyboxes
        DynamicGI.UpdateEnvironment();

        _sectorSize = Random.Range(3, 10) * 1000;

        // Spawn stations
        Station station;
        for(int i=0; i<Random.Range(0,7); i++)
        {
            StationLoadout stationData = GetStationData(GetRandomFaction(factions));
            station = GameObject.Instantiate(ObjectFactory.Instance.GetStationByName(stationData.ModelName), GetRandomPosition(), GetRandomRotation()).GetComponent<Station>();
            station.faction = GetRandomFaction(factions);
            station.ID = "x" + position.x + "y" + position.y + "st" + RandomString(6);
            station.Loadout = stationData;
            station.gameObject.name = station.faction.name + " Station (" + station.ID + ")";
        }

        // Spawn asteroid fields
        AsteroidField field;
        for (int i = 0; i < 3; i++)
        {
            if (Random.value > FIELD_SPAWN_PROBABILITY) 
                continue;

            field = GameObject.Instantiate(ObjectFactory.Instance.AsteroidFieldPrefab, GetRandomPosition(), Quaternion.identity).GetComponent<AsteroidField>();
            field.ID = "x" + position.x + "y" + position.y + "f" + RandomString(6);
            field.range = Random.Range(800, 3500);
            field.velocity = Random.Range(0, 15);
            field.asteroidCount = Random.Range(2, 15) * 100;
            int rockSizeMin = Random.Range(2, 15);
            field.scaleRange = new Vector2(rockSizeMin, rockSizeMin+Random.Range(2, 15));

            var randomValue = Random.value;
            if(randomValue < 0.33f)
            {
                field.MineableResource = "Ore";
                field.FieldType = FieldType.Rock;
            }
            else if(randomValue < 0.66f)
            {
                field.MineableResource = "Water";
                field.FieldType = FieldType.Ice;
            }
            else
            {
                field.MineableResource = "Alloys";
                field.FieldType = FieldType.Scrap;
            }

            if(Random.value < MINEABLE_FIELD_PROBABILITY)
            {
                // Make a mineable field
                float minYield = Random.Range(1, 5);
                field.YieldMinMax = new Vector2(minYield, minYield + Random.Range(4, 10));
            }
            else
            {
                field.MineableResource = null;
            }
        }

        // Spawn jumpgates
        GenerateJumpgates(position, previousSector);

        // Spawn wrecks
        int n = Random.Range(0, 2);
        for(int i=0; i<n; i++)
        {
            var prefab = ObjectFactory.Instance.Wrecks[Random.Range(0, ObjectFactory.Instance.Wrecks.Length - 1)];
            var instance = GameObject.Instantiate(prefab, GetRandomPosition(), Quaternion.identity);
            instance.name = prefab.name;
        }
    }

    public static List<GameObject> GenerateJumpgates(Vector2 position, Vector2 previousSector)
    {
        List<GameObject> jumpgates = new List<GameObject>();

        Vector2 adjacentSectorPosition = new Vector2(position.x, position.y + 1);
        // North sector
        GameObject gate = TryGetJumpgate(position, previousSector, adjacentSectorPosition);
        if (gate != null)
            jumpgates.Add(gate);

        adjacentSectorPosition = new Vector2(position.x + 1, position.y);
        // East sector
        gate = TryGetJumpgate(position, previousSector, adjacentSectorPosition);
        if (gate != null)
            jumpgates.Add(gate);

        adjacentSectorPosition = new Vector2(position.x, position.y - 1);
        // South sector
        gate = TryGetJumpgate(position, previousSector, adjacentSectorPosition);
        if (gate != null)
            jumpgates.Add(gate);

        adjacentSectorPosition = new Vector2(position.x - 1, position.y);
        // West sector
        gate = TryGetJumpgate(position, previousSector, adjacentSectorPosition);
        if (gate != null)
            jumpgates.Add(gate);

        return jumpgates;
    }

    private static GameObject TryGetJumpgate(Vector2 newPosition, Vector2 previousSectorPosition, Vector2 adjacentSectorPosition)
    {
        SerializableUniverseSector sector = null;

        Universe.Sectors.TryGetValue(adjacentSectorPosition, out sector);
        if (sector != null)
        {
            if (sector.SectorPosition == previousSectorPosition)
            {
                // Add mandatory connection to previous sector
                return GetJumpgateToPosition(newPosition, adjacentSectorPosition).gameObject;
            }
            else if (sector.Connections.Contains(newPosition))
            // Check if adjacent sector has a gate leading to this sector, and connect if necessary
            {
                return GetJumpgateToPosition(newPosition, adjacentSectorPosition).gameObject;
            }
        }
        else
        {
            if (Random.value < 0.7f)
            {
                // Generate jumpgate to new, unexplored sector
                return GetJumpgateToPosition(newPosition, adjacentSectorPosition).gameObject;
            }
        }

        return null;
    }

    #region Utils
    private static Jumpgate GetJumpgateToPosition(Vector2 jumpgateSector, Vector2 targetSector)
    {
        Jumpgate jumpgate = GameObject.Instantiate(ObjectFactory.Instance.JumpGatePrefab, GetRandomPosition(), Quaternion.Euler(0, Random.Range(0, 360), 0)).GetComponent<Jumpgate>();
        jumpgate.NextSector = targetSector;
        jumpgate.ID = "x" + jumpgateSector.x + "y" + jumpgateSector.y + "jg" + RandomString(6);
        jumpgate.gameObject.name = "Jumpgate To (" + targetSector.x + ", " + targetSector.y + ")";
        return jumpgate;
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[Random.Range(0, s.Length)]).ToArray());
    }

    private static Faction GetRandomFaction(Faction[] factions)
    {
        return factions[Random.Range(0, factions.Length - 1)];
    }

    private static Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(-_sectorSize/2, _sectorSize / 2),
            Random.Range(-_sectorSize / 10, _sectorSize / 10),  // Ensure sectors are more or less oriented on a 2D plane (XY)
            Random.Range(-_sectorSize / 2, _sectorSize / 2)
            );
    }

    public static Color GetRandomColor()
    {
        return new Color(
            Random.value*0.5f + 0.5f,
            Random.value * 0.5f + 0.5f,
            Random.value * 0.5f + 0.5f
            );
    }

    private static StationLoadout GetStationData(Faction sectorFaction)
    {
        if(Random.value > 0.5)
        {
            return GetRandomLoadout();
        }

        int numberOfFactionStations;

        if (sectorFaction != null)
        {
            numberOfFactionStations = sectorFaction.Stations.Length;
            if(numberOfFactionStations > 0)
                return sectorFaction.Stations[Random.Range(0, numberOfFactionStations - 1)];
        }

        Faction randomFaction = ObjectFactory.Instance.Factions[Random.Range(0, ObjectFactory.Instance.Factions.Length - 1)];
        numberOfFactionStations = randomFaction.Stations.Length;
        if(numberOfFactionStations > 0)
        {
            StationLoadout loadout = randomFaction.Stations[Random.Range(0, numberOfFactionStations - 1)];
            return loadout;
        }

        return GetRandomLoadout();
    }

    private static StationLoadout GetRandomLoadout()
    {
        StationLoadout[] loadouts = Resources.LoadAll<StationLoadout>("Stations/");
        return loadouts[Random.Range(0, loadouts.Length)];
    }

    private static Quaternion GetRandomRotation()
    {
        return Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
    }
    #endregion Utils
}
}