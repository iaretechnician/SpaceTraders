using UnityEngine;
using System.Collections.Generic;

namespace SpaceSimFramework
{
public class StationDock : MonoBehaviour {

    [Tooltip("Waypoints for docking small ships, from farthest to closest")]
    public GameObject[] DockWaypoints;
    public Station StationController;
    [Tooltip("Time in sec after which the docking will be cancelled")]
    public float DockingTimeLimitSec = 60f;
    public float _dockTimer;

    [HideInInspector]
    public Queue<GameObject> DockingQueue;
    public GameObject ShipDocking
    {
        get { return _shipDocking; }
    }
    private GameObject _shipDocking;

    private void Awake()
    {
        DockingQueue = new Queue<GameObject>();
        if (StationController == null)
        {
            StationController = GetComponentInParent<Station>();
        }
    }

    private void Update()
    {
        if (_shipDocking == null)
        {
            if(DockingQueue.Count == 0) // No ship docking or waiting to dock
                return;

            _shipDocking = DockingQueue.Dequeue();  // Next ship can now start docking
            _dockTimer = DockingTimeLimitSec;   // Reset docking timer
            foreach (var waypoint in DockWaypoints)
                waypoint.GetComponent<MeshRenderer>().enabled = true;
        }

        if (_dockTimer < 0)
        {
            // Docking expired
            Ship ship = _shipDocking.GetComponent<Ship>();
            ship.AIInput.FinishOrder();
            if(ship.faction == Player.Instance.PlayerFaction)
            {
                ConsoleOutput.PostMessage("[" + name + " Approach Control]: Docking time expired, docking denied.", Color.yellow);
            }

            _shipDocking = null;
            // Disable waypoint indicators
            foreach (var waypoint in DockWaypoints)
                waypoint.GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            _dockTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        StationController.DockAnimator.SetBool("DockOpen", true);   // Prevents ships from getting stuck inside docks
        if (_shipDocking != null && other.gameObject == _shipDocking.gameObject)
        {
            StationController.OnDockContact(other);

            _shipDocking = null;
            // Destroy waypoint indicators
            foreach (var waypoint in DockWaypoints)
                waypoint.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public bool CanProceedWithDocking(GameObject ship)
    {
        return _shipDocking == ship;
    }
}
}