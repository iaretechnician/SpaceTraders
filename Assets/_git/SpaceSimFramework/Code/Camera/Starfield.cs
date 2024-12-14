using UnityEngine;
using System.Collections;

namespace SpaceSimFramework
{
public class Starfield : MonoBehaviour
{
    private ParticleSystem.Particle[] _points;
    private Vector3[] _velocities;
    private ParticleSystem _ps;

    public int starsMax = 100;
    public float starSize = 1;
    public float starDistance = 10;
    public float starClipDistance = 1;
    private float _starDistanceSqr;
    private float _starClipDistanceSqr;


    // Use this for initialization
    void Start()
    {
        _ps = GetComponent<ParticleSystem>();
        _starDistanceSqr = starDistance * starDistance;
        _starClipDistanceSqr = starClipDistance * starClipDistance;
    }


    private void CreateStars()
    {
        _points = new ParticleSystem.Particle[starsMax];
        _velocities = new Vector3[starsMax];

        for (int i = 0; i < starsMax; i++)
        {
            _points[i].position = Random.insideUnitSphere * starDistance + transform.position;
            _points[i].color = new Color(1, 1, 1, 1);
            _points[i].size = starSize;
            _velocities[i] = new Vector3(Random.value - .5f, Random.value - .5f, Random.value - .5f) * 0.2f;
        }

        _ps.SetParticles(_points, _points.Length);
    }


    // Update is called once per frame
    void LateUpdate()
    {
        if (_points == null) CreateStars();

        for (int i = 0; i < starsMax; i++)
        {

            if ((_points[i].position - transform.position).sqrMagnitude > _starDistanceSqr)
            {
                _points[i].position = Random.insideUnitSphere.normalized * starDistance + transform.position;
            }

            if ((_points[i].position - transform.position).sqrMagnitude <= _starClipDistanceSqr)
            {
                float percent = (_points[i].position - transform.position).sqrMagnitude / _starClipDistanceSqr;
                _points[i].color = new Color(1, 1, 1, percent);
                _points[i].size = percent * starSize;
            }

            _points[i].position += _velocities[i];

        }

        _ps.SetParticles(_points, _points.Length);
        
    }
}
}