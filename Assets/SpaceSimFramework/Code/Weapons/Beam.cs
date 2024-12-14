using UnityEngine;
using System.Collections;

namespace SpaceSimFramework
{
public class Beam : MonoBehaviour
{
    public LineRenderer BeamLine;

    // To prevent ships from shooting themselves...
    private float minRange = 15f;

    public bool PlayerShot = false;

    public void FireProjectile(Vector3 direction, float range, int dps)
    {
        RaycastHit hit;

        if (BeamLine == null)
            BeamLine = GetComponent<LineRenderer>();

        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        if (Physics.Raycast(transform.position+direction.normalized* minRange, direction, out hit, range, layerMask)){
            if (hit.transform.tag == "Ship")
                hit.transform.GetComponent<Ship>().TakeDamage(dps * Time.deltaTime, PlayerShot);
            if (hit.transform.tag == "Asteroid")
                hit.transform.GetComponent<Asteroid>().TakeDamage(dps * Time.deltaTime);

            BeamLine.SetPosition(0, transform.position);
            BeamLine.SetPosition(1, hit.transform.position);
        }
        else
        {
            BeamLine.SetPosition(0, transform.position);
            BeamLine.SetPosition(1, transform.position+ direction * range);
        }
       

        
    }
}
}