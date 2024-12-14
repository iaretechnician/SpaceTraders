using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
public class ShipSteering
{

    public static void SteerTowardsTarget(ShipAI controller)
    {
        Vector3 target;
        // Temporary destination overrides waypoints
        if (controller.tempDest == Vector3.zero && controller.wayPointList.Count > 0)
            target = controller.wayPointList[controller.nextWayPoint].position;
        else
            target = controller.tempDest;

        SteerTowardsDestination(controller, target);
    }

    public static void SteerTowardsDestination(ShipAI controller, Vector3 destination)
    {
        float distance = Vector3.Distance(destination, controller.transform.position);       
        if (distance > 10)
        {
            Vector3 angularVelocityError = controller.rBody.angularVelocity * -1;
            Vector3 angularVelocityCorrection = controller.pid_velocity.Update(angularVelocityError, Time.deltaTime);

            Vector3 lavc = controller.transform.InverseTransformVector(angularVelocityCorrection);

            Vector3 desiredHeading = destination - controller.transform.position;
            Vector3 currentHeading = controller.transform.forward;
            Vector3 headingError = Vector3.Cross(currentHeading, desiredHeading);
            Vector3 headingCorrection = controller.pid_angle.Update(headingError, Time.deltaTime);

            // Convert angular heading correction to local space to apply relative angular torque
            Vector3 lhc = controller.transform.InverseTransformVector(headingCorrection * 200f);

            controller.angularTorque = lavc + lhc;
        }
        else
        {
            controller.angularTorque = Vector3.zero;
        }
    }
}
}