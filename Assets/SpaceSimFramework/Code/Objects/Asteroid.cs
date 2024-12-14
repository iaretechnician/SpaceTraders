using UnityEngine;

namespace SpaceSimFramework
{
public class Asteroid : MonoBehaviour {

    public int Yield = 0;
    public float Health = 1000;
    public string Resource = "";
    public GameObject CargoItemPrefab;
	
    public void TakeDamage(float damage)
    {
        Health -= damage;
        if(Health <= 0)
        {
            ParticleController.Instance.CreateShipExplosionAtPos(transform.position);

            if (Resource == null || Resource == "")
            {
                GameObject.Destroy(this.gameObject);
                return;
            }

            // Drop random cargo items from the ones available
            Vector3 randomAddition = new Vector3(Random.Range(1, 5), Random.Range(1, 5), Random.Range(1, 5));

            // Eject item to a random location
            GameObject cargo = GameObject.Instantiate(
                CargoItemPrefab,
                transform.position + randomAddition,
                Quaternion.identity);

            cargo.GetComponent<CargoItem>().InitCargoItem(HoldItem.CargoType.Ware, Yield, Resource);            
            GameObject.Destroy(this.gameObject);
        }
    }

    public void ApplyMaterial(Material mat)
    {
        foreach (MeshRenderer lod in GetComponentsInChildren<MeshRenderer>())
        {
            lod.material = mat;
        }
    }
}
}