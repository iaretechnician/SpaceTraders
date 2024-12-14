using System;
using UnityEngine;

namespace SpaceSimFramework
{
// Missile guidance system
public class Missile : MonoBehaviour
{
    private static float ARM_TIME = 1f;

    private PIDController pid_angle, pid_velocity;
    private float pid_P = 2, pid_I = 0.8f, pid_D = 0.8f;

    // Properties
    private float missileSpeed;
    private int damage;
    private Color trailColor;
    private float turnRate;
    private bool isGuided;
    private float range;

    // Local members
    private Transform target;
    private Rigidbody rBody;
    private Vector3 angularTorque;
    private bool isPlayerShot = false;
    private float timer;
    private Vector3 lastPos;
    private float distanceTravelled = 0;

    public void FireProjectile(MissileWeaponData missileWeaponData, Transform target, bool isPlayerShot)
    {
        missileSpeed = missileWeaponData.MissileSpeed;
        damage = missileWeaponData.Damage;
        trailColor = missileWeaponData.TrailColor;
        turnRate = missileWeaponData.TurnRate;
        isGuided = missileWeaponData.IsGuided;
        range = missileWeaponData.Range;
        this.target = target;
        this.isPlayerShot = isPlayerShot;

        rBody = gameObject.GetComponent<Rigidbody>();
        pid_angle = new PIDController(pid_P, pid_I, pid_D);
        pid_velocity = new PIDController(pid_P, pid_I, pid_D);
        lastPos = transform.position;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        distanceTravelled += Vector3.Distance(lastPos, transform.position);
        if (distanceTravelled > range)
            GameObject.Destroy(gameObject);
        lastPos = transform.position;

        if (target == null)
            return;

        // Turn missile towards target
        if (isGuided)
        {
            SteerTowardsDestination(Targeting.PredictTargetLead3D(gameObject, target.gameObject, missileSpeed));
        }
        else
        {
            SteerTowardsDestination(target.position);
        }
    }

    void FixedUpdate()
    {
        if (rBody != null)
        {
            rBody.AddRelativeForce(new Vector3(0, 0, missileSpeed), ForceMode.Force);
            rBody.AddRelativeTorque(ShipPhysics.ClampVector3(angularTorque, -Vector3.one * turnRate, Vector3.one*turnRate), ForceMode.Force);
        }
    }

    private void SteerTowardsDestination(Vector3 destination)
    {
        float distance = Vector3.Distance(destination, transform.position);

        if (distance > 10)
        {
            Vector3 angularVelocityError = rBody.angularVelocity * -1;
            Vector3 angularVelocityCorrection = pid_velocity.Update(angularVelocityError, Time.deltaTime);

            Vector3 lavc = transform.InverseTransformVector(angularVelocityCorrection);

            Vector3 desiredHeading = destination - transform.position;
            Vector3 currentHeading = transform.forward;
            Vector3 headingError = Vector3.Cross(currentHeading, desiredHeading);
            Vector3 headingCorrection = pid_angle.Update(headingError, Time.deltaTime);

            // Convert angular heading correction to local space to apply relative angular torque
            Vector3 lhc = transform.InverseTransformVector(headingCorrection * 200f);

            angularTorque = lavc + lhc;
        }
        else
        {
            angularTorque = Vector3.zero;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (timer > ARM_TIME)
        {
            ParticleController.Instance.CreateParticleEffectAtPos(transform.position);

            if (other.gameObject.tag == "Ship")
            {
                other.gameObject.GetComponent<Ship>().TakeDamage(damage, isPlayerShot);
            }
            else if (other.gameObject.tag == "Asteroid")
            {
                other.gameObject.GetComponent<Asteroid>().TakeDamage(damage);
            }

            GameObject.Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        ParticleSystem particleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        particleSystem.transform.parent = null;
        Destroy(particleSystem.gameObject, 5.0f);
    }
}
}