using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimFramework
{
public class MainMenuDialog : MonoBehaviour {

    public MainMenuController menuCtrl;

	void Update () {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnYesClicked();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnNoClicked();
        }
    }

    public void OnYesClicked()
    {
        menuCtrl.OnYesClicked();
    }

    public void OnNoClicked()
    {
        menuCtrl.OnNoClicked();
    }
}
}