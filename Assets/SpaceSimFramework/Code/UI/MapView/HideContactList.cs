using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
public class HideContactList : MonoBehaviour
{

    public GameObject NavContactList;

    public void OnClick()
    {
        NavContactList.SetActive(!NavContactList.activeInHierarchy);
    }
}
}