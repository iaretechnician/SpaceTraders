using System.Collections;
using System.Collections.Generic;

namespace SpaceSimFramework
{
/// <summary>
/// Class describing items in the cargo hold
/// </summary>
public class HoldItem
{
    public enum CargoType
    {
        Weapon, Ware
    }

    public CargoType cargoType;
    public string itemName;
    public int amount;

    public HoldItem(CargoType type, string item, int amount)
    {
        this.cargoType = type;
        this.itemName = item;
        this.amount = amount;
    }

    public HoldItem(string type, string item, int amount)
    {
        if (type == "Weapon")
            cargoType = CargoType.Weapon;
        else
            cargoType = CargoType.Ware;

        this.itemName = item;
        this.amount = amount;
    }

    public override bool Equals(object obj)
    {
        HoldItem other = (HoldItem)obj;

        if (itemName == other.itemName && amount == other.amount)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Compares the cargo type and tells whether the type equals.
    /// </summary>
    /// <param name="other">HoldItem to compare to</param>
    /// <returns>True if the type of the cargo is same</returns>
    public bool SameTypeAs(HoldItem other)
    {
        return (itemName == other.itemName) ? true : false;
    }
}
}