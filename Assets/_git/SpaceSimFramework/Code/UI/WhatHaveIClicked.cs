using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceSimFramework
{
public class WhatHaveIClicked : MonoBehaviour {

	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("CLICKED ON "+EventSystem.current.currentSelectedGameObject.name);
        }	
	}
}
}