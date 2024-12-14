using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceSimFramework
{
public class Station : MonoBehaviour {


    private const int DOCK_DOOR_OPEN_DISTANCE = 150;
    private const float UNDOCKING_DURATION = 5f;
    private Vector3 STATION_CAMERA_OFFSET = new Vector3(0, 50, -200);

    /// <summary>
    /// Used to notify Map Markers that a ship's marker should be removed
    /// </summary>
    public static event EventHandler ShipDockedEvent;

    // Unique in-game object ID 
    public string ID;
    [Tooltip("Dock for small ships")]
    public StationDock Dock;
    [Tooltip("Mooring points for large ships")]
    public StationMooring[] Moorings;

    public List<GameObject> DockedShips;

    [Tooltip("Station owner faction")]
    public Faction faction;
    [Tooltip("Animator of the dock doors")]
    public Animator DockAnimator;

    [Tooltip("Station type data holder")]
    public StationLoadout Loadout;

    [Header("Station Facilities")]
    public bool HasCargoDealer = true;
    public bool HasShipDealer = true;

    private GameObject _shipUnmooring;

	void Awake () {
        DockedShips = new List<GameObject>();
        // Mooring points and their docked ships
        CloseDockDoors();
    }

    private void Start()
    {
        if(Loadout != null)
            StationLoadout.ApplyLoadoutToStation(Loadout, this);
    }

    private void Update()
    {
        if(Dock.ShipDocking != null)
        {
            // Track distance to ship, open dock if near, close if far
            if(Vector3.Distance(Dock.ShipDocking.transform.position, Dock.transform.position) < DOCK_DOOR_OPEN_DISTANCE)
            {
                if(!DockAnimator.GetBool("DockOpen"))
                    OpenDockDoors();
            }
            else
            {
                if (DockAnimator.GetBool("DockOpen"))
                    CloseDockDoors();
            }
        }

        // Keep dock open until ship leaves vicinity
        if(_shipUnmooring != null)
        {
            if (Vector3.Distance(_shipUnmooring.transform.position, Dock.transform.position) > DOCK_DOOR_OPEN_DISTANCE)
            {
                CloseDockDoors();
                _shipUnmooring = null;
            }
        }
    }


    /// <summary>
    /// Invoked when a smaller ship has entered the dock and docked.
    /// </summary>
    /// <param name="other">Ship which docked</param>
    public void OnDockContact(Collider other)
    {
        GameObject ship = other.gameObject;
        Ship shipComponent = ship.GetComponent<Ship>();
        if (DockedShips.Contains(ship)) // OnTriggerEnter will be called multiple times
            return;
        if (shipComponent.ShipModelInfo.ExternalDocking)
            return; // refuse docking for large vessels

        ShipDockedEvent?.Invoke(ship, EventArgs.Empty);
        CloseDockDoors();
        DockedShips.Add(ship);
        ship.SetActive(false);
        shipComponent.StationDocked = ID;
        if(ship == Ship.PlayerShip.gameObject)
        {
            DockShip(ship);
        }
        else if(shipComponent.faction != Ship.PlayerShip.faction)
        {
            StartCoroutine(UndockAIShip(ship));
        }

        if (shipComponent.AIInput.CurrentOrder != null && shipComponent.AIInput.CurrentOrder.Name == "Trade")
        {
            ((OrderTrade)shipComponent.AIInput.CurrentOrder).PerformTransaction(shipComponent.AIInput);
            StartCoroutine(UndockAIShip(ship));
        }
    }  

    /// <summary>
    /// Invoked when a capital ship has connected to the mooring point.
    /// </summary>
    public void OnMooringContact(StationMooring mooring, GameObject ship)
    {
        if (DockedShips.Contains(ship)) // OnTriggerEnter will be called multiple times
            return;

        Ship shipComponent = ship.GetComponent<Ship>();
        if (!shipComponent.ShipModelInfo.ExternalDocking)
            return; // refuse mooring for small vessels

        ship.transform.position = mooring.transform.position;
        ship.transform.rotation = mooring.transform.rotation;
        shipComponent.StationDocked = ID;
        shipComponent.MovementInput.throttle = 0;
        shipComponent.MovementInput.strafe = 0;
        shipComponent.Physics.enabled = false;
        ship.GetComponent<Rigidbody>().isKinematic = true;

        DockedShips.Add(ship);

        ShipDockedEvent?.Invoke(ship, EventArgs.Empty);

        if (ship == Ship.PlayerShip.gameObject)
        {
            DockShip(ship);
        }
        else if (shipComponent.faction != Ship.PlayerShip.faction)
        {
            StartCoroutine(UndockAIShip(ship));
        }

        if (shipComponent.AIInput.CurrentOrder != null && shipComponent.AIInput.CurrentOrder.Name == "Trade")
        {
            ((OrderTrade)shipComponent.AIInput.CurrentOrder).PerformTransaction(shipComponent.AIInput);
            StartCoroutine(UndockAIShip(ship));
        }
    }

    private void DockShip(GameObject ship)
    {
        CanvasController.Instance.CloseAllMenus();
        CanvasController.Instance.CloseMenu();  // Close ingame menu as well, if open
        var cam = Camera.main.GetComponent<CameraController>();
        cam.State = CameraController.CameraState.Chase;
        cam.SetTargetStation(this.transform, STATION_CAMERA_OFFSET);
        CanvasViewController.Instance.SetHUDActive(false);
        if (CanvasViewController.IsMapActive)
        {
            Camera.main.GetComponent<MapCameraController>().CanMove = false;
            CanvasViewController.Instance.TacticalCanvas.gameObject.SetActive(false);
        }
        Ship.PlayerShip.UsingMouseInput = true;
        InputHandler.Instance.gameObject.SetActive(false);

        OpenStationMenu(ship);
    }

    public StationMooring GetFreeMooringPoint()
    {
        if (Moorings == null)
            Moorings = GetComponentsInChildren<StationMooring>();

        foreach(StationMooring mooring in Moorings)
        {
            if (mooring.Ship == null)    // Mooring point is free
                return mooring;
        }

        return null;
    }

    private void OpenStationMenu(GameObject ship)
    {
        StationMainMenu stationMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.StationMainMenu)
           .GetComponent<StationMainMenu>();

        stationMenu.PopulateMenuOptions(ship, this);     
    }

    private IEnumerator UndockAIShip(GameObject ship)
    {
        yield return new WaitForSeconds(3);
        UndockShip(ship);
    }

    /// <summary>
    /// Requested undocking, handle ship undocking
    /// </summary>
    /// <param name="ship"></param>
    public void UndockShip(GameObject ship)
    {
        Ship shipComp = ship.GetComponent<Ship>();
        DockedShips.Remove(ship);
        shipComp.StationDocked = "none";

        if (shipComp.ShipModelInfo.ExternalDocking)
        {
            shipComp.Physics.enabled = true;
            ship.GetComponent<Rigidbody>().isKinematic = false;

            StationMooring mooring = Moorings.FirstOrDefault(pair => pair.Ship == ship);   // This is the mooring point for the docked ship
            if(mooring != null)
                mooring.Ship = null;
        }
        else
        {
            ship.SetActive(true);
            ship.transform.rotation = Dock.transform.rotation;
            ship.transform.position = Dock.transform.position;
            OpenDockDoors();
            _shipUnmooring = ship;
        }

        ship.GetComponent<Rigidbody>().velocity = Vector3.zero;

        if (Ship.PlayerShip.gameObject == ship)
        {
            CanvasController.Instance.CloseAllStationMenus();
            InputHandler.Instance.SelectedObject = null;
            InputHandler.Instance.gameObject.SetActive(true);
            Camera.main.GetComponent<CameraController>().SetTargetPlayerShip();
            CanvasViewController.Instance.SetHUDActive(!CanvasViewController.IsMapActive);
            if (CanvasViewController.IsMapActive)
            {
                CanvasViewController.Instance.TacticalCanvas.gameObject.SetActive(true);
                InputHandler.Instance.SelectedObject = null;
            }
            EquipmentIconUI.Instance.SetIconsForShip(shipComp);

            if (shipComp.AIInput.CurrentOrder == null || shipComp.AIInput.CurrentOrder.Name != "Trade") { 
                Ship.PlayerShip.IsPlayerControlled = true;
                Ship.IsShipInputDisabled = false;
            }
        }

        StartCoroutine(FlyShipAwayFromDock(shipComp));
    }

    /// <summary>
    /// Takes over ship control while undocking to ensure safe distance from dock.
    /// Wait 3 second for dock doors to open, then 2 seconds of full throttle.
    /// </summary>
    private IEnumerator FlyShipAwayFromDock(Ship shipComp)
    {
        bool wasPlayerControlled = shipComp.IsPlayerControlled;
        shipComp.IsPlayerControlled = false;

        shipComp.AIInput.IsUndocking = true;
        float timer = UNDOCKING_DURATION;

        while (timer > 0) {
            shipComp.AIInput.angularTorque = Vector3.zero;
            if (timer < 2)
            {
                shipComp.AIInput.throttle = 1.0f;
            }
            timer -= Time.deltaTime;
            yield return null;
        }

        shipComp.AIInput.IsUndocking = false;
        shipComp.IsPlayerControlled = wasPlayerControlled;
    }

    /// <summary>
    /// Allows a requestee to dock. If docking a fighter, waits for OnDockContact.
    /// If docking a capship, waits for OnMooringContact
    /// </summary>
    /// <param name="ship"></param>
    /// <returns>Docking pattern if docking is granted; null otherwise</returns>
    public GameObject[] RequestDocking(GameObject ship) 
    {
        Ship shipComponent = ship.GetComponent<Ship>();
        // Check reputation
        if (shipComponent.faction.RelationWith(faction) < 0)
        {
            ConsoleOutput.PostMessage("[" + name + " Approach Control]: Docking denied, please leave the area.", Color.red);
            throw new DockingForbiddenException(this);
        }

        if (shipComponent.ShipModelInfo.ExternalDocking)
        {
            StationMooring mooring = GetFreeMooringPoint();
            if (mooring == null)
            {
                ConsoleOutput.PostMessage("[" + name + " Approach Control]: Docking denied - All mooring points occupied!", Color.red);
                throw new MooringUnavailableException(this); // Moorings full, cannot dock capital ship
            }
            else
            {
                ConsoleOutput.PostMessage("[" + name + " Approach Control]: Docking granted, proceed to mooring point.", Color.green);
                mooring.Ship = ship;
                return mooring.Waypoints;
            }
        }
        else
        {
            Dock.DockingQueue.Enqueue(ship);
            ConsoleOutput.PostMessage("[" + name + " Approach Control]: Docking granted, proceed to docking bay.", Color.green);
            return Dock.DockWaypoints;
        }
    }

    /// <summary>
    /// Forces docking of a ship to a station. Used for loading docked ships.
    /// </summary>
    /// <param name="ship">Ship that will be docked to this station</param>
    public void ForceDockShip(Ship ship)
    {
        if (ship == Ship.PlayerShip)
            // Make sure ingame menu is closed
            CanvasController.Instance.IngameMenu.SetActive(false);

        // Find free docking slot
        if (ship.ShipModelInfo.ExternalDocking)
        {
            StationMooring mooring = GetFreeMooringPoint();
            if(mooring != null)
            {
                mooring.Ship = ship.gameObject;
                GameObject[] waypoints = mooring.Waypoints;
                ship.transform.position = waypoints[waypoints.Length - 1].transform.position;
            }
            // OnMooringContact happens now
        }
        else
        {
            Dock.DockingQueue.Clear();
            Dock.DockingQueue.Enqueue(ship.gameObject);
            ship.transform.position = Dock.DockWaypoints[Dock.DockWaypoints.Length - 1].transform.position;
            // OnDockContact happens now
        }
    }

    /// <summary>
    /// Force dock doors opening.
    /// </summary>
    private void OpenDockDoors()
    {
        DockAnimator.SetBool("DockOpen", true);
    }

    public void CloseDockDoors()
    {
        DockAnimator.SetBool("DockOpen", false);
    }

}
}