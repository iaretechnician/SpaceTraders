using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Keeps references to all interactable game objects in the current sector (scene).
/// Is used to obtain ships, stations, jumpgates, cargo crates, etc.
/// </summary>
public class SectorNavigation : Singleton<SectorNavigation> {

    public static Vector2 UNSET_SECTOR = new Vector2(9999, 9999);
    #region static members
    public static Vector2 CurrentSector {
        get { return _currentSector; }
    }
    private static Vector2 _currentSector = Vector2.zero;

    public static Vector2 PreviousSector
    {
        get { return _previousSector; }
    }
    private static Vector2 _previousSector = UNSET_SECTOR;

    public static int SectorSize;
    public static List<GameObject> Ships;
    public static List<GameObject> Stations;
    public static List<GameObject> Cargo;
    public static List<GameObject> Jumpgates;
    public static List<GameObject> Fields;

    private static Dictionary<string, GameObject> stationIDs;
    private static Dictionary<string, GameObject> fieldIDs;
    private static Dictionary<string, GameObject> jumpgateIDs;

    /// <summary>
    /// Sets the current sector when jumping or loading game.
    /// </summary>
    /// <param name="newSector">Sector to be set</param>
    /// <param name="markPreviousSector">Set true if jumping and false if loading</param>
    public static void ChangeSector(Vector2 newSector, bool markPreviousSector)
    {
        if(markPreviousSector)
            _previousSector = _currentSector;
        _currentSector = newSector;
        Ships = new List<GameObject>();
        Stations = new List<GameObject>();
        Cargo = new List<GameObject>();
        Jumpgates = new List<GameObject>();
        Fields = new List<GameObject>();

        Instance.Awake();
    }

    private static void GetExistingObjects()
    {
        Ships = new List<GameObject>();
        Stations = new List<GameObject>();
        Cargo = new List<GameObject>();
        Jumpgates = new List<GameObject>();
        Fields = new List<GameObject>();

        stationIDs = new Dictionary<string, GameObject>();
        fieldIDs = new Dictionary<string, GameObject>();
        jumpgateIDs = new Dictionary<string, GameObject>();

        // Find all pre-existing sector entities
        Ships.AddRange(GameObject.FindGameObjectsWithTag("Ship"));
        Stations.AddRange(GameObject.FindGameObjectsWithTag("Station"));
        Jumpgates.AddRange(GameObject.FindGameObjectsWithTag("Jumpgate"));
        Cargo.AddRange(GameObject.FindGameObjectsWithTag("Cargo"));
        Fields.AddRange(GameObject.FindGameObjectsWithTag("AsteroidField"));

        foreach (GameObject station in Stations)
        {
            stationIDs.Add(station.GetComponent<Station>().ID, station);
        }
        foreach (GameObject jumpgate in Jumpgates)
        {
            jumpgateIDs.Add(jumpgate.GetComponent<Jumpgate>().ID, jumpgate);
        }
        foreach (GameObject field in Fields)
        {
            fieldIDs.Add(field.GetComponent<AsteroidField>().ID, field);
        }
    }

    /*
     * Getters for objects in sector, by ID. Return null if object not found in sector, gameObject otherwise.
     */
    public static GameObject GetStationByID(string id)
    {
        if (id == null || id == "" || id=="none")
            return null;

        return stationIDs.ContainsKey(id) ? stationIDs[id] : null;
    }

    public static GameObject GetJumpgateByID(string id)
    {
        return jumpgateIDs.ContainsKey(id) ? jumpgateIDs[id] : null;
    }

    public static GameObject GetFieldByID(string id)
    {
        return fieldIDs.ContainsKey(id) ? fieldIDs[id] : null;
    }

    #endregion static members

    [Tooltip("How often do player ships check their surroundings to discover new sector objects?")]
    public float ObjectDiscoveryInterval = 3f;
    private float discoveryTimer;
    private List<string> knownIds;
    private SerializableSectorData sectorKnowledge;

    public void Awake()
    {
        GetExistingObjects();
        SceneManager.sceneLoaded += OnSceneLoaded;
        discoveryTimer = ObjectDiscoveryInterval;

        if (UniverseMap.Knowledge == null)
        {
            LoadGame.LoadPlayerKnowledge();
        }

        if (UniverseMap.Knowledge.ContainsKey(CurrentSector))
        {
            sectorKnowledge = UniverseMap.Knowledge[CurrentSector];
        }
        else
        {
            sectorKnowledge = new SerializableSectorData();

            if (sectorKnowledge.Jumpgates == null)
                sectorKnowledge.Jumpgates = new List<SerializableGateData>();
            if (sectorKnowledge.Stations == null)
                sectorKnowledge.Stations = new List<SerializableStationData>();
            if (sectorKnowledge.Fields == null)
                sectorKnowledge.Fields = new List<SerializableFieldData>();

            UniverseMap.Knowledge.Add(CurrentSector, sectorKnowledge);
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GetExistingObjects();
        StationContentGenerator.OnSectorChanged(_currentSector, Stations);
    }

    void Update()
    {
        // Check for new detected objects each few seconds
        discoveryTimer -= Time.deltaTime;

        if(discoveryTimer < 0)
        {
            discoveryTimer = ObjectDiscoveryInterval;

            if (knownIds == null)
                GetKnownObjectIds(sectorKnowledge);


            foreach (var ship in Player.Instance.Ships)
            {
                // Ignore docked ships
                if (!ship.activeInHierarchy)
                    continue;

                Ship shipScript = ship.GetComponent<Ship>();
                // Get all stations/jumpgates(/ships/cargo) in scanner range of this ship
                var objectsInRange = GetClosestObjects(ship.transform, shipScript.ScannerRange, Int32.MaxValue);

                foreach(GameObject objFound in objectsInRange)
                {
                    Station station = objFound.GetComponent<Station>();
                    if (station != null && !knownIds.Contains(station.ID))
                    {
                        SerializableStationData stationDescriptor = new SerializableStationData();
                        stationDescriptor.ID = station.ID;
                        stationDescriptor.LoadoutName = station.Loadout.name;
                        stationDescriptor.Position = station.transform.position;
                        stationDescriptor.Rotation = station.transform.rotation.eulerAngles;
                        // Add station
                        sectorKnowledge.Stations.Add(stationDescriptor);
                        // Add ID
                        knownIds.Add(station.ID);

                        continue;
                    }

                    Jumpgate gate = objFound.GetComponent<Jumpgate>();
                    if (gate != null && !knownIds.Contains(gate.ID))
                    {
                        SerializableGateData gateDescriptor = new SerializableGateData();
                        gateDescriptor.ID = gate.ID;
                        gateDescriptor.Sector = gate.NextSector;
                        gateDescriptor.Position = gate.transform.position;
                        gateDescriptor.Rotation = gate.transform.rotation.eulerAngles;
                        // Add gate
                        sectorKnowledge.Jumpgates.Add(gateDescriptor);
                        // Add ID
                        knownIds.Add(gate.ID);
                    }
                }

                foreach (var field in GameObject.FindGameObjectsWithTag("AsteroidField"))
                {
                    float dist = Vector3.Distance(field.transform.position, ship.transform.position);
                    AsteroidField fieldProps = field.GetComponent<AsteroidField>();
                    if (dist < shipScript.ScannerRange && !knownIds.Contains(fieldProps.ID))
                    {
                        SerializableFieldData FieldDescriptor = new SerializableFieldData();
                        FieldDescriptor.ID = fieldProps.ID;
                        FieldDescriptor.Position = fieldProps.transform.position;
                        FieldDescriptor.Rotation = fieldProps.transform.rotation.eulerAngles;
                        // Add Field
                        sectorKnowledge.Fields.Add(FieldDescriptor);
                        // Add ID
                        knownIds.Add(FieldDescriptor.ID);
                    }
                }
            }
        }
    }

    private void GetKnownObjectIds(SerializableSectorData sectorKnowledge)
    {
        knownIds = new List<string>();

        if(sectorKnowledge.Stations != null)
        {
            foreach (var station in sectorKnowledge.Stations)
                knownIds.Add(station.ID);
        }

        if (sectorKnowledge.Fields != null)
        {
            foreach (var field in sectorKnowledge.Fields)
                knownIds.Add(field.ID);
        }
           
        if(sectorKnowledge.Jumpgates != null) { 
            foreach (var gate in sectorKnowledge.Jumpgates)
                knownIds.Add(gate.ID);
        }
    }


    /// <summary>
    /// Returns a required number of selectable objects (ships, stations, loot, etc.)
    /// within a desired range of a given object.
    /// </summary>
    /// <param name="shipPosition">Source of the scanner</param>
    /// <param name="scannerRange">Range of the scanner</param>
    /// <param name="num">Maximum number of required targets</param>
    /// <returns></returns>
    public List<GameObject> GetClosestObjects(Transform shipPosition, float scannerRange, int num)
    {
        List<GameObject> objectsInRange = new List<GameObject>();

       /* GameObject[] CHECK = GameObject.FindGameObjectsWithTag("Cargo");
        if (CHECK.Length != Cargo.Count)
            Debug.LogError("Cargo Array doesn't contain all of the scene loot items!");*/

        foreach(GameObject cargo in Cargo)
        {
            if (Vector3.Distance(shipPosition.position, cargo.transform.position) < scannerRange)
            {
                objectsInRange.Add(cargo);
                num--;

                if (num <= 0)
                    return objectsInRange;
            }
        }       

        objectsInRange.AddRange(GetShipsInRange(shipPosition, scannerRange, num));
  

        /*CHECK = GameObject.FindGameObjectsWithTag("Station");
        if (CHECK.Length != Stations.Count)
            Debug.LogError("Station Array doesn't contain all of the scene stations!");*/

        foreach (GameObject obj in Stations)
        {
            if (Vector3.Distance(shipPosition.position, obj.transform.position) < scannerRange)
            {
                objectsInRange.Add(obj);
                num--;

                if (num <= 0)
                    return objectsInRange;
            }
        }

        foreach (GameObject obj in Jumpgates)
        {
            if (Vector3.Distance(shipPosition.position, obj.transform.position) < scannerRange)
            {
                objectsInRange.Add(obj);
                num--;

                if (num <= 0)
                    return objectsInRange;
            }
        }

        return objectsInRange;
    }

    /// <summary>
    /// Returns a required number of dynamic selectabe objects (ships and loot)
    /// within a desired range of a given object.
    /// </summary>
    /// <param name="shipPosition">Source of the scanner</param>
    /// <param name="scannerRange">Range of the scanner</param>
    /// <param name="num">Maximum number of required targets</param>
    /// <returns></returns>
    public List<GameObject> GetClosestShipsAndCargo(Transform shipPosition, float scannerRange, int num)
    {
        List<GameObject> objectsInRange = new List<GameObject>();
        foreach (GameObject cargo in Cargo)
        {
            if (Vector3.Distance(shipPosition.position, cargo.transform.position) < scannerRange)
            {
                objectsInRange.Add(cargo);
                num--;

                if (num <= 0)
                    return objectsInRange;
            }
        }

        objectsInRange.AddRange(GetShipsInRange(shipPosition, scannerRange, num));
        return objectsInRange;
    }


    /// <summary>
    /// Returns a required number of ships
    /// within a desired range of a given object.
    /// </summary>
    /// <param name="shipPosition">Source of the scanner</param>
    /// <param name="scannerRange">Range of the scanner</param>
    /// <param name="num">Maximum number of required targets</param>
    /// <returns></returns>
    public List<GameObject> GetShipsInRange(Transform shipPosition, float scannerRange, int num)
    {
        List<GameObject> objectsInRange = new List<GameObject>();

        /*var CHECK = GameObject.FindGameObjectsWithTag("Ship");
        if (CHECK.Length != Ships.Count)
            Debug.LogError("Ship Array doesn't contain all of the scene ships!");*/

        foreach (GameObject ship in Ships)
        {
            if (Vector3.Distance(shipPosition.position, ship.transform.position) < scannerRange)
            {
                if(ship != shipPosition.gameObject && ship.activeInHierarchy)
                {
                    objectsInRange.Add(ship);
                    num--;
                }

                if (num <= 0)
                    return objectsInRange;
            }

        }

        return objectsInRange;
    }

    /// <summary>
    /// Returns a the closes found ship which is of a hostile faction.
    /// </summary>
    /// <param name="shipPosition">Source of the scanner</param>
    /// <param name="scannerRange">Range of the scanner</param>
    /// <returns></returns>
    public List<GameObject> GetClosestEnemyShip(Transform shipPosition, float scannerRange)
    {
        Dictionary<GameObject, float> shipDistances = new Dictionary<GameObject, float>();
        Faction myfaction = shipPosition.gameObject.GetComponent<Ship>().faction;
        float distance;

        /*var CHECK = GameObject.FindGameObjectsWithTag("Ship");
        if (CHECK.Length != Ships.Count)
            Debug.LogError("Ship Array doesn't contain all of the scene ships!");*/

        foreach (GameObject ship in Ships)
        {
            distance = Vector3.Distance(shipPosition.position, ship.transform.position);
            if (distance < scannerRange)
            {
                if (ship == shipPosition.gameObject)
                    continue;

                Faction shipFaction = ship.GetComponent<Ship>().faction;

                if(myfaction.RelationWith(shipFaction) < 0)
                    shipDistances.Add(ship, distance);
            }

        }

        // Sort by distance to get closest targets
        List<KeyValuePair<GameObject, float>> shipList = shipDistances.ToList();

        shipList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

        List<GameObject> closestContacts = new List<GameObject>();

        foreach(KeyValuePair<GameObject, float> pair in shipList){
            closestContacts.Add(pair.Key);
        }

        return closestContacts;
    }

    public Transform[] GetPatrolWaypoints()
    {
        List<Transform> waypoints = new List<Transform>();

        foreach (GameObject child in GameObject.FindGameObjectsWithTag("Waypoint"))
        {
             waypoints.Add(child.transform);
        }

        if(waypoints.Count == 0)    // Create some waypoints based on interesting points of the sector
        {
            foreach(var station in Stations)
            {
                Transform wp = GameObject.Instantiate(ObjectFactory.Instance.WaypointPrefab).transform;
                wp.position = station.transform.position + Vector3.up * 200;
                waypoints.Add(wp);
            }
            foreach (var gate in Jumpgates)
            {
                Transform wp = GameObject.Instantiate(ObjectFactory.Instance.WaypointPrefab).transform;
                wp.position = gate.transform.position - Vector3.up * 200;
                waypoints.Add(wp);
            }
            return waypoints.ToArray();
        }

        return waypoints.ToArray();
    }

    public GameObject[] GetJumpgates()
    {
        // Why is this happening? TODO
        if(Jumpgates == null || Jumpgates.Count == 0) {
            Jumpgates = new List<GameObject>();
            Jumpgates.AddRange(GameObject.FindGameObjectsWithTag("Jumpgate"));
        }

        return Jumpgates.ToArray();
    }

    public List<GameObject> GetDockableObjects()
    {
        List<GameObject> dockables = new List<GameObject>();

        /*var CHECK = GameObject.FindGameObjectsWithTag("Ship");
        if (CHECK.Length != Ships.Count)
            Debug.LogError("Ship Array doesn't contain all of the scene ships!");*/
        dockables.AddRange(Stations);

        /*CHECK = GameObject.FindGameObjectsWithTag("Ship");
        if (CHECK.Length != Ships.Count)
            Debug.LogError("Ship Array doesn't contain all of the scene ships!");*/
        dockables.AddRange(Jumpgates);

        return dockables;
    }

}
}