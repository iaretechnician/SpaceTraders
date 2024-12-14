using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceSimFramework
{
public class Jumpgate : MonoBehaviour {
    // Unique in-game object ID 
    [Tooltip("DO NOT touch this if you don't have to")]
    public string ID;
    public Vector2 NextSector;
    public GameObject[] DockWaypoints;
    public Transform SpawnPos;
    public AnimationCurve cameraFovCurve;

    private List<JumpSequence> jumps;

    private class JumpSequence
    {
        private float accelTime = 1f;
        public float Timer;
        public GameObject Ship;
        public float Speed = 10f;

        public JumpSequence(GameObject ship)
        {
            Timer = accelTime;
            Ship = ship;

            //ship.GetComponent<Rigidbody>().velocity = Vector3.zero;
            //ship.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            ship.GetComponent<Rigidbody>().isKinematic = true;
            ship.GetComponent<ShipPhysics>().enabled = false;
        }
    }

    private void Start()
    {
        jumps = new List<JumpSequence>();
    }

    private void Update()
    {
        for(int i=0; i<jumps.Count; i++)
        {
            if (jumps[i].Timer < 0)
            {
                ParticleController.Instance.CreateJumpFlashAtPos(jumps[i].Ship.transform.position);
                if (jumps[i].Ship == Ship.PlayerShip.gameObject) {
                    if (CanvasViewController.IsMapActive)
                        CanvasViewController.Instance.ToggleMap();
                    CanvasController.Instance.CloseAllMenus();
                    jumps[i].Ship.GetComponent<Rigidbody>().isKinematic = false;
                    jumps[i].Ship.GetComponent<ShipPhysics>().enabled = true;
                    jumps[i].Ship.transform.position = Vector3.zero;
                    SaveGame.SaveAndJump(NextSector);
                    SectorNavigation.ChangeSector(NextSector, true);
                    SceneManager.LoadScene("EmptyFlight");
                }
                else if (jumps[i].Ship.GetComponent<Ship>().faction == Player.Instance.PlayerFaction)
                {
                    Ship shipJumping = jumps[i].Ship.GetComponent<Ship>();

                    // Save OOS Ship
                    Player.ShipDescriptor oosShip = new Player.ShipDescriptor();
                    oosShip.Armor = shipJumping.Armor;
                    oosShip.ModelName = shipJumping.ShipModelInfo.ModelName;
                    oosShip.StationDocked = "none";
                    oosShip.Sector = NextSector;
                    oosShip.Position = Vector3.zero;
                    oosShip.Rotation = Quaternion.identity;

                    oosShip.Guns = new ProjectileWeaponData[shipJumping.Equipment.Guns.Count];
                    int w_i = 0;
                    foreach (GunHardpoint gun in shipJumping.Equipment.Guns)
                    {
                        oosShip.Guns[w_i++] = gun.mountedWeapon;
                    }
                    oosShip.Turrets = new ProjectileWeaponData[shipJumping.Equipment.Turrets.Count];
                    w_i = 0;
                    foreach (GunHardpoint gun in shipJumping.Equipment.Turrets)
                    {
                        oosShip.Turrets[w_i++] = gun.mountedWeapon;
                    }

                    oosShip.CargoItems = new HoldItem[shipJumping.ShipCargo.CargoContents.Count];
                    int cargo_i = 0;
                    foreach (HoldItem cargoitem in shipJumping.ShipCargo.CargoContents)
                    {
                        oosShip.CargoItems[cargo_i++] = cargoitem;
                    }

                    Player.Instance.OOSShips.Add(oosShip);
                    Player.Instance.Ships.Remove(jumps[i].Ship);
                    GameObject.Destroy(jumps[i].Ship);
                    jumps.RemoveAt(i);
                    i--;
                }
                else
                {
                    GameObject.Destroy(jumps[i].Ship);
                    jumps.RemoveAt(i);
                    i--;
                }
                
            }
            else { 
                jumps[i].Timer -= Time.deltaTime;
                jumps[i].Ship.transform.position -= SpawnPos.forward * jumps[i].Speed;
                jumps[i].Speed += 10;
            }
        }
       
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Ship")
        {    
            if (!AlreadyJumping(other.gameObject))
                jumps.Add(new JumpSequence(other.gameObject));
            if (other.gameObject == Ship.PlayerShip.gameObject)
            {
                Camera.main.GetComponent<CameraController>().SetTargetStation(null, Vector3.zero);
                CanvasViewController.Instance.Hud.SetActive(false);
                // Start FOV animation
                StartCoroutine(AnimateFOV(jumps.Count-1));
            }
        }
    }

    private IEnumerator AnimateFOV(int playerJumpIndex)
    {
        float timer = jumps[playerJumpIndex].Timer;
        float initialFov = Camera.main.fieldOfView;

        while(timer > 0) {
            timer = jumps[playerJumpIndex].Timer;
            Camera.main.fieldOfView = Mathf.Lerp(initialFov, 170f, cameraFovCurve.Evaluate(1f-timer));
            yield return null;
        }

        yield return null;
    }

    // Check if this ship has already been added to the jump queue (for ships with multiple colliders)
    private bool AlreadyJumping(GameObject ship)
    {
        foreach (JumpSequence js in jumps)
            if (js.Ship == ship)
                return true;

        return false;
    }

}
}