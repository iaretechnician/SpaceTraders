using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Background flashing when inside of nebula.
/// </summary>
[RequireComponent(typeof(Nebula))]
public class NebulaLightning : MonoBehaviour
{
    public Color Color;
    public bool Flash = false;
    public float FlashDuration = 0.3f;
    public float AvgTimeBetweenFlashes = 3f;
    public int MaxFlashesAtOnce = 3;
    [HideInInspector] public bool IsFlashActive = false;

    private float _timer, _onTimer, _offTimer;
    private bool _isOn;
    private int _numFlashes;
    private Nebula _parentNeb;

    void Awake()
    {
        _parentNeb = GetComponent<Nebula>();
    }

    private void Start()
    {
        _timer = AvgTimeBetweenFlashes + Random.value * 4f - 1f;
        _numFlashes = Random.Range(1, MaxFlashesAtOnce);
    }

    void LateUpdate()
    {
        if (!IsFlashActive)
            return;

        _timer -= Time.deltaTime;
        if(_timer < 0)
        {
            // Flash now
            _timer = AvgTimeBetweenFlashes + Random.value * 4f - 1f;
            _numFlashes = Random.Range(1, MaxFlashesAtOnce);
            Flash = true;

            _isOn = true;
            _onTimer = FlashDuration;            
        }

        if (Flash)
        {
            if (_isOn)
            {
                // Keep flash on for duration of onTimer
                _onTimer -= Time.deltaTime;
                if (_onTimer > 0)
                {
                    RenderSettings.ambientLight = Color;
                }
                else
                {
                    // Move to next flash
                    _numFlashes--;

                    if (_numFlashes <= 0)
                        Flash = false;
                    else
                    {
                        _isOn = false;
                        _offTimer = FlashDuration;
                    }
                }
            }
            else
            {
                _offTimer -= Time.deltaTime;
                if (_offTimer < 0)
                {
                    _isOn = true;
                    _onTimer = FlashDuration;
                }               
            }
            
        }
        else
        {
            RenderSettings.ambientLight = _parentNeb.AmbientLight;
        }
    }
}
}