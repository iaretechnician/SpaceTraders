using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public class IngameMenuController : MonoBehaviour {

    public Text Credits, Experience;

    private int _selectedItem = -1;
    private List<ClickableImage> _menuItems;

    private GameObject[] _openedMenus;   // Tracks which menus are currently open

    private System.EventHandler _entryDelegate, _exitDelegate;

    void Awake () {
        _menuItems = new List<ClickableImage>();        

        foreach (Transform child in transform)
        {
            _menuItems.Add(child.gameObject.GetComponent<ClickableImage>());
        }

        _openedMenus = new GameObject[_menuItems.Count];

        _entryDelegate = new System.EventHandler(OnPointerEntry);
        _exitDelegate = new System.EventHandler(OnPointerExit);
    }
	
	void Update () {
        // Ignore input if a menu is open
        if (CanvasController.Instance.GetNumberOfOpenMenus() > 0)
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (_selectedItem <= 0)
            {
                _selectedItem = 0;
            }
            else
            {
                _menuItems[_selectedItem].SetColor(Color.white);
                _selectedItem--;
                MusicController.Instance.PlaySound(AudioController.Instance.ScrollSound);
            }
            _menuItems[_selectedItem].SetColor(Color.red);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_selectedItem == -1) { 
                _selectedItem = 0;
            }
            else if(_selectedItem+1 < _menuItems.Count){
                _menuItems[_selectedItem].SetColor(Color.white);
                _selectedItem++;
                MusicController.Instance.PlaySound(AudioController.Instance.ScrollSound);
            }
            _menuItems[_selectedItem].SetColor(Color.red);
        }
        if (Input.GetKeyDown(KeyCode.Return)){
            OnItemSelected();
        }
    }

    private void OnEnable()
    {
        transform.parent.GetComponent<Animator>().SetTrigger("OpenIngameMenu");

        _selectedItem = 0;
        _menuItems[0].SetColor(Color.red);
        for (int i=1; i<_menuItems.Count; i++)
            _menuItems[i].SetColor(Color.white);

        EventManager.PointerEntry += _entryDelegate;
        EventManager.PointerExit += _exitDelegate;

        Credits.text = "Credits: \n"+Player.Instance.Credits;
        Experience.text = "Experience: \n" + Progression.Experience;
    }

    private void OnDisable()
    {
        EventManager.PointerEntry -= _entryDelegate;
        EventManager.PointerExit -= _exitDelegate;
    }

    private void OnPointerEntry(object sender, System.EventArgs e)
    {
        ClickableImage textComponent;
        if ((textComponent = ((GameObject)sender).GetComponent<ClickableImage>()) == null)
            return;
        if (CanvasController.Instance.GetNumberOfOpenMenus() > 0)
            return;

        // Set all (other) items to unselected (white)
        foreach (ClickableImage ci in _menuItems)
        {
            ci.SetColor(Color.white);
        }

        textComponent.SetColor(Color.red);
        _selectedItem = _menuItems.IndexOf(textComponent);
    }

    private void OnPointerExit(object sender, System.EventArgs e)
    {
        if (((GameObject)sender).GetComponent<ClickableImage>() == null)
            return;
        if (CanvasController.Instance.GetNumberOfOpenMenus() > 0)
            return;

        // Set all (other) items to unselected (white)
        foreach (ClickableImage ci in _menuItems)
        {
            ci.SetColor(Color.white);
        }

        _selectedItem = -1;
    }

    /// <summary>
    /// This method contains the functionality of the menu, and performs the 
    /// desired operation depending on which option was selected by the user.
    /// </summary>
    public void OnItemSelected()
    {
        if (_openedMenus[_selectedItem] != null)
            return;

        if (_selectedItem == 0)   // Current ship menu
        {
            _openedMenus[0] = CanvasController.Instance.OpenMenu(UIElements.Instance.TargetMenu);
            TargetScrollMenu menu = _openedMenus[0].GetComponent<TargetScrollMenu>();

            menu.PopulateMenuOptions(Ship.PlayerShip.gameObject);
        }
        if (_selectedItem == 1)  // Sector navigation menu
        {
            _openedMenus[1] = CanvasController.Instance.OpenMenuAtPosition(UIElements.Instance.SimpleMenu, new Vector2(Screen.width/2, Screen.height/2));
            SimpleMenuController navMenu = _openedMenus[1].GetComponent<SimpleMenuController>();
            navMenu.HeaderText.text = "Select map type";

            navMenu.AddMenuOption("Sector Map").AddListener(() => {
                CanvasController.Instance.CloseAllMenus();
                CanvasViewController.Instance.ToggleMap();
            });
            navMenu.AddMenuOption("Universe Map").AddListener(() => {
                CanvasController.Instance.CloseMenu();
                _openedMenus[1] = CanvasController.Instance.OpenMenu(UIElements.Instance.UniverseMap);
            });
        }
        if (_selectedItem == 2)  // Target Menu
        {
            GameObject target = InputHandler.Instance.GetCurrentSelectedTarget();
            if (target == null)
            {
                MusicController.Instance.PlaySound(AudioController.Instance.ScrollSound);
                return;
            }

            _openedMenus[2] = CanvasController.Instance.OpenMenu(UIElements.Instance.TargetMenu);
            TargetScrollMenu menu = _openedMenus[2].GetComponent<TargetScrollMenu>();

            menu.PopulateMenuOptions(target);
        }
        if (_selectedItem == 3)  // Reputation menu
        {
            _openedMenus[3] = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText);
            ScrollTextController reputationMenu = _openedMenus[3].GetComponent<ScrollTextController>();

            reputationMenu.HeaderText.text = "Player Information";

            reputationMenu.AddMenuItem("Level: " + Progression.Level, true, Color.white);
            reputationMenu.AddMenuItem("Experience: " + Progression.Experience, false, Color.white);
            reputationMenu.AddMenuItem("", false, Color.white);
            reputationMenu.AddMenuItem("Credits: "+Player.Instance.Credits, false, Color.white);
            reputationMenu.AddMenuItem("Ships owned: " + (Player.Instance.Ships.Count+Player.Instance.OOSShips.Count), false, Color.white);
            reputationMenu.AddMenuItem("", false, Color.white);

            reputationMenu.AddMenuItem("Player reputation", true, Color.white);
            foreach (Faction otherFaction in ObjectFactory.Instance.Factions)
            {
                if (otherFaction == Player.Instance.PlayerFaction)
                    continue;

                reputationMenu.AddMenuItem(otherFaction.name + ": " + Player.Instance.PlayerFaction.RelationWith(otherFaction).ToString("0.0"),
                    false, Player.Instance.PlayerFaction.GetRelationColor(otherFaction));
            }

            reputationMenu.AddMenuItem("",false, Color.white);
            reputationMenu.AddMenuItem("Fighter and capital ship kills by faction:", true, Color.white);
            foreach(var killsByFaction in Player.Instance.Kills)
            {
                reputationMenu.AddMenuItem(killsByFaction.Key.name+" --- "+ killsByFaction.Value.x+", "+ killsByFaction.Value.y, false, Color.white);
            }

            if(MissionControl.CurrentJob != null)
            {
                reputationMenu.AddMenuItem("", false, Color.white);
                reputationMenu.AddMenuItem("Current mission: " + MissionControl.CurrentJob.Type+ " for "+
                    MissionControl.CurrentJob.Employer, true, Color.white);
                reputationMenu.AddMenuItem("Time remaining: " +
                    (MissionControl.CurrentJob.Duration - Time.time - MissionControl.CurrentJob.TimeStarted), true, Color.white);
            }
        }
        if (_selectedItem == 4)
        {
            _openedMenus[4] = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText);
            ScrollTextController menu = _openedMenus[4].GetComponent<ScrollTextController>();

            menu.HeaderText.text = "Game controls";
            menu.AddMenuItem("Space - switch between mouse and keyboard flight modes", false, Color.white);
            menu.AddMenuItem("<b>Mouse flight mode:</b>\nW,S - Accelerate,Decelerate\n" +
                "A,D - Strafe left and right", false, Color.white);
            menu.AddMenuItem("<b>Keyboard flight mode:</b>\nW,S - Pitch up and down\n" +
                "A,D - Yaw left and right", false, Color.white);
            menu.AddMenuItem("Q/E - roll left/right", false, Color.white);
            menu.AddMenuItem("Mousewheel - throttle control", false, Color.white);
            menu.AddMenuItem("Shift+A - toggle autopilot (depends on selected target)", false, Color.white);
            menu.AddMenuItem("Shift+C - request docking at Station", false, Color.white);
            menu.AddMenuItem("Shift+W - toggle Supercruise", false, Color.white);
            menu.AddMenuItem("H - toggle orbit camera", false, Color.white);
            menu.AddMenuItem("Z - toggle main engines", false, Color.white);
            menu.AddMenuItem("Esc - cancel current menu", false, Color.white);
            menu.AddMenuItem("Enter - ingame menu", false, Color.white);
            menu.AddMenuItem("T - get closest target", false, Color.white);
            menu.AddMenuItem("F2 - toggle HUD", false, Color.white);
            menu.AddMenuItem("", false, Color.white);
            menu.AddMenuItem("Map View Controls", true, Color.white);
            menu.AddMenuItem("M - toggle map view", false, Color.white);
            menu.AddMenuItem("Space - lock camera movement", false, Color.white);
            menu.AddMenuItem("W/A/S/D - move camera forwards/left/backwards/right", false, Color.white);
            menu.AddMenuItem("E/Q - move campera up/down", false, Color.white);
            menu.AddMenuItem("F - center camera on selected object", false, Color.white);
            menu.AddMenuItem("R - change ship to selected ship", false, Color.white);
            menu.AddMenuItem("Right click - issue order to ship", false, Color.white);
            menu.AddMenuItem("Shift + Right click - issue waypoint move order to ship", false, Color.white);
        }
        MusicController.Instance.PlaySound(AudioController.Instance.SelectSound);
    }

    public void OnItemSelected(int itemID)
    {
        _selectedItem = itemID;
        OnItemSelected();
    }
}
}