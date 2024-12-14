using UnityEngine;

namespace SpaceSimFramework
{
public class HUDLag : MonoBehaviour {

    private Vector3 _prevRotation, _rotation;
    private int _frameCount;
    private Transform _target;

    public float TurningRate = 80f;

    private void Awake()
    {
        _target = Camera.main.transform;
    }

    // Update and Lateupdate causes jitter with rotation
    // FixedUpdate causes sporadic  jitter along the movement axis
    private void FixedUpdate()
    {
        // Turn towards our target rotation.
        transform.rotation = Quaternion.Lerp(transform.rotation, _target.rotation, TurningRate * Time.deltaTime);
    }

    private void LateUpdate()
    {
        // Copy position
        transform.position = _target.position;
    }
}
}