using UnityEngine;

namespace SpaceSimFramework
{
public class AsteroidItem : CargoItem
{
    public GameObject ParticleAttractorEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ship")
        {
            other.gameObject.GetComponent<ShipCargo>().AddCargoItem(this);
            var attractor = GameObject.Instantiate(ParticleAttractorEffect, transform.position, transform.rotation);
            attractor.GetComponent<ParticleAttractorLinear>().Target = other.gameObject.transform;
            GameObject.Destroy(this.gameObject);
        }
    }

}
}