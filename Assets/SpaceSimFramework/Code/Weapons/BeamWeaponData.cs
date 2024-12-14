using UnityEngine;

namespace SpaceSimFramework
{
[CreateAssetMenu(menuName = "Weapons/BeamWeaponData")]
public class BeamWeaponData : WeaponData
{
    [Tooltip("Energy drain per second")]
    public float EnergyDrainPerSecond;
    [Tooltip("Damage per second")]
    public int DPS;
    [Tooltip("Beam line prefab")]
    public GameObject BeamPrefab;

    public override float GetProjectileSpeed()
    {
        return int.MaxValue;
    }

    public override void InitWeapon(GunHardpoint hardpoint)
    {
        hardpoint.weaponBeam = GameObject.Instantiate(BeamPrefab).GetComponent<Beam>();
        // Mark if projectile belongs to player (to record kills and change rep)
        hardpoint.weaponBeam.PlayerShot = hardpoint.ship.faction == Player.Instance.PlayerFaction;
        hardpoint.weaponBeam.gameObject.SetActive(false);
    }

    public override void OnTriggerFireWeapon(GunHardpoint hardpoint, Vector3 forwardVector)
    {
        Fire(hardpoint, forwardVector);
    }

    public override void UpdateWeapon(GunHardpoint hardpoint)
    {
        if (hardpoint.timer > 0)
            hardpoint.timer -= Time.deltaTime;
        else {
            hardpoint.gunAudio.Stop();
            hardpoint.weaponBeam.gameObject.SetActive(false);
        }

    }

    public override void Fire(GunHardpoint hardpoint, Vector3 forwardVector)
    {
        // Check available energy against minimum beam initiation threshold
        if (hardpoint.ship.Equipment.energyAvailable < 10.0f)
            return;

        // Half second till the effect is off
        hardpoint.timer = 0.2f;


        Beam beam = hardpoint.weaponBeam;
        beam.FireProjectile(forwardVector, Range, DPS);
        beam.gameObject.SetActive(true);

        beam.transform.position = hardpoint.Barrel.transform.position;

        hardpoint.Particles.Play();
        if(!hardpoint.gunAudio.isPlaying)
            hardpoint.gunAudio.Play();
        hardpoint.ship.Equipment.WeaponFired(EnergyDrainPerSecond*Time.deltaTime);

    }
}
}