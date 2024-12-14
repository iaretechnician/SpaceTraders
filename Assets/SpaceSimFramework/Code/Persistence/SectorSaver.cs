using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace SpaceSimFramework
{
public class SectorSaver
{

    public static void SaveSectorToFile(SectorModel sectorModel, string fileName)
    {
        if (!Directory.Exists(Utils.SECTORS_FOLDER))
        {
            Directory.CreateDirectory(Utils.SECTORS_FOLDER);
        }
        string path = Utils.SECTORS_FOLDER + fileName;
        SaveSectorToPath(sectorModel, path);
    }

    public static void SaveSectorToPath(SectorModel sectorModel, string path)
    {
        SerializableSectorData data = new SerializableSectorData();

        // STATIONS
        data.Stations = new List<SerializableStationData>();
        foreach (var station in sectorModel.stations)
        {
            data.Stations.Add(SerializableStationData.FromStation(station.GetComponent<Station>()));
        }

        // ENVIRONMENT
        data.Fields = new List<SerializableFieldData>();
        foreach (var fieldObj in sectorModel.fields)
        {
            data.Fields.Add(SerializableFieldData.FromField(fieldObj.GetComponent<AsteroidField>()));
        }

        // JUMPGATES
        data.Jumpgates = new List<SerializableGateData>();
        foreach (var gate in sectorModel.jumpgates)
        {
            data.Jumpgates.Add(SerializableGateData.FromGate(gate.GetComponent<Jumpgate>()));
        }

        // NEBULA
        Nebula nebula = GameObject.FindObjectOfType<Nebula>();
        if (nebula != null)
        {
            data.Nebula = SerializableNebulaData.FromNebula(nebula);
        }

        // WRECKS
        data.Wrecks = new List<SerializableWreckData>();
        foreach(var wreck in sectorModel.wrecks)
        {
            data.Wrecks.Add(SerializableWreckData.FromWreck(wreck.GetComponent<Wreck>()));
        }

        // SYSTEM
        data.SkyboxIndex = GetSkyboxIndex();
        data.StarIndex = GetSunIndex();
        data.Size = sectorModel.sectorSize;

        Color skyboxColor = Color.white;
        if (RenderSettings.skybox.HasProperty("_Tint"))
            skyboxColor = RenderSettings.skybox.GetColor("_Tint");
        else if (RenderSettings.skybox.HasProperty("_SkyTint"))
            skyboxColor = RenderSettings.skybox.GetColor("_SkyTint");
        data.SkyboxTint = new SerializableVector3(skyboxColor.r, skyboxColor.g, skyboxColor.b);

        // Save it
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static void SaveCurrentSectorToFile()
    {
        SectorNavigation.Instance.Awake();
        Universe.AddCurrentSector(SectorNavigation.CurrentSector);
        SectorModel sectorModel = new SectorModel(
            SectorNavigation.Stations.ToArray(),
            SectorNavigation.Jumpgates.ToArray(),
            SectorNavigation.Fields.ToArray(),
            GameObject.FindGameObjectsWithTag("Wreck"),
            SectorNavigation.SectorSize
        );
        SaveSectorToFile(sectorModel, ProfileMenuController.PLAYER_PROFILE + "_x" + SectorNavigation.CurrentSector.x + "y" + SectorNavigation.CurrentSector.y);
    }

    private static int GetSkyboxIndex()
    {
        for (int i = 0; i < SectorVisualData.Instance.Skybox.Length; i++)
            if (SectorVisualData.Instance.Skybox[i] == RenderSettings.skybox)
                return i;
        return 0;
    }

    private static int GetSunIndex()
    {
        Flare sun = GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>().flare;

        for (int i = 0; i < SectorVisualData.Instance.Flares.Length; i++)
            if (SectorVisualData.Instance.Flares[i] == sun)
                return i;
        return 0;
    }

}
}