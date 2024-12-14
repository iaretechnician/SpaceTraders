using UnityEngine;

namespace SpaceSimFramework
{
[CreateAssetMenu(menuName = "DataHolders/SectorVisualData")]
public class SectorVisualData: ScriptableObject
{
    public Flare[] Flares;
    public Material[] Skybox;

    private static SectorVisualData _instance;

    public static SectorVisualData Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<SectorVisualData>("Sector Visual Data");

            if (_instance == null)
                Debug.LogError("ERROR: SectorVisualData not found! Asset must be in the Resources folder!");
            return _instance;
        }
    }
}
}