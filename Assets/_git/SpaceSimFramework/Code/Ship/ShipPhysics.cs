using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Applies linear and angular forces to a ship.
/// This is based on the ship physics from https://github.com/brihernandez/UnityCommon/blob/master/Assets/ShipPhysics/ShipPhysics.cs
/// </summary>
public class ShipPhysics : MonoBehaviour
{
    [Tooltip("X: Lateral thrust\nY: Vertical thrust\nZ: Longitudinal Thrust")]
    [HideInInspector]
    public Vector3 linearForce = new Vector3(100.0f, 100.0f, 100.0f);

    [Tooltip("X: Pitch\nY: Yaw\nZ: Roll")]
    [HideInInspector]
    public Vector3 angularForce = new Vector3(100.0f, 100.0f, 100.0f);

    [Tooltip("Multiplier for all forces. Can be used to keep force numbers smaller and more readable.")]
    private float _forceMultiplier = 100.0f;

    public Rigidbody Rigidbody { get { return _rbody; } }

    private Vector3 _appliedLinearForce = Vector3.zero;
    private Vector3 _appliedAngularForce = Vector3.zero;

    private Vector3 _maxAngularForce;

    private Rigidbody _rbody;

    // Engine kill controls
    private float _rBodyDrag;
    public bool IsEngineOn
    {
        get { return _isEngineOn;  }
    }
    private bool _isEngineOn = true;

    // Keep a reference to the ship this is attached to just in case.
    private Ship _ship;

    // Use this for initialization
    void Awake()
    {
        _rbody = GetComponent<Rigidbody>();
        if (_rbody == null)
        {
            Debug.LogWarning(name + ": ShipPhysics has no rigidbody.");
        }

        _ship = GetComponent<Ship>();
        linearForce = _ship.ShipModelInfo.LinearForce;
        angularForce = _ship.ShipModelInfo.AngularForce;
    }

    private void Start()
    {
        _rBodyDrag = _rbody.drag;
        _maxAngularForce = angularForce * _forceMultiplier;
    }

    void FixedUpdate()
    {
        if (_rbody != null)
        {
            if(_isEngineOn)
                _rbody.AddRelativeForce(_appliedLinearForce, ForceMode.Force);

            _rbody.AddRelativeTorque(
                ClampVector3(_appliedAngularForce, -1 * _maxAngularForce, _maxAngularForce),
                ForceMode.Force);
        }
    }

    private void Update()
    {
        Vector3 linearInput, angularInput;

        if (_ship.IsPlayerControlled)
        {
            linearInput = new Vector3(_ship.MovementInput.strafe, 0, _ship.Throttle);
            angularInput = new Vector3(_ship.MovementInput.pitch, _ship.MovementInput.yaw, _ship.MovementInput.roll);
            _appliedLinearForce = MultiplyByComponent(linearInput, linearForce) * _forceMultiplier;
            _appliedAngularForce = MultiplyByComponent(angularInput, angularForce) * _forceMultiplier;
        }
        else
        {
            linearInput = new Vector3(0, 0, _ship.AIInput.throttle);
            _appliedLinearForce = MultiplyByComponent(linearInput, linearForce) * _forceMultiplier;
            _appliedAngularForce = _ship.AIInput.angularTorque;
            _appliedAngularForce.z = 0;
        }
    }

    /// <summary>
    /// Turns the main engine intertial dampening off or on, by disabling the linear drag on the ship.
    /// </summary>
    public void ToggleEngines()
    {
        _isEngineOn = !_isEngineOn;
        if (!_isEngineOn)
        {
            _rbody.drag = 0;
            ConsoleOutput.PostMessage("Engines off. ", Color.yellow);
        }
        else
        {
            _rbody.drag = _rBodyDrag;
            ConsoleOutput.PostMessage("Engines on. ", Color.yellow);
        }
    }

    #region helper methods
    /// <summary>
    /// Returns a Vector3 where each component of Vector A is multiplied by the equivalent component of Vector B.
    /// </summary>
    public static Vector3 MultiplyByComponent(Vector3 a, Vector3 b)
    {
        Vector3 ret;

        ret.x = a.x * b.x;
        ret.y = a.y * b.y;
        ret.z = a.z * b.z;

        return ret;
    }

    /// <summary>
    /// Clamps vector components to a value between the minimum and maximum values given in min and max vectors.
    /// </summary>
    /// <param name="vector">Vector to be clamped</param>
    /// <param name="min">Minimum vector components allowed</param>
    /// <param name="max">Maximum vector components allowed</param>
    /// <returns></returns>
    public static Vector3 ClampVector3(Vector3 vector, Vector3 min, Vector3 max)
    {
        return new Vector3(
            Mathf.Clamp(vector.x, min.x, max.x),
            Mathf.Clamp(vector.y, min.y, max.y),
            Mathf.Clamp(vector.z, min.z, max.z)
            );
    }
    #endregion helper methods
}
}