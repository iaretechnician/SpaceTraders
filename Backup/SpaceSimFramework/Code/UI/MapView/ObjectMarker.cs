using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace SpaceSimFramework
{
public class ObjectMarker : MonoBehaviour, IPointerClickHandler
{
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
            // Object selected
            InputHandler.Instance.OnMarkerLeftClicked(_target);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Dock/move command given
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

        if (_target == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            ChangeMarkerImage(value.tag);
            gameObject.SetActive(true);
            _markerImage.color = Player.Instance.PlayerFaction.GetTargetColor(_target);
            EquatorialIndicator.Target = _target.transform;
        }
    }

    private void ChangeMarkerImage(string tag)
    {
        Sprite image = IconManager.Instance.GetMarkerIcon(tag);
        if(image != null)
        {
            _markerImage.sprite = image;
        }
    }

    public void SwitchMode(MarkerMode mode)
    {
        if(mode == MarkerMode.Flight)
        {
            EquatorialIndicator.gameObject.SetActive(false);
            EquatorialImage.gameObject.SetActive(false);
        }
        else
        {
            EquatorialIndicator.gameObject.SetActive(true);
            EquatorialImage.gameObject.SetActive(true);
        }
    }
}
}