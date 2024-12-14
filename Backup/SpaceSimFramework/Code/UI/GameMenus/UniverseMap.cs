using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public class UniverseMap : MonoBehaviour {

    #region static functionality
    public static Dictionary<SerializableVector2, SerializableSectorData> Knowledge;
    #endregion static functionality

    public GameObject SectorIconPrefab, LinePrefab, SectorSelectedPrefab;
    public RectTransform MapContainer, IconContainer;
    public float MapSize = 10;

    private ScrollTextController _sectorDetailsPanel;
    private RectTransform _selectedSectorIcon;

    // Dictionary<Map object, Image icon>
    private Dictionary<SerializableUniverseSector, GameObject> _mapIcons;

    private SerializableUniverseSector _selectedSector = null;

    private void Awake()
    {
        _mapIcons = new Dictionary<SerializableUniverseSector, GameObject>();
        _sectorDetailsPanel = GetComponent<ScrollTextController>();
        _selectedSectorIcon = GameObject.Instantiate(SectorSelectedPrefab, IconContainer.transform)
            .GetComponent<RectTransform>();

        SetSelectedSector(SectorNavigation.CurrentSector);
    }

    private void Start()
    {
        GameObject sectorIcon;

        // Add sector icons and connections
        foreach(SerializableUniverseSector sector in Universe.Sectors.Values)
        {
            SerializableUniverseSector sd = sector;

            if (IsKnownSector(sector.SectorPosition)) {
                sectorIcon = GameObject.Instantiate(SectorIconPrefab, IconContainer.transform);
                sectorIcon.GetComponent<RectTransform>().anchoredPosition = WorldToMapPosition(sector.SectorPosition);
                sectorIcon.GetComponentInChildren<Text>().text = sector.SectorPosition.ToString();
                sectorIcon.GetComponent<Image>().color = GetSectorColor(sector);
                sectorIcon.name = sector.Name;

                // For mouseclick selection of Universe.Sectors on the map
                sectorIcon.GetComponent<Button>().onClick.AddListener(() =>
                {
                    _selectedSector = sd;
                    SetSelectedSector(sd.SectorPosition);
                });

                if (sector.SectorPosition == SectorNavigation.CurrentSector)
                    sectorIcon.GetComponent<Image>().color = Color.yellow;

                _mapIcons.Add(sector, sectorIcon);
          
                // Add connection lines to jumpgates
                foreach(Vector2 connectingSectorPosition in sector.Connections)
                {
                    Vector2 otherSystemPos = WorldToMapPosition(connectingSectorPosition);
                    Vector2 differenceVector = otherSystemPos - WorldToMapPosition(sector.SectorPosition);

                    RectTransform imageRectTransform = GameObject.Instantiate(LinePrefab, IconContainer.transform).GetComponent<RectTransform>();
                    imageRectTransform.gameObject.GetComponent<Image>().sprite = null;

                    imageRectTransform.sizeDelta = new Vector2(differenceVector.magnitude, 3);
                    imageRectTransform.pivot = new Vector2(0, 0.5f);
                    imageRectTransform.anchoredPosition = WorldToMapPosition(sector.SectorPosition);
                    float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
                    imageRectTransform.rotation = Quaternion.Euler(0, 0, angle);
                }
            }
        }
    }

    private bool IsKnownSector(Vector2 position)
    {
        foreach(var knownSector in Knowledge)
        {
            if (knownSector.Key == position)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Sets the selected sector on the universe map
    /// </summary>
    /// <param name="sectorName"></param>
    private void SetSelectedSector(Vector2 sectorPosition)
    {
        Vector2 iconPos = SectorNavigation.UNSET_SECTOR;
        foreach (SerializableUniverseSector sd in Universe.Sectors.Values)
        {
            if (sd.SectorPosition == sectorPosition)
            {
                _selectedSector = sd;
                iconPos = WorldToMapPosition(sd.SectorPosition);
                break;
            }
        }
        if(iconPos == SectorNavigation.UNSET_SECTOR)
        {
            Debug.LogError("Current sector " + sectorPosition + " not found in database!");
            return;
        }

        _selectedSectorIcon.anchoredPosition = iconPos;

        UpdateSectorText();
    }

    /// <summary>
    /// Updates sector text when a sector has been selected
    /// </summary>
    private void UpdateSectorText()
    {
        _sectorDetailsPanel.ClearItems();

        _sectorDetailsPanel.AddMenuItem(_selectedSector.Name, true, Color.red);
        _sectorDetailsPanel.AddMenuItem("Owner faction: "+_selectedSector.OwnerFaction, false, Color.white);
        if (Knowledge.ContainsKey(_selectedSector.SectorPosition))
        {
            SerializableSectorData sectorData = Knowledge[_selectedSector.SectorPosition];
            _sectorDetailsPanel.AddMenuItem("Sector size: " + sectorData.Size, false, Color.white);
            _sectorDetailsPanel.AddMenuItem("Known stations: " + sectorData.Stations.Count, false, Color.white);
            _sectorDetailsPanel.AddMenuItem("Known jumpgates: " + sectorData.Jumpgates.Count, false, Color.white);
            _sectorDetailsPanel.AddMenuItem("Asteroid fields: " + sectorData.Fields.Count, false, Color.white);
            _sectorDetailsPanel.AddMenuItem("Nebula presence: " + sectorData.Nebula != null ? "Yes" : "No", false, Color.white);
        }

        _sectorDetailsPanel.AddMenuItem("Player ships in " + _selectedSector.Name, true, Color.red);
        // Display number of player ships in selected sector
        if(SectorNavigation.CurrentSector == _selectedSector.SectorPosition)
        {
            foreach (var ship in Player.Instance.Ships)
                _sectorDetailsPanel.AddMenuItem(ship.name, false, Color.white);
        }
        else
        {
            foreach (var ship in Player.Instance.OOSShips)
                if(ship.Sector == _selectedSector.SectorPosition)
                    _sectorDetailsPanel.AddMenuItem(ship.ModelName, false, Color.white);
        }
    }


    /// <summary>
    /// Converts the universe position of a sector to the 2D map position determined by the 
    /// size of the map rect transform. Universe positions are clamped from -100 to 100.
    /// </summary>
    /// <param name="sectorCoords"></param>
    /// <returns></returns>
    private Vector2 WorldToMapPosition(Vector2 sectorCoords)
    {
        // Width and height must be identical
        Vector2 mapDimensions = new Vector2(MapContainer.rect.width, MapContainer.rect.width);

        return new Vector2(mapDimensions.x / 2 * sectorCoords.x / MapSize, mapDimensions.y / 2 * sectorCoords.y / MapSize);
    }

    /// <summary>
    /// Gets the color of the sector icon depending on the player's reputation to the 
    /// faction which holds the sector.
    /// </summary>
    /// <param name="sector">Sector whose icon should be colored</param>
    private Color GetSectorColor(SerializableUniverseSector sector)
    {
        Faction playerFaction = Player.Instance.PlayerFaction;
        Faction sectorFaction = ObjectFactory.Instance.GetFactionFromName(sector.OwnerFaction);
        return playerFaction.GetRelationColor(sectorFaction);
        // If faction ownership is 1, display full color, otherwise blend toward white
        /*return new Color(
            relation < 0 ? 1 : sector.Influence,
            relation > 0 ? 1 : sector.Influence,
            sector.Influence
        );*/
    }

}
}