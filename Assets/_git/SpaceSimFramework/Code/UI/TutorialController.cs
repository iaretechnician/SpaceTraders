using UnityEngine;
using System.Collections;

namespace SpaceSimFramework { 
    public class TutorialController : MonoBehaviour
    {
        public GameObject MovementWaypoint;
        public GameObject[] Targets;

        // Chapter indices
        private const int WELCOME_SCREEN = 0,
            PART1_ACCELERATE = 1,
            PART1_STEERING = 2,
            PART1_MOUSEFLIGHT = 3,
            PART1_MOVETOLOCATION = 4,
            PART1_EPILOGUE = 5,
            PART2_WELCOME = 6,
            PART2_SELECTTARGET = 7,
            PART2_DESTROYTARGET = 8,
            PART2_DESTROYSHIPS = 9,
            PART2_EPILOGUE = 10;

        private int checkpoint = 0;
        private ScrollTextController currentMenu = null;
        private bool waitingForInput = false;
        private float timer;

        private void Start()
        {
            var welcomeScreen = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText)
                .GetComponent<ScrollTextController>();

            welcomeScreen.HeaderText.text = "TUTORIAL";
            welcomeScreen.AddMenuItem("Welcome to the Tutorial Mission!", true, Color.white);
            welcomeScreen.AddMenuItem("\n\n", true, Color.white);
            welcomeScreen.AddMenuItem("* <b>Part 1</b>: Basic movement", false, Color.white);
            welcomeScreen.AddMenuItem("* <b>Part 2</b>: Combat", false, Color.white);
            welcomeScreen.AddMenuItem("* <b>Part 3</b>: Menu Navigation", false, Color.white);
            welcomeScreen.AddMenuItem("* <b>Part 4</b>: Autopilot", false, Color.white);
            welcomeScreen.AddMenuItem("* <b>Part 5</b>: Stations", false, Color.white);

            welcomeScreen.AddMenuItem("\nTo view the controls press the <b>F1</b> " +
                "key at any point in this tutorial", false, Color.white);

            currentMenu = welcomeScreen;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                OpenHelpMenu();
            }

            if (timer > 0)
                timer -= Time.deltaTime;


            switch (checkpoint)
            {
                case WELCOME_SCREEN:
                    if (currentMenu == null)
                    {
                        ReportCheckPointAchieved();
                    }
                    break;
                case PART1_ACCELERATE:
                    if (currentMenu == null)
                    {
                        if (!waitingForInput)
                        {
                            TextFlash.ShowYellowText("Hold the R key to accelerate!");
                            ConsoleOutput.PostMessage("Hold the R key to accelerate!", Color.yellow);
                            waitingForInput = true;
                        }
                        else
                        {
                            if (Ship.PlayerShip.Throttle == 1.0f)
                            {
                                waitingForInput = false;
                                ReportCheckPointAchieved();
                            }
                        }
                    }
                    break;
                case PART1_STEERING:
                    if (timer <= 0)
                    {
                        waitingForInput = false;
                        ReportCheckPointAchieved();
                    }
                    break;
                case PART1_MOUSEFLIGHT:
                    if (currentMenu == null)
                    {
                        if (!waitingForInput)
                        {
                            TextFlash.ShowYellowText("Press the Space key to enable Mouse Flight");
                            ConsoleOutput.PostMessage("Press the Space key to enable Mouse Flight", Color.yellow);
                            waitingForInput = true;
                        }
                        else
                        {
                            if (Input.GetKeyDown(KeyCode.Space))
                            {
                                waitingForInput = false;
                                ReportCheckPointAchieved();
                            }
                        }
                    }
                    break;
                case PART1_MOVETOLOCATION:
                    if (timer <= 0 && waitingForInput)
                    {
                        waitingForInput = false;
                        InputHandler.Instance.SelectedObject = MovementWaypoint;
                        TextFlash.ShowYellowText("Move towards the selected target marked in purple");
                        ConsoleOutput.PostMessage("Move towards the selected target marked in purple", Color.yellow);
                    }
                    else
                    {
                        if (Vector3.Distance(Ship.PlayerShip.transform.position, MovementWaypoint.transform.position) < 10f)
                        {
                            TextFlash.ShowYellowText("Excellent work. Part 1 completed.");
                            ConsoleOutput.PostMessage("Excellent work. Part 1 completed.", Color.yellow);
                            ReportCheckPointAchieved();
                        }
                    }
                    break;
                case PART1_EPILOGUE:
                    if (currentMenu == null)
                    {
                        ReportCheckPointAchieved();
                    }
                    break;
                case PART2_WELCOME:
                    if (currentMenu == null)
                    {
                        if (!waitingForInput)
                        {
                            TextFlash.ShowYellowText("Hold Ctrl or Right Mouse Button!");
                            ConsoleOutput.PostMessage("Hold Ctrl or Right Mouse Button!", Color.yellow);
                            // Starting ship is a turreted ship, switch guns to manual control:
                            Ship.PlayerShip.Equipment.SetTurretCommand(TurretCommands.TurretOrder.Manual);
                            waitingForInput = true;
                        }
                        else
                        {
                            if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetMouseButtonUp(1))
                            {
                                waitingForInput = false;
                                ReportCheckPointAchieved();
                            }
                        }
                    }
                    break;
                case PART2_SELECTTARGET:
                    if (InputHandler.Instance.GetCurrentSelectedTarget() == Targets[0])
                    {
                        ReportCheckPointAchieved();
                    }
                    break;
                case PART2_DESTROYTARGET:
                    if (Targets[0] == null)
                    {
                        ReportCheckPointAchieved();
                    }
                    break;
                case PART2_DESTROYSHIPS:
                    if (currentMenu == null)
                    {
                        if (!waitingForInput)
                        {
                            waitingForInput = true;
                            // Activate target ships
                            Targets[1].SetActive(true);
                            Targets[2].SetActive(true);
                            Targets[1].GetComponent<ShipAI>().PatrolPath(SectorNavigation.Instance.GetPatrolWaypoints());
                            Targets[2].GetComponent<ShipAI>().PatrolPath(SectorNavigation.Instance.GetPatrolWaypoints());
                            TextFlash.ShowYellowText("Destroy the two enemy fighters in the sector");
                        }
                        else if (Targets[1] == null && Targets[2] == null)
                        {
                            ReportCheckPointAchieved();
                        }
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
                case PART1_ACCELERATE:
                    // Open Section 1: Basic movement
                    var section1 = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText)
                        .GetComponent<ScrollTextController>();

                    section1.HeaderText.text = "<b>PART 1</b>: Basic movement";
                    section1.AddMenuItem("Your ship has two movement modes: mouse follow mode and keyboard control mode. To switch between the modes," +
                        "press Space or click the flight mode indicator on the bottom center of the HUD.", false, Color.white);
                    section1.AddMenuItem("\n\n", true, Color.white);
                    section1.AddMenuItem("Currently you are in the keyboard flight mode which means the mouse can be used to navigate" +
                        " the user interface. You can use the A and D keys to turn the ship sideways," +
                        " the W and S keys to pitch down and up and the Q and E keys to roll around the forward axis." +
                        " Let's begin by accelerating to full speed. <b>Use the R key to accelerate</b>," +
                        " and the F key to reduce throttle.", false, Color.white);

                    currentMenu = section1;
                    break;
                case PART1_STEERING:
                    TextFlash.ShowYellowText("Steer the ship with the W,A,S,D keys");
                    ConsoleOutput.PostMessage("Steer the ship with the W,A,S,D keys", Color.yellow);
                    // 20 seconds for trying keyboard steering
                    timer = 5f;
                    waitingForInput = true;
                    break;
                case PART1_MOUSEFLIGHT:
                    currentMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText)
                       .GetComponent<ScrollTextController>();

                    currentMenu.HeaderText.text = "<b>PART 1</b>: Basic movement - Mouse flight";
                    currentMenu.AddMenuItem("Keyboard flight mode is useful for flying larger ships while controlling their turrets." +
                        "For fighters, you might be more successful using the mouse control mode.", false, Color.white);
                    currentMenu.AddMenuItem("\nWhile mouse flight is engaged, you can also strafe your ship which allows for efficient dodging." +
                        "\n*Control throttle with the W and S keys \n*Strafe with A and D \n* Roll with Q and E.", false, Color.white);
                    currentMenu.AddMenuItem("\n Proceed by pressing <b>Space</b> to engage the mouse flight mode.", false, Color.white);

                    waitingForInput = false;
                    break;
                case PART1_MOVETOLOCATION:
                    waitingForInput = true;
                    // 15 seconds for trying out mouse flight
                    timer = 5f;
                    break;
                case PART1_EPILOGUE:
                    currentMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText)
                       .GetComponent<ScrollTextController>();

                    currentMenu.HeaderText.text = "<b>PART 1</b> Completed!";
                    currentMenu.AddMenuItem("Your ship also has some advanced movement methods such as the <b>ENGINE KILL</b>" +
                        "which allows you to glide through space using inertia and <b>SUPERCRUISE</b> which allows you to " +
                        "redirect all energy into propulsion and achieve greater speeds at the cost of weapon control.", false, Color.white);
                    currentMenu.AddMenuItem(" - To toggle your engines and use <b>ENGINE KILL</b> hit the <b>Z</b> key" +
                        "\n - To toggle <b>SUPERCRUISE</b> press <b>Shift+W</b>", false, Color.white);
                    currentMenu.AddMenuItem("\n\nClose this menu to proceed to part two - combat! Pewpew!", false, Color.white);
                    break;
                case PART2_WELCOME:
                    currentMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText)
                       .GetComponent<ScrollTextController>();

                    currentMenu.HeaderText.text = "<b>PART 2</b> Combat!";
                    currentMenu.AddMenuItem("Select a target by left-clicking on it or pressing <b>T</b> while it's under" +
                        "the crosshair. To fire your forward firing guns, hold <b>Ctrl</b> or the <b>Right Mouse Button</b>.", false, Color.white);
                    currentMenu.AddMenuItem("Some ships have guns and some have turrets. Guns are under your control by default but" +
                        "turrets will, by default, engage enemies. This can be changed in the ship menu," +
                        " which will be discussed later.", false, Color.white);
                    currentMenu.AddMenuItem("\n\nProceed by firing the weapon systems, either by holding <b>Control</b> or " +
                        "the <b>Right Mouse Button</b>!", false, Color.white);
                    // Stop the ship
                    Ship.PlayerShip.MovementInput.throttle = 0;   
                    break;
                case PART2_SELECTTARGET:
                    // Spawn fighter in front of player and disable it
                    Targets[0].SetActive(true); 
                    Targets[0].transform.position = Ship.PlayerShip.transform.position + Ship.PlayerShip.transform.forward * 100;
                    Targets[0].GetComponent<ShipAI>().enabled = false;
                    TextFlash.ShowYellowText("Click on the enemy ship in front to select it");
                    ConsoleOutput.PostMessage("Click on the enemy ship in front to select it", Color.yellow);
                    break;
                case PART2_DESTROYTARGET:
                    TextFlash.ShowYellowText("Shoot the ship and destroy it!");
                    ConsoleOutput.PostMessage("Shoot the ship and destroy it!", Color.yellow);
                    break;
                case PART2_DESTROYSHIPS:
                    currentMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText)
                       .GetComponent<ScrollTextController>();

                    currentMenu.HeaderText.text = "<b>PART 2</b> Combat - engage ships";
                    currentMenu.AddMenuItem("Two enemy ships have entered the area. <b>Destroy them.</b>", false, Color.white);
                    currentMenu.AddMenuItem("\nRemember to lead the target by shooting the circular lead indicator" +
                        "in front of the ship.", false, Color.white);
                    currentMenu.AddMenuItem("\nIf you lose sight of the targets, press the M key to bring up the sector map.", false, Color.white);
                    currentMenu.AddMenuItem("\nKeep in mind that actual enemy ships will likely shoot you in real combat.", false, Color.white);
                    break;
                case PART2_EPILOGUE:
                    GetComponent<TutorialController2>().enabled  = true;
                    break;
            }
        }

        private static void OpenHelpMenu()
        {
            ScrollTextController menu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText)
                            .GetComponent<ScrollTextController>();

            menu.HeaderText.text = "Game controls";
            menu.AddMenuItem("Space - switch between mouse and keyboard flight modes", false, Color.white);
            menu.AddMenuItem("<b>Mouse flight mode:</b>\nW,S - Accelerate,Decelerate\n" +
                "A,D - Strafe left and right", false, Color.white);
            menu.AddMenuItem("<b>Keyboard flight mode:</b>\nW,S - Pitch up and down\n" +
                "A,D - Yaw left and right\nR,F - Accelerate and decelerate", false, Color.white);
            menu.AddMenuItem("Q/E - roll left/right", false, Color.white);
            menu.AddMenuItem("Mousewheel - throttle control", false, Color.white);
            menu.AddMenuItem("Shift+A - toggle autopilot (depends on selected target)", false, Color.white);
            menu.AddMenuItem("Shift+C - request docking at Station", false, Color.white);
            menu.AddMenuItem("Shift+W - toggle Supercruise", false, Color.white);
            menu.AddMenuItem("H - toggle orbit camera", false, Color.white);
            menu.AddMenuItem("Z - toggle main engines", false, Color.white);
            menu.AddMenuItem("Esc - cancel current menu", false, Color.white);
            menu.AddMenuItem("Enter - ingame menu", false, Color.white);
            menu.AddMenuItem("T - target object under crosshairs", false, Color.white);
        }
    }  
}