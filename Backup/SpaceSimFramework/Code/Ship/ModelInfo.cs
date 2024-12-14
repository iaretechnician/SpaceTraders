using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Contains specifications of a certain ship model
/// </summary>
[CreateAssetMenu(menuName = "ModelInfo")]
public class ModelInfo : ScriptableObject
{
    [Tooltip("Ship model name")]
    public string ModelName;
    [Tooltip("Does this ship use external docking or docking bays?")]
    public bool ExternalDocking;
    [Tooltip("Distance at which the camera follows the ship (for different ship sizes)")]
    public float CameraOffset;
    [Tooltip("Ship class determines maximum level of equipment mountable to it")]
    public int Class;
    [Tooltip("Ship cost in credits")]
    public int Cost;
    [Tooltip("Armor (hull integrity) value")]
    public int MaxArmor;
    [Tooltip("Total generator power output")]
    public int GeneratorPower;
    [Tooltip("Generator power regen rate per second")]
    public float GeneratorRegen;
    [Tooltip("Cargo size in container units")]
    public int CargoSize;
    [Tooltip("X: Linear thrust\nY: Vertical thrust\nZ: Longitudinal Thrust")]
    public Vector3 LinearForce;
    [Tooltip("X: Angular thrust:\nX: Pitch\nY: Yaw\nZ: Roll")]
    public Vector3 AngularForce;
    [Tooltip("Number of equipment items this model can equip")]
    public int EquipmentSlots;
}
}