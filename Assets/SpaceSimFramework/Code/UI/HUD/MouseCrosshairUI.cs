using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
/// <summary>
/// Updates the position of this GameObject to reflect the position of the mouse
/// when the player ship is using mouse input. Otherwise, it just hides it.
/// </summary>
public class MouseCrosshairUI : MonoBehaviour
{
    [Tooltip("Middle of the screen crosshair")]
    public Image Crosshair;

    private Image cursor;
    private static int ROTATION_SPEED = 500;

    private void Awake()
    {
        cursor = GetComponent<Image>();
    }

    private void Update()
    {
        if (cursor != null && Ship.PlayerShip != null)
        {
            cursor.enabled = Ship.PlayerShip.UsingMouseInput || CanvasViewController.IsMapActive;

            if (cursor.enabled)
            {
                cursor.transform.position = Input.mousePosition;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else
        {
            Cursor.visible = true;
        }

        // Spin me right round right round (when shooting)
        if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.LeftControl))
        {
            Crosshair.transform.Rotate(new Vector3(0, 0, ROTATION_SPEED * Time.deltaTime));
        }
        if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.LeftControl))
        {
            Crosshair.transform.rotation = Quaternion.identity;
        }
    }

        
}
}