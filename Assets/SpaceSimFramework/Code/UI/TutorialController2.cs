using UnityEngine;

namespace SpaceSimFramework
{
public class TutorialController2 : MonoBehaviour
{
    public GameObject MenuOverlayPrefab;
    private GameObject menuOverlay;

    // Chapter indices
    private const int PART3_INTRO = 0,
        PART3_MENUOVERLAY = 1,
        PART4_ORDERS = 2,
        PART4_DOCKING = 3,
        PART4_STATIONMENUS = 4;

    private int checkpoint = 0;
    private ScrollTextController currentMenu = null;
    private bool waitingForInput = false;
    private float timer;

    private void Start()
    {
        currentMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText)
                  .GetComponent<ScrollTextController>();

        currentMenu.HeaderText.text = "<b>PART 3</b>: Menu navigation";
        currentMenu.AddMenuItem("While in flight you can access several menus which control different stuff you might need.", false, Color.white);
        currentMenu.AddMenuItem("\n* <b>My Ship</b> menu gives you control and overview of your ship. You can access the <b>autopilot," +
            "cargo status, weapons and info</b> of the ship you are currently flying.", false, Color.white);
        currentMenu.AddMenuItem("\n* <b>Current Target</b> menu gives you information on the selected target, be it a ship, a station, " +
            "an asteroid or something else.", false, Color.white);
        currentMenu.AddMenuItem("\n* <b>Navigation</b> lets you access the <b>Sector</b> and <b>Universe</b> star charts.", false, Color.white);
        currentMenu.AddMenuItem("\n* <b>Player Info</b> displays your statistics and relations to different factions, as well as your property" +
            " and account.", false, Color.white);
        currentMenu.AddMenuItem("\nProceed by pressing <b>Return (Enter)</b> to open the InGame Menu. You will command" +
            "your ship to <b>Attack Enemies</b> via the <b>My Ship > Commands</b>", false, Color.white);

        waitingForInput = false;
    }

    private void Update()
    {
        if (timer > 0)
            timer -= Time.deltaTime;


        switch (checkpoint)
        {
            case PART3_INTRO:
                if (currentMenu == null)
                {
                    if (!waitingForInput)
                    {
                        waitingForInput = true;
                        TextFlash.ShowYellowText("Press <b>Return (Enter)</b> to open the InGame Menu!");
                        ConsoleOutput.PostMessage("Press <b>Return (Enter)</b> to open the InGame Menu!", Color.yellow);
                    }
                    else
                    {
                        if (Input.GetKeyDown(KeyCode.Return))
                            ReportCheckPointAchieved();
                    }
                }
                break;
            case PART3_MENUOVERLAY:
                if (menuOverlay == null)
                {
                    if (!waitingForInput)
                    {
                        TextFlash.ShowYellowText("Go to My Ship > Commands > Idle");
                        ConsoleOutput.PostMessage("Go to My Ship > Commands > Idle", Color.yellow);
                        waitingForInput = true;
                    }
                    else
                    {
                        if (Ship.PlayerShip.AIInput.CurrentOrder != null && Ship.PlayerShip.AIInput.CurrentOrder.Name == "Idle")
                        {
                            ReportCheckPointAchieved();
                        }
                    }
                }
                break;
            case PART4_ORDERS:
                if (menuOverlay == null)
                {
                    ReportCheckPointAchieved();
                }
                break;
            case PART4_DOCKING:
                if (waitingForInput)
                {
                    if (Ship.PlayerShip.StationDocked != "none")
                        ReportCheckPointAchieved();
                }
                break;
        }

    }

    /// <summary>
    /// Invoked by components which determine that a mission checkpoint has been
    /// reached (ie. menu choice confirmed, target destroyed, action performed...)
    /// </summary>
    public void ReportCheckPointAchieved()
    {
        checkpoint++;

        switch (checkpoint)
        {
            case PART3_MENUOVERLAY:
                menuOverlay = CanvasController.Instance.OpenMenu(MenuOverlayPrefab);
                waitingForInput = false;
                break;
            case PART4_ORDERS:
                currentMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText)
                  .GetComponent<ScrollTextController>();

                currentMenu.HeaderText.text = "<b>PART 4</b>: Ship autopilot";
                currentMenu.AddMenuItem("Your ships can be ordered to perform certain actions. These are " +
                    "\n* <b>Move To position/object</b>" +
                    "\n* <b>Follow a ship</b>" +
                    "\n* <b>Patrol the sector</b>" +
                    "\n* <b>Attack all enemies</b>" +
                    "\n* <b>Attack specific target</b>" +
                    "\n* <b>Dock at a station</b>" +
                    "\n* <b>Idle</b>", false, Color.white);
                currentMenu.AddMenuItem("If your ship has turrets, you can issue orders to the turrets. These are " +
                    "\n* <b>None</b> - Do nothing" +
                    "\n* <b>Attack Target</b> - Engage a specific target if possible" +
                    "\n* <b>Attack All</b> - Engage closest target" +
                    "\n* <b>Manual</b> - Allows your to manually control and fire turrets (recommendably use the Orbit View)",
                    false, Color.white);
                currentMenu.AddMenuItem("\nYou can turn on or off the autopilot on your current ship by pressing <b>Shift+A</b>." +
                    "The order given will depend on the selected target.", false, Color.white);
                currentMenu.AddMenuItem("\nNow we are going to dock to a station, using the autopilot." +
                    "Manual docking is also possible by requesting docking on the Target menu of the ingame menu.", false, Color.white);
                currentMenu.AddMenuItem("\n\nSelect the <b>Dock At</b> order and on the Sector Map select the <b>Station</b> to initiate" +
                    " autopilot docking (My Ship > Commands > Dock At).", false, Color.yellow);
                break;
            case PART4_DOCKING:
                TextFlash.ShowYellowText("Go to My Ship > Commands > Dock At and select the Station");
                ConsoleOutput.PostMessage("Go to My Ship > Commands > Dock At and select the Station", Color.yellow);
                waitingForInput = true;
                break;
            case PART4_STATIONMENUS:
                currentMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText)
                  .GetComponent<ScrollTextController>();

                currentMenu.HeaderText.text = "<b>PART 5</b>: Station menus";
                currentMenu.AddMenuItem("Once docked to the station you can interact with it to change the state" +
                    "of your ship. Here are the available station menus:" +
                    "\n* <b>Show docked ships</b> - Displays ships docked to this station and enables interaction with them" +
                    "\n* <b>Trade</b> - Enables loading and unloading of goods into and from your ship's cargo bay" +
                    "\n* <b>Equipment</b> - Allows you to install and uninstall ship equipment like weapons and upgrades" +
                    "\n* <b>Showroom</b> - This is where you purchase new ships. Once a ship is purchased, it will be docked to the station" +
                    "\n* <b>Info</b> - Displays information on this station, including commodity prices and available equipment" +
                    "\n* <b>Jobs</b> - The job board is where you can accept missions and tasks", false, Color.white);
                currentMenu.AddMenuItem("\nTake time to explore the options here. You have completed the tutorial successfully." +
                    "Go out there and have fun.", false, Color.white);
                break;
        }
    }
}
}