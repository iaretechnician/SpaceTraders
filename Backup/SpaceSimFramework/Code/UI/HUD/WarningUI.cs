using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SpaceSimFramework
{
public class WarningUI : Singleton<WarningUI>
{

    public Text CurrentSectorText;
    public GameObject Corrosion, SensorObscuring;

    private void Start()
    {
        CurrentSectorText.text = "Current Sector: (" + SectorNavigation.CurrentSector.x + ", " + SectorNavigation.CurrentSector.y + ")";
    }
}
}