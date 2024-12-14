using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Uses nebula puffs to render the nebula when flying through it. Spawns puffs
/// when camera flies into the nebula.
/// </summary>
[RequireComponent(typeof(Nebula))]
public class NebulaPuffs : MonoBehaviour
{
    public SpaceParticleQuad Particle;
    public bool EmitPuffs = false;

    public Color PuffColor = Color.white;

    public int Count = 20;
    public float Radius = 150f;
    public float SizeVariation = 0.2f;
    public float MaxDistance = 400f;
    public float Drift = 1f;
    public float DriftRotation = 5f;
    public float NearFadeDistance = 125f;
    public float FarFadeDistance = 150f;

    private Transform _refCamTransform;
    private SpaceParticlePool _pool;

    void Awake()
    {
        _refCamTransform = Camera.main.transform;
    }

    void Start()
    {
        _pool = new SpaceParticlePool(Count, Particle, transform);
    }

    void Update()
    {
        
        Vector3 pos = Vector3.zero;
        Vector3 startVel = Vector3.zero;
        float startRadius = Radius;
        float startRot = 0f;

        // Find a random point in front of the camera and instantiate a particle there.
        while (_pool.CheckAvailable())
        {
            // Sphere method.

            // Generate the sphere with respect to the camera's velocity. If sitting still,
            // then the sphere is centered. If the camera moves, then the sphere is biased in
            // that direction.
            pos = Random.onUnitSphere * MaxDistance * 0.999f;

            startVel = Random.onUnitSphere * Random.Range(-Drift, Drift);
            startRot = Random.Range(-DriftRotation, DriftRotation);

            startRadius = Radius * Random.Range(1f - SizeVariation, 1f + SizeVariation);

            // Convert all these local coordinates into world coordinates.
            pos = _refCamTransform.TransformPoint(pos);

            // DEBUG 
            //Debug.DrawLine(refCamTransform.position, pos, Color.red);

            // Fade the particles in based on how dense the nebula is at this point.
            float densityFade = 1;

            // Create the particle.
            _pool.ActivateParticle(pos, startVel, startRot, startRadius, MaxDistance, NearFadeDistance, FarFadeDistance, PuffColor, densityFade);
        }
    }

   
}
}