using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceSimFramework
{
[RequireComponent(typeof(IntentLineManager))]
public class InputHandler : Singleton<InputHandler>
{
    public enum OrderType
    {
        MOVE, ATTACK, MOVE_WP
    }

    [HideInInspector]
    public List<GameObject> SelectedObjects;
    public GameObject SelectedObject {
        get
        {
            return SelectedObjects.Count > 0 ? SelectedObjects[0] : null;
        }
        set
        {
            if (CanvasViewController.Instance.IsMapOpenForSelection)
            {
                CanvasViewController.Instance.SetMapSelectedObject(value.transform);
            }
            SelectedObjects = value == null ? new List<GameObject>() : new List<GameObject>() { value };
        }
    }

    public TargetInfocard Infocard;

    private IntentLineManager _intentLineManager;
    private bool _isPlayerShipSelected = true;

    private List<Vector3> _selectedWaypoints;
    private bool _isMultipleSelecting = false;

    #region Selection Utility Rectangles
    private static Texture2D _whiteTexture;
    private static Texture2D WhiteTexture
    {
        get
        {
            if (_whiteTexture == null)
            {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }

            return _whiteTexture;
        }
    }


    private static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        // Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    private static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, WhiteTexture);
        GUI.color = Color.white;
    }

    private static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Top
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }

    private static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
    {
        var v1 = Camera.main.ScreenToViewportPoint(screenPosition1);
        var v2 = Camera.main.ScreenToViewportPoint(screenPosition2);
        var min = Vector3.Min(v1, v2);
        var max = Vector3.Max(v1, v2);
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    bool isSelecting = false;
    Vector3 mousePosition1;

    private bool IsWithinSelectionBounds(GameObject gameObject)
    {
        var camera = Camera.main;
        var viewportBounds =
            GetViewportBounds(camera, mousePosition1, Input.mousePosition);

        return viewportBounds.Contains(
            camera.WorldToViewportPoint(gameObject.transform.position));
    }

    void OnGUI()
    {
        if (isSelecting)
        {
            // Create a rect from both mouse positions
            var rect = GetScreenRect(mousePosition1, Input.mousePosition);
            // Draw transparent rectangle
            DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            // Draw rectangle border
            DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }
    #endregion Selection Utility Rectangles

    private void Awake()
    {
        _intentLineManager = GetComponent<IntentLineManager>();
        Ship.ShipDestroyedEvent += OnShipDestroyed;
    }

    private void Update()
    {
        if (CanvasViewController.IsMapActive)  // Disable the input handler if map view is not active
            HandleMapViewInput();

        if (Input.GetMouseButtonDown(0))
        {
            mousePosition1 = Input.mousePosition;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int layerMask = 1 << 8;
            layerMask = ~layerMask;
            if (Physics.Raycast(ray, out hit, 5000.0f, layerMask))
            {
                if (hit.transform.gameObject == Ship.PlayerShip.gameObject)
                    return;

                string tag = hit.transform.gameObject.tag;
                if (tag == "Ship" || tag == "Station" || tag == "Asteroid"
                    || tag == "Waypoint" || tag == "Jumpgate" || tag == "Wreck")
                    SelectedObject = hit.transform.gameObject;
            }
        }
    }

    private void HandleMapViewInput()
    {
        if (Input.GetKeyDown(KeyCode.R) && _isPlayerShipSelected)
        {
            Ship otherShip = SelectedObjects[0].GetComponent<Ship>();

            Ship.PlayerShip.IsPlayerControlled = false;
            otherShip.IsPlayerControlled = true;
            otherShip.AIInput.FinishOrder();
            Camera.main.GetComponent<CameraController>().SetTargetShip(otherShip);
        }

        if (Input.GetMouseButtonDown(0))
        {
            mousePosition1 = Input.mousePosition;
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                SelectedObjects.Clear();
                Infocard.InfocardPanel.SetActive(false);
            }
            else
            {
                Camera.main.GetComponent<MapCameraController>().IsTrackingTarget = false;
            }
        }
        if (Input.GetMouseButton(0) && Vector3.Distance(Input.mousePosition, mousePosition1) > 10)
        {
            isSelecting = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (!isSelecting)
            {
                return;
            }

            isSelecting = false;
            SelectedObjects.Clear();
            Infocard.InfocardPanel.SetActive(false);

            foreach (GameObject playerShip in Player.Instance.Ships)
            {
                if (IsWithinSelectionBounds(playerShip))
                {
                    SelectedObjects.Add(playerShip);
                    _isPlayerShipSelected = true;
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (!_isMultipleSelecting)
                    OnPositionClicked(Input.mousePosition);
                else
                    OnPositionAdded(Input.mousePosition);
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _isMultipleSelecting = true;
            _selectedWaypoints = new List<Vector3>();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _isMultipleSelecting = false;
            if (_selectedWaypoints.Count > 0)
            {
                // Issue order
                foreach (GameObject selectedShip in SelectedObjects)
                {
                    Ship shipComp = selectedShip.GetComponent<Ship>();
                    shipComp.IsPlayerControlled = false;
                    shipComp.AIInput.MoveWaypoints(_selectedWaypoints);
                    _intentLineManager.RegisterGivenOrder(selectedShip, OrderType.MOVE_WP, _selectedWaypoints.ToArray());
                }
            }
        }
    }

    private void OnShipDestroyed(object sender, EventArgs e)
    {
        GameObject ship = ((GameObject)sender);
        if (SelectedObjects.Contains(ship))
        {
            SelectedObjects.Remove(ship);
        }
    }

    private void OnPositionAdded(Vector3 mousePosition)
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        float enter = 0.0f;
        if (plane.Raycast(ray, out enter))
        {
            Vector3 positionClicked = ray.GetPoint(enter);
            _selectedWaypoints.Add(positionClicked);
        }
    }

    private void OnPositionClicked(Vector3 mousePosition)
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        float enter = 0.0f;
        if (plane.Raycast(ray, out enter))
        {
            Vector3 positionClicked = ray.GetPoint(enter);
            if (CanvasViewController.Instance.IsMapOpenForSelection)
            {
                CanvasViewController.Instance.SetMapSelectedPosition(positionClicked);
            }
            else
            {
                // Issue order
                foreach (GameObject selectedShip in SelectedObjects)
                {
                    Ship shipComp = selectedShip.GetComponent<Ship>();
                    if (shipComp == null || shipComp.faction != Player.Instance.PlayerFaction)
                        continue;   // Selected object can be a station or gate
                    shipComp.IsPlayerControlled = false;
                    shipComp.AIInput.MoveTo(positionClicked);
                    _intentLineManager.RegisterGivenOrder(selectedShip, OrderType.MOVE, new Vector3[] { });
                }
            }
        }
    }

    public void OnMarkerLeftClicked(GameObject clickedObject)
    {
        Camera.main.GetComponent<MapCameraController>().IsTrackingTarget = false;
        if (clickedObject.tag == "Ship")
        {
            if (CanvasViewController.IsMapActive)
            {
                if (clickedObject.GetComponent<Ship>().faction == Player.Instance.PlayerFaction)
                {
                    _isPlayerShipSelected = true;
                    TextFlash.ShowYellowText("Press R to switch player ship\nPress F to start tracking");
                }
                else
                {
                    _isPlayerShipSelected = false;
                    TextFlash.ShowYellowText("Press F to start tracking");
                }
            }
          
            // Select ship
            SelectedObjects = new List<GameObject>();
            SelectedObjects.Add(clickedObject);
            Infocard.InitializeInfocard(clickedObject.GetComponent<Ship>());
        }
        else
        {
            SelectedObject = clickedObject;
            Infocard.InitializeInfocard(null);
        }
    }

    public void OnMarkerRightClicked(GameObject clickedObject)
    {
        // Issue order
        if(clickedObject.tag == "Ship")
        {
            Faction targetShipFaction = clickedObject.GetComponent<Ship>().faction;
            foreach (GameObject selectedShip in SelectedObjects)
            {
                Ship shipComp = selectedShip.GetComponent<Ship>();
                shipComp.IsPlayerControlled = false;
                if (targetShipFaction == Player.Instance.PlayerFaction)
                {
                    shipComp.AIInput.Follow(clickedObject.transform);
                    _intentLineManager.RegisterGivenOrder(selectedShip, OrderType.MOVE, new Vector3[] { });
                }
                else
                {
                    shipComp.AIInput.Attack(clickedObject);
                    _intentLineManager.RegisterGivenOrder(selectedShip, OrderType.ATTACK, new Vector3[] { });
                }
            }
        }
        else if (clickedObject.tag == "Station")
        {
            foreach (GameObject selectedShip in SelectedObjects)
            {
                Ship shipComp = selectedShip.GetComponent<Ship>();
                shipComp.IsPlayerControlled = false;
                shipComp.AIInput.DockAt(clickedObject);
                _intentLineManager.RegisterGivenOrder(selectedShip, OrderType.MOVE, new Vector3[] { });
            }
        }
        else if (clickedObject.tag == "Asteroid")
        {
            foreach (GameObject selectedShip in SelectedObjects)
            {
                Ship shipComp = selectedShip.GetComponent<Ship>();
                shipComp.IsPlayerControlled = false;
                shipComp.AIInput.Attack(clickedObject);
                _intentLineManager.RegisterGivenOrder(selectedShip, OrderType.ATTACK, new Vector3[] { });
            }
        }
        else if (clickedObject.tag == "Jumpgate")
        {
            foreach (GameObject selectedShip in SelectedObjects)
            {
                Ship shipComp = selectedShip.GetComponent<Ship>();
                shipComp.IsPlayerControlled = false;
                shipComp.AIInput.DockAt(clickedObject);
                _intentLineManager.RegisterGivenOrder(selectedShip, OrderType.MOVE, new Vector3[] { });
            }
        }
    }

    public GameObject GetCurrentSelectedTarget()
    {
        return SelectedObject;
    }
}
}