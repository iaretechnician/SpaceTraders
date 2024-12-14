using UnityEngine;

namespace SpaceSimFramework
{
[CreateAssetMenu(menuName = "Equipment/Armor")]
public class AppliqueArmor : Equipment
{
    public float ArmorMultiplier;
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
        ship.MaxArmor = (int)(ship.MaxArmor * ArmorMultiplier);
        ship.Armor = Mathf.Clamp(ship.Armor * ArmorMultiplier, 0, ship.MaxArmor);
    }

    public override void RemoveItem(Ship ship)
    {
        ship.MaxArmor = (int)(ship.MaxArmor / ArmorMultiplier);
        ship.Armor = Mathf.Clamp(ship.Armor / ArmorMultiplier, 0, ship.MaxArmor);
    }

    public override void UpdateItem(Ship ship)
    {
    }

}
}