using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
public class ShipCargo : MonoBehaviour {

    public static float CARGO_DROP_PROBABILITY = 0.1f, WEAPON_DROP_PROBABILITY = 0.07f;
    public GameObject CargoItemPrefab;

    [HideInInspector]
    public int CargoSize = 30, CargoOccupied = 0;
    public List<HoldItem> CargoContents;    
   

	void Awake () {
        CargoContents = new List<HoldItem>();
        // Get cargo size 
        CargoSize = gameObject.GetComponent<Ship>().ShipModelInfo.CargoSize;

        // Generate random cargo items
        if (gameObject.GetComponent<Ship>().faction == Player.Instance.PlayerFaction)
            return;

        while(true)
        {
            int item = Random.Range(0, Commodities.Instance.NumberOfWares);
            int amount = Random.Range(1, 10);

            if(CargoOccupied + amount <= CargoSize) { 
                AddWare(HoldItem.CargoType.Ware, Commodities.Instance.CommodityTypes[item].Name, amount);
            }
            else
            {
                break;
            }
        }

	}

    /// <summary>
    /// Invoked by the Ship script when owned ship has been destroyed. Drops loot.
    /// </summary>
    public void OnShipDestroyed()
    {
        // Drop random cargo items from the ones available
        foreach(HoldItem holdItem in CargoContents)
        {
            if(Random.Range(0f,1f) < CARGO_DROP_PROBABILITY)
            {
                Vector3 randomAddition = new Vector3(Random.Range(1, 5), Random.Range(1, 5), Random.Range(1, 5));
                // Eject item to a random location
                GameObject cargo = GameObject.Instantiate(
                    CargoItemPrefab,
                    transform.position+randomAddition,
                    Quaternion.identity);

                cargo.GetComponent<CargoItem>().InitCargoItem(holdItem.cargoType, holdItem.amount, holdItem.itemName);
            }
        }

        Ship ship = GetComponent<Ship>();
        foreach (GunHardpoint hardpoint in ship.Equipment.Guns)
        {
            if (hardpoint.mountedWeapon != null && Random.Range(0f, 1f) < WEAPON_DROP_PROBABILITY)
            {
                Vector3 randomAddition = new Vector3(Random.Range(1, 5), Random.Range(1, 5), Random.Range(1, 5));
                // Eject item to a random location
                GameObject cargo = GameObject.Instantiate(
                    CargoItemPrefab,
                    transform.position + randomAddition,
                    Quaternion.identity);

                cargo.GetComponent<CargoItem>().InitCargoItem(HoldItem.CargoType.Weapon, 1, hardpoint.mountedWeapon.name);
            }
        }
        foreach (GunHardpoint hardpoint in ship.Equipment.Turrets)
        {
            if (hardpoint.mountedWeapon != null && Random.Range(0f, 1f) < WEAPON_DROP_PROBABILITY)
            {
                Vector3 randomAddition = new Vector3(Random.Range(1, 5), Random.Range(1, 5), Random.Range(1, 5));
                // Eject item to a random location
                GameObject cargo = GameObject.Instantiate(
                    CargoItemPrefab,
                    transform.position + randomAddition,
                    Quaternion.identity);

                cargo.GetComponent<CargoItem>().InitCargoItem(HoldItem.CargoType.Weapon, 1, hardpoint.mountedWeapon.name);
            }
        }
    }

    /// <summary>
    /// Invoked when a ship picks up a cargo container.
    /// </summary>
    /// <param name="item">Cargo item which was picked up</param>
    public void AddWare(HoldItem.CargoType type, string name, int amount)
    {
        if (CargoOccupied < CargoSize)
        {
            if (CargoOccupied + amount <= CargoSize)
            {
                // Take all the cargo
                CargoOccupied += amount;
            }
            else
            {
                // Take as much as fits
                amount = CargoSize - CargoOccupied;
                CargoOccupied = CargoSize;
            }

            HoldItem cargoItem = GetCargo(name);
            if (cargoItem == null)
                CargoContents.Add(new HoldItem(type, name, amount));
            else
                cargoItem.amount += amount;

            if (Ship.PlayerShip != null && this.gameObject == Ship.PlayerShip.gameObject)
                ConsoleOutput.PostMessage("Cargobay now contains " + name + " (" + amount + ")");
        }
        else
        {
            if (this.gameObject == Ship.PlayerShip.gameObject)
                ConsoleOutput.PostMessage("Cargobay full!", Color.yellow);
        }
    }

    /// <summary>
    /// Invoked when a ship picks up a cargo container.
    /// </summary>
    /// <param name="item">Cargo item which was picked up</param>
    public void AddCargoItem(CargoItem cargo)
    {
        AddWare(cargo.item.cargoType, cargo.item.itemName, cargo.item.amount);
    }

    /// <summary>
    /// Invoked to remove some of the cargo hold items.
    /// </summary>
    /// <param name="cargo">Cargo item which is to be removed</param>
    public void RemoveCargoItem(string itemName, int amount)
    {
        foreach (HoldItem cargoitem in CargoContents)
        {
            if (cargoitem.itemName == itemName)
            {                
                if(cargoitem.amount <= amount || amount == 0)
                {
                    // Remove all cargo of this type from hold
                    CargoContents.Remove(cargoitem);
                }
                else
                {
                    // Remove only some cargo of this type
                    cargoitem.amount -= amount;
                }
                CargoOccupied -= amount;

                return;
            }
        }
    }

    /// <summary>
    /// Invoked to remove all of the cargo hold items.
    /// </summary>
    public void RemoveCargo()
    {
        CargoContents = new List<HoldItem>();
        CargoOccupied = 0;
    }

    /// <summary>
    /// Whether the cargobay contains a specified type of cargo.
    /// </summary>
    /// <param name="cargo">Cargo to find in cargobay</param>
    /// <returns>Given cargo type is present in cargobay</returns>
    private HoldItem GetCargo(string cargoType)
    {
        foreach (HoldItem cargoitem in CargoContents)
        {
            if (cargoitem.itemName == cargoType)
                return cargoitem;
        }
        return null;
    }


}
}