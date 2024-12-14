using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
/// <summary>
/// Manages the target healthbar hovering above the 
/// selected target together with the marker.
/// </summary>
public class HealthBar : MonoBehaviour {

    private RectTransform rectTransform;
    private Slider healthSlider;
    private GameObject target;

    private Ship _targetShip;
    private Wreck _targetWreck;
    private Asteroid _targetAsteroid;

    public void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        healthSlider = GetComponent<Slider>();

        SetMaxValue(1f);
    }

    private void FixedUpdate()
    {
        if (target.tag == "Ship")
        {
            UpdateSlider(_targetShip.Armor/_targetShip.MaxArmor);
        }        
        else if(target.tag == "Wreck")
        {
            UpdateSlider(_targetWreck.Armor / _targetWreck.WreckModelInfo.MaxArmor);
        }
        else if(target.tag == "Asteroid")
        {
            UpdateSlider(_targetAsteroid.Health / 1000f);
        }
    }

    public void SetMaxValue(float value)
    {
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        healthSlider.maxValue = value;
        healthSlider.value = value;
    }

    public void UpdateSlider(float value)
    {
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        healthSlider.value = value;
    }

    public void SetTarget(GameObject targetObject)
    {
        target = targetObject;
        if (target != null && target.tag == "Ship")
        {
            this.gameObject.SetActive(true);
            _targetShip = target.GetComponent<Ship>();
        }
        else if (target != null && target.tag == "Wreck")
        {
            this.gameObject.SetActive(true);
            _targetWreck = target.GetComponent<Wreck>();
        }
        else if (target != null && target.tag == "Asteroid")
        {
            this.gameObject.SetActive(true);
            _targetAsteroid = target.GetComponent<Asteroid>();
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

}
}