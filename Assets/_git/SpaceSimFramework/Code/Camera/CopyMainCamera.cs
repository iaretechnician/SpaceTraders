using UnityEngine;

namespace SpaceSimFramework
{
public class CopyMainCamera : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
        transform.position = Camera.main.transform.position;
    }
}
}