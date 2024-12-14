using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace SpaceSimFramework
{
    /// <summary>
    /// Class specifically to deal with input.
    /// </summary>
    public class ShipAI : MonoBehaviour
    {
        public static event EventHandler OnOrderFinished;

        public Order CurrentOrder;
        // Don't execute the current order if the ship is undocking
        [HideInInspector]
        public bool IsUndocking = false;

        // Variables used by AI orders
        [HideInInspector] public List<Transform> wayPointList;
        [HideInInspector] public int nextWayPoint;
        [HideInInspector] public Vector3 tempDest;
        [HideInInspector] public float throttle;

        // Used by Steering Action
        [HideInInspector] public Vector3 angularTorque;
        [HideInInspector] public PIDController pid_angle, pid_velocity;
        [HideInInspector] public float pid_P = 10, pid_I = 0.5f, pid_D = 0.5f;

        [HideInInspector] public Rigidbody rBody;
        [HideInInspector] public Ship ship;

        private bool _isWaitingForMap = false;
        // Which order is being given in on the map
        private string _mapWaitingOrder;
        // Reference to map coroutine to ensure it cannot be open twice
        private Coroutine _mapWaitCoroutine = null;

        // Collision avoidance overrides any order execution
        private bool _avoidCollision = false;
        // Temp destination will be overriden if avoiding a collision, so remember the original
        private Vector3 _savedTempDest = Vector3.zero;
    

        private void Awake()
        {
            ship = GetComponent<Ship>();
            rBody = GetComponent<Rigidbody>();

            pid_angle = new PIDController(pid_P, pid_I, pid_D);
            pid_velocity = new PIDController(pid_P, pid_I, pid_D);

            wayPointList = new List<Transform>();
        }

        private void Update()
        {
            // Wait for user to select location on map
            if (_isWaitingForMap && _mapWaitCoroutine == null)
            {
                _mapWaitCoroutine = StartCoroutine("WaitForMapPosition");
            }
            if (!_isWaitingForMap && _mapWaitCoroutine != null)
            {
                StopCoroutine(_mapWaitCoroutine);
                _mapWaitCoroutine = null;
            }

            if (ship.IsPlayerControlled) // Flown by player, not AI
                return;

            // If an order is present, perform it
            if (CurrentOrder != null && !IsUndocking)
            {
                // Disable collision detection when docking for obvious reasons
                if (!(CurrentOrder.Name == "Dock" && ((OrderDock)CurrentOrder).InFinalApproach) &&
                    !(CurrentOrder.Name == "Trade" && ((OrderTrade)CurrentOrder).InFinalApproach))
                {
                    CheckForwardCollisions();
                }

                if (_avoidCollision)
                {
                    ShipSteering.SteerTowardsTarget(this);
                    if (Vector3.Angle(transform.forward, tempDest - transform.position) < 10)
                        throttle = 0.5f;
                    else
                        throttle = 0f;
                    // If the avoidance destination was reached, resume flight towards original destination
                    if (Vector3.Distance(transform.position, tempDest) < 10)
                    {
                        tempDest = _savedTempDest;
                        _avoidCollision = false;
                    }
                }
                else if (!IsUndocking)
                {                
                    // Update the order
                    CurrentOrder.UpdateState(this);
                }
                else
                {
                    throttle = 0f;
                }
            }
        }

        private void OnDestroy()
        {
            OnOrderFinished?.Invoke(ship, EventArgs.Empty);   // Notify listeners, if there are no listeners this will be null
        }

        /// <summary>
        /// Contextually enables or disables autopilot of player ship. If the ship is flown by the player, the autopilot will turn on
        /// depending on the selected target.
        /// </summary>
        public void OnAutopilotToggle()
        {
            if (!ship.IsPlayerControlled)
            {
                ConsoleOutput.PostMessage("Autopilot off.", Color.red);
                FinishOrder();
            }
            else
            {
                GameObject selectedTarget = InputHandler.Instance.GetCurrentSelectedTarget();
                if (selectedTarget != null)
                {
                    ConsoleOutput.PostMessage("Autopilot on.", Color.green);

                    if (selectedTarget.tag == "Station")
                        DockAt(selectedTarget);
                    else if (selectedTarget.tag == "Ship")
                        Follow(selectedTarget.transform);
                    else if (selectedTarget.tag == "Jumpgate")
                        DockAt(selectedTarget);
                    else
                        MoveTo(selectedTarget.transform);
                }
            }
        }

        /// <summary>
        /// Finish current ship order and clean up.
        /// </summary>
        public void FinishOrder()
        {
            if (CurrentOrder == null)
                return;

            OnOrderFinished?.Invoke(ship, EventArgs.Empty);   // Notify listeners, if there are no listeners this will be null

            CurrentOrder.Destroy();
            CurrentOrder = null;
            tempDest = Vector3.zero;
            _avoidCollision = false;

            wayPointList.Clear();
            if (ship.faction == Player.Instance.PlayerFaction)
                ConsoleOutput.PostMessage(name + " has completed the order.");
            throttle = 0;

            if (ship == Ship.PlayerShip)
                ship.IsPlayerControlled = true;
        }

        // When giving some orders (like move to destination, dock or attack) a map is opened to 
        // select a target/destination
        #region map-dependent commands
        private IEnumerator WaitForMapPosition()
        {
            while (true)
            {
                CanvasViewController.Instance.IsMapOpenForSelection = true;
                Transform target = CanvasViewController.Instance.GetMapSelectedObject();
                if (target != null)
                {
                    _isWaitingForMap = false;
                    if(_mapWaitingOrder == "MoveTo")
                        MoveTo(target);
                    else if (_mapWaitingOrder == "DockAt")
                        DockAt(target.gameObject);
                    else if (_mapWaitingOrder == "Follow")
                        Follow(target);

                    yield return null;
                }
                tempDest = CanvasViewController.Instance.GetMapSelectedPosition();
                if (tempDest != Vector3.zero)
                {
                    _isWaitingForMap = false;
                    MoveTo(tempDest);
                    yield return null;
                }
                yield return null;
            }
        }

        private void OpenMap()
        {
            // Clear map previous map selections
            CanvasViewController.Instance.IsMapOpenForSelection = true;
            CanvasViewController.Instance.SetMapSelectedObject(null);
            CanvasViewController.Instance.SetMapSelectedPosition(Vector2.zero);
            // Open map for selection
            CanvasViewController.Instance.ToggleMap();
        }

        private void UndockIfNecessary()
        {
            GameObject stationDocked = SectorNavigation.GetStationByID(ship.StationDocked);
            if (stationDocked != null)
            {
                stationDocked.GetComponent<Station>().UndockShip(ship.gameObject);
            }
        }

        #endregion map-dependent commands

        #region collision avoidance
        private Vector3 upOffset = new Vector3(0, 3, 0),
            downOffset = new Vector3(0, -3, 0),
            leftOffset = new Vector3(-3, 0, 0),
            rightOffset = new Vector3(3, 0, 0);
    
        private void CheckForwardCollisions()
        {
            if (throttle < 0.05f)
                return;

            // Shoot 4 raycasts from each tip of the ship

            RaycastHit hit;
            float minDistance = float.MaxValue;
            Vector3 _avoidancePosition = Vector3.zero;

            Debug.DrawRay(transform.position + upOffset, transform.forward);
            if (Physics.Raycast(transform.position + upOffset, transform.forward, out hit, 100))
            {
                if(hit.transform.tag == "Asteroid" || hit.transform.tag == "Station")
                    if(hit.distance < minDistance)
                    {
                        var colliderSize = hit.collider.bounds.extents;
                        _savedTempDest = tempDest;
                        minDistance = hit.distance;
                        _avoidancePosition = hit.transform.position - transform.up * colliderSize.y - transform.right * colliderSize.x;
                    }
            }
            Debug.DrawRay(transform.position + downOffset, transform.forward);
            if (Physics.Raycast(transform.position + downOffset, transform.forward, out hit, 100))
            {
                if (hit.transform.tag == "Asteroid" || hit.transform.tag == "Station")
                    if (hit.distance < minDistance)
                    {
                        var colliderSize = hit.collider.bounds.extents;
                        _savedTempDest = tempDest;
                        minDistance = hit.distance;
                        _avoidancePosition = hit.transform.position + transform.up * colliderSize.y - transform.right * colliderSize.x;
                    }
            }
            Debug.DrawRay(transform.position + rightOffset, transform.forward);
            if (Physics.Raycast(transform.position + rightOffset, transform.forward, out hit, 100))
            {
                if (hit.transform.tag == "Asteroid" || hit.transform.tag == "Station")
                    if (hit.distance < minDistance)
                    {
                        var colliderSize = hit.collider.bounds.extents;
                        _savedTempDest = tempDest;
                        minDistance = hit.distance;
                        _avoidancePosition = hit.transform.position + transform.up * colliderSize.y - transform.right * colliderSize.x;
                    }
            }
            Debug.DrawRay(transform.position + leftOffset, transform.forward);
            if (Physics.Raycast(transform.position + leftOffset, transform.forward, out hit, 100))
            {
                if (hit.transform.tag == "Asteroid" || hit.transform.tag == "Station")
                    if (hit.distance < minDistance)
                    {
                        var colliderSize = hit.collider.bounds.extents;
                        _savedTempDest = tempDest;
                        minDistance = hit.distance;
                        _avoidancePosition = hit.transform.position + transform.up * colliderSize.y + transform.right * colliderSize.x;
                    }
            }

            if (minDistance != float.MaxValue && hit.collider != null)
            {
                if (!_avoidCollision)    // Don't lose the original destination when doing multiple runs of collision avoidance
                    _savedTempDest = tempDest;

                tempDest = _avoidancePosition;
                _avoidCollision = true;
            }
        }
        #endregion collision avoidance

        // Autopilot commands
        #region commands
        /// <summary>
        /// Commands the ship to move to a given object.
        /// </summary>
        /// <param name="destination"></param>
        public void MoveTo(Transform destination)
        {
            FinishOrder();
            ship.IsPlayerControlled = false;
            if (destination == null)
            {
                _isWaitingForMap = true;
                _mapWaitingOrder = "MoveTo";
                OpenMap();
            }
            else
            {
                wayPointList.Clear();
                wayPointList.Add(destination);
                nextWayPoint = 0;

                CurrentOrder = new OrderMove();
                if (ship.faction == Player.Instance.PlayerFaction)
                    ConsoleOutput.PostMessage(name + " command Move accepted");
            }
            UndockIfNecessary();
        }

        /// <summary>
        /// Commands the ship to move to a specified position.
        /// </summary>
        /// <param name="position">world position of destination</param>
        public void MoveTo(Vector3 position)
        {
            FinishOrder();
            tempDest = position;
            if (tempDest == Vector3.zero)
                return;
            ship.IsPlayerControlled = false;

            CurrentOrder = new OrderMove();
            UndockIfNecessary();

            if(ship.faction == Player.Instance.PlayerFaction)
                ConsoleOutput.PostMessage(name + " command Move accepted");
        }

        /// <summary>
        /// Commands the ship to move through the given waypoints. Once the last one is reached,
        /// the route is restarted from the first waypoint.
        /// </summary>
        /// <param name="waypoints"></param>
        public void PatrolPath(Transform[] waypoints)
        {
            FinishOrder();
            ship.IsPlayerControlled = false;
            CurrentOrder = new OrderPatrol();

            wayPointList.Clear();

            UndockIfNecessary();
            if (ship.faction == Player.Instance.PlayerFaction)
                ConsoleOutput.PostMessage(name + " command Patrol accepted");
        }

        /// <summary>
        /// Commands the ship to move randomly at low speed, roughly in the same area.
        /// </summary>
        public void Idle()
        {
            FinishOrder();
            ship.IsPlayerControlled = false;
            CurrentOrder = new OrderIdle();
            tempDest = transform.position;

            UndockIfNecessary();
            if (ship.faction == Player.Instance.PlayerFaction)
                ConsoleOutput.PostMessage(name + " command Idle accepted");
        }

        /// <summary>
        /// Commands the ship to follow player ship
        /// </summary>
        public void FollowMe()
        {
            FinishOrder();
            // Cant chase its own tail
            if (ship == Ship.PlayerShip)
                return;

            ship.IsPlayerControlled = false;
            CurrentOrder = new OrderFollow(this, Ship.PlayerShip);

            wayPointList.Clear();
            wayPointList.Add(Ship.PlayerShip.transform);
            nextWayPoint = 0;

            UndockIfNecessary();
            if (ship.faction == Player.Instance.PlayerFaction)
                ConsoleOutput.PostMessage(name + " command Follow accepted");
        }

        /// <summary>
        /// Commands the ship to follow a target
        /// </summary>
        public void Follow(Transform target)
        {
            FinishOrder();
            ship.IsPlayerControlled = false;
            if (target == null)
            {
                _isWaitingForMap = true;
                _mapWaitingOrder = "Follow";
                OpenMap();
            }
            else
            {
                wayPointList.Clear();
                wayPointList.Add(target);
                nextWayPoint = 0;

                CurrentOrder = new OrderFollow(this, target.GetComponent<Ship>());
                if (ship.faction == Player.Instance.PlayerFaction)
                    ConsoleOutput.PostMessage(name + " command Follow accepted");
            }
            UndockIfNecessary();
        }

        /// <summary>
        /// Commands the ship to dock at a station
        /// </summary>
        public void DockAt(GameObject dockable)
        {
            FinishOrder();
            UndockIfNecessary();
            if (dockable == null)
            {
                _isWaitingForMap = true;
                _mapWaitingOrder = "DockAt";
                OpenMap();
            }
            else
            {
                ship.IsPlayerControlled = false;
                GameObject[] dockWaypoints = null;

                if (dockable.tag == "Station")
                {
                    Station dockingTarget = dockable.GetComponent<Station>();
                    CurrentOrder = new OrderDock(dockingTarget, ship);
                    try
                    {
                        dockWaypoints = dockingTarget.RequestDocking(gameObject);
                    }
                    catch (DockingException e)
                    {
                        FinishOrder();
                        return;
                    }
                }
                else if (dockable.tag == "Jumpgate")
                {
                    CurrentOrder = new OrderDock();
                    dockWaypoints = dockable.GetComponent<Jumpgate>().DockWaypoints;
                }

                wayPointList.Clear();
       
                for (int i = 0; i < dockWaypoints.Length; i++)
                    wayPointList.Add(dockWaypoints[i].transform);
       
                nextWayPoint = 0;

                if (ship.faction == Player.Instance.PlayerFaction)
                    ConsoleOutput.PostMessage(name + " command Dock accepted");
            }
        }

        /// <summary>
        /// Commands the ship to move around a list of positions
        /// </summary>
        public void MoveWaypoints(List<Vector3> positions)
        {
            FinishOrder();
            ship.IsPlayerControlled = false;
            if (positions != null && positions.Count > 0)
            {
                CurrentOrder = new OrderMovePositions(positions);
            }
            UndockIfNecessary();
        }

        /// <summary>
        /// Commands the ship to attack an object
        /// </summary>
        public void Attack(GameObject target)
        {
            FinishOrder();
            CurrentOrder = new OrderAttack();
            ship.IsPlayerControlled = false;

            wayPointList.Clear();
            wayPointList.Add(target.transform);
            nextWayPoint = 0;

            UndockIfNecessary();
            if (ship.faction == Player.Instance.PlayerFaction)
                ConsoleOutput.PostMessage(name + " command Attack accepted");
        }

        /// <summary>
        /// Commands the ship to attack all enemies in the area
        /// </summary>
        public void AttackAll()
        {
            FinishOrder();
            CurrentOrder = new OrderAttackAll();
            ship.IsPlayerControlled = false;

            wayPointList.Clear();
            nextWayPoint = 0;

            UndockIfNecessary();
            if (ship.faction == Player.Instance.PlayerFaction)
                ConsoleOutput.PostMessage(name + " command Attack All accepted");
        }

        /// <summary>
        /// Commands the ship to attack all enemies in the area
        /// </summary>
        public void AutoTrade()
        {
            FinishOrder();
            CurrentOrder = new OrderTrade();
            ship.IsPlayerControlled = false;

            wayPointList.Clear();
            nextWayPoint = 0;

            UndockIfNecessary();
            if (ship.faction == Player.Instance.PlayerFaction)
                ConsoleOutput.PostMessage(name + " command AutoTrade accepted");
        }

        #endregion commands
    }
}