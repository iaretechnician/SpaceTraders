using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public class NavigationContactList : MonoBehaviour {

    public TargetInfocard Infocard;
    public Sprite[] ItemIcons;
    public Text HeaderText;
    public Transform OptionContainer;

    public enum ObjectIcon
    {
        Ship = 0,
        Station = 1,
        Jumpgate = 2
    }

    private const float REFRESH_TIMER = 2f;
    private float _timer = 0;
    private List<ClickableText> _availableOptions;  // Clickable text items available in menu
    private List<GameObject> _availableObjects;  // Objects available for selection
    private Dictionary<ClickableText, GameObject> _displayedShips;  
    private int _selectedOption = 0;

    private void Awake()
    {
        HeaderText.text = "Current Sector: " + SectorNavigation.CurrentSector;
        _availableOptions = new List<ClickableText>();
        _availableObjects = new List<GameObject>();
        _displayedShips = new Dictionary<ClickableText, GameObject>();

        foreach (GameObject station in SectorNavigation.Stations)
        {
            Color relationColor = Player.Instance.PlayerFaction.GetTargetColor(station);
            AddContact(station, relationColor, (int)ObjectIcon.Station);
        }
        foreach (GameObject gate in SectorNavigation.Jumpgates)
        {
            Color relationColor = Player.Instance.PlayerFaction.GetTargetColor(gate);
            AddContact(gate, Color.white, (int)ObjectIcon.Jumpgate);
        }
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if(_timer < 0)
        {
            UpdateShipContactsList();
            _timer = REFRESH_TIMER;
        }
    }

    private void OnEnable()
    {
        _timer = 0; // Update the contact list   
    }

    private void UpdateShipContactsList()
    {
        List<ClickableText> shipItems = new List<ClickableText>(_displayedShips.Keys);
        foreach (var textItem in shipItems)
        {
            GameObject ship = _displayedShips[textItem];
            // Remove non-existing and outside-of-range ships
            if(ship == null || ShipOutsideScannerRange(ship.transform.position))
            {
                if (_selectedOption >= _availableOptions.Count)
                {
                    _selectedOption = _availableOptions.Count - 1;
                    InputHandler.Instance.SelectedObject = _availableObjects[_availableObjects.Count - 1];
                }
                _displayedShips.Remove(textItem);
                _availableObjects.RemoveAt(_availableOptions.IndexOf(textItem));
                _availableOptions.Remove(textItem);
                GameObject.Destroy(textItem.gameObject);
            }
        }
        foreach(GameObject ship in SectorNavigation.Ships)
        {
            // Add new ships to contact list
            if (!_displayedShips.ContainsValue(ship) && !ShipOutsideScannerRange(ship.transform.position)) {
                Color relationColor = Player.Instance.PlayerFaction.GetTargetColor(ship);
                AddContact(ship, relationColor, (int)ObjectIcon.Ship);
                _displayedShips.Add(_availableOptions[_availableOptions.Count - 1], ship);
            }
        }
    }

    private bool ShipOutsideScannerRange(Vector3 shipPosition)
    {
        foreach (var playerShip in Player.Instance.Ships)
        {
            // TODO possible performance issues
            if (Vector3.Distance(playerShip.transform.position, shipPosition) < playerShip.GetComponent<Ship>().ScannerRange)
                return false;
        }

        return true;
    }

    private void AddContact(GameObject item, Color color, int iconIndex)
    {
        AddMenuOption(item.name, color, ItemIcons[iconIndex], item).AddListener(() => // On Click
        {
            // Cheap implementation of double click, center camera on target
            Camera.main.GetComponent<MapCameraController>().IsTrackingTarget = InputHandler.Instance.SelectedObject == item;

            InputHandler.Instance.SelectedObject = item;
            foreach(var option in _availableOptions)
            {
                option.SetColor(Color.white);
            }

            _selectedOption = _availableObjects.IndexOf(item);
            _availableOptions[_selectedOption].SetColor(Color.red);

            Ship ship = item.GetComponent<Ship>();
            if(ship != null)
                Infocard.InitializeInfocard(ship);
        });

    }

    private Button.ButtonClickedEvent AddMenuOption(string text, Color color, Sprite icon, GameObject addedObject)
    {
        GameObject listItem = Instantiate(UIElements.Instance.ClickableImageText);
        listItem.name = text;

        RectTransform rt = listItem.GetComponent<RectTransform>();
        rt.SetParent(OptionContainer.transform);

        NavigationListItem nli = listItem.GetComponent<NavigationListItem>();
        nli.SetText(text);
        nli.Icon.color = color;
        nli.Icon.sprite = icon;
        nli.Icon.GetComponent<AspectRatioFitter>().aspectRatio = 1;
        nli.OwnerMenu = this.gameObject;

        listItem.GetComponent<LayoutElement>().preferredHeight = 40;

        _availableOptions.Add(nli);
        _availableObjects.Add(addedObject);

        return nli.GetButtonOnClick();
    }

}
}