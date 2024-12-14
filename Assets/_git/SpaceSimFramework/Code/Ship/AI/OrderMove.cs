using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
public class OrderMove : Order
{
    public OrderMove()
    {
        Name = "Move";
    }

    public override void UpdateState(ShipAI controller)
    {
        ShipSteering.SteerTowardsTarget(controller);

        if (CheckExitCondition(controller))
            controller.FinishOrder();
    }

    protected bool CheckExitCondition(ShipAI controller)
    {
        Vector3 target;
        // Temporary destination overrides waypoints
        if (controller.tempDest == Vector3.zero && controller.wayPointList.Count > 0)
            target = controller.wayPointList[controller.nextWayPoint].position;
        else
            target = controller.tempDest;

        float distance = Vector3.Distance(target, controller.transform.position);

        if (distance < 10)
        {
            controller.tempDest = Vector3.zero;
            return true;
        }

        float thr = distance > 100f ? 1f : (distance / 100f);
        thr = distance > 700f ? 3f : thr;   // Supercruise
        controller.throttle = Mathf.MoveTowards(controller.throttle, thr, Time.deltaTime * 0.5f);

        return false;
    }

    public override void Destroy()
    {
    }
}
}