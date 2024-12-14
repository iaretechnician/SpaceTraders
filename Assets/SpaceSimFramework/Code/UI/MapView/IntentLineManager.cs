using UnityEngine;
using System;
using System.Collections.Generic;

namespace SpaceSimFramework
{
public class IntentLineManager : MonoBehaviour
{
    public GameObject LinePrefab;
    // Ties together ships and their order line descriptors
    private Dictionary<GameObject, LineCoords> _lineDescriptors;

    private abstract class LineCoords
    {
        public LineRenderer _line;
        public Transform _originShip;

        public abstract Vector3[] GetLinePoints();
    }

    private class LineCoordsDestination : LineCoords
    {
        public Vector3 Destination;

        public override Vector3[] GetLinePoints()
        {
            return new Vector3[]{
                _originShip.position,
                Destination
            };
        }
    }

    private class LineCoordsWaypoints : LineCoords
    {
        public Vector3[] Destinations;

        private List<Vector3> _waypoints;

        public override Vector3[] GetLinePoints()
        {
            if (_waypoints == null)
                Init();

            // Check if any of the waypoints need to be removed (because they've been reached)
            RemoveReachedWaypoints();

            Vector3[] linePoints = new Vector3[_waypoints.Count + 1];
            linePoints[0] = _originShip.position;
            for (int i = 0; i < _waypoints.Count; i++)
                linePoints[i + 1] = _waypoints[i];
            return linePoints;
        }

        private void Init()
        {
            _waypoints = new List<Vector3>();
            _waypoints.AddRange(Destinations);
        }

        private void RemoveReachedWaypoints()
        {
            if (_waypoints.Count < 2)
                return;

            float distance = Vector3.Distance(_waypoints[0], _originShip.position);

            if (distance < 30)
            {
                _waypoints.RemoveAt(0);
            }
        }
    }

    private class LineCoordsTarget : LineCoords
    {
        public Transform Target;

        public override Vector3[] GetLinePoints()
        {
            if (Target == null)
                return null;

            return new Vector3[]{
                _originShip.position,
                Target.position
            };
        }
    }

    private void Awake()
    {
        _lineDescriptors = new Dictionary<GameObject, LineCoords>();
    }

    private void Update()
    {
        foreach (var shipIntent in _lineDescriptors)
        {
            if (shipIntent.Key == null || shipIntent.Value.GetLinePoints() == null)
            {
                GameObject.DestroyImmediate(_lineDescriptors[shipIntent.Key]._line);
                continue;
            }

            shipIntent.Value._line.positionCount = shipIntent.Value.GetLinePoints().Length;
            shipIntent.Value._line.SetPositions(shipIntent.Value.GetLinePoints());
        }
    }

    public void RegisterGivenOrder(GameObject ship, InputHandler.OrderType orderType, Vector3[] destinations)
    {
        ShipAI shipAI = ship.GetComponent<ShipAI>();
        if (shipAI.CurrentOrder == null)
            return;

        // Create a new intent line
        LineRenderer line = GameObject.Instantiate(LinePrefab, transform).GetComponent<LineRenderer>();
        line.useWorldSpace = true;

        // Determine color       
        Color color = orderType == InputHandler.OrderType.ATTACK ? Color.red : Color.white;
        line.startColor = line.endColor = color;

        // Get notified when order is finished in order to remove the intent line
        ShipAI.OnOrderFinished += OnGivenOrderFinished;
        Station.ShipDockedEvent += OnShipDocked;

        // Add line to descriptor pool so that it can be updated
        LineCoords lineDescriptor = null;
        if (orderType == InputHandler.OrderType.ATTACK)
        {
            lineDescriptor = new LineCoordsTarget();
            ((LineCoordsTarget)lineDescriptor).Target = shipAI.wayPointList[0];
        }
        else if (orderType == InputHandler.OrderType.MOVE)
        {
            if(shipAI.tempDest != Vector3.zero)
            {
                lineDescriptor = new LineCoordsDestination();
                ((LineCoordsDestination)lineDescriptor).Destination = shipAI.tempDest;
            }
            else
            {
                lineDescriptor = new LineCoordsTarget();
                ((LineCoordsTarget)lineDescriptor).Target = shipAI.wayPointList[0];
            }
        }
        else
        {
            lineDescriptor = new LineCoordsWaypoints();
            ((LineCoordsWaypoints)lineDescriptor).Destinations = destinations;
        }
        lineDescriptor._originShip = ship.transform;
        lineDescriptor._line = line;

        // Remove any previously existing lines
        if (_lineDescriptors.ContainsKey(ship))
        {
            Destroy(_lineDescriptors[ship]._line);
            _lineDescriptors.Remove(ship.gameObject);
        }

        _lineDescriptors.Add(ship, lineDescriptor);
    }

    private void OnGivenOrderFinished(object sender, EventArgs e)
    {
        Ship ship = (Ship)sender;
        if (_lineDescriptors.ContainsKey(ship.gameObject))
            GameObject.DestroyImmediate(_lineDescriptors[ship.gameObject]._line);
        _lineDescriptors.Remove(ship.gameObject);
    }

    private void OnShipDocked(object sender, EventArgs e)
    {
        GameObject ship = (GameObject)sender;
        if (_lineDescriptors.ContainsKey(ship))
            GameObject.Destroy(_lineDescriptors[ship]._line);
        _lineDescriptors.Remove(ship);
    }

    public void SwitchMode(ShipMarker.MarkerMode markerMode)
    {
        foreach(LineCoords lineData in _lineDescriptors.Values)
        {
            lineData._line.enabled = markerMode == ShipMarker.MarkerMode.Map;
        }
    }
}
}