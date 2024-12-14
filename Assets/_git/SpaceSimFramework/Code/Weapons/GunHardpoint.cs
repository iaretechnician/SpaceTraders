using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Fixed direction gun hardpoint
/// </summary>
public class GunHardpoint : MonoBehaviour {

    // Weapon mounted in this hardpoint
    public WeaponData mountedWeapon;
    public float Range
    {
        get { return mountedWeapon == null ? 0 : mountedWeapon.Range; }
    }
    public float ProjectileSpeed
    {
        get { return mountedWeapon == null ? 0 : mountedWeapon.GetProjectileSpeed(); }
    }

    public GameObject Barrel;
    public ParticleSystem Particles;

    // Projectile pool, for each gun
    [HideInInspector]
    public List<GameObject> projectilePool;
    [HideInInspector]
    public int projectilePoolSize = 20;
    // For beam weapons
    [HideInInspector]
    public Beam weaponBeam;

    [HideInInspector]
    public float timer;
    [HideInInspector]
    public Ship ship;
    [HideInInspector]
    public AudioSource gunAudio;

    public void Awake()
    {
        gunAudio = GetComponent<AudioSource>();
        ship = GetComponentInParent<Ship>();
        projectilePool = new List<GameObject>(projectilePoolSize);
        RegisterHardpointWithShip();
    }

    protected void Start()
    {
        timer = 0;

        if (mountedWeapon == null)
            return; // Will be lazy-loaded later

        mountedWeapon.InitWeapon(this);
    }

    protected void FixedUpdate () {
        if(mountedWeapon)
            mountedWeapon.UpdateWeapon(this);
    }

    protected virtual void RegisterHardpointWithShip()
    {
        ship.Equipment.AddGun(this);
    }

    public virtual void OnTriggerFireGun(bool isPrimary)
    {
        if (mountedWeapon == null)
            return;

        if (ship == Ship.PlayerShip && Ship.PlayerShip.IsPlayerControlled && Ship.IsShipInputDisabled)
            return;

        // Projectile pool will not be initialized after jumping to the next system.
        if (isPrimary)
        {
            if ((mountedWeapon.Type == WeaponData.WeaponType.Projectile && projectilePool[0] == null) ||
                 (mountedWeapon.Type == WeaponData.WeaponType.Beam && weaponBeam == null))
            {
                SetWeapon(mountedWeapon);
            }
            if (mountedWeapon.Type == WeaponData.WeaponType.Beam || mountedWeapon.Type == WeaponData.WeaponType.Projectile)
            {
                mountedWeapon.OnTriggerFireWeapon(this, Barrel.transform.up);
            }
        }
        else
        {
            if (mountedWeapon.Type == WeaponData.WeaponType.Missile)
                mountedWeapon.OnTriggerFireWeapon(this, Barrel.transform.up);
        }
    }

    public void SetWeapon(WeaponData weapon)
    {
        if (weapon == null)
        {
            // Reset all weapon data
            mountedWeapon = null;
            if(projectilePool != null && projectilePool.Count == projectilePoolSize) { 
                for (int i = 0; i < projectilePoolSize; i++)
                {
                    if (projectilePool[i])
                        GameObject.Destroy(projectilePool[i]);
                }
            }
            if (weaponBeam != null)
                GameObject.Destroy(weaponBeam.gameObject);
        }
        else
            weapon.InitWeapon(this);

        mountedWeapon = weapon;
    }

}
}