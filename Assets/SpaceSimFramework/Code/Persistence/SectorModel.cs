using UnityEngine;

namespace SpaceSimFramework
{
public class SectorModel
{
    public GameObject[] stations;
    public GameObject[] jumpgates;
    public GameObject[] fields;
    public GameObject[] wrecks;
    public int sectorSize;

    public SectorModel(GameObject[] stations, GameObject[] jumpgates, GameObject[] fields, GameObject[] wrecks, int sectorSize)
    {
        this.stations = stations;
        this.jumpgates = jumpgates;
        this.fields = fields;
        this.wrecks = wrecks;
        this.sectorSize = sectorSize;
    }
}
}