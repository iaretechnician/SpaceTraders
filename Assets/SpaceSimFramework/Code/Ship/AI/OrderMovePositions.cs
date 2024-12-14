using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
public class OrderMovePositions : Order
{
    private List<Vector3> _waypoints;
    private int _nextWaypoint = 0;

    public OrderMovePositions(List<Vector3> selectedWaypoints)
    {
        _waypoints = selectedWaypoints;
    }

    public override void Destroy()
    {
    }

    public override void UpdateState(ShipAI controller)
    {
        float distance = Vector3.Distance(_waypoints[_nextWaypoint], controller.transform.position);

        if (distance < 30)
        {
            _nextWaypoint++;

            if (_nextWaypoint >= _waypoints.Count)
            {
                controller.FinishOrder();
                return;
            }
        }

        float thr = distance > 100f ? 1f : (distance / 100f);
        thr = distance > 700f ? 3f : thr;   // Supercruise
        controller.throttle = Mathf.MoveTowards(controller.throttle, thr, Time.deltaTime * 0.5f);

        ShipSteering.SteerTowardsDestination(controller, _waypoints[_nextWaypoint]);
    }

}
}