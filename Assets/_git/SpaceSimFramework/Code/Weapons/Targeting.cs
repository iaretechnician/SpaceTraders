using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
public class Targeting : MonoBehaviour {

    /// <summary>
    /// Computes the required lead of the target based on its speed
    /// and the weapon projectile speeds.
    /// </summary>
    /// <returns>the screen position of the lead indicator</returns>
    public static Vector2 PredictTargetLead2D(GameObject shooter, GameObject target, float projectileSpeed)
    {
        //calculate intercept
        Vector3 interceptPoint = PredictTargetLead3D(shooter, target, projectileSpeed);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(interceptPoint);
        return new Vector2(screenPos.x, screenPos.y);
    }

    /// <summary>
    /// Computes the required lead of the target based on its speed
    /// and the weapon projectile speeds. Approximates gun position as shooter position.
    /// </summary>
    /// <returns>the world position of the lead indicator</returns>
    public static Vector3 PredictTargetLead3D(GameObject shooter, GameObject target, float projectileSpeed)
    {
        //positions
        Vector3 shooterPosition = shooter.transform.position;
        Vector3 targetPosition = target.transform.position;
        //velocities
        Vector3 shooterVelocity = shooter.GetComponent<Rigidbody>() ? shooter.GetComponent<Rigidbody>().velocity : Vector3.zero;
        Vector3 targetVelocity = target.GetComponent<Rigidbody>() ? target.GetComponent<Rigidbody>().velocity : Vector3.zero;

        //calculate intercept
        Vector3 interceptPoint = FirstOrderIntercept
        (
            shooterPosition,
            shooterVelocity,
            projectileSpeed,
            targetPosition,
            targetVelocity
        );
        
        return interceptPoint;
    }

    /// <summary>
    /// Computes the required lead of the target based on its speed
    /// and the weapon projectile speeds. Uses the offset to precisely calculate for
    /// gun offsets.
    /// </summary>
    /// <returns>the world position of the lead indicator</returns>
    public static Vector3 PredictTargetLead3D(Transform weapon, GameObject target, float projectileSpeed)
    {
        // positions
        Vector3 shooterPosition = weapon.position;
        Vector3 targetPosition = target.transform.position;
        // velocities
        // Shooter velocity is zero because ship speed is not added to weapon initial velocity
        Vector3 shooterVelocity = Vector3.zero; 
        Vector3 targetVelocity = target.GetComponent<Rigidbody>() ? target.GetComponent<Rigidbody>().velocity : Vector3.zero;

        // calculate intercept
        Vector3 interceptPoint = FirstOrderIntercept
        (
            shooterPosition,
            shooterVelocity,
            projectileSpeed,
            targetPosition,
            targetVelocity
        );

        return interceptPoint;
    }

    //first-order intercept using absolute target position
    private static Vector3 FirstOrderIntercept
    (
        Vector3 shooterPosition,
        Vector3 shooterVelocity,
        float shotSpeed,
        Vector3 targetPosition,
        Vector3 targetVelocity
    )
    {
        Vector3 targetRelativePosition = targetPosition - shooterPosition;
        Vector3 targetRelativeVelocity = targetVelocity - shooterVelocity;
        float t = FirstOrderInterceptTime
        (
            shotSpeed,
            targetRelativePosition,
            targetRelativeVelocity
        );
        return targetPosition + t * (targetRelativeVelocity);
    }
    //first-order intercept using relative target position
    private static float FirstOrderInterceptTime
    (
        float shotSpeed,
        Vector3 targetRelativePosition,
        Vector3 targetRelativeVelocity
    )
    {
        float velocitySquared = targetRelativeVelocity.sqrMagnitude;
        if (velocitySquared < 0.001f)
            return 0f;

        float a = velocitySquared - shotSpeed * shotSpeed;

        //handle similar velocities
        if (Mathf.Abs(a) < 0.001f)
        {
            float t = -targetRelativePosition.sqrMagnitude /
            (
                2f * Vector3.Dot
                (
                    targetRelativeVelocity,
                    targetRelativePosition
                )
            );
            return Mathf.Max(t, 0f); //don't shoot back in time
        }

        float b = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
        float c = targetRelativePosition.sqrMagnitude;
        float determinant = b * b - 4f * a * c;

        if (determinant > 0f)
        { //determinant > 0; two intercept paths (most common)
            float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a),
                    t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
            if (t1 > 0f)
            {
                if (t2 > 0f)
                    return Mathf.Min(t1, t2); //both are positive
                else
                    return t1; //only t1 is positive
            }
            else
                return Mathf.Max(t2, 0f); //don't shoot back in time
        }
        else if (determinant < 0f) //determinant < 0; no intercept path
            return 0f;
        else //determinant = 0; one intercept path, pretty much never happens
            return Mathf.Max(-b / (2f * a), 0f); //don't shoot back in time
    }
}
}