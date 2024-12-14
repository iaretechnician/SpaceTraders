using UnityEngine;
using Random = UnityEngine.Random;

namespace SpaceSimFramework
{
/// <summary>
/// Attach to jumpgates. Spawns ships periodically.
/// </summary>
public class ShipSpawner : MonoBehaviour {
    // Prevent spawning too many ships
    public int ShipNumberLimit = 30;

    [Header("Spawn properties")]
    public float SpawnTimeSpacing = 10f;
    public float ProbabilitySquadron = 0.4f;

    private float spawnTimer;
    private Transform spawnPos;
    private Vector3[] escortOffsets = { new Vector3(30, 0, 0), new Vector3(30, 30, 0), new Vector3(0, 30, 0)};
    private GameObject[] shipPrefabs;

    void Awake()
    {
        spawnTimer = SpawnTimeSpacing + Random.value * 10;

        spawnPos = GetComponent<Jumpgate>().SpawnPos;

        shipPrefabs = ObjectFactory.Instance.Ships;
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer < 0)
        {
            spawnTimer = SpawnTimeSpacing + Random.value * 10;
            if (SectorNavigation.Ships.Count > ShipNumberLimit)
                return;
            SpawnRandomShip();
        }
    }

    private void SpawnRandomShip()
    {
        Ship ship;

        string debugText = "";

        // Spawn random ship
        ship = GameObject.Instantiate(
                     shipPrefabs[Random.Range(0, shipPrefabs.Length)],
                     spawnPos.position,
                     spawnPos.rotation).GetComponent<Ship>();

        ship.IsPlayerControlled = false;
        debugText += "Spawning " + ship.name;

        // Generate random faction
        Faction shipFaction = GetShipFaction();
        ship.faction = shipFaction;
        ship.gameObject.name = shipFaction.name + " " + ship.ShipModelInfo.ModelName;
        // Give a random loadout
        ShipLoadout.ApplyLoadoutToShip(shipFaction.GetRandomLoadout(ship.ShipModelInfo.ModelName), ship);

        debugText += ", faction is " + shipFaction.name;

        // Assign order to ship
        debugText = AIShipController.IssueOrder(ship.gameObject, debugText, spawnPos.gameObject);

        // Generate escort ships, if needed
        if (Random.value < ProbabilitySquadron)
        {
            SpawnEscortsFor(ship);
        }

        //Debug.Log("Adding ship " + gameObject.name);
        //SectorNavigation.Ships.Add(ship.gameObject);
    }

    /// <summary>
    /// Gets the ship's faction taking into account sector ownership.
    /// If sector is under complete ownership, 50% chance of spawning owner's ship.
    /// If sector is under no control, 100% random selection
    /// </summary>
    private static Faction GetShipFaction()
    {
        Faction[] factions = ObjectFactory.Instance.Factions;
        //float influence = Universe.Sectors[SectorNavigation.CurrentSector].Influence;

        if (Random.value < /*1 - influence **/ 0.5f) { 
            // Completely random
            return factions[Random.Range(0, factions.Length - 1)];  // Do not spawn player ships
        }
        else {
            string ownerFaction = Universe.Sectors[SectorNavigation.CurrentSector].OwnerFaction;
            return ObjectFactory.Instance.GetFactionFromName(ownerFaction); // Bias towards owner faction
        }
    }

    private void SpawnEscortsFor(Ship escortLeader)
    {
        Ship escort = null; 
        int numEscorts = Random.Range(1, 3);

        for (int e_i = 0; e_i < numEscorts; e_i++)
        {
            // Spawn random ship
            escort = GameObject.Instantiate(
                         shipPrefabs[Random.Range(0, shipPrefabs.Length)],
                         spawnPos.position,
                         spawnPos.rotation).GetComponent<Ship>();
            escort.IsPlayerControlled = false;
            // Generate random faction
            escort.faction = escortLeader.faction;
            // Give a random loadout
            ShipLoadout.ApplyLoadoutToShip(escortLeader.faction.GetRandomLoadout(escort.ShipModelInfo.ModelName), escort);

            escort.gameObject.name = escort.faction.name + " Escort " + escort.ShipModelInfo.ModelName;
            // Assign order to ship
            escort.gameObject.GetComponent<ShipAI>().Follow(escortLeader.transform);
            escort.gameObject.transform.position += escortOffsets[e_i];
        }
        
    }

    /// <summary>
    /// Spawns a given number of ships which belong to the requested faction. One of the ships is
    /// returned so that it can be saved as the mission target.
    /// </summary>
    /// <param name="numOfShips">Number of ships to spawn</param>
    /// <returns>Target ship (squadron leader)</returns>
    public GameObject SpawnMissionTarget(Faction faction)
    {
        Ship ship;

        ship = GameObject.Instantiate(
                   shipPrefabs[Random.Range(0, shipPrefabs.Length)],
                   spawnPos.position,
                   spawnPos.rotation).GetComponent<Ship>();
        ship.IsPlayerControlled = false;

        // Generate random faction
        Faction[] factions = ObjectFactory.Instance.Factions;        
        ship.faction = faction;
        ship.gameObject.name = faction.name + " " + ship.ShipModelInfo.ModelName + "*";


        // Assign order to ship
        ship.gameObject.GetComponent<ShipAI>().AttackAll();

        // Generate escort ships
        SpawnEscortsFor(ship);

        return ship.gameObject;
    }

}
}