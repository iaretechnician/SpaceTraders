using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
public class OrderFollow : Order
{
    private Vector3 _formationOffset;
    private Ship _targetShip;
    private ShipAI _controller;

    public OrderFollow(ShipAI controller, Ship target)
    {
        // Give a random formation offset
        _formationOffset = target.GetWingmanPosition(controller.transform.GetComponent<Ship>());
        _targetShip = target;
        this._controller = controller;
        Name = "Follow";
    }

    public override void UpdateState(ShipAI controller)
    {
        if (CheckExitCondition(controller))
        {
            controller.FinishOrder();
            return;
        }
       
        controller.tempDest = _targetShip.transform.position + _formationOffset;
        ShipSteering.SteerTowardsTarget(controller);
    }

    protected bool CheckExitCondition(ShipAI controller)
    {
        if (_targetShip == null) // Ship being followed is gone/destroyed
        {
            // Defend!
            controller.AttackAll();
            return true;
        }

        float distance = Vector3.Distance(controller.tempDest, controller.transform.position);


        float thr = distance > 80f ? 1f : 0f;
        thr = distance > 700f ? 3f : thr;   // Supercruise
        controller.throttle = Mathf.MoveTowards(controller.throttle, thr, Time.deltaTime * 0.5f);

        return false;
    }

    public override void Destroy()
    {
        if(_targetShip != null)
            _targetShip.RemoveWingman(_controller.ship);
    }
}
}