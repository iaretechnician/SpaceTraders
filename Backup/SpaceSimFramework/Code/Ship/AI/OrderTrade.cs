using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
public class OrderTrade : Order
{
    private State _state;
    private Station _destinationStation;
    private string _currentCargo = null;

    public bool InFinalApproach = false;
    private bool _dockingRetry = false;
    private float _timer; 

    private enum State
    {
        None,           // Uninitialized
        AcquireCargo,   // Fly to station with good ware price and purchase it
        SellCargo       // Find a station where the ware will sell for a better price and sell it
    }

    public OrderTrade()
    {
        Name = "Trade";
        _state = State.None;
    }

   
    public override void UpdateState(ShipAI controller)
    {
        if (_dockingRetry)
        {
            _timer += Time.deltaTime;
            if (_timer > 3f) { 
                _timer = 0;
                _dockingRetry = false;
                Debug.Log("/// Retrying docking, queue was full");
                RequestDocking(controller);
            }
            return;
        }

        if(_state == State.None)
        {
            if (controller.ship.ShipCargo.CargoOccupied > controller.ship.ShipCargo.CargoSize*0.8f)
                _state = State.SellCargo;    // Ship is already loaded
            else
                _state = State.AcquireCargo; // Ship is empty
        }
        if(_state == State.AcquireCargo)
        {
            if (_destinationStation == null)
            {  
                // No destination
                _destinationStation = FindStationForCargoPurchase(controller);
                if (_destinationStation != null) {
                    RequestDocking(controller);
                }
                else {
                    ConsoleOutput.PostMessage(controller.name +
                       ": trade command end - no suitable traderun found");
                    Debug.Log(controller.name +
                       ": trade command end - no suitable traderun found");
                    controller.FinishOrder();
                }
            }
            else { 
                FlyToStation(controller);
                
            }
        }
        if (_state == State.SellCargo)
        {
            FlyToStation(controller);
        }

        ShipSteering.SteerTowardsTarget(controller);
    }

    private void SellCargoToStation(ShipAI controller)
    {
        int pricePerUnit = GetPricePerUnit();
        foreach (HoldItem cargoitem in controller.ship.ShipCargo.CargoContents)
        {
            if (cargoitem.itemName == _currentCargo)
            {
                Debug.Log("/// Sold " + cargoitem.amount + " of " + _currentCargo + " for "+ (cargoitem.amount * pricePerUnit));
                Player.Instance.Credits += (cargoitem.amount * pricePerUnit);
                controller.ship.ShipCargo.RemoveCargoItem(_currentCargo, cargoitem.amount);
                return;
            }
        }
    }

    private void BuyCargoFromStation(ShipAI controller)
    {
        int pricePerUnit = GetPricePerUnit();

        int freeCargo = controller.ship.ShipCargo.CargoSize - controller.ship.ShipCargo.CargoOccupied;
        if(freeCargo * pricePerUnit <= Player.Instance.Credits)
        {
            controller.ship.ShipCargo.CargoContents.Add(new HoldItem(HoldItem.CargoType.Ware, _currentCargo, freeCargo));
            Player.Instance.Credits -= (freeCargo * pricePerUnit);
            Debug.Log("/// Loaded " + freeCargo + " of " + _currentCargo + " for " + (freeCargo * pricePerUnit));
        }
        else
        {
            int amount = Player.Instance.Credits / pricePerUnit;
            controller.ship.ShipCargo.CargoContents.Add(new HoldItem(HoldItem.CargoType.Ware, _currentCargo, amount));
            Player.Instance.Credits -= (amount * pricePerUnit);
            Debug.Log("/// Loaded "+amount+" of " + _currentCargo + " for " + (amount * pricePerUnit));
        }
        
    }

    private int GetPricePerUnit()
    {
        Commodities.WareType cargo = Commodities.Instance.GetWareByName(_currentCargo);
        StationDealer stationDealer = _destinationStation.GetComponent<StationDealer>();

        int averageWarePrice = (cargo.MaxPrice + cargo.MinPrice) / 2;
        int pricePerUnit = stationDealer.WarePrices.ContainsKey(_currentCargo)
            ? stationDealer.WarePrices[_currentCargo]
            : averageWarePrice;

        return pricePerUnit;
    }

    private void RequestDocking(ShipAI controller)
    {
        GameObject[] dockWaypoints;
        try { 
            dockWaypoints = _destinationStation.RequestDocking(controller.gameObject);
        }
        catch (DockingQueueException e)
        {
            _dockingRetry = true;
            return;
        }
        catch (DockingException e) {
            controller.FinishOrder();
            return;
        }
        
        controller.wayPointList.Clear();

        for (int i = 0; i < dockWaypoints.Length; i++)
            controller.wayPointList.Add(dockWaypoints[i].transform);

        controller.nextWayPoint = 0;
    }

    private IEnumerator DockingRetry()
    {
        yield return null;
    }

    private Station FindStationForCargoSale(ShipAI controller)
    {
        if (_currentCargo == null)
            return null;

        Station station = null;
        float bestWareValue = 0;

        var knownStations = UniverseMap.Knowledge[SectorNavigation.CurrentSector].Stations;

        foreach (var stationInfo in knownStations)
        {
            if (stationInfo.ID == _destinationStation.ID)    // Skip station at which ship's currently docked.
                continue;

            StationDealer stationDealer = SectorNavigation.GetStationByID(stationInfo.ID).GetComponent<StationDealer>();
            if(stationDealer.WarePrices[_currentCargo] > bestWareValue)
            {
                bestWareValue = Commodities.Instance.GetWareSellRating(_currentCargo, stationDealer.WarePrices[_currentCargo]);
                station = stationDealer.GetComponent<Station>();
            }
        }

        //Debug.Log("/// Found station for cargo dropoff: " + station.name + ", cargo: " + currentCargo + ", value:" + bestWareValue);
        return station;
    }

    private Station FindStationForCargoPurchase(ShipAI controller)
    {
        Station station = null;
        float bestWareValue = 0;

        var knownStations = UniverseMap.Knowledge[SectorNavigation.CurrentSector].Stations;

        foreach (var stationInfo in knownStations)
        {
            StationDealer stationDealer = SectorNavigation.GetStationByID(stationInfo.ID).GetComponent<StationDealer>();
            foreach(KeyValuePair<string, int> ware in stationDealer.WarePrices)
            {
                float wareValue = Commodities.Instance.GetWareBuyRating(ware.Key, ware.Value);
                if (wareValue > bestWareValue && wareValue > 0.5f)
                {
                    bestWareValue = wareValue;
                    _currentCargo = ware.Key;
                    station = stationDealer.GetComponent<Station>();
                }
            }
        }

        //Debug.Log("/// Found station for cargo purchase: " + station.name + ", cargo: " + currentCargo + ", value:" + bestWareValue);
        return station;
    }

    private void FlyToStation(ShipAI controller)
    {
        // Check angle to waypoint: first steer towards waypoint, then move to it
        if (OrderDock.FacingWaypoint(controller))
            OrderDock.MoveToWaypoint(controller);
        else
            controller.throttle = 0f;

        // Disable collision avoidance check in ShipAI
        if (controller.nextWayPoint > 0)
            InFinalApproach = true;
    }

    /// <summary>
    /// Station callback when ship has docked.
    /// </summary>
    public void PerformTransaction(ShipAI controller)
    {
        if(_state == State.SellCargo)
        {
            SellCargoToStation(controller);
            _state = State.AcquireCargo;
            _destinationStation = null;  // Trade run complete
            return;
        }
        if (_state == State.AcquireCargo)
        {
            BuyCargoFromStation(controller);
            _state = State.SellCargo;

            _destinationStation = FindStationForCargoSale(controller);
            if (_destinationStation != null)
            {
                RequestDocking(controller);
            }
            else
            {
                ConsoleOutput.PostMessage(controller.name +
                   ": trade command error - no profitable sale location");
                Debug.Log(controller.name +
                   ": trade command end - no profitable sale location");
                controller.FinishOrder();
            }

            return;
        }
    }

    public override void Destroy()
    {
        Debug.Log("/// Order Trade terminated");
    }
}
}