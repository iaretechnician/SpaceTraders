using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
/// <summary>
/// Shows throttle and speed of the player ship.
/// </summary>
public class SpeedUI : MonoBehaviour
{
    public Text TextVelocity, TextSpeed;

    // Update is called once per frame
    void Update()
    {
        if(Ship.PlayerShip != null)
        {
            if (TextVelocity != null)
            {
                TextVelocity.text = string.Format("THR\n{0}", (Ship.PlayerShip.Throttle * 100.0f).ToString("000"));
            }
            if (TextSpeed != null)
            {
                TextSpeed.text = string.Format("SPD\n{0}", Ship.PlayerShip.Velocity.magnitude.ToString("000"));
            }
        }
    }
}
}