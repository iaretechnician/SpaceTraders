using UnityEngine;

namespace SpaceSimFramework
{
public class Explosion : MonoBehaviour {

    private ParticleSystem ps;
    private Light explosionLight;
    private float initialIntensity;

	void Start () {
		ps = GetComponent<ParticleSystem>();
        explosionLight = GetComponent<Light>();
        //GetComponent<AudioSource>().Play();
        if (explosionLight)
            initialIntensity = explosionLight.intensity;
    }
	
	void Update () {
        if (ps.isStopped)
        {
            Destroy(gameObject);
        }
        if (explosionLight)
        {
            explosionLight.intensity = Mathf.Lerp(initialIntensity, 0, ps.time / ps.main.duration);
        }
    }
}
}