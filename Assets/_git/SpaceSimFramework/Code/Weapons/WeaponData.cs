using UnityEngine;

namespace SpaceSimFramework
{
public abstract class WeaponData : ScriptableObject
{
    public enum WeaponType
    {
        Projectile, Beam, Area, Missile
    }

    public WeaponType Type;
    public float Range;
    public int Cost;
    public int Class;
    [TextArea]
    public string Description;

    public abstract void OnTriggerFireWeapon(GunHardpoint hardpoint, Vector3 forwardVector);
    public abstract void UpdateWeapon(GunHardpoint hardpoint);
    public abstract void InitWeapon(GunHardpoint hardpoint);
    public abstract void Fire(GunHardpoint hardpoint, Vector3 forwardVector);
    public abstract float GetProjectileSpeed();
}
}