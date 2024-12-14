using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
[CreateAssetMenu(menuName = "Weapons/ProjectileWeaponData")]
public class ProjectileWeaponData : WeaponData{

    [Tooltip("Minimum time between shots")]
    public float ReloadTime;
    [Tooltip("Projectile speed in units per second")]
    public float ProjectileSpeed;
    [Tooltip("Damage per shot")]
    public int Damage;
    [Tooltip("Projectile prefab")]
    public GameObject Projectile;
    [Tooltip("Energy usage per shot")]
    public int EnergyDrain;

    public override float GetProjectileSpeed()
    {
        return ProjectileSpeed;
    }

    public override void InitWeapon(GunHardpoint hardpoint)
    {
            // Use a monobehaviour to start a coroutine
            SectorNavigation.Instance.StartCoroutine(ProjectileInstantiator(hardpoint));
    }

    /// <summary>
    /// This coroutine is used to spread the instantiation of projectiles through many frames.
    /// This will hopefully reduce lagspikes.
    /// </summary>
    public IEnumerator ProjectileInstantiator(GunHardpoint hardpoint)
    {
        hardpoint.projectilePool = new List<GameObject>();
        hardpoint.projectilePoolSize = (int)(Range / (ProjectileSpeed * ReloadTime)) + 1;   // Max number of simultaneous projectiles 
        yield return null;
        for (int i = 0; i < hardpoint.projectilePoolSize; i++)
        {
            GameObject proj = GameObject.Instantiate(Projectile);
            // Mark if projectile belongs to player (to record kills and change rep)
            proj.GetComponent<Projectile>().PlayerShot = hardpoint.ship.faction == Player.Instance.PlayerFaction;
            hardpoint.projectilePool.Add(proj);
            proj.SetActive(false);

            if (hardpoint.ship == null)
                break;

            Physics.IgnoreCollision(hardpoint.ship.GetComponent<Collider>(), hardpoint.projectilePool[i].GetComponent<Collider>(), true);
            yield return null;
        }
        yield return null;
    }

    public override void OnTriggerFireWeapon(GunHardpoint hardpoint, Vector3 forwardVector)
    {
        if (hardpoint.timer <= 0)
        {
            Fire(hardpoint, forwardVector);
            hardpoint.timer = ReloadTime;
        }
    }

    public override void UpdateWeapon(GunHardpoint hardpoint)
    {
        if (hardpoint.timer > 0)
            hardpoint.timer -= Time.deltaTime;
    }

    public override void Fire(GunHardpoint hardpoint, Vector3 forwardVector)
    {
        // Check available energy
        if (hardpoint.ship.Equipment.energyAvailable < this.EnergyDrain)
            return;

        forwardVector += new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f)*0.02f;
        if (hardpoint.ship.faction != Player.Instance.PlayerFaction)
            forwardVector *= 2f;

        // Find first unused projectile
        for (int i = 0; i < hardpoint.projectilePoolSize; i++)
        {
            if (!hardpoint.projectilePool[i].activeInHierarchy)
            {

                hardpoint.projectilePool[i].transform.position = hardpoint.Barrel.transform.position + forwardVector * 7;
                hardpoint.projectilePool[i].transform.rotation = hardpoint.Barrel.transform.rotation;
                hardpoint.projectilePool[i].SetActive(true);
                Projectile proj = hardpoint.projectilePool[i].GetComponent<Projectile>();
                proj.FireProjectile(forwardVector, ProjectileSpeed, Range, Damage);

                hardpoint.Particles.Play();
                hardpoint.gunAudio.Play();
                hardpoint.ship.Equipment.WeaponFired(EnergyDrain);

                return;
            }
        }
    }
}
}