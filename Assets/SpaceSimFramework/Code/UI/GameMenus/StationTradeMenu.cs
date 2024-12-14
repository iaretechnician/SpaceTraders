using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SpaceSimFramework
{
public class StationTradeMenu : MonoBehaviour {

    public ScrollMenuController StationMenu, ShipMenu;
    public Text CreditsText; 

    private ShipCargo _shipCargo;
    private StationDealer _stationWares;
    private Station _station;

    private void Start()
    {
        StationMenu.HeaderText.text = "Trade with " + _stationWares.gameObject.name;
        UpdateCredits();

        // Disable station menu and focus ship menu
        StationMenu.DisableKeyInput = true;
        ShipMenu.DisableKeyInput = false;
        ShipMenu.selectedOption = 0;
    }

    private void UpdateCredits()
    {
        // Keep credits display updated
        CreditsText.text = "Credits: " + Player.Instance.Credits;
    }

    private void Update()
    {
        if (StationMenu.SubMenu != null || ShipMenu.SubMenu != null)
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StationMenu.DisableKeyInput = true;
            ShipMenu.DisableKeyInput = false;
            ShipMenu.selectedOption = 0;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            StationMenu.DisableKeyInput = false;
            StationMenu.selectedOption = 0;
            ShipMenu.DisableKeyInput = true;
        }
    }

    /// <summary>
    /// Invoked when a trade menu was opened for a ship docked on a station.
    /// </summary>
    /// <param name="ship">Ship trading with station</param>
    /// <param name="station">Station to trade with</param>
    public void PopulateMenu(GameObject ship, Station station)
    {
        _shipCargo = ship.GetComponent<ShipCargo>();
        _stationWares = station.gameObject.GetComponent<StationDealer>();
        this._station = station;

        ShipMenuSetOptions();
        StationMenuSetOptions();
    }

    /// <summary>
    /// Resets and updates ship menu options
    /// </summary>
    private void ShipMenuSetOptions()
    {
        ShipMenu.ClearMenuOptions();
        foreach (HoldItem cargo in _shipCargo.CargoContents)
        {
            // Add only trade wares
            if(cargo.cargoType == HoldItem.CargoType.Ware)
            {
                Color color = Commodities.Instance.GetWareSellColor(cargo.itemName, GetWareSellingPrice(cargo.itemName));
                ShipMenu.AddMenuOption(cargo.itemName + " (" + cargo.amount + ")", Color.white, IconManager.Instance.GetWareIcon(cargo.itemName), 1, 80, color)
                    .AddListener(() => AddShipCargo(cargo));
            }
                
        }

    }

    /// <summary>
    /// Services a cargo item to the left-hand side Sell menu, opening an appropriate sell menu
    /// when invoked.
    /// </summary>
    /// <param name="cargo">Cargo item to sell</param>
    private void AddShipCargo(HoldItem cargo)
    {
        if (StationMenu.SubMenu != null)
            StationMenu.SubMenu = null;

        int wareSellPrice = GetWareSellingPrice(cargo.itemName);

        // Open Sell Dialog
        ShipMenu.SubMenu = GameObject.Instantiate(UIElements.Instance.SliderDialog, ShipMenu.transform.parent);
        // Reposition submenu
        RectTransform rt = ShipMenu.SubMenu.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 0);

        // Populate text menus
        PopupSliderMenuController sellMenu = ShipMenu.SubMenu.GetComponent<PopupSliderMenuController>();
        sellMenu.SetTextFields("Sell " + cargo.itemName + ", select amount:", "Amount: 0");

        // Edit slider value
        sellMenu.Slider.maxValue = cargo.amount;        
        sellMenu.Slider.onValueChanged.AddListener(value => {
            sellMenu.InfoText.text = "Amount: " + value;
            sellMenu.AmountText.text = "Credits: " + value* wareSellPrice;
        });

        // What happens when Ok or Cancel is pressed
        sellMenu.AcceptButton.onClick.RemoveAllListeners();
        sellMenu.AcceptButton.onClick.AddListener(() => {
            int profit = (int)sellMenu.Slider.value * wareSellPrice;

            // Sell cargo
            _shipCargo.RemoveCargoItem(cargo.itemName, (int)sellMenu.Slider.value);
            Player.Instance.Credits += profit;

            var msg = "Sold " + sellMenu.Slider.value + " cargo units for " + profit + ", cargo occupied: " + _shipCargo.CargoOccupied;
            ConsoleOutput.PostMessage(msg);
            Debug.Log(msg);

            if(MissionControl.CurrentJob != null)
            {
                if (MissionControl.CurrentJob.Type == Mission.JobType.CargoDelivery)
                {
                    CargoDelivery job = (CargoDelivery)MissionControl.CurrentJob;
                    if (job.StationID == _station.ID && job.Ware == cargo.itemName)
                    {
                        job.Amount -= (int)sellMenu.Slider.value;
                        if (job.Amount <= 0)
                            job.FinishJob();
                    }
                }
                else if(MissionControl.CurrentJob.Type == Mission.JobType.Courier)
                {
                    Courier job = (Courier)MissionControl.CurrentJob;
                    if (job.StationID == _station.ID && job.Ware == cargo.itemName)
                    {
                        job.Amount -= (int)sellMenu.Slider.value;
                        if (job.Amount <= 0)
                            job.FinishJob();
                    }
                }
            }
            UpdateCredits();

            ShipMenuSetOptions();

            GameObject.Destroy(sellMenu.gameObject);
        });
        sellMenu.CancelButton.onClick.RemoveAllListeners();
        sellMenu.CancelButton.onClick.AddListener(() => {
            GameObject.Destroy(sellMenu.gameObject);
        });

    }

    /// <summary>
    /// Gets the buying price of the ware on the station. If ware is not found, returns 
    /// avergae ware price.
    /// </summary>
    /// <param name="item">Name of the ware</param>
    /// <returns>Selling price of ware on station</returns>
    private int GetWareSellingPrice(string item)
    {
        int itemPrice = (Commodities.Instance.GetWareByName(item).MinPrice+ Commodities.Instance.GetWareByName(item).MaxPrice) / 2;
        int stationPrice;
        string wareName;

        foreach(var pair in _stationWares.WarePrices)
        {
            wareName = pair.Key;
            stationPrice = pair.Value;

            if (item == wareName)
                return stationPrice;
        }
        return itemPrice;
    }

    /// <summary>
    /// Resets and updates ship menu options
    /// </summary>
    private void StationMenuSetOptions()
    {
        foreach (KeyValuePair<string, int> ware in _stationWares.WarePrices)
        {
            Color color = Commodities.Instance.GetWareSellColor(ware.Key, ware.Value);
            StationMenu.AddMenuOption(ware.Key + " " + ware.Value + "Cr", Color.white, IconManager.Instance.GetWareIcon(ware.Key), 1, 80, color)
                .AddListener(() => AddStationWare(ware));
        }
    }

    /// <summary>
    /// Displays a ware sold by the station in the appropriate menu and handles
    /// onClick events when invoked.
    /// </summary>
    /// <param name="ware">Ware sold at station</param>
    private void AddStationWare(KeyValuePair<string, int> ware)
    {
        if (ShipMenu.SubMenu != null)
            ShipMenu.SubMenu = null;

        // Open Sell Dialog
        StationMenu.SubMenu = GameObject.Instantiate(UIElements.Instance.SliderDialog, StationMenu.transform.parent);
        // Reposition submenu
        RectTransform rt = StationMenu.SubMenu.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(/*gameObject.GetComponent<RectTransform>().sizeDelta.x / 2*/0, 0);

        // Populate text menus
        PopupSliderMenuController wareMenu = StationMenu.SubMenu.GetComponent<PopupSliderMenuController>();
        wareMenu.SetTextFields("Buy " + ware.Key + ", select amount:", "Amount: 0");

        // Edit slider value
        wareMenu.Slider.maxValue = _shipCargo.CargoSize - _shipCargo.CargoOccupied;
        wareMenu.Slider.onValueChanged.AddListener(value => {
            wareMenu.InfoText.text = "Amount: " + value;
            wareMenu.AmountText.text = "Price: " + value * ware.Value;
        });

        // What happens when Ok or Cancel is pressed
        wareMenu.AcceptButton.onClick.RemoveAllListeners();
        wareMenu.AcceptButton.onClick.AddListener(() => {
            int price = (int)wareMenu.Slider.value * ware.Value;
            if (price <= Player.Instance.Credits)
            {
                // Buy ware
                _shipCargo.AddWare(HoldItem.CargoType.Ware, ware.Key, (int)wareMenu.Slider.value);
                Player.Instance.Credits -= price;
                UpdateCredits();

                var msg = "Bought " + wareMenu.Slider.value + " cargo units for " + price + ", cargo occupied: " + _shipCargo.CargoOccupied;
                ConsoleOutput.PostMessage(msg);
                Debug.Log(msg);
            }

            ShipMenuSetOptions();

            GameObject.Destroy(wareMenu.gameObject);
        });
        wareMenu.CancelButton.onClick.RemoveAllListeners();
        wareMenu.CancelButton.onClick.AddListener(() => {
            GameObject.Destroy(wareMenu.gameObject);
        });

    }

    public void OnCloseClicked()
    {
        ShipMenu.OnCloseClicked();
    }

}
}