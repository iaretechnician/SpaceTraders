using UnityEngine;
using System;
using System.Collections;

namespace SpaceSimFramework
{
/// <summary>
/// Class specifically to deal with input.
/// </summary>
public class ShipMovementInput : MonoBehaviour
{
    [Tooltip("When using Keyboard/Joystick input, should roll be added to horizontal stick movement. This is a common trick in traditional space sims to help ships roll into turns and gives a more plane-like feeling of flight.")]
    public bool addRoll = true;
    [Tooltip("When true, the mouse and mousewheel are used for ship input and A/D can be used for strafing like in many arcade space sims.\n\nOtherwise, WASD/Arrows/Joystick + R/T are used for flying, representing a more traditional style space sim.")]
    public bool useMouseInput = true;

    [Range(-1, 1)]
    public float pitch;
    [Range(-1, 1)]
    public float yaw;
    [Range(-1, 1)]
    public float roll;
    [Range(-1, 1)]
    public float strafe;
    [Range(-0.1f, 1)]
    public float throttle;


    // How quickly the throttle reacts to input.
    private const float THROTTLE_SPEED = 0.5f;

    // Keep a reference to the ship this is attached to just in case.
    private Ship _ship;
    private CameraController _cam;

    private Light[] _engineTorches;
    private TrailRenderer[] _engineTrails;

    private void Awake()
    {
        _ship = GetComponent<Ship>();
        _cam = Camera.main.GetComponent<CameraController>();
        _engineTorches = GetComponentsInChildren<Light>();
        _engineTrails = GetComponentsInChildren<TrailRenderer>();
    }

    private void Update()
    {
        if (!_ship.IsPlayerControlled)
            return;

        if (Ship.IsShipInputDisabled)
            return;

        if (useMouseInput)
        {
            strafe = Input.GetAxis("Horizontal");
            SetStickCommandsUsingMouse();
            if (!_ship.InSupercruise)
            {
                UpdateMouseWheelThrottle();
                UpdateKeyboardThrottle(KeyCode.W, KeyCode.S);
            }
        }
        else
        {
            pitch = Input.GetAxis("Vertical");
            yaw = Input.GetAxis("Horizontal");

            if (addRoll)
                roll = -Input.GetAxis("Horizontal") * 0.5f;

            strafe = 0.0f;
            UpdateKeyboardThrottle(KeyCode.R, KeyCode.F);
        }

        roll = Input.GetAxis("Roll");

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            throttle = 0.0f;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ToggleEngines();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            var closestEnemies = SectorNavigation.Instance.GetClosestEnemyShip(transform, _ship.ScannerRange);
            if(closestEnemies.Count > 0)
            {
                InputHandler.Instance.SelectedObject = closestEnemies[0];               
            }
        }

        // Request docking at station
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
        {
            GameObject target = InputHandler.Instance.GetCurrentSelectedTarget();
            if (target != null && target.tag == "Station")
            {
                try
                {
                    target.GetComponent<Station>().RequestDocking(gameObject);
                }
                catch (DockingException e) { }
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.W))
        {
            ToggleSupercruise();
        }

    }

    public void ToggleSupercruise()
    {
        // Make sure the engine is on
        if (!_ship.Physics.IsEngineOn)
        {
            ToggleEngines();
        }

        _ship.InSupercruise = !_ship.InSupercruise;
        if (_ship.InSupercruise)
            ConsoleOutput.PostMessage("Engaging supercruise.");
        else
            ConsoleOutput.PostMessage("Disengaging supercruise.");

        if (_ship.InSupercruise)
        {
            StartCoroutine(EngageSupercruise());
            for (int i = 0; i < _engineTorches.Length; i++)
            {
                _engineTorches[i].intensity = 5f;
            }
        }
        if (!_ship.InSupercruise)
        {
            throttle = 1f;
            for (int i = 0; i < _engineTorches.Length; i++)
            {
                _engineTorches[i].intensity = 1f;
            }
        }
    }

    private void ToggleEngines()
    {
        _ship.ToggleEngine();

        for (int i = 0; i < _engineTorches.Length; i++)
        {
            if (_ship.Physics.IsEngineOn)
            {
                _engineTorches[i].intensity = 1.0f;
                _engineTrails[i].gameObject.SetActive(true);
            }
            else
            {
                _engineTorches[i].intensity = 0f;
                _engineTrails[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Accelerate ship to 3x max throttle
    /// </summary>
    /// <returns></returns>
    private IEnumerator EngageSupercruise()
    {
        while(throttle < 3.0f)
        {
            if (!_ship.InSupercruise)
                yield break;

            throttle = Mathf.MoveTowards(throttle, 3.0f, Time.deltaTime * THROTTLE_SPEED);

            yield return null;
        }
        yield return null;
    }


    /// <summary>
    /// Freelancer style mouse controls. This uses the mouse to simulate a virtual joystick.
    /// When the mouse is in the center of the screen, this is the same as a centered stick.
    /// </summary>
    private void SetStickCommandsUsingMouse()
    {
        Vector3 mousePos = Input.mousePosition;

        // Figure out most position relative to center of screen.
        // (0, 0) is center, (-1, -1) is bottom left, (1, 1) is top right.      
        pitch = (mousePos.y - (Screen.height * 0.5f)) / (Screen.height * 0.5f);
        yaw = (mousePos.x - (Screen.width * 0.5f)) / (Screen.width * 0.5f);

        // Make sure the values don't exceed limits.
        pitch = -Mathf.Clamp(pitch, -1.0f, 1.0f);
        yaw = Mathf.Clamp(yaw, -1.0f, 1.0f);
    }

    /// <summary>
    /// Uses R and F to raise and lower the throttle.
    /// </summary>
    private void UpdateKeyboardThrottle(KeyCode increaseKey, KeyCode decreaseKey)
    {
        if(_ship.InSupercruise && Input.GetKey(decreaseKey))
        {
            throttle = 1.0f;
            _ship.InSupercruise = false;
            return;
        }
        if (_ship.InSupercruise)
            return;

        float target = throttle;

        if (Input.GetKey(increaseKey))
            target = 1.0f;
        else if (Input.GetKey(decreaseKey))
            target = -0.1f;

        throttle = Mathf.MoveTowards(throttle, target, Time.deltaTime * THROTTLE_SPEED);
    }

    /// <summary>
    /// Uses the mouse wheel to control the throttle.
    /// </summary>
    private void UpdateMouseWheelThrottle()
    {
        throttle += Input.GetAxis("Mouse ScrollWheel");
        throttle = Mathf.Clamp(throttle, -0.1f, 1.0f);
    }
}
}