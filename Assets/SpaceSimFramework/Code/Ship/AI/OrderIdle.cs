using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
public class OrderIdle : Order
{
    public OrderIdle()
    {
        Name = "Idle";
    }

    public override void UpdateState(ShipAI controller)
    {
        ShipSteering.SteerTowardsTarget(controller);

        if (CheckExitCondition(controller))
            controller.FinishOrder();
    }

    protected bool CheckExitCondition(ShipAI controller)
    {
        if (controller.tempDest == Vector3.zero)
            controller.tempDest = GenerateNextWaypoint(controller.transform);

        float distance = Vector3.Distance(controller.tempDest, controller.transform.position);

        if (distance < 30)
        {
            controller.tempDest = GenerateNextWaypoint(controller.transform);
        }

        controller.throttle = Mathf.MoveTowards(controller.throttle, 0.5f, Time.deltaTime * 0.5f);

        return false;
    }

    private Vector3 GenerateNextWaypoint(Transform currPos)
    {
        Vector3 randomDirection = new Vector3(Random.Range(-200, 200),
            Random.Range(-200, 200),
            Random.Range(-200, 200));

        randomDirection = currPos.position + randomDirection;

        return randomDirection;
    }

    public override void Destroy()
    {
    }
}
}