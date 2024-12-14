using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Lists equipment carried on a ship.
/// </summary>
[CreateAssetMenu(menuName = "DataHolders/ShipLoadout")]
public class ShipLoadout : ScriptableObject {

    public string ShipModel;
    public WeaponData[] ShipGuns, ShipTurrets;
    public Equipment[] ShipEquipment;

    /// <summary>
    /// Gives a specific loadout to the specified ship
    /// </summary>
    /// <param name="loadout">The loadout to be applied on the ship</param>
    /// <param name="ship">The ship to receive the loadout</param>
    public static void ApplyLoadoutToShip(ShipLoadout loadout, Ship ship)
    {
        if (loadout == null)
            return;

        if (ship.ShipModelInfo.ModelName != loadout.ShipModel) { 
            Debug.LogWarning("Warning: Trying to apply " + loadout.ShipModel +
                " loadout to " + ship.ShipModelInfo.ModelName);
            return;
        }

        // By this point ship has either applied loadout or default loadout (which is FIIIINE.)

        int gun_i, turr_i;
        for (gun_i = 0; gun_i < ship.Equipment.Guns.Count; gun_i++)
        {
            ship.Equipment.Guns[gun_i].SetWeapon(loadout.ShipGuns[gun_i]);
        }
        for (turr_i = 0; turr_i < ship.Equipment.Turrets.Count; turr_i++)
        {
            ship.Equipment.Turrets[turr_i].SetWeapon(loadout.ShipTurrets[turr_i]);
        }

        foreach (var equipmentItem in loadout.ShipEquipment)
            ship.Equipment.MountEquipmentItem(equipmentItem);
    }

}
}