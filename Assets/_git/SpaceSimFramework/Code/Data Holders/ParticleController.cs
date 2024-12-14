using UnityEngine;

[CreateAssetMenu(menuName = "DataHolders/ParticleController")]
public class ParticleController : SingletonScriptableObject<ParticleController>
{

    public GameObject ParticleEffectPrefab;
    public GameObject ShipExplosionPrefab;
    public GameObject JumpFlashPrefab;
    public GameObject ShipDamageEffect;

    public void CreateParticleEffectAtPos(Vector3 position)
    {
        GameObject.Instantiate(ParticleEffectPrefab, position, Quaternion.identity);
    }

    public void CreateShipExplosionAtPos(Vector3 position)
    {
        GameObject.Instantiate(ShipExplosionPrefab, position, Quaternion.identity);
    }

    public void CreateJumpFlashAtPos(Vector3 position)
    {
        GameObject.Instantiate(JumpFlashPrefab, position, Quaternion.identity);
    }
}