using System;
using UnityEngine;

namespace SpaceSimFramework
{
public class Nebula : MonoBehaviour
{
    // Unique in-game object ID 
    [HideInInspector]
    public string ID;
    [Space(20)]
    public NebulaPuffs Clouds;
    public NebulaPuffs Particles;
    public Color AmbientLight = Color.white;
    public Color NebulaColor = Color.white;
    public float MaxViewDistance = 2000f;
    public float FogStart = 0f;
    public float FogEnd = 2000f;

    [Header("Gameplay")]
    [Tooltip("If true, sensors will be obscured while inside nebula")]
    public bool IsSensorObscuring = false;
    [Tooltip("Corrosive nebula deals a certain damage per minute to ships within")]
    public float CorrosionDamagePerSecond = 0;
    [Tooltip("Resource mineable in this nebula, null if none")]
    public string Resource;
    [Tooltip("Max yield provided by mining this nebula")]
    public int YieldPerSecond;

    private Light[] _sunLights;
    private Color _sunColor = Color.black;

    public static Nebula Instance
    {
        get { return _instance; }
    }
    private static Nebula _instance = null;

    private void Awake()
    {
        if (_instance != null)
            Debug.LogError("Two nebulae found in scene, please fix this.");
        _instance = this;
    }

    private void Start()
    {
        GameObject[] obj = GameObject.FindGameObjectsWithTag("Sun");
        if (obj != null)
        {
            _sunLights = new Light[obj.Length];

            for (int i = 0; i < obj.Length; i++)
                _sunLights[i] = obj[i].GetComponent<Light>();

            if (_sunLights.Length > 0)
                _sunColor = _sunLights[0].color;
        }

        ApplyNebulaVisualEffects();
        ApplyNebulaGameEffects();
    }

    private void ApplyNebulaGameEffects()
    {
        if (IsSensorObscuring)
        {
            Ship shipComponent;
            foreach(GameObject ship in SectorNavigation.Ships)
            {
                shipComponent = ship.GetComponent<Ship>();
                shipComponent.ScannerRange = (int)(shipComponent.ScannerRange*0.2f); 
            }
        }
    }

    private void ApplyNebulaVisualEffects()
    {
        RenderSettings.ambientLight = AmbientLight;

        Camera.main.farClipPlane = MaxViewDistance;
        Camera.main.backgroundColor = NebulaColor;
        Camera.main.clearFlags = CameraClearFlags.Color;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientSkyColor = AmbientLight;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = NebulaColor;
        RenderSettings.fogStartDistance = FogStart;
        RenderSettings.fogEndDistance = FogEnd;

        // Fade the light because the nebula is blocking most of it.
        foreach (Light sun in _sunLights)
        {
            sun.color = Color.Lerp(_sunColor, NebulaColor, 0.5f);
            sun.shadowStrength = 0.2f;
        }
    }
}
}