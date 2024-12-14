using System.Collections;
using UnityEngine;

namespace SpaceSimFramework
{
[AddComponentMenu("Camera-Control/Mouse drag Orbit with zoom")]
public class CameraController : MonoBehaviour
{
    [HideInInspector] public Transform target;

    public enum CameraState
    {
        Chase, Orbit, Cockpit
    }

    public float Distance = 25;
    public float XSpeed = 80;
    public float YSpeed = 80;
    public float YMinLimit = -80;
    public float YMaxLimit = 80;
    public float DistanceMin = 20;
    public float DistanceMax = 80;
    public float SmoothTime = 5f;
    public float CameraFollowSpeed = 15f;

    private float RotationYAxis = 0.0f;
    private float RotationXAxis = 0.0f;
    private float VelocityX = 0.0f;
    private float VelocityY = 0.0f;

    public CameraState State;

    [HideInInspector]
    public bool LockMovement = false;

    // Chase camera
    public float RotateSpeed = 90.0f;
    private Vector3 _startOffset;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        RotationYAxis = angles.y;
        RotationXAxis = angles.x;
        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().freezeRotation = true;
        }

        if (Ship.PlayerShip == null) { 
            Debug.Log("Player ship is null.");
        }
        else if (target == null)
        {
            _startOffset = new Vector3(0, 5, -Ship.PlayerShip.ShipModelInfo.CameraOffset);
            target = Ship.PlayerShip.transform;
        }
    }

    private void Update()
    {
        if(State == CameraState.Cockpit)
        {
            transform.position = Ship.PlayerShip.Cockpit.position;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (State == CameraState.Orbit)
                State = CameraState.Chase;
            else
                State = CameraState.Orbit;
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (Ship.PlayerShip.Cockpit == null)
                return;
            if (State == CameraState.Cockpit)
                State = CameraState.Chase;
            else
                State = CameraState.Cockpit;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleFlightMode();
        }
        if (Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            Distance = Mathf.Clamp(Distance - 10, DistanceMin, DistanceMax);
            _startOffset.z = -Distance;
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            Distance = Mathf.Clamp(Distance + 10, DistanceMin, DistanceMax);
            _startOffset.z = -Distance;
        } 
    }

    public void ToggleFlightMode()
    {
        if (State == CameraState.Orbit)
        {
            LockMovement = !LockMovement;
            ControlStatusUI.Instance.SetControlStatusText("Turret view");
        }
        else
        {
            Ship.PlayerShip.UsingMouseInput = !Ship.PlayerShip.UsingMouseInput;
            ControlStatusUI.Instance.SetControlStatusText(Ship.PlayerShip.UsingMouseInput ? "Mouse Flight" : "Keyboard Flight");
        }
    }

    void FixedUpdate()
    {
        if (!target)
            return;

        if (State == CameraState.Orbit)
        {
            // Orbit Camera
            if (!LockMovement)
            {
                VelocityX = 0;
                VelocityY = 0;
                if (Input.mousePosition.x < Screen.width * 0.25f)
                {
                    VelocityX = (Input.mousePosition.x - 0.25f * Screen.width) / (0.25f * Screen.width) * XSpeed * 0.02f;
                }
                if (Input.mousePosition.x > Screen.width * 0.75f)
                {
                    VelocityX = (Input.mousePosition.x - 0.75f * Screen.width) / (0.25f * Screen.width) * XSpeed * 0.02f;
                }
                if (Input.mousePosition.y < Screen.height * 0.25f)
                {
                    VelocityY = (Input.mousePosition.y - 0.25f * Screen.height) / (0.25f * Screen.width) * YSpeed * 0.02f;
                }
                if (Input.mousePosition.y > Screen.height * 0.75f)
                {
                    VelocityY = (Input.mousePosition.y - 0.75f * Screen.height) / (0.25f * Screen.width) * YSpeed * 0.02f;
                }
            }

            RotationYAxis += VelocityX;
            RotationXAxis -= VelocityY;
            RotationXAxis = ClampAngle(RotationXAxis, YMinLimit, YMaxLimit);
            Quaternion rotation = Quaternion.Euler(RotationXAxis, RotationYAxis, 0);

            // Camera should target slightly above the ship
            Vector3 position = rotation * _startOffset + target.position + target.up * Distance * 0.1f;

            transform.rotation = rotation;
            transform.position = position;// Vector3.Lerp(transform.position, position, Time.deltaTime * CameraFollowSpeed);
            VelocityX = Mathf.Lerp(VelocityX, 0, Time.deltaTime * SmoothTime);
            VelocityY = Mathf.Lerp(VelocityY, 0, Time.deltaTime * SmoothTime);
        }
        else
        {
            if (State == CameraState.Chase)
            {
                // Chase camera
                transform.position = Vector3.Lerp(
                    transform.position,
                    target.TransformPoint(_startOffset),
                    Time.deltaTime * CameraFollowSpeed);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    target.rotation,
                    RotateSpeed * Time.deltaTime);
            }
            // Camera interpolation must be done in Update to ensure no stutter or lag occurs
            if (State == CameraState.Cockpit)
            {
                // Cockpit camera
                if (!Ship.PlayerShip.UsingMouseInput)
                {
                    transform.rotation = Ship.PlayerShip.Cockpit.rotation;
                }
                else
                {
                    transform.rotation = Ship.PlayerShip.Cockpit.rotation;

                    float horizontalOffset = Mathf.Lerp(0, 60, Input.mousePosition.x / Screen.width) - 30f;
                    float verticalOffset = Mathf.Lerp(0, 30, (Input.mousePosition.y - Screen.height / 2) / (Screen.height / 2));
                    transform.Rotate(new Vector3(-verticalOffset, horizontalOffset, 0), Space.Self);
                }

            }
        }

    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    public void SetTargetShip(Ship newTarget)
    {
        target = newTarget.transform;
        State = CameraState.Chase;
        Distance = newTarget.ShipModelInfo.CameraOffset;
        DistanceMin = 20;
        DistanceMax = 50;
        _startOffset = new Vector3(0, 5, -newTarget.ShipModelInfo.CameraOffset);
    }

    public void SetTargetStation(Transform newTarget, Vector3 offset)
    {
        target = newTarget;
        _startOffset = offset;
        LockMovement = true;
        State = CameraState.Orbit;
        Distance = 200;
        DistanceMin = 200;
        DistanceMax = 200;
    }

    public void SetTargetPlayerShip()
    {
        target = Ship.PlayerShip.transform;
        _startOffset = new Vector3(0, 5, -Ship.PlayerShip.ShipModelInfo.CameraOffset);
        State = CameraState.Chase;
        Distance = Ship.PlayerShip.ShipModelInfo.CameraOffset;
        DistanceMin = 20;
        DistanceMax = 50;
    }

    public static IEnumerator Shake()
    {           
        float shakeDuration = 0;
        Vector3 originalRotation = Camera.main.transform.eulerAngles;

        if (!CanvasViewController.IsMapActive)
        {   // Don't shake in map view
            while (shakeDuration < 0.1f)
            {
                Vector3 rotationAmount = Random.insideUnitSphere;
                rotationAmount.z = 0;   // Don't change the Z; it looks funny.

                shakeDuration += Time.deltaTime;

                Camera.main.transform.eulerAngles += rotationAmount;    // Set the local rotation the be the rotation amount.

                yield return null;
            }
        }

        Camera.main.transform.eulerAngles = originalRotation;
    }

    public Vector3 GetTargetCameraPosition()
    {
        return target == null ? Vector3.zero : target.TransformPoint(_startOffset);
    }
}
}