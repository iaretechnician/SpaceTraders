using System.Collections.Generic;
using UnityEngine;
using TurretOrder = SpaceSimFramework.TurretCommands.TurretOrder;

namespace SpaceSimFramework
{
public partial class ShipEquipment : MonoBehaviour {

    #region weapons
    public List<GunHardpoint> Guns
    {
        get {
            if (_guns == null)
            {
                _guns = new List<GunHardpoint>();
            }
            return _guns;
        }
    }
    // Guns are player/ship controlled
    private List<GunHardpoint> _guns;
    // Turrets are individually/automatically controlled

    public List<TurretHardpoint> Turrets
    {
        get {
            if (_turrets == null)
            {
                _turrets = new List<TurretHardpoint>();
            }
            return _turrets;
        }
    }
    // Turrets are individually/automatically controlled
    private List<TurretHardpoint> _turrets;

    // Set by AI ship controller when firing conditions are met
    [HideInInspector] public bool IsFiring = false;

    private TurretOrder TurretCmd = TurretOrder.AttackEnemies;
    #endregion

    #region energy management
    [HideInInspector] public float energyCapacity;
    [HideInInspector] public float energyRegenRate;
    [HideInInspector] public float energyAvailable;
    #endregion

    #region equipment
    public List<Equipment> MountedEquipment
    {
        get { return _mountedEquipment; }
    }
    private List<Equipment> _mountedEquipment;
    #endregion equipment

    private Ship _ship;

    private void Awake()
    {
        _ship = gameObject.GetComponent<Ship>();
        energyCapacity = _ship.ShipModelInfo.GeneratorPower;
        energyRegenRate = _ship.ShipModelInfo.GeneratorRegen;
        _mountedEquipment = new List<Equipment>();
    }

    private void Update()
    {
        CheckWeaponInput();
        ComputeEnergyRegen();
        UpdateMountedEquipment();
    }

    #region weapons
    private void CheckWeaponInput()
    {
        // Player input
        if (Ship.PlayerShip != null && this.gameObject == Ship.PlayerShip.gameObject && !_ship.InSupercruise) {
            if (Ship.IsShipInputDisabled)
                return;

            if ((Input.GetMouseButton(1) || Input.GetKey(KeyCode.LeftControl)) && !CanvasViewController.IsMapActive)
            {
                foreach (GunHardpoint gun in Guns)
                    gun.OnTriggerFireGun(true);

                if (TurretCmd == TurretOrder.Manual)
                    foreach (TurretHardpoint turret in Turrets)
                        turret.OnTriggerFireGun(true);

                IsFiring = false;
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                foreach (GunHardpoint gun in Guns)
                    gun.OnTriggerFireGun(false);

                IsFiring = false;
            }

        }

        // AI input
        if (IsFiring)
        {
            foreach (GunHardpoint gun in Guns)
                gun.OnTriggerFireGun(true);

            IsFiring = false;
        }

    }

    /// <summary>
    /// Sets all turrets to a given state.
    /// </summary>
    /// <param name="order">New order issued to all turrets</param>
    public void SetTurretCommand(TurretOrder order)
    {
        TurretCmd = order;
        foreach (TurretHardpoint turret in Turrets)
            turret.Command = TurretCmd;
    }

    /// <summary>
    /// Get the range of the ship's forward mounted weapons array.
    /// </summary>
    /// <returns></returns>
    public float GetWeaponRange()
    {
        foreach (GunHardpoint gun in Guns)
            if(gun.mountedWeapon != null)
                return gun.mountedWeapon.Range;

        return 0;
    }

    /// <summary>
    /// Adds a gun to the weapons control of this ship. Should be invoked by the hardpoint itself upon start.
    /// </summary>
    public void AddGun(GunHardpoint gun)
    {
        Guns.Add(gun);
    }

    /// <summary>
    /// Adds a turret to the weapons control of this ship. Should be invoked by the hardpoint itself upon start.
    /// </summary>
    public void AddTurret(TurretHardpoint turret)
    {
        Turrets.Add(turret);
    }
    #endregion weapons

    #region energy

    /// <summary>
    /// Apply the energy drain caused by firing the weapon by reducing the available power 
    /// </summary>
    /// <param name="drain">Amount of energy used by the weapon fired</param>
    public void WeaponFired(float drain)
    {
        if(_ship.faction == Player.Instance.PlayerFaction)
            energyAvailable = Mathf.Clamp(energyAvailable - drain, 0, energyCapacity);
        else
            energyAvailable = Mathf.Clamp(energyAvailable - drain*1.5f, 0, energyCapacity);
    }

    private void ComputeEnergyRegen()
    {
        if (_ship.InSupercruise)
            return;
        energyAvailable = Mathf.Clamp(energyAvailable + Time.deltaTime * energyRegenRate, 0, energyCapacity);
    }

    public void SupercruiseDrain()
    {
        if(energyAvailable > 0)
            energyAvailable = Mathf.Clamp(energyAvailable - Time.deltaTime * energyRegenRate * 3, 0, energyCapacity);
    }
    #endregion energy

    #region equipment
    private void UpdateMountedEquipment()
    {
        // Apply all mounted items
        for(int i=0; i<_mountedEquipment.Count; i++)
        {
            _mountedEquipment[i].UpdateItem(_ship);
        }
    }

    /// <summary>
    /// Mounts the specified equipment on the ship, filling an equipment slot
    /// </summary>
    /// <param name="item">Equipment item to mount</param>
    public bool MountEquipmentItem(Equipment item)
    {
        // Check if all slots are full
        if(_mountedEquipment.Count < _ship.ShipModelInfo.EquipmentSlots)
        {
            _mountedEquipment.Add(item);
            item.InitItem(_ship);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes the equipment item from the ship. This is invoked when selling equipment
    /// and when saving game.
    /// </summary>
    /// <param name="item">Equipment item to unmount</param>
    public void UnmountEquipmentItem(Equipment item)
    {
        _mountedEquipment.Remove(item);
        item.RemoveItem(_ship);
    }
    #endregion equipment
}
}