using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TurretOrder = SpaceSimFramework.TurretCommands.TurretOrder;

namespace SpaceSimFramework
{
/// <summary>
/// Automatic revolute gun hardpoint in a semispherical area of effect
/// </summary>
public class TurretHardpoint : GunHardpoint
{
    public TurretRotation turretController;
    [HideInInspector] public TurretOrder Command = TurretOrder.AttackEnemies;

    private Transform target;

    protected void FixedUpdate()
    {
        // Empty hardpoint?
        if (mountedWeapon == null)
            return;

        base.FixedUpdate();

        // Idle turrets while in supercruise mode
        if (ship.GetComponent<Ship>().InSupercruise)
            return;

        switch (Command)
        {
            case TurretOrder.None:
                turretController.SetIdle(true);
                return;
            case TurretOrder.AttackEnemies:
                {
                    turretController.SetIdle(false);
                    if (target != null && TargetInRange(target.position))
                    {
                        AttackTarget();
                    }
                    else
                    {
                        var hostiles = SectorNavigation.Instance.GetClosestEnemyShip(ship.transform, Range);

                        if (hostiles.Count > 0)
                        {
                            turretController.SetIdle(false);
                            target = hostiles[0].transform;
                            turretController.SetAimpoint(target.position);
                        }
                        else
                            turretController.SetIdle(true);
                    }
                    break;
                }
            case TurretOrder.AttackTarget:
                {
                    turretController.SetIdle(false);
                    if (InputHandler.Instance.GetCurrentSelectedTarget() != null)
                    {
                        target = InputHandler.Instance.GetCurrentSelectedTarget().transform;
                        if (target != null && TargetInRange(target.position))
                        {
                            AttackTarget();
                        }
                    }
                    break;
                }
            case TurretOrder.Manual:
                {
                    var target = InputHandler.Instance.GetCurrentSelectedTarget();
                    var distance = Range;
                    if (target != null)
                        distance = Vector3.Distance(Ship.PlayerShip.transform.position, target.transform.position);

                    turretController.SetIdle(false);
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Vector3 aimPosition = ray.origin + (ray.direction * distance);
                    // Aim assist ===
                    if(target != null) {
                        Vector3 targetPosition = Targeting.PredictTargetLead3D(transform, target.gameObject, ProjectileSpeed);
                        if(Vector3.Distance(aimPosition, targetPosition) < 20)
                            turretController.SetAimpoint(targetPosition);
                        else
                            turretController.SetAimpoint(aimPosition);
                    }
                    else
                    {
                        turretController.SetAimpoint(aimPosition);
                    }
                    // Aim assist ===
                    break;
                }
            default:
                return;
        }
    }

    protected override void RegisterHardpointWithShip()
    {
        ship.Equipment.AddTurret(this);
    }

    public override void OnTriggerFireGun(bool isPrimary)
    {
        if (mountedWeapon) {
            if (Command == TurretOrder.Manual)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 aimPosition = ray.origin + (ray.direction * Range);
                if (Vector3.Angle(Barrel.transform.forward, aimPosition - Barrel.transform.position) > 10)
                {
                    return;
                }
            }

            // Projectile pool will not be initialized after jumping to the next system.
            if (mountedWeapon.Type == WeaponData.WeaponType.Projectile && projectilePool[0] == null)
                SetWeapon(mountedWeapon);
            if (mountedWeapon.Type == WeaponData.WeaponType.Beam && weaponBeam == null)
                SetWeapon(mountedWeapon);


            mountedWeapon.OnTriggerFireWeapon(this, Barrel.transform.forward);
        }
    }

    public void AttackTarget()
    {
        Vector3 pos = Targeting.PredictTargetLead3D(transform, target.gameObject, ProjectileSpeed);
        turretController.SetAimpoint(pos);
        // Check if guns are on target
        float angle = Vector3.Angle(turretController.turretBarrels.transform.forward, pos - transform.position);

        if (angle < 10 && mountedWeapon != null)
            mountedWeapon.OnTriggerFireWeapon(this, Barrel.transform.forward);
    }

    public bool TargetInRange(Vector3 target)
    {
        if (Vector3.Distance(target, transform.position) < Range)
            return true;
        else
            return false;
    }

}
}