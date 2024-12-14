using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceSimFramework
{
public class CanvasController : Singleton<CanvasController> {

    public GameObject IngameMenu;

    private Stack<GameObject> _openMenus;
    private System.EventHandler _onClickDelegate;

    private void Awake()
    {
        _openMenus = new Stack<GameObject>();
        _onClickDelegate = new EventHandler(OnCloseClicked);
        EventManager.CloseClicked += _onClickDelegate;
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        CloseMenu();
    }

    private void OnDestroy()
    {
        EventManager.CloseClicked -= _onClickDelegate;
    }

    // Either the ingame menu or popup menus can be open
    void Update () {

        if(GetNumberOfOpenMenus() > 0 || IngameMenu.activeInHierarchy)
        {
            Ship.PlayerShip.UsingMouseInput = false;
            Ship.IsShipInputDisabled = true;
        }

        if (Input.GetKeyDown(KeyCode.Return) && _openMenus.Count == 0)
        {
            IngameMenu.SetActive(true);
            Ship.IsShipInputDisabled = true;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }

        if(!Ship.PlayerShip.UsingMouseInput) { // Mouseover on top of the screen opens the Ingame Menu if in keyboard flight mode
            if(Input.mousePosition.x > 0.25*Screen.width && Input.mousePosition.x < 0.75 * Screen.width && Input.mousePosition.y > Screen.height - 10)
                if (_openMenus.Count == 0)
                {
                    IngameMenu.SetActive(true);
                    Ship.IsShipInputDisabled = true;
                }
        }
        if(IngameMenu.activeInHierarchy && Input.GetMouseButtonDown(0) && GetNumberOfOpenMenus() == 0)
        {
            if (Input.mousePosition.x < 0.25 * Screen.width || Input.mousePosition.x > 0.75 * Screen.width || Input.mousePosition.y < Screen.height * 0.9f)
            {
                CloseMenu();
            }
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            // Check for already existing menu popup
            if(GetNumberOfOpenMenus() > 0) {                
                var topMostMenu = _openMenus.Peek();
                if (topMostMenu != null && topMostMenu.GetComponent<PopupConfirmMenuController>() != null)
                {
                    var popupMenu = topMostMenu.GetComponent<PopupConfirmMenuController>();
                    if (popupMenu.HeaderText.text != "Exit to Main Menu?")
                    {
                        OpenMainMenuPopup();
                    }                 
                }
            }
            else
            {
                OpenMainMenuPopup();
            }
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            CanvasViewController.Instance.ToggleHUDActive();
            ObjectUIMarkers.Instance.gameObject.SetActive(!ObjectUIMarkers.Instance.gameObject.activeInHierarchy);
        }
    }

    private void OpenMainMenuPopup()
    {
        var popupMenu = OpenMenuAtPosition(UIElements.Instance.SimpleMenu, new Vector2(Screen.width / 2, Screen.height / 2), true)
               .GetComponent<SimpleMenuController>();

        popupMenu.HeaderText.text = "";
        popupMenu.AddMenuOption("Save game").AddListener(() =>
        {
            SaveGame.Save(SectorNavigation.UNSET_SECTOR);
            ConsoleOutput.PostMessage("Game Saved.");
            CanvasController.Instance.CloseMenu();
        });
        popupMenu.AddMenuOption("Exit to Main Menu").AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });
    }

    /// <summary>
    /// Opens a requested menu and returns the reference to the UI gameobject.
    /// </summary>
    /// <param name="menu">Menu prefab from the UIElements object</param>
    /// <returns>UI element gameobject</returns>
    public GameObject OpenMenu(GameObject menu)
    {
        GameObject menuInstance = GameObject.Instantiate(menu, this.transform);
        // Set currently open menu to inactive
        if (_openMenus.Count > 0)
            _openMenus.Peek().SetActive(false);
        // Add new open menu
        _openMenus.Push(menuInstance);

        //Debug.Log("OPENED MENU " + menuInstance.name + " ,open menus: " + GetNumberOfOpenMenus());

        return menuInstance;
    }

    /// <summary>
    /// Opens a requested menu at a given 2D position (usually mousePosition) 
    /// and returns the reference to the UI gameobject.
    /// </summary>
    /// <param name="menu">Menu prefab from the UIElements object</param>
    /// <param name="position">2D screen position of UI element's upper-right corner</param>
    /// <returns>UI element gameobject</returns>
    public GameObject OpenMenuAtPosition(GameObject menu, Vector2 position, bool hideMenuBelow = true)
    {
        GameObject menuInstance = null;

        if (!hideMenuBelow)
        {
            menuInstance = GameObject.Instantiate(menu, this.transform);

            // Add new open menu
            _openMenus.Push(menuInstance);
        }
        else
            menuInstance = OpenMenu(menu);

        menuInstance.GetComponent<RectTransform>().anchoredPosition = position;

        //Debug.Log("OPENED MENU " + menuInstance.name + " ,open menus: " + GetNumberOfOpenMenus());

        return menuInstance;
    }

   

    /// <summary>
    /// Closes the currently visible (open) menu (and opens a menu one layer below, if such
    /// exists)
    /// </summary>
    public void CloseMenu()
    {
        if (_openMenus.Count > 0 && _openMenus.Peek().GetComponent<StationMainMenu>())
            return; // Do not close station main menu. Ever.

        if (_openMenus.Count == 0)
        {
            IngameMenu.SetActive(false);
        }
        if (GetNumberOfOpenMenus() == 0 && !IngameMenu.activeInHierarchy)
        {
            Ship.IsShipInputDisabled = false;
        }

        if (_openMenus.Count > 0) {

            GameObject menu = _openMenus.Pop();
            GameObject.Destroy(menu);
        }
        if (_openMenus.Count > 0)
            _openMenus.Peek().SetActive(true);
    }

    /// <summary>
    /// Closes all active menus, including the Station Menu.
    /// </summary>
    public void CloseAllStationMenus()
    {
        while (_openMenus.Count > 0)
        {

            GameObject menu = _openMenus.Pop();
            GameObject.Destroy(menu);
        }
    }

    public int GetNumberOfOpenMenus()
    {
        return _openMenus.Count;
    }

    /// <summary>
    /// Closes all opened menus, including the special menus (ingame menu and map)
    /// </summary>
    public void CloseAllMenus()
    {
        while (GetNumberOfOpenMenus() > 0)
            CloseMenu();
    }  
}
}