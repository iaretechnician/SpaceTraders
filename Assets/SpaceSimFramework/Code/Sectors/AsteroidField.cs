using UnityEngine;

namespace SpaceSimFramework
{
public enum RandomSpawnerShape
{
    Box,
    Sphere,
}

public enum FieldType
{
    Rock,
    Ice,
    Scrap
}


// Used mostly for testing to provide stuff to fly around and into.
public class AsteroidField : MonoBehaviour
{   
    // Unique in-game object ID 
    [Tooltip("DO NOT touch this if you don't have to")]
    [Header("Sector object ID")]
    public string ID;

    [Header("General settings:")]

    [Tooltip("Prefab to spawn.")]
    public GameObject[] OrePrefabs;
    public Material OreMaterial;
    public GameObject[] IcePrefabs;
    public Material IceMaterial;
    public GameObject[] ScrapPrefabs;
    public Material ScrapMaterial;

    [Tooltip("Type of field to be created.")]
    public FieldType FieldType;

    [Tooltip("Shape to spawn the prefabs in.")]
    public RandomSpawnerShape spawnShape = RandomSpawnerShape.Sphere;

    [Tooltip("Multiplier for the spawn shape in each axis.")]
    public Vector3 shapeModifiers = Vector3.one;

    [Tooltip("How many prefab to spawn.")]
    public int asteroidCount = 50;

    [Tooltip("Distance from the center of the gameobject that prefabs will spawn")]
    public float range = 1000.0f;

    [Tooltip("Should prefab have a random rotation applied to it.")]
    public bool randomRotation = true;

    [Tooltip("Random min/max scale to apply.")]
    public Vector2 scaleRange = new Vector2(1.0f, 3.0f);

    [Header("Rigidbody settings:")]

    [Tooltip("Apply a velocity from 0 to this value in a random direction.")]
    public float velocity = 0.0f;

    [Tooltip("Apply an angular velocity (deg/s) from 0 to this value in a random direction.")]
    public float angularVelocity = 0.0f;

    [Tooltip("If true, raise the mass of the object based on its scale.")]
    public bool scaleMass = true;

    [Header("Mining properties")]

    [Tooltip("If null field is not mineable, otherwise enter commodity name")]
    public string MineableResource = "";

    [Tooltip("Minimum and maximum yield of asteroids (if mineable) in cargo units. Must be integer.")]
    public Vector2 YieldMinMax;

    // Use this for initialization
    void Start()
    {
        switch (FieldType)
        {
            case FieldType.Rock:
                if (OrePrefabs.Length > 0)
                {
                    for (int i = 0; i < asteroidCount; i++)
                        CreateAsteroid(OrePrefabs, OreMaterial);
                }
                return;
            case FieldType.Ice:
                if (IcePrefabs.Length > 0)
                {
                    for (int i = 0; i < asteroidCount; i++)
                        CreateAsteroid(IcePrefabs, IceMaterial);
                }
                return;
            case FieldType.Scrap:
                if (ScrapPrefabs.Length > 0)
                {
                    for (int i = 0; i < asteroidCount; i++)
                        CreateAsteroid(ScrapPrefabs, ScrapMaterial);
                }
                return;
        }
       
    }

    private void CreateAsteroid(GameObject[] prefabs, Material material)
    {
        Vector3 spawnPos = Vector3.zero;
         
        // Create random position based on specified shape and range.
        if (spawnShape == RandomSpawnerShape.Box)
        {
            spawnPos.x = Random.Range(-range, range) * shapeModifiers.x;
            spawnPos.y = Random.Range(-range, range) * shapeModifiers.y;
            spawnPos.z = Random.Range(-range, range) * shapeModifiers.z;
        }
        else if (spawnShape == RandomSpawnerShape.Sphere)
        {
            spawnPos = Random.insideUnitSphere * range;
            spawnPos.x *= shapeModifiers.x;
            spawnPos.y *= shapeModifiers.y;
            spawnPos.z *= shapeModifiers.z;
        }

        // Offset position to match position of the parent gameobject.
        spawnPos += transform.position;

        // Apply a random rotation if necessary.
        Quaternion spawnRot = (randomRotation) ? Random.rotation : Quaternion.identity;

        // Create the object and set the parent to this gameobject for scene organization.
        var index = Random.Range(0, prefabs.Length - 1);
        GameObject t = Instantiate(prefabs[index], spawnPos, spawnRot) as GameObject;
        t.transform.SetParent(transform);
        t.name = prefabs[index].name;

        // Asteroid properties
        Asteroid asteroid = t.GetComponent<Asteroid>();
        asteroid.Yield = Random.Range((int)YieldMinMax.x, (int)YieldMinMax.y);
        asteroid.Resource = MineableResource;
        asteroid.ApplyMaterial(material);

        // Apply scaling.
        float scale = Random.Range(scaleRange.x, scaleRange.y);
        t.transform.localScale = Vector3.one * scale;

        // Apply rigidbody values.
        Rigidbody r = t.GetComponent<Rigidbody>();
        if (r)
        {
            if (scaleMass)
                r.mass *= scale * scale * scale;

            r.AddRelativeForce(Random.insideUnitSphere * velocity, ForceMode.VelocityChange);
            r.AddRelativeTorque(Random.insideUnitSphere * angularVelocity * Mathf.Deg2Rad, ForceMode.VelocityChange);
        }
    }
}
}