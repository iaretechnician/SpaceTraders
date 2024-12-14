using UnityEngine;

namespace SpaceSimFramework
{
public class ToggleObjectsOnKey : MonoBehaviour
{
    public GameObject[] Objects;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            foreach(var obj in Objects)
            {
                obj.SetActive(!obj.activeInHierarchy);
            }
        }
    }
}
}