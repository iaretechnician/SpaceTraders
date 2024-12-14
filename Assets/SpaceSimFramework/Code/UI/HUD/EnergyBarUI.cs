using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public class EnergyBarUI : MonoBehaviour {

    private Image energyBar;

    private void Awake()
    {
        energyBar = GetComponent<Image>();
    }

    void Update () {
        if(Ship.PlayerShip != null)
            energyBar.fillAmount = 
                Ship.PlayerShip.Equipment.energyAvailable / Ship.PlayerShip.Equipment.energyCapacity;
	}
}
}