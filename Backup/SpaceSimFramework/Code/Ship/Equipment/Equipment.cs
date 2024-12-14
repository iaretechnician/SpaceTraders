using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// This is a mountable equipment item which can be installed on a ship. It applies
/// certain effects to the carrier ship.
/// </summary>
public abstract class Equipment: ScriptableObject
{
    // Cost of the item
    public abstract int Cost { get; }
    [TextArea]
    public string Description;
    public bool isActivateable = false;
    public bool isSingleUse = false;

    /// <summary>
    /// Initializes the mounted piece of equipment by applying the initial effects of the
    /// item to the ship.
    /// </summary>
    public abstract void InitItem(Ship ship);

    /// <summary>
    /// Removes the mounted piece of equipment by disabling its effects on the ship.
    /// </summary>
    public abstract void RemoveItem(Ship ship);

    /// <summary>
    /// Applies the effects of this equipment item to the ship it's mounted on.
    /// This method is called from the ship's ShipEquipment component.
    /// </summary>
    public abstract void UpdateItem(Ship ship);

}
}