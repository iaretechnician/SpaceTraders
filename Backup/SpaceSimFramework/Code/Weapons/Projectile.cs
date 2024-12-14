using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
public class Projectile : MonoBehaviour {

    public TrailRenderer Trail;

    private float range;
    private Vector3 initialPos;
    private int damage;

    // To prevent ships from shooting themselves...
    private float minRange = 15f;
    private SphereCollider projCollider;

    public bool PlayerShot = false;

    private void Awake()
    {
        projCollider = GetComponent<SphereCollider>();
        projCollider.enabled = false;
    }

    void Update () {
        float distanceTravelled = Vector3.Distance(initialPos, transform.position);

        if(distanceTravelled > minRange)
        {
            projCollider.enabled = true;
        }
		if(distanceTravelled > range)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            gameObject.SetActive(false);
        }
	}

    public void FireProjectile(Vector3 direction, float force, float range, int dmg)
    {

        GetComponent<Rigidbody>().AddForce(direction * force, ForceMode.Impulse);        
        // To prevent ships from shooting themselves...
        this.range = range;
        this.initialPos = transform.position;
        this.damage = dmg;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!projCollider.enabled)
            return;

        ParticleController.Instance.CreateParticleEffectAtPos(collision.contacts[0].point);

        if(collision.gameObject.tag == "Ship")
        {
            collision.gameObject.GetComponent<Ship>().TakeDamage(damage, PlayerShot);
        }
        else if (collision.gameObject.tag == "Asteroid")
        {
            collision.gameObject.GetComponent<Asteroid>().TakeDamage(damage);
        }
        else if (collision.gameObject.tag == "Wreck")
        {
            collision.gameObject.GetComponent<Wreck>().TakeDamage(damage);
        }

        GetComponent<Rigidbody>().velocity = Vector3.zero;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if(Trail)
            Trail.Clear();
    }
}
}