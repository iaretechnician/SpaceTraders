using UnityEngine.UI;
using UnityEngine;

namespace SpaceSimFramework
{
public class TargetDescUI : MonoBehaviour {

    private Text text;
    private GameObject target;

	void Awake () {
        text = GetComponent<Text>();
	}
	
	void Update () {
        target = InputHandler.Instance.GetCurrentSelectedTarget();
        if (target == null && Ship.PlayerShip != null)
            text.text = "Target: none";
        else if (Ship.PlayerShip != null)
            text.text = "Target: " + target.name + "\nDistance: " + (int)Vector3.Distance(Ship.PlayerShip.transform.position, target.transform.position);
	}
}
}