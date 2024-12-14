using UnityEngine;

namespace SpaceSimFramework
{
public class StationMainMenu : ScrollMenuController
{
    private GameObject shipLandedAtStation;

    public void PopulateMenuOptions(GameObject ship, Station station)
    {
        shipLandedAtStation = ship;

        HeaderText.text = gameObject.name;
        AddMenuOption("Undock").AddListener(() => {
            CanvasController.Instance.CloseMenu();
            station.UndockShip(Ship.PlayerShip.gameObject);
        });

        ShowDockedShips(station);

        if (station.HasCargoDealer)
            AddMenuOption("Trade").AddListener(() => {
                var tradeMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.StationTradeMenu);
                tradeMenu.GetComponent<StationTradeMenu>().PopulateMenu(shipLandedAtStation, station);
            });

        AddMenuOption("Equipment").AddListener(() => {
            var tradeMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.StationEquipmentMenu);
            tradeMenu.GetComponent<StationEquipmentMenu>().PopulateMenu(shipLandedAtStation, station);
        });

        if (station.HasShipDealer)
            OpenShipDealer(shipLandedAtStation, station);
            

        AddMenuOption("Info").AddListener(() => {
            var infoMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText);
            ScrollTextController infoCard = infoMenu.GetComponent<ScrollTextController>();

            // Faction text in color according to relation with station
            float relation = shipLandedAtStation.GetComponent<Ship>().faction.RelationWith(station.faction);
            infoCard.AddMenuItem("Faction: " + station.faction.name, false,
                relation == 0 ? Color.white : (relation < 0 ? Color.red : Color.green));

            StationDealer dealer = station.GetComponent<StationDealer>();
            foreach (var ware in dealer.WarePrices)
            {
                infoCard.AddMenuItem(ware.Key, "(" + ware.Value + ")", false, Color.white);
            }
        });

        AddMenuOption("Jobs").AddListener(() => {
            int numJobs = Random.Range(0, 5);

            var jobMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollMenu);
            ScrollMenuController jobsBoard = jobMenu.GetComponent<ScrollMenuController>();

            if (numJobs == 0)
            {
                jobsBoard.HeaderText.text = "No jobs are offered currently.";
                return;
            }

            jobsBoard.HeaderText.text = "Select a job to view details:";
            for (int i = 0; i < numJobs; i++)
            {
                Mission m_i = MissionControl.GetNewMission(station.faction);
                jobsBoard.AddMenuOption(
                    m_i.Type + " (" + m_i.Payout + " credits, time: " + m_i.Duration / 60 + " minutes)",
                    Color.white,
                    IconManager.Instance.GetMissionIcon(m_i.Type.ToString()),
                    1, 80).AddListener(() => {
                        // Player accepts mission, start timer
                        m_i.TimeStarted = Time.time;
                        MissionControl.CurrentJob = m_i;
                        m_i.OnMissionStarted();                                      

                        OnCloseClicked();
                });
            }

        });

        if (shipLandedAtStation == Ship.PlayerShip.gameObject && Ship.PlayerShip.Armor < Ship.PlayerShip.MaxArmor)
        {
            OpenRepairMenu(shipLandedAtStation, station);
        }
    }

    public void ShowDockedShips(Station station)
    {
        AddMenuOption("Show Docked Ships").AddListener(() => {
        var infoMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollMenu);
        ScrollMenuController dockedList = infoMenu.GetComponent<ScrollMenuController>();
            dockedList.HeaderText.text = "Ships docked at " + station.name;

            for(int ship_i=0; ship_i<station.DockedShips.Count; ship_i++)
            {
                GameObject ship = station.DockedShips[ship_i];
                dockedList.AddMenuOption(ship.name).AddListener(() =>
                {
                    var submenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollMenu);
                    ScrollMenuController dockedShipMenu = submenu.GetComponent<ScrollMenuController>();

                    // General options for any ship
                    dockedShipMenu.AddMenuOption("Info").AddListener(() =>
                    {
                        TargetScrollMenu.OpenInfoMenu(ship);
                    });

                    if (ship.GetComponent<Ship>().faction.name != Ship.PlayerShip.faction.name)
                        return;

                    // Options for player-owned ships
                    dockedShipMenu.AddMenuOption("Undock Ship").AddListener(() =>
                    {
                        station.UndockShip(ship);
                        ship.GetComponent<ShipMovementInput>().throttle = 0.5f;
                    });

                    dockedShipMenu.AddMenuOption("Change Ship").AddListener(() =>
                    {                        
                        Ship otherShip = ship.GetComponent<Ship>();
                        if (otherShip == Ship.PlayerShip)
                            return;

                        // Reset camera to follow in case ship doesn't have a cockpit
                        Camera.main.GetComponent<CameraController>().State = CameraController.CameraState.Chase;

                        Ship.PlayerShip.IsPlayerControlled = false;
                        Ship.PlayerShip = otherShip;
                        Ship.PlayerShip.IsPlayerControlled = true;
                        shipLandedAtStation = ship;

                        OnCloseClicked();
                    });
                });
            }
        });
    }

    private void OpenShipDealer(GameObject ship, Station station)
    {
        AddMenuOption("Showroom").AddListener(() =>
        {
            var shipDealerMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.StationDealershipMenu);
            shipDealerMenu.GetComponent<StationShipDealerMenu>().PopulateMenu(shipLandedAtStation, station);
        });
    }

    private void OpenRepairMenu(GameObject ship, Station station)
    {
        float hullPercentage = Ship.PlayerShip.Armor / (float)Ship.PlayerShip.MaxArmor;
        int repairCost = (int)(Ship.PlayerShip.ShipModelInfo.Cost / 2.0 * hullPercentage);

        if (repairCost > Player.Instance.Credits)
            return;

        AddMenuOption("Repair Ship").AddListener(() =>
        {
            // Open Repair Dialog
            SubMenu = GameObject.Instantiate(UIElements.Instance.ConfirmDialog, transform.parent);
            // Reposition submenu
            RectTransform rt = SubMenu.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 0);

            var repairMenu = SubMenu.GetComponent<PopupConfirmMenuController>();
            repairMenu.HeaderText.text = "Repair ship for " + repairCost + " credits?";

            // What happens when Ok or Cancel is pressed
            repairMenu.AcceptButton.onClick.AddListener(() => {
                // Fix the ship
                Ship.PlayerShip.Armor = Ship.PlayerShip.MaxArmor;
                Player.Instance.Credits -= repairCost;

                GameObject.Destroy(repairMenu.gameObject);
            });
            repairMenu.CancelButton.onClick.AddListener(() => {
                GameObject.Destroy(repairMenu.gameObject);
            });
        });
    }

}
}