using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public class LeadIndicatorUI : MonoBehaviour {

    private Image image;    

	void Start () {
        image = GetComponent<Image>();
        image.enabled = false;

        ConsoleOutput.PostMessage("Screen size " + Screen.width + " by " + Screen.height);
	}
	
	void Update () {
        GameObject target = InputHandler.Instance.GetCurrentSelectedTarget();

        // Ship has no weapons
        if (Ship.PlayerShip == null || (Ship.PlayerShip.Equipment.Guns.Count == 0 && Ship.PlayerShip.Equipment.Turrets.Count == 0))
            return;

        float projectileSpeed = Ship.PlayerShip.Equipment.Guns.Count > 0 ? 
            Ship.PlayerShip.Equipment.Guns[0].ProjectileSpeed : Ship.PlayerShip.Equipment.Turrets[0].ProjectileSpeed;

        if (target != null || Ship.PlayerShip == null)
        {
            GameObject shooter = Ship.PlayerShip.gameObject;           

            image.enabled = true;
            image.rectTransform.anchoredPosition =
                Targeting.PredictTargetLead2D(Camera.main.gameObject, target, projectileSpeed) - 
                new Vector2(Screen.width/2, Screen.height/2);
        }
        else
        {
            image.enabled = false;
        }
	}
}
}