using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public class PhysicsUI : MonoBehaviour {

    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (text != null && Ship.PlayerShip != null)
        {
            text.text = string.Format("x force: {0}\ny force: {1}\n z force: {2}",
                (Ship.PlayerShip.Velocity.x * 10.0f).ToString("000"),
                (Ship.PlayerShip.Velocity.y * 10.0f).ToString("000"),
                (Ship.PlayerShip.Velocity.z * 10.0f).ToString("000"));
        }
    }
}
}