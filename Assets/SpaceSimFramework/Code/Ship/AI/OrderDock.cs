using UnityEngine;

namespace SpaceSimFramework
{
public class OrderDock : Order
{

    public bool InFinalApproach
    {
        get { return _state == State.FinalApproach; }
    }

    enum State
    {
        QueueWait, DockingApproach, FinalApproach
    }

    private Station _targetStation;
    private Ship _ship;
    private State _state = State.DockingApproach;

    public OrderDock(Station station, Ship ship)
    {
        _ship = ship;
        _targetStation = station;
        Name = "Dock";
    }

    public OrderDock()
    {
        Name = "Dock";
    }

    public override void UpdateState(ShipAI controller)
    {

        if (CheckTransitions(controller))
            controller.FinishOrder();
        ComputeState(controller);
    }

    private bool CheckTransitions(ShipAI controller)
    {
        float distance = Vector3.Distance(controller.wayPointList[controller.nextWayPoint].position, controller.transform.position);
        if(_targetStation == null)
        {
            _state = State.DockingApproach;
        }
        else
        {
            if (distance > 700)
            {
                _state = State.DockingApproach;
            }
            else
            {
                _state = State.QueueWait;
            }

            if (controller.ship.ShipModelInfo.ExternalDocking)
            {
                _state = State.DockingApproach;
            }
            else
            {
                _state = _targetStation.Dock.CanProceedWithDocking(controller.gameObject) ? State.DockingApproach : State.QueueWait;
            }

            // Disable collision avoidance check in ShipAI (on waypoints other than first)
            if (controller.nextWayPoint > 0)
                _state = State.FinalApproach;
        }

        return false;
    }

    private void ComputeState(ShipAI controller)
    {
        if (_state != State.QueueWait)
        {
            ShipSteering.SteerTowardsTarget(controller);

            // Check angle to waypoint: first steer towards waypoint, then move to it
            if (FacingWaypoint(controller))
                MoveToWaypoint(controller);
            else
                controller.throttle = 0f;
        }
        else
        {
            controller.throttle = 0f;
        }
    }

    public static void MoveToWaypoint(ShipAI controller)
    {
        float distance = Vector3.Distance(controller.wayPointList[controller.nextWayPoint].position, controller.transform.position);

        if (distance < 20)
            controller.nextWayPoint = (controller.nextWayPoint + 1) % controller.wayPointList.Count;

        if(distance>100)
            Debug.DrawLine(controller.transform.position, controller.wayPointList[controller.nextWayPoint].position, Color.blue);
        else
            Debug.DrawLine(controller.transform.position, controller.wayPointList[controller.nextWayPoint].position, Color.red);

        float thr;
        if (controller.ship.ShipModelInfo.ExternalDocking)
            thr = distance > 250f ? 1f : Mathf.Clamp(distance / 250f, 0.1f, 1f);
        else
            thr = distance > 100f ? 1f : Mathf.Clamp(distance / 100f, 0.1f, 1f);
        thr = distance > 700 ? 3f : thr;   // Supercruise

        controller.throttle = Mathf.MoveTowards(controller.throttle, thr, Time.deltaTime * 0.5f);
    }

    public static bool FacingWaypoint(ShipAI controller)
    {
        Vector3 desiredHeading = controller.wayPointList[controller.nextWayPoint].position - controller.transform.position;
        Vector3 actualHeading = controller.transform.forward;

        Debug.DrawLine(controller.transform.position, controller.wayPointList[controller.nextWayPoint].position);

        if (Vector3.Angle(desiredHeading, actualHeading) < 7)
        {
            return true;
        }
        else
        { 
            return false;
        }
    }

    public override void Destroy()
    {
        // Free the mooring point
        if (_ship != null && _ship.ShipModelInfo.ExternalDocking)
        {
            foreach(var mooring in _targetStation.Moorings)
            {
                if(mooring.Ship == _ship.gameObject)
                {
                    mooring.Ship = null;
                    _ship.StationDocked = "none";
                    return;
                }
            }
        }
    }
}
}