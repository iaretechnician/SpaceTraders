using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceSimFramework
{
/// <summary>
/// Ties all the primary ship components together.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ShipAI))]
[RequireComponent(typeof(ShipPhysics))]
[RequireComponent(typeof(ShipCargo))]
[RequireComponent(typeof(ShipMovementInput))]
[RequireComponent(typeof(ShipEquipment))]
public class Ship : MonoBehaviour
{
    public static event EventHandler ShipDestroyedEvent;

    public static int DEFAULT_SCANNER_RANGE = 3000;

    #region ship components
    // Ship cargo
    public ShipCargo ShipCargo
    {
        get { return _shipCargo; }
    }
    private ShipCargo _shipCargo;

    // Player controls
    public ShipMovementInput MovementInput
    {
        get { return _movementInput; }
    }
    private ShipMovementInput _movementInput;

    // Artificial intelligence controls
    public ShipAI AIInput
    {
        get { return _shipAI; }
    }
    private ShipAI _shipAI;

    // Weapon systems
    public ShipEquipment Equipment
    {
        get
        {
            if (shipEquipment == null)
                shipEquipment = GetComponent<ShipEquipment>();
            return shipEquipment;
        }
    }
    private ShipEquipment shipEquipment;

    // Ship rigidbody physics
    public ShipPhysics Physics
    {
        get { return physics; }
    }
    private ShipPhysics physics;

    #endregion ship components

    // Getters for external objects to reference things like input.
    public bool UsingMouseInput
    {
        get { return _movementInput.useMouseInput; }
        set { _movementInput.useMouseInput = value; }
    }
    public Vector3 Velocity { get { return physics.Rigidbody.velocity; } }
    public float Throttle { get { return _movementInput.throttle; } }
    public bool InSupercruise {
        get { return _inSupercruise; }
        set { _inSupercruise = value;
            ControlStatusUI.SetSupercruiseIcon(_inSupercruise);
        }
    }
    private bool _inSupercruise = false;

    // Keep a static reference for whether or not this is the player ship. It can be used
    // by various gameplay mechanics. Returns the player ship if possible, otherwise null.
    public static Ship PlayerShip { get { return _playerShip; } set { _playerShip = value; } }
    private static Ship _playerShip;
    public static bool IsShipInputDisabled = false;

    public ModelInfo ShipModelInfo;
    [Tooltip("Cockpit object, if available, will allow toggling cockpit view while flying this ship.")]
    public Transform Cockpit;

    [Header("Ship instance info")]
    public Faction faction;
    public bool IsPlayerControlled = true;

    [HideInInspector]
    public int ScannerRange;
    [HideInInspector]
    public float MaxArmor;  // Maximum armor value can be modified by equipment
    [HideInInspector]
    public float Armor;
    [HideInInspector]
    public string StationDocked = "none";

    private AudioSource _engineSound;
    private Ship _portWingman = null, _starboardWingman = null;
    private GameObject _fireEffect;
    private bool _isDestroyed = false;

    private void Awake()
    {
        // Initialize ship properties
        MaxArmor = ShipModelInfo.MaxArmor;
        Armor = MaxArmor;

        _engineSound = GetComponent<AudioSource>();
        ScannerRange = DEFAULT_SCANNER_RANGE;

        _movementInput = GetComponent<ShipMovementInput>();
        _shipAI = GetComponent<ShipAI>();
        physics = GetComponent<ShipPhysics>();
        _shipCargo = GetComponent<ShipCargo>();

        if (_movementInput == null || _shipAI == null || physics == null)
            Debug.LogError("Component not found on ship "+name);

        if (IsPlayerControlled)
        {
            _playerShip = this;
        }
    }

    private void Update()
    {
        _engineSound.pitch = 1.0f + Throttle * 2.0f;
        if (physics.IsEngineOn)
            _engineSound.volume = 1f;
        else
            _engineSound.volume = 0f;

        if (InSupercruise)
            shipEquipment.SupercruiseDrain();

        if(Nebula.Instance != null && Nebula.Instance.CorrosionDamagePerSecond > 0)
        {
            TakeDamage(Nebula.Instance.CorrosionDamagePerSecond * Time.deltaTime, false, false);
        }

        // If this is the player ship, then set the static reference. If more than one ship
        // is set to player, then whatever happens to be the last ship to be updated will be
        // considered the player. Don't let this happen.
        if (IsPlayerControlled)
        {
            _playerShip = this;
        }

        // Enable or disable autopilot
        if (this == _playerShip && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.A))
        {
            AIInput.OnAutopilotToggle();
        }
    }

    /// <summary>
    /// Turns the engine on and off.
    /// </summary>
    public void ToggleEngine()
    {
        physics.ToggleEngines();
    }

    /// <summary>
    /// Invoked when this ship takes damage. Amount of damage is given.
    /// </summary>
    /// <param name="damage">amount of damage taken</param>
    public void TakeDamage(float damage, bool isPlayerShot, bool shouldShake=true)
    {
        if (_isDestroyed)
            return;

        if(this == PlayerShip)
        {
            StartCoroutine(CameraController.Shake());
            if(shouldShake)
                MusicController.Instance.PlaySound(AudioController.Instance.SmallImpact);
        }

        Armor -= damage;
        if(Armor < 0)
        {
            _isDestroyed = true;

            ShipDestroyedEvent(gameObject, EventArgs.Empty);

            ParticleController.Instance.CreateShipExplosionAtPos(transform.position);
            if (InputHandler.Instance.GetCurrentSelectedTarget() == this.gameObject)
                InputHandler.Instance.SelectedObject = null;
            _shipCargo.OnShipDestroyed();

            if (isPlayerShot) {
                // Broadcast kill
                Progression.RegisterKill(this);
                MissionControl.RegisterKill(this);

                // Mark player kill
                if (!ShipModelInfo.ExternalDocking)
                {                   
                    Player.Instance.Kills[faction] += new Vector2(1, 0);
                    TextFlash.ShowYellowText(faction.name + " fighter destroyed!");
                    ConsoleOutput.PostMessage(faction.name + " fighter destroyed!");
                }
                else
                {
                    Player.Instance.Kills[faction] += new Vector2(0, 1);
                    TextFlash.ShowYellowText(faction.name + " capital ship destroyed!");
                    ConsoleOutput.PostMessage(faction.name + " capital ship destroyed!");
                }

                if(faction != Player.Instance.PlayerFaction)
                    Player.Instance.AddFactionPenalty(faction);
            }
            if (faction == Player.Instance.PlayerFaction)
            {
                if (this == PlayerShip)
                {
                    Player.Instance.Ships.Remove(this.gameObject);
                    if (Player.Instance.Ships.Count == 0)
                    {
                        MusicController.Instance.PlaySound(AudioController.Instance.HardImpact);
                        MusicController.Instance.StartCoroutine(ExitToMainMenu());
                    }
                    else
                    {
                        Ship nextplayership = Player.Instance.Ships[0].GetComponent<Ship>();
                        nextplayership.IsPlayerControlled = true;
                        Camera.main.GetComponent<CameraController>().SetTargetShip(nextplayership);
                    }
                }
                else
                {
                    Player.Instance.Ships.Remove(this.gameObject);
                }
            }

            GameObject.Destroy(this.gameObject);
        }
        else if (Armor < 0.25 * MaxArmor && _fireEffect == null)
        {
            // Spawn particle effects
            _fireEffect = GameObject.Instantiate(ParticleController.Instance.ShipDamageEffect, transform);
        }
        else if (_fireEffect != null)
        {
            // Remove particle effects
            GameObject.Destroy(_fireEffect);
        }

    }

    private IEnumerator ExitToMainMenu()
    {
        var timer = 2.0f;
        while (timer > 0) {
            timer -= Time.deltaTime;
            yield return null;
        }
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDisable()
    {
        SectorNavigation.Ships.Remove(this.gameObject);
    }

    private void OnEnable()
    {
        if (SectorNavigation.Ships != null)
            SectorNavigation.Ships.Add(this.gameObject);
    }

    // Gets the formation position offset when invoked by an escort ship
    public Vector3 GetWingmanPosition(Ship requestee)
    {
        if (requestee == this)
            return Vector3.zero;

        if (_portWingman == null)            // Port slot not occupied
        {
            //Debug.Log("[WINGMAN]: Ship " + requestee.name + " is port wingman for " + name);
            _portWingman = requestee;
            return new Vector3(-ShipModelInfo.CameraOffset, 0, -ShipModelInfo.CameraOffset);
        }
        else if (_starboardWingman == null)  // Starboard slot not occupied
        {
            //Debug.Log("[WINGMAN]: Ship " + requestee.name + " is starboard wingman for " + name);
            _starboardWingman = requestee;
            return new Vector3(-ShipModelInfo.CameraOffset, 0, ShipModelInfo.CameraOffset);
        }
        else    // Both slots occupied, ask port wingman 
        {
            return new Vector3(ShipModelInfo.CameraOffset, 0, -ShipModelInfo.CameraOffset) + _portWingman.GetWingmanPosition(requestee);
        }
    }

    public void RemoveWingman(Ship wingman)
    {
        if (_portWingman == wingman)
        {
            _portWingman = null;
        }
        else
        {
            _starboardWingman = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag != "Projectile" && !_shipAI.IsUndocking)
        {
            TakeDamage(collision.relativeVelocity.magnitude, false);    // Dont take damage from projectiles and when undocking
        }
        if (this == Ship.PlayerShip)
        {
            MusicController.Instance.PlaySound(AudioController.Instance.HardImpact);
        }
    }

}
}