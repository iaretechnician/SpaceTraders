using UnityEngine;

namespace SpaceSimFramework
{
    public class TurretCommands  {

        public enum TurretOrder
        {
            None,           // Turrets disabled
            AttackEnemies,  // Turrets attack tomatoes
            AttackTarget,   // Turrets attack a selected target (even if friendly)
            Manual          // Turrets are under player control, fired via mouse 
        }

    }
}