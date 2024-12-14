using System;
using System.Collections.Generic;

[Serializable]
public class SerializableUniverseSector
{
    public string Name;
    public SerializableVector2 SectorPosition;
    public List<SerializableVector2> Connections;
    public string OwnerFaction;

    public SerializableUniverseSector(string name, int x, int y, string owner)
    {
        Name = name;
        SectorPosition.x = x;
        SectorPosition.y = y;
        Connections = new List<SerializableVector2>();
        OwnerFaction = owner;
    }
}
