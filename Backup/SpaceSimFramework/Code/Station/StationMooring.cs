using UnityEngine;

namespace SpaceSimFramework
{
public class StationMooring : MonoBehaviour {

    public Station StationController;
    public GameObject[] Waypoints;

    [HideInInspector]
    public GameObject Ship {
        get
        {
            return _ship;
        }
        set
        {
            _ship = value;
            // Light up the docking waypoints
            if(value != null)
            {
                foreach (var waypoint in Waypoints)
                    waypoint.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    private GameObject _ship;

    private void OnTriggerEnter(Collider other)
    {
        if (StationController != null)
        {
            if (Ship != null && other.gameObject == Ship.gameObject)
            {
                Ship = null;
                StationController.OnMooringContact(this, other.gameObject);
                // Turn off the docking waypoints
                foreach (var waypoint in Waypoints)
                    waypoint.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
}
}