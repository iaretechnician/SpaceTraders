using UnityEngine;
using System.Collections.Generic;

namespace SpaceSimFramework
{
public class SpaceParticleQuad : MonoBehaviour
{
    public bool ParticleActive = false;
    [Tooltip("When false, particles align with view vector rather than point at the camera.")]
    public bool PointAtCamera = true;
    public Texture[] TextureOptions;

    private Material _mat;
    private MeshRenderer _mesh;
    private Transform _meshTransform;

    private int _colorPropID = 0;
    private Color _startColor;
    private Color _startColorNoA;

    private Transform _transform;
    private Camera _refCam;
    private Transform _refCamTransform;

    private Vector3 _vel;
    private float _rotVel;
    private float _radius;
    private float _drawDistance;

    private float _nearFadeDistance;
    private float _farFadeDistance;

    private float _densityFade;

    void Awake()
    {
        _transform = GetComponent<Transform>();
        _refCam = Camera.main;
        _refCamTransform = _refCam.transform;

        _mesh = GetComponentInChildren<MeshRenderer>();
        _meshTransform = _mesh.GetComponent<Transform>();
        _mat = _mesh.material;
    }

    void Start()
    {
        // Set a random texture based on the choices given.
        _mat.SetTexture(0, TextureOptions[Random.Range(0, TextureOptions.Length)]);
        _mat.renderQueue = 4000;
        _startColor = _mat.GetColor("_TintColor");
        
        _startColorNoA = _startColor;
        _startColorNoA.a = 0f;

        _colorPropID = Shader.PropertyToID("_TintColor");

        // Each space particle gameobject is expected to have the Quad on a child gameobject.
        // This allows the quad to be easily given a random rotation to start with.
        _meshTransform.localEulerAngles = new Vector3(0f, 0f, Random.Range(-90f, 90f));
    }

    void Update()
    {
        if (ParticleActive)
        {
            // First find if the particle is outside of the cameras bounds.
            bool visible = CheckVisibility();
            
            // Normal case when it's on camera.
            if (visible)
            {
                // Move the quad around based on the initial velocity.
                _transform.position += _vel * Time.deltaTime;

                // Rotate quad to face the camera
                if (PointAtCamera)
                {
                    Vector3 vec2Cam = _refCamTransform.position - _transform.position;
                    _transform.rotation = Quaternion.LookRotation(vec2Cam, _transform.up);
                }

                // Alternatively, align the quad with the camera's look vector.
                else
                {
                    _transform.rotation = Quaternion.LookRotation(-_refCamTransform.forward, _transform.up);
                }

                // Rotate the (actual) quad to give it more interest.
                if (_rotVel != 0f)
                    _meshTransform.Rotate(0f, 0f, _rotVel * Time.deltaTime);
            }
            else
            {
                ParticleActive = false;
                _mesh.enabled = false;
            }
        }

        else
        {
            _mesh.enabled = false;
        }
    }

    private bool CheckVisibility()
    {
        bool visible = false;

        // Check FrustumCode.txt for the old frustum based code.

        // If they are in the frustum, then check for distance. They are allowed to persist
        // at half the max distance when out of view. This way particles immediately behind you
        // and just off screen don't disappear just because you looked away for an instant.
        float distToCam = Vector3.Distance(_refCamTransform.position, _transform.position);

        // Distance method     
        if (distToCam < _drawDistance)
        {
            visible = true;

            // Fade the particle if it's getting too far.
            float endFadeOutStart = _drawDistance - _radius;
            if (distToCam > endFadeOutStart)
            {
                Color col = Color.Lerp(_startColor, _startColorNoA, (distToCam - endFadeOutStart) / (_radius));
                col.a *= _densityFade;
                _mat.SetColor(_colorPropID, col);
            }

            // Fade the particle out if it's getting too close.
            if (distToCam < _farFadeDistance && distToCam > _nearFadeDistance)
            {
                Color col = Color.Lerp(_startColorNoA, _startColor, (distToCam - _nearFadeDistance) / (_farFadeDistance - _nearFadeDistance));
                col.a *= _densityFade;
                _mat.SetColor(_colorPropID, col);

                visible = true;
            }

            else if (distToCam < _nearFadeDistance)
            {
                _mat.SetColor(_colorPropID, _startColorNoA);
            }
        }        

        return visible;
    }

    public bool Initialize(Vector3 startVel, float startRot, float startRadius, float maxDistance, float nearFade, float farFade, Color newCol, float nebDensity)
    {
        _mesh.enabled = true;
        ParticleActive = true;

        _vel = startVel;
        _rotVel = startRot;

        _radius = startRadius;
        _transform.localScale = Vector3.one * _radius * 2f;

        _drawDistance = maxDistance;
        _nearFadeDistance = nearFade;
        _farFadeDistance = farFade;

        _startColor = newCol;
        _startColorNoA = _startColor;
        _startColorNoA.a = 0f;
        _mat.SetColor("_TintColor", _startColor);

        _densityFade = nebDensity;

        return true;
    }
}

public class SpaceParticlePool
{
    List<SpaceParticleQuad> pool;
    SpaceParticleQuad spacePar;

    public SpaceParticlePool(int poolSize, SpaceParticleQuad newPar, Transform parentNebula)
    {
        pool = new List<SpaceParticleQuad>(poolSize);
        spacePar = newPar;

        for (int i = 0; i < pool.Capacity; i++)
        {
            SpaceParticleQuad par = (SpaceParticleQuad)GameObject.Instantiate(spacePar, parentNebula);
            pool.Add(par);
        }
    }

    public bool ActivateParticle(Vector3 pos, Vector3 startVel, float startRot, float startRadius, float maxDistance, float nearFade, float farFade, Color color, float nebDensity)
    {
        // Find the first inactive particle.
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].ParticleActive)
            {
                pool[i].transform.position = pos;
                pool[i].Initialize(startVel, startRot, startRadius, maxDistance, nearFade, farFade, color, nebDensity);
                return true;
            }
        }

        // Return false if there weren't any available particles.
        return false;
    }

    public bool CheckAvailable()
    {
        foreach (SpaceParticleQuad quad in pool)
        {
            if (!quad.ParticleActive)
                return true;
        }

        return false;
    }

    //public void PrepForDeleteParticles()
    //{
    //    for (int i = 0; i < pool.Count; i++)
    //    {
    //        if (pool[i] != null)
    //            GameObject.Destroy(pool[i].gameObject);
    //    }
    //}
}
}