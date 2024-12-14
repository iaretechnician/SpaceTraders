namespace SpaceSimFramework
{
public abstract class ActivatableEquipment: Equipment
{
    protected bool _isActive;

    /// <summary>
    /// Activates or deactivates the item. Item must be activatable (isActivatable = true)
    /// </summary>
    /// <returns>Whether the item has changed state</returns>
    public abstract bool SetActive(bool active, Ship ship);

    public bool IsActive()
    {
        return _isActive;
    }

    public override void InitItem(Ship ship)
    {
        isActivateable = true;
    }

    public override void RemoveItem(Ship ship)
    {
        if (isSingleUse)
        {
            ship.Equipment.MountedEquipment.Remove(this);
            EquipmentIconUI.Instance.SetIconsForShip(ship);
        }
    }
}
}