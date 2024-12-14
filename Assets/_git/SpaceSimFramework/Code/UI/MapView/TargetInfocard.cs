using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public class TargetInfocard : MonoBehaviour
{
    public GameObject InfocardPanel;

    public Text UnitName;
    public Text ModelName;
    public Text ModelData;
    public Text Status;
    public Slider EnergyBar;
    public Slider HealthBar;
    public Image ShipIcon;

    private string _status;
    private float _hullPercentage;
    private float _energyPercentage;

    private Ship _targetShip;

    public void InitializeInfocard(Ship targetShip)
    {
        if(targetShip == null)
        {
            InfocardPanel.SetActive(false);
            return;
        }

        _targetShip = targetShip;

        ModelName.text = targetShip.ShipModelInfo.name;
        ModelData.text = "Class: " + targetShip.ShipModelInfo.Class + "\nArmor: " + targetShip.ShipModelInfo.MaxArmor +
            "\nGenerator: " + targetShip.ShipModelInfo.GeneratorPower + "\nRegen: " + targetShip.ShipModelInfo.GeneratorRegen;
        UnitName.text = targetShip.name;
        ShipIcon.sprite = IconManager.Instance.GetShipIcon(targetShip.ShipModelInfo.ModelName);

        InfocardPanel.SetActive(true);
    }

    private void Update()
    {
        if (_targetShip == null)
            return;

        _hullPercentage = _targetShip.Armor / _targetShip.MaxArmor;
        _energyPercentage = _targetShip.Equipment.energyAvailable / _targetShip.Equipment.energyCapacity;

        HealthBar.value = _hullPercentage;
        EnergyBar.value = _energyPercentage;
        Status.text = _targetShip.AIInput.CurrentOrder != null ? _targetShip.AIInput.CurrentOrder.Name : "";
    }
}
}