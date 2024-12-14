using UnityEngine;

namespace SpaceSimFramework
{
[CreateAssetMenu(menuName = "Equipment/RegenerativeStasisField")]
public class RegenStasisField : ActivatableEquipment
{
    public int ItemCost;
    public float Duration;
    public float RegenPercentage;

    private float _timer;
    private float _regenAmount, _initialArmor;

    public override int Cost
    {
        get
        {
            return ItemCost;
        }
    }


    public override void InitItem(Ship ship)
    {
        base.InitItem(ship);
        isSingleUse = true;
        _isActive = false;       
    }

    public override void RemoveItem(Ship ship)
    {
        base.RemoveItem(ship);
    }

    public override void UpdateItem(Ship ship)
    {
        if (_isActive)
        {
            _timer += Time.deltaTime;
            ship.Armor = _initialArmor + _timer / Duration * _regenAmount;
            ship.Armor = Mathf.Clamp(ship.Armor, 0, ship.MaxArmor);

            if (_timer > Duration)
            {
                // When finished with operation
                RemoveItem(ship);
            }
        }
    }

    public override bool SetActive(bool isActive, Ship ship)
    {
        // Dont use item if there is nothing to heal
        if (ship.Armor == ship.MaxArmor)
            return false;

        _isActive = true;
        _timer = 0;
        _regenAmount = RegenPercentage * ship.MaxArmor * 0.01f;
        _initialArmor = ship.Armor;
        return true;
    }
}
}