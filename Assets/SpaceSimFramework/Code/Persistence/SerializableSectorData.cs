using SpaceSimFramework;
using System;
using System.Collections.Generic;

[Serializable]
public class SerializableSectorData
{
    public int Size;
    public int StarIndex;
    public int SkyboxIndex;
    public SerializableVector3 SkyboxTint;
    public SerializableNebulaData Nebula;

    public List<SerializableStationData> Stations;
    public List<SerializableGateData> Jumpgates;
    public List<SerializableFieldData> Fields;
    public List<SerializableWreckData> Wrecks;
}

[Serializable]
public class SerializableStationData
{
    public SerializableVector3 Position;
    public SerializableVector3 Rotation;
    public string LoadoutName;
    public string ID;

    public static SerializableStationData FromStation(Station station)
    {
        SerializableStationData data = new SerializableStationData();

        data.Position = station.transform.position;
        data.Rotation = station.transform.rotation.eulerAngles;
        data.ID = station.ID;
        data.LoadoutName = station.Loadout.name;

        return data;
    }
}

[Serializable]
public class SerializableGateData
{
    public SerializableVector3 Position;
    public SerializableVector3 Rotation;
    public SerializableVector2 Sector;
    public string ID;

    public static SerializableGateData FromGate(Jumpgate gate)
    {
        SerializableGateData data = new SerializableGateData();

        data.Position = gate.transform.position;
        data.Rotation = gate.transform.rotation.eulerAngles;
        data.ID = gate.ID;
        data.Sector = gate.NextSector;

        return data;
    }
}

[Serializable]
public class SerializableWreckData
{
    public SerializableVector3 Position;
    public SerializableVector3 Rotation;
    public string PrefabName;

    public static SerializableWreckData FromWreck(Wreck wreck)
    {
        SerializableWreckData data = new SerializableWreckData();

        data.Position = wreck.transform.position;
        data.Rotation = wreck.transform.rotation.eulerAngles;
        data.PrefabName = wreck.gameObject.name;

        return data;
    }
}

[Serializable]
public class SerializableFieldData
{
    public SerializableVector3 Position;
    public SerializableVector3 Rotation;
    public string ID;
    public int RockCount;
    public float Range;
    public SerializableVector2 RockScaleMinMax;
    public float Velocity;
    public float AngularVelocity;
    public string Resource;
    public SerializableVector2 YieldMinMax;
    public string Type;

    public static SerializableFieldData FromField(AsteroidField field)
    {
        SerializableFieldData data = new SerializableFieldData();

        data.Position = field.transform.position;
        data.Rotation = field.transform.rotation.eulerAngles;
        data.ID = field.ID;
        data.Range = field.range;
        data.RockCount = field.asteroidCount;
        data.RockScaleMinMax = field.scaleRange;
        data.Velocity = field.velocity;
        data.AngularVelocity = field.angularVelocity;
        data.Resource = field.MineableResource;
        data.YieldMinMax = field.YieldMinMax;
        data.Type = field.FieldType.ToString();

        return data;
    }
}

[Serializable]
public class SerializableNebulaData
{
    public SerializableColor AmbientLight;
    public float FogStart;
    public float FogEnd;
    public float MaxViewDistance;
    public SerializableColor NebulaColor;
    public SerializableColor NebulaCloudColor;
    public SerializableColor NebulaParticleColor;

    public float CorrosionDPS;
    public bool IsSensorObscuring;
    public string Resource;
    public int YieldPerSecond;

    public static SerializableNebulaData FromNebula(Nebula nebula)
    {
        SerializableNebulaData data = new SerializableNebulaData();

        data.AmbientLight = nebula.AmbientLight;
        data.FogStart = nebula.FogStart;
        data.FogEnd = nebula.FogEnd;
        data.MaxViewDistance = nebula.MaxViewDistance;
        data.NebulaColor = nebula.NebulaColor;
        data.NebulaCloudColor = nebula.Clouds.PuffColor;
        data.NebulaParticleColor = nebula.Particles.PuffColor;

        data.CorrosionDPS = nebula.CorrosionDamagePerSecond;
        data.IsSensorObscuring = nebula.IsSensorObscuring;
        data.Resource = nebula.Resource;
        data.YieldPerSecond = nebula.YieldPerSecond;

        return data;
    }
}