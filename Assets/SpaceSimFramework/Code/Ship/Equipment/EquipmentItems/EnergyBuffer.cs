using UnityEngine;

namespace SpaceSimFramework
{
[CreateAssetMenu(menuName = "Equipment/EnergyBuffer")]
public class EnergyBuffer : Equipment
{
    public float RegenerationIncreaseFactor;
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
        ship.Equipment.energyRegenRate *= RegenerationIncreaseFactor;
    }

    public override void RemoveItem(Ship ship)
    {
        ship.Equipment.energyRegenRate = (int)(ship.Equipment.energyRegenRate/RegenerationIncreaseFactor);
    }

    public override void UpdateItem(Ship ship)
    {
    }

}
}