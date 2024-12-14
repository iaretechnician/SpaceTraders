using UnityEngine;

namespace SpaceSimFramework
{
public class Wreck : MonoBehaviour
{
    [Tooltip("Ship Model Info for used to get armor value.")]
    public ModelInfo WreckModelInfo;
    [Tooltip("Pick-uppable cargo container prefab.")]
    public GameObject CargoItemPrefab;
    [Tooltip("Names of the items droppable by wreck, corresponding to ObjectFactory Weapon names or Commodities names.")]
    public string[] CargoItemNames;
    [Tooltip("Numbers of items droppable by wreck, must be one number for each item above.")]
    public int[] CargoItemCounts;
    [Range(0,1)]
    [Tooltip("Chance of dropping each of the above items.")]
    public float DropChance;

    // Current armor
    public float Armor
    {
        get { return _armor; }
    }
    private float _armor;


    void Start()
    {
        _armor = WreckModelInfo.MaxArmor;
        if (CargoItemCounts.Length != CargoItemNames.Length)
            Debug.LogError("Wreck " + gameObject.name + " doesn't have equal lengths of Item and Count arrays!");
    }

    public void TakeDamage(float damage)
    {
        _armor -= damage;
        if (_armor <= 0)
        {
            ParticleController.Instance.CreateShipExplosionAtPos(transform.position);

            for(int i = 0; i < CargoItemNames.Length; i++)
            {
                if (Random.Range(0f, 1f) > DropChance)
                    continue;

                // Eject item to a random location
                Vector3 randomAddition = new Vector3(Random.Range(1, 5), Random.Range(1, 5), Random.Range(1, 5));

                GameObject cargo = GameObject.Instantiate(
                    CargoItemPrefab,
                    transform.position + randomAddition,
                    Quaternion.identity);

                // Determine whether dropping weapon or ware
                WeaponData weapon = ObjectFactory.Instance.GetWeaponByName(CargoItemNames[i]);
                cargo.GetComponent<CargoItem>().InitCargoItem(
                    weapon == null ? HoldItem.CargoType.Ware : HoldItem.CargoType.Weapon,
                    CargoItemCounts[i],
                    CargoItemNames[i]);
            }
            
            GameObject.Destroy(this.gameObject);
        }
    }
}
}