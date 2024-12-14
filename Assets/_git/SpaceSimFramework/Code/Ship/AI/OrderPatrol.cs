using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SpaceSimFramework
{
public class OrderPatrol : Order
{
    private State _state;
    private Transform _target;
    private Vector3[] _waypoints;
    private int _nextWaypoint = 0;

    private enum State
    {
        Patrol, // Fly between patrol waypoints
        Attack  // Engage an enemy contact
    }

    public OrderPatrol()
    {
        Name = "Patrol";
        _state = State.Patrol;
    }

    private Vector3[] GetWaypoints(ShipAI controller)
    {
        Faction shipfaction = controller.ship.faction;
        List<Vector3> wpList = new List<Vector3>();

        foreach (var station in SectorNavigation.Stations)
        {
            if (station.GetComponent<Station>().faction == shipfaction)
                wpList.Add(station.transform.position + GetRandomOffset());
        }

        foreach (var jg in SectorNavigation.Jumpgates)
        {
            wpList.Add(jg.transform.position + GetRandomOffset());
        }

        // Get a few random points too
        int mapSize = SectorNavigation.SectorSize;
        for (int i = 0; i < Random.Range(2, 5); i++)
        {
            wpList.Add(new Vector3(
                Random.Range(0, mapSize) - mapSize / 2,
                Random.Range(0, mapSize) - mapSize / 2,
                Random.Range(50, 300) * Mathf.Sign(Random.value - 0.5f)));
        }

        return wpList.ToArray();
    }

    // Get a random point above or below the target, between 50 and 300 units distant
    private Vector3 GetRandomOffset()
    {
        return new Vector3(0, 0, Random.Range(50, 300) * Mathf.Sign(Random.value - 0.5f));
    }

    public override void UpdateState(ShipAI controller)
    {
        if (_waypoints == null || _waypoints.Length < 2)
            _waypoints = GetWaypoints(controller);

        if (controller.wayPointList.Count == 0)
        {
            controller.FinishOrder();
            return;
        }

        if (_state == State.Patrol)
        {
            PatrolWaypoints(controller);
        }
        if (_state == State.Attack)
        {
            ShipSteering.SteerTowardsTarget(controller);
            AttackTarget(controller);
        }

        // Scan for enemies
        var enemies = SectorNavigation.Instance.GetClosestEnemyShip(controller.transform, controller.ship.ScannerRange);
        if (enemies.Count > 0)
        {
            _target = enemies[0].transform;
            _state = State.Attack;
        }
    }

    private void PatrolWaypoints(ShipAI controller)
    {
        float distance = Vector3.Distance(_waypoints[_nextWaypoint], controller.transform.position);

        if (distance < 30)
        {
            _nextWaypoint = (_nextWaypoint + 1) % _waypoints.Length;
        }

        controller.throttle = Mathf.MoveTowards(controller.throttle, 1.0f, Time.deltaTime * 0.5f);

        ShipSteering.SteerTowardsDestination(controller, _waypoints[_nextWaypoint]);
    }

    private void AttackTarget(ShipAI controller)
    {
        if (_target == null)
        {
            _state = State.Patrol;
            return;
        }

        float distance = Vector3.Distance(_target.position, controller.transform.position);
        float range = controller.ship.Equipment.GetWeaponRange();

        if (distance < range)
        {
            // Predict lead
            GameObject shooter = controller.gameObject;
            float projectileSpeed = controller.ship.Equipment.Guns[0].ProjectileSpeed;

            controller.tempDest = Targeting.PredictTargetLead3D(shooter, _target.gameObject, projectileSpeed);

            // Fire if on target
            Vector3 attackVector = _target.position - shooter.transform.position;
            float angle = Vector3.Angle(attackVector, controller.transform.forward);

            if (angle < 10)
                controller.ship.Equipment.IsFiring = true;
        }
        else
        {
            controller.tempDest = Vector3.zero;
        }

        float thr = distance > 100f ? 1f : (distance / 100f);
        controller.throttle = Mathf.MoveTowards(controller.throttle, thr, Time.deltaTime * 0.5f);
    }

    public override void Destroy()
    {
    }

}
}