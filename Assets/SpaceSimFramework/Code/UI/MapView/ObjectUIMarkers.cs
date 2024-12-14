using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceSimFramework
{
[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(IntentLineManager))]
public class ObjectUIMarkers : Singleton<ObjectUIMarkers>
{
    public GameObject SelectedIndicatorPrefab;
    public GameObject NonSelectedIndicatorPrefab;
    public GameObject ObjectMarkerPrefab;
    private int MaxNumberOfMarkers = 30;

    private ShipMarker[] _selectedMarkerPool;
    private ShipMarker[] _nonSelectedMarkerPool;

    private float _hScreenWidth, _hScreenHeight;
    private Dictionary<int, GameObject> _selectedUnitMap;
    private Dictionary<int, GameObject> _nonSelectedUnitMap;
    private Dictionary<ObjectMarker, GameObject> _objectMarkerMap;

    private InputHandler _unitSelection;
    private IntentLineManager _intentLineManager;

    private void Start()
    {
        Station.ShipDockedEvent += OnShipDocked;
        _unitSelection = GetComponent<InputHandler>();
        _intentLineManager = GetComponent<IntentLineManager>();

        _selectedUnitMap = new Dictionary<int, GameObject>();
        _nonSelectedUnitMap = new Dictionary<int, GameObject>();
        _objectMarkerMap = new Dictionary<ObjectMarker, GameObject>();
        _hScreenHeight = Screen.height / 2;
        _hScreenWidth = Screen.width / 2;

        _selectedMarkerPool = new ShipMarker[MaxNumberOfMarkers];
        _nonSelectedMarkerPool = new ShipMarker[MaxNumberOfMarkers];

        // Initialize marker pool
        for (int i = 0; i < MaxNumberOfMarkers; i++)
        {
            GameObject selected = Instantiate(SelectedIndicatorPrefab, this.transform);
            _selectedMarkerPool[i] = selected.GetComponent<ShipMarker>();
            selected.SetActive(false);
            _selectedUnitMap.Add(i, null);

            GameObject nonSelected = Instantiate(NonSelectedIndicatorPrefab, this.transform);
            _nonSelectedMarkerPool[i] = nonSelected.GetComponent<ShipMarker>();
            nonSelected.SetActive(false);
            _nonSelectedUnitMap.Add(i, null);
        }
        // Instantiate fixed markers (always shown)
        foreach(GameObject station in SectorNavigation.Stations)
        {
            ObjectMarker marker = Instantiate(ObjectMarkerPrefab, this.transform).GetComponent<ObjectMarker>();
            _objectMarkerMap.Add(marker, station);
            marker.SetTarget(station);
        }
        foreach (GameObject jumpgate in SectorNavigation.Jumpgates)
        {
            ObjectMarker marker = Instantiate(ObjectMarkerPrefab, this.transform).GetComponent<ObjectMarker>();
            _objectMarkerMap.Add(marker, jumpgate);
            marker.SetTarget(jumpgate);
        }
    }

    void FixedUpdate()
    {
        // Selected objects
        var selectedObjects = _unitSelection.SelectedObjects;
        ProcessTargetMarkers(selectedObjects, _selectedUnitMap, _selectedMarkerPool);

        // Non-selected objects
        List<GameObject> objectsInRange =
            SectorNavigation.Instance.GetClosestShipsAndCargo(Camera.main.transform, Ship.PlayerShip.ScannerRange, MaxNumberOfMarkers);
        objectsInRange.RemoveAll(new Predicate<GameObject>(IsSelected));    // Remove selected or player ship
        ProcessTargetMarkers(objectsInRange, _nonSelectedUnitMap, _nonSelectedMarkerPool);

        // Static objects
        ProcessStaticObjects();
    }

    private void ProcessTargetMarkers(List<GameObject> targetList, Dictionary<int, GameObject> markerObjectMap, ShipMarker[] markerPool)
    {
        for (int i = 0; i < targetList.Count; i++)
        {
            GameObject obj = targetList[i];
            // Check if obj is already attached to a marker
            bool alreadyUsed = false;
            foreach (var markerObj in markerObjectMap.Values)
                if (obj == markerObj)
                {
                    alreadyUsed = true;
                    break;
                }

            if (alreadyUsed)
                continue;

            bool shouldDisplayOffscreen = ShouldDisplayOffscreen(obj);
            bool isOnScreen = IsObjectOnScreen(obj.transform);
            if (shouldDisplayOffscreen || isOnScreen)
            {
                // Find first available HUD marker
                for (int j = 0; j < MaxNumberOfMarkers; j++)
                {
                    if (markerObjectMap[j] == null)
                    {
                        // Assign marker to onscreen object
                        markerObjectMap[j] = obj;

                        markerPool[j].SetTarget(obj);
                        markerPool[j].MarkerImage.rectTransform.localPosition = GetScreenPosOfObject(obj.transform);

                        break;
                    }
                }
            }
        }

        // Pass all markers, turn off unused ones
        for (int j = 0; j < MaxNumberOfMarkers; j++)
        {
            if (markerObjectMap[j] != null)
            {
                GameObject obj = markerObjectMap[j];
                if ((!ShouldDisplayOffscreen(obj) && !IsObjectOnScreen(obj.transform)) || !targetList.Contains(obj))
                {
                    // Turn off marker
                    markerPool[j].gameObject.SetActive(false);
                    markerObjectMap[j] = null;
                }
                else
                {
                    // Update marker position
                    markerPool[j].MarkerImage.rectTransform.localPosition = GetScreenPosOfObject(obj.transform);
                    // and color.
                    markerPool[j].MarkerImage.color = Player.Instance.PlayerFaction.GetTargetColor(obj);
                }
            }
            else
            {
                // Turn off marker
                markerPool[j].gameObject.SetActive(false);
            }
        }
    }

    private void ProcessStaticObjects()
    {
        foreach(var markerObjectPair in _objectMarkerMap)
        {
            // Update marker position
            markerObjectPair.Key.MarkerImage.rectTransform.localPosition = GetScreenPosOfObject(markerObjectPair.Value.transform);
            // and color.
            markerObjectPair.Key.MarkerImage.color = Player.Instance.PlayerFaction.GetTargetColor(markerObjectPair.Value);
        }
    }

    // Do not display offscreen nonselected ships
    private bool ShouldDisplayOffscreen(GameObject obj)
    {
        return !(obj.tag == "Ship" && !_unitSelection.SelectedObjects.Contains(obj));
    }

    private void OnShipDocked(object sender, EventArgs e)
    {
        GameObject ship = ((GameObject)sender);

        // If docked ship is not selected
        try { 
            KeyValuePair<int, GameObject> item = _nonSelectedUnitMap.First(kvp => kvp.Value == ship);    
            _nonSelectedUnitMap[item.Key] = null;
            _nonSelectedMarkerPool[item.Key].SetTarget(null);
        }
        catch(Exception ignored){}

        // If docked ship is selected
        try
        {
            KeyValuePair<int, GameObject> item = _selectedUnitMap.First(kvp => kvp.Value == ship);
            _selectedUnitMap[item.Key] = null;
            _unitSelection.SelectedObjects.Remove(ship);
            _selectedMarkerPool[item.Key].SetTarget(null);
        }
        catch (Exception ignored) {}
    }

    /// <summary>
    /// Invoked by the Canvas View Controller when the player switches between map and flight views (third
    /// person vs tactical map mode). All markers will switch their display mode accordingly.
    /// </summary>
    public void OnViewModeChanged(bool isMapViewActive)
    {
        ShipMarker.MarkerMode markerMode = isMapViewActive ? ShipMarker.MarkerMode.Map : ShipMarker.MarkerMode.Flight;

        foreach (ShipMarker marker in _selectedMarkerPool)
        {
            marker.SwitchMode(markerMode);
        }
        foreach (ShipMarker marker in _nonSelectedMarkerPool)
        {
            marker.SwitchMode(markerMode);
        }
        _intentLineManager.SwitchMode(markerMode);
    }

    #region utils
    private bool IsSelected(GameObject ship)
    {
        return _unitSelection.SelectedObjects.Contains(ship);
    }

    private bool IsObjectOnScreen(Transform obj)
    {
        float x = Camera.main.WorldToScreenPoint(obj.position).x;
        float y = Camera.main.WorldToScreenPoint(obj.position).y;
        float z = Camera.main.WorldToScreenPoint(obj.position).z;

        // Check if Target is off-screen            
        if (x < 0 || x > Screen.width || y < 0 || y > Screen.height)
        {
            return false;
        }
        else if (z > 0) // Target is in front of the camera
        {
            return true;
        }
        else // Target is behind the camera
        {
            return false;
        }

    }

    private Vector3 GetScreenPosOfObject(Transform target)
    {
        float x = Camera.main.WorldToScreenPoint(target.position).x - _hScreenWidth;
        float y = Camera.main.WorldToScreenPoint(target.position).y - _hScreenHeight;
        float z = Camera.main.WorldToScreenPoint(target.position).z;

        if (z > 0)
        {
            return new Vector3(
                        Mathf.Clamp(x, -_hScreenWidth, _hScreenWidth),
                        Mathf.Clamp(y, -_hScreenHeight, _hScreenHeight),
                        0f);
        }
        else
        {
            if (x > y)
            {
                return new Vector3(
                        x < 0 ? _hScreenWidth : -_hScreenWidth,
                        Mathf.Clamp(y, _hScreenHeight, -_hScreenHeight),
                        0f);
            }
            else
            {
                return new Vector3(
                        Mathf.Clamp(x, _hScreenWidth, -_hScreenWidth),
                        y < 0 ? _hScreenHeight : -_hScreenHeight,
                        0f);
            }
            
        }
        
    }
    #endregion utils
}
}