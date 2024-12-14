using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public class EquatorialLine : MonoBehaviour
{

    public Transform Target
    {
        get { return _target; }
        set
        {
            if(_line == null)
            {
                _line = GetComponent<LineRenderer>();
            }
            _target = value;
            _line.startColor = _line.endColor = GetComponentInParent<Image>().color;
            RenderLine();
        }
    }
    public Transform EquatorialMarker;

    private Transform _target;
    private LineRenderer _line;
    private float _timer = 2f;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        RenderLine();

        _timer -= Time.deltaTime;
        if (_timer < 0)
        {
            _timer = 2f;
            _line.startColor = _line.endColor = GetComponentInParent<Image>().color;
        }
    }

    private void RenderLine()
    {
        if (_target != null)
        {
            _line.SetPositions(new Vector3[] {
                _target.position,
                new Vector3(_target.position.x, 0, _target.position.z)
            });
            EquatorialMarker.position = new Vector3(_target.position.x, 0, _target.position.z);
        }
    }

}
}