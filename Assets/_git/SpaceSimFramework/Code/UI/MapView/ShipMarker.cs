using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace SpaceSimFramework
{
public class ShipMarker : MonoBehaviour, IPointerClickHandler
{
    public HealthBar HealthIndicator;
    public EquatorialLine EquatorialIndicator;
    public GameObject EquatorialImage;
    private GameObject _target;
    public Image MarkerImage
    {
        get
        {
            if (_markerImage == null)
            {
                _markerImage = GetComponent<Image>();
            }
            return _markerImage;
        }
    }

    public enum MarkerMode { Map, Flight }

    private Image _markerImage;

    private void Awake()
    {
        EquatorialIndicator.gameObject.SetActive(false);
        EquatorialImage.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Ship selected
            InputHandler.Instance.OnMarkerLeftClicked(_target);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Attack command given
            InputHandler.Instance.OnMarkerRightClicked(_target);
        }
    }

    public void SetTarget(GameObject value)
    {
        _target = value;
        if (_markerImage == null)
        {
            _markerImage = GetComponent<Image>();
        }

        if (_target == null || (value == Ship.PlayerShip.gameObject && !CanvasViewController.IsMapActive))
        {
            HealthIndicator.SetTarget(null);
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        HealthIndicator.SetTarget(_target);
        _markerImage.color = Player.Instance.PlayerFaction.GetTargetColor(_target);
        EquatorialIndicator.Target = _target.transform;
    }

    public void SwitchMode(MarkerMode mode)
    {
        if (mode == MarkerMode.Flight)
        {
            if (_target == Ship.PlayerShip.gameObject)
            {
                HealthIndicator.SetTarget(null);
                gameObject.SetActive(false);
            }

            EquatorialIndicator.gameObject.SetActive(false);
            EquatorialImage.gameObject.SetActive(false);
        }
        else
        {
            if (_target == Ship.PlayerShip.gameObject)
            {
                HealthIndicator.SetTarget(_target);
                gameObject.SetActive(true);
            }

            EquatorialIndicator.gameObject.SetActive(true);
            EquatorialImage.gameObject.SetActive(true);
        }
    }
}
}