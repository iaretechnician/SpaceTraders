using UnityEngine;

namespace SpaceSimFramework
{
[CreateAssetMenu(menuName = "Equipment/EnergyCapacitor")]
public class EnergyCapacitor : Equipment
{
    public float CapacityIncreaseFactor;
    public int ItemCost;

    public override int Cost
    {
        get
        {
            return ItemCost;
        }
    }

    

    public override void InitItem(Ship ship)
    {
        ship.Equipment.energyCapacity *= CapacityIncreaseFactor;
    }

    public override void RemoveItem(Ship ship)
    {
        ship.Equipment.energyCapacity = (int)(ship.Equipment.energyCapacity / CapacityIncreaseFactor);
    }

    public override void UpdateItem(Ship ship)
    {
    }

}
}