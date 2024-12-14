using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

namespace SpaceSimFramework
{
public class SectorLoader : MonoBehaviour
{
    public static void LoadSectorData(string sectorName)
    {
        string path = Utils.SECTORS_FOLDER + sectorName;
        LoadSectorIntoScene(path);
    }

    public static void LoadSectorIntoScene(string filepath)
    {
        Flare[] Flares = SectorVisualData.Instance.Flares;
        Material[] Skybox = SectorVisualData.Instance.Skybox;
        Faction[] factions = ObjectFactory.Instance.Factions;

        if (!File.Exists(filepath))
        {
            Debug.LogError("Tried to load sector but file " + filepath + " was not found!");
            return;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(filepath, FileMode.Open);

        SerializableSectorData data = formatter.Deserialize(stream) as SerializableSectorData;
        stream.Close();

        GameObject spawnedObject;
        Vector3 spawnPosition;
        Quaternion spawnRotation;

        SectorNavigation.SectorSize = data.Size;
        GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>().flare = Flares[data.StarIndex];
        RenderSettings.skybox = Skybox[data.SkyboxIndex];
        Color skyboxColor = new Color(data.SkyboxTint.x, data.SkyboxTint.y, data.SkyboxTint.z);
        if (RenderSettings.skybox.HasProperty("_Tint"))
            RenderSettings.skybox.SetColor("_Tint", skyboxColor);
        else if (RenderSettings.skybox.HasProperty("_SkyTint"))
            RenderSettings.skybox.SetColor("_SkyTint", skyboxColor);
        // This fixes the ambient light problem when dynamically changing skyboxes
        DynamicGI.UpdateEnvironment();

        // Spawn nebula, if one exists
        if(data.Nebula != null)
        {
            Nebula nebula = GameObject.Instantiate(ObjectFactory.Instance.NebulaPrefab).GetComponent<Nebula>();
            nebula.AmbientLight = data.Nebula.AmbientLight;
            nebula.FogEnd = data.Nebula.FogEnd;
            nebula.FogStart = data.Nebula.FogStart;
            nebula.MaxViewDistance = data.Nebula.MaxViewDistance;
            nebula.NebulaColor = data.Nebula.NebulaColor;
            nebula.Clouds.PuffColor = data.Nebula.NebulaCloudColor;
            nebula.Particles.PuffColor = data.Nebula.NebulaParticleColor;

            nebula.YieldPerSecond = data.Nebula.YieldPerSecond;
            nebula.CorrosionDamagePerSecond = data.Nebula.CorrosionDPS;
            nebula.Resource = data.Nebula.Resource;
            nebula.IsSensorObscuring = data.Nebula.IsSensorObscuring;
        }

        foreach (var stationData in data.Stations)
        {
            // Get station loadout
            StationLoadout loadout = StationLoadout.GetLoadoutByName(stationData.LoadoutName);

            // Spawn the station
            spawnPosition = stationData.Position;
            spawnRotation = Quaternion.Euler(stationData.Rotation);
            spawnedObject = GameObject.Instantiate(
                ObjectFactory.Instance.GetStationByName(loadout.ModelName),
                spawnPosition, spawnRotation);

            // Fill data from loadout
            Station station = spawnedObject.GetComponent<Station>();
            station.Loadout = loadout;
            station.ID = stationData.ID;
            station.faction = loadout.faction;
            station.HasCargoDealer = loadout.HasCargoDealer;
            station.HasShipDealer = loadout.ShipsForSale.Length > 0;
            spawnedObject.name = station.faction.name + " Station (" + station.ID + ")";

            // Fill dealer items for sale
            StationDealer dealer = station.GetComponent<StationDealer>();
            dealer.EquipmentForSale = loadout.EquipmentForSale;
            dealer.WeaponsForSale = loadout.WeaponsForSale;
            if (station.HasShipDealer)
                dealer.ShipsForSale = loadout.ShipsForSale;
        }

        foreach (var jumpgateData in data.Jumpgates)
        {
            spawnPosition = jumpgateData.Position;
            spawnRotation = Quaternion.Euler(jumpgateData.Rotation);
            spawnedObject = GameObject.Instantiate(ObjectFactory.Instance.JumpGatePrefab,
                spawnPosition, spawnRotation);

            spawnedObject.name = "Jumpgate To (" + jumpgateData.Sector.x + ", " + jumpgateData.Sector.y + ")";
            spawnedObject.GetComponent<Jumpgate>().NextSector = jumpgateData.Sector;
            spawnedObject.GetComponent<Jumpgate>().ID = jumpgateData.ID;
        }

        foreach (var fieldData in data.Fields)
        {
            spawnPosition = fieldData.Position;
            spawnRotation = Quaternion.Euler(fieldData.Rotation);
            spawnedObject = GameObject.Instantiate(ObjectFactory.Instance.AsteroidFieldPrefab,
                spawnPosition, spawnRotation);

            AsteroidField asteroidField = spawnedObject.GetComponent<AsteroidField>();
            asteroidField.ID = fieldData.ID;
            asteroidField.range = fieldData.Range;
            asteroidField.asteroidCount = fieldData.RockCount;
            asteroidField.scaleRange = fieldData.RockScaleMinMax;
            asteroidField.velocity = fieldData.Velocity;
            asteroidField.angularVelocity = fieldData.AngularVelocity;
            asteroidField.MineableResource = fieldData.Resource;
            asteroidField.YieldMinMax = fieldData.YieldMinMax;
            asteroidField.FieldType = (FieldType)Enum.Parse(typeof(FieldType), fieldData.Type);
        }

        foreach (var wreckData in data.Wrecks)
        {
            spawnPosition = wreckData.Position;
            spawnRotation = Quaternion.Euler(wreckData.Rotation);
            var prefab = ObjectFactory.Instance.GetWreckByName(wreckData.PrefabName);
            spawnedObject = GameObject.Instantiate(prefab, spawnPosition, spawnRotation);
            spawnedObject.name = prefab.name;
        }

    }

}
}