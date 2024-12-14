using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace SpaceSimFramework
{
public class AIShipController : MonoBehaviour {

    [Tooltip("How often will the AI Controller check for ships which have completed their orders?")]
    public float CheckInterval = 3f;

    private float _timer = 0;

    void Update()
    {
        _timer += Time.deltaTime;

        if(_timer > CheckInterval)
        {
            _timer = 0;

            GameObject[] aiShips = SectorNavigation.Ships.ToArray();
            Ship currentShip;

            // Check each ship to see if one has finished its order
            foreach (GameObject ship in aiShips)
            {
                currentShip = ship.GetComponent<Ship>();
                if (currentShip.faction == Player.Instance.PlayerFaction)
                    continue;

                if (currentShip.AIInput.CurrentOrder == null)     // give new order
                    IssueOrder(ship, "", null);
            }
        }
    }
 
    /// <summary>
    /// Issue an order to a ship. The spawn location (jumpgate or base) of the ship is given
    /// so that the given destination is not the same as the spawn location.
    /// </summary>
    /// <param name="ship">Ship to be given an order</param>
    /// <param name="debugText">Internal debug text for logging</param>
    /// <param name="spawnPosition">Jumpgate if ship has spawned at a gate, station if ship has undocked</param>
    /// <returns></returns>
    public static string IssueOrder(GameObject ship, string debugText, GameObject spawnPosition)
    {
        ShipAI shipAI = ship.gameObject.GetComponent<ShipAI>();
        switch (Random.Range(0, 5))
        {
            case 0: // Attack all enemies
                shipAI.AttackAll();
                debugText += ", order: AttackAll";
                break;
            case 1: // Dock
                List<GameObject> dockables = SectorNavigation.Instance.GetDockableObjects();
                if(spawnPosition)
                    dockables.Remove(spawnPosition);  // Dont head back to where you started
                for (int i = 0; i < dockables.Count; i++)
                {
                    // Remove station if it is of an enemy faction
                    if (dockables[i].tag == "Station")
                        if (ship.GetComponent<Ship>().faction.RelationWith(dockables[i].GetComponent<Station>().faction) < 0)
                        {
                            dockables.RemoveAt(i);
                            i--;
                        }
                }

                GameObject dockTarget;
                if (dockables.Count > 0)
                {
                    dockTarget = dockables[Random.Range(0, dockables.Count)];
                    shipAI.DockAt(dockTarget);
                    debugText += ", order: DockAt " + dockTarget.name;
                }
                else
                {
                    shipAI.Idle();
                    debugText += ", order: Idle";
                }

                break;
            case 2: // Patrol
                shipAI.PatrolPath(SectorNavigation.Instance.GetPatrolWaypoints());
                debugText += ", order: Patrol";
                break;
            default: // Idle
                debugText += ", order: Idle";
                shipAI.Idle();
                break;
        }

        return debugText;
    }

    /// <summary>
    /// Gets a random target for docking.
    /// </summary>
    /// <returns></returns>
    private static GameObject GetRandomDockableObject()
    {
        List<GameObject> dockables = SectorNavigation.Instance.GetDockableObjects();

        return dockables[Random.Range(0, dockables.Count)];
    }

}
}