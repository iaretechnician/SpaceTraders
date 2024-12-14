using UnityEngine;

namespace SpaceSimFramework
{
public class OrderAttack : Order
{
    enum State
    {
        Chase, Shoot, Evade, GetDistance
    }

    private State _state;
    private Transform _target;
    private float _minrange = 150f;
    private float _previousArmorValue;
    private float _evadeTimer;

    public OrderAttack()
    {
        Name = "Attack";
        _state = State.Chase;
    }

    public override void UpdateState(ShipAI controller)
    {
        if(CheckTransitions(controller))
            controller.FinishOrder();
        ComputeState(controller);
    }

    private bool CheckTransitions(ShipAI controller)
    {
        _target = controller.wayPointList[controller.nextWayPoint];

        // Check if target is gone/destroyed
        if (_target == null)
            return true;

        float distance = Vector3.Distance(_target.position, controller.transform.position);
        float gunrange = controller.ship.Equipment.GetWeaponRange();

        if (_state == State.Chase && distance < gunrange)
        {
            _state = State.Shoot;
        }
        else if (_state == State.Shoot && distance < _minrange)
        {
            _state = State.GetDistance;
            controller.tempDest = _target.position + Vector3.up * 30;
        }
        else if (_state == State.Shoot && distance > gunrange)
        {
            _state = State.Chase;
        }
        else if (_state == State.GetDistance && distance > gunrange / 3)
        {
            _state = State.Chase;
        }
        if (_previousArmorValue/controller.ship.MaxArmor> 0.75f && controller.ship.Armor/ controller.ship.MaxArmor < 0.75f) {
            _evadeTimer = Random.value * 5 + 3;
            _state = State.Evade;
        }
        if (_previousArmorValue / controller.ship.MaxArmor > 0.5f && controller.ship.Armor / controller.ship.MaxArmor < 0.5f)
        {
            _evadeTimer = Random.value * 5 + 3;
            _state = State.Evade;
        }
        if (_previousArmorValue / controller.ship.MaxArmor > 0.25f && controller.ship.Armor / controller.ship.MaxArmor < 0.25f)
        {
            _evadeTimer = Random.value * 5 + 3;
            _state = State.Evade;
        }

        _previousArmorValue = controller.ship.Armor;

        return false;
    }

    private void ComputeState(ShipAI controller)
    {
        if (_target == null)
            return;

        float distance = Vector3.Distance(_target.position, controller.transform.position);
        float thr = distance > 100f ? 1f : (distance / 100f);
        controller.throttle = Mathf.MoveTowards(controller.throttle, thr, Time.deltaTime * 0.5f);

        switch (_state)
        {
            case State.Chase:
                controller.tempDest = Vector3.zero;
                ShipSteering.SteerTowardsTarget(controller);
                break;
            case State.Shoot:
                // Predict lead
                GameObject shooter = controller.gameObject;
                float projectileSpeed = controller.ship.Equipment.Guns[0].ProjectileSpeed;

                controller.tempDest = Targeting.PredictTargetLead3D(shooter, _target.gameObject, projectileSpeed);

                // Fire if on target
                Vector3 attackVector = _target.position - shooter.transform.position;
                float angle = Vector3.Angle(attackVector, controller.transform.forward);

                if (angle < 10)
                    controller.ship.Equipment.IsFiring = true;

                ShipSteering.SteerTowardsTarget(controller);
                break;
            case State.GetDistance:                
                controller.throttle = 1f;
                if (Vector3.Distance(controller.transform.position, controller.tempDest) < 5f)
                    controller.tempDest = Vector3.zero;
                if (controller.tempDest != Vector3.zero)
                    ShipSteering.SteerTowardsTarget(controller);
                break;
            case State.Evade:
                // Jam the throttle and rudder (yaw left, pitch up)
                controller.throttle = 1f;
                // Force multiplier is 100.0f from ShipPhysics
                controller.angularTorque = new Vector3(
                    -controller.ship.Physics.angularForce.x,
                    -controller.ship.Physics.angularForce.y,
                    -controller.ship.Physics.angularForce.z) * 100.0f;

                _evadeTimer -= Time.deltaTime;
                if (_evadeTimer == 0)
                    _state = State.Chase;

                break;
            default:
                break;
        }
    }

    public override void Destroy()
    {
    }


}
}