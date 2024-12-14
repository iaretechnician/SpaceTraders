using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Cargo container with a hold item, can be picked up by ships.
/// </summary>
public class CargoItem : MonoBehaviour {

    public HoldItem item;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Ship")
        {
            other.gameObject.GetComponent<ShipCargo>().AddCargoItem(this);
            GameObject.Destroy(this.gameObject);
        }
    }

    public void InitCargoItem(HoldItem.CargoType type, int numOfItems, string itemName)
    {
        item = new HoldItem(type, itemName, numOfItems);

        gameObject.name = itemName + " (" + numOfItems + ")";
        SectorNavigation.Cargo.Add(this.gameObject);
    }

    private void OnDestroy()
    {
        SectorNavigation.Cargo.Remove(this.gameObject);
    }
}
}