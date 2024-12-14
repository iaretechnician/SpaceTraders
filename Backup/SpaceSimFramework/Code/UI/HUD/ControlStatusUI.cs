using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public class ControlStatusUI : Singleton<ControlStatusUI>
{
    public Image SuperCruise;
    public Text Text;

    private void Start()
    {
        SetControlStatusText(Ship.PlayerShip.UsingMouseInput ? "Mouse Flight" : "Keyboard Flight");
    }

    public void SetControlStatusText(string controlStatus)
    { 
        Text.text = controlStatus;
    }

    public static void SetSupercruiseIcon(bool InSupercruise)
    {
        if(Instance != null && Instance.SuperCruise != null)
        {
            Color color = Color.white;
            color.a = Ship.PlayerShip.InSupercruise ? 1 : 0.5f;
            Instance.SuperCruise.color = color;
        }
    }

    #region on click listeners for UI buttons

    public void OnControlStatusClicked()
    {
        Camera.main.GetComponent<CameraController>().ToggleFlightMode();
    }

    public void OnSupercruiseClicked()
    {
        if(Ship.PlayerShip.IsPlayerControlled)
            Ship.PlayerShip.MovementInput.ToggleSupercruise();
    }

    #endregion on click listeners for UI buttons
}
}