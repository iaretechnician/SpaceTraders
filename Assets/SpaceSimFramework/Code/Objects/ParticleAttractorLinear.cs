using UnityEngine;

namespace SpaceSimFramework
{
[RequireComponent(typeof(ParticleSystem))]
public class ParticleAttractorLinear : MonoBehaviour {

	public Transform Target;
	public float Force = 0.5f;

	private int numParticlesAlive;
    private ParticleSystem ps;
    private ParticleSystem.Particle[] m_Particles;

    void Start () {
		ps = GetComponent<ParticleSystem>();
        if (!ps)
            Debug.LogError("ParticleSystem not found on attractor " + gameObject.name);
	}

	void FixedUpdate () {
		m_Particles = new ParticleSystem.Particle[ps.main.maxParticles];
		numParticlesAlive = ps.GetParticles(m_Particles);

		float step = Force * Time.deltaTime;
		for (int i = 0; i < numParticlesAlive; i++) {
            Vector3 m_vector = Target.position - m_Particles[i].position;
            m_Particles[i].velocity += step*m_vector.normalized * m_Particles[i].remainingLifetime / m_Particles[i].startLifetime * 8F;
        }

        ps.SetParticles(m_Particles, numParticlesAlive);
	}
}
}