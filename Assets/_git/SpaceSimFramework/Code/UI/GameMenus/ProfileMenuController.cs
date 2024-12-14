using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;

namespace SpaceSimFramework
{
public class ProfileMenuController : MonoBehaviour {

    public static string PLAYER_PROFILE = "undefined";

    public GameObject[] MenuItems;
    private Text[] menuTextComponents;

    public GameObject DialogCanvas;
    public UIElements UIElements;

    public AudioClip ScrollSound, ConfirmSound;
    private AudioSource audioSource;

    private int selectedItem = 0;
    private int numberOfProfiles = 0;

    // Camera animation controls
    public AnimationCurve curve;
    public Transform ProfilePosition, MainPosition;
    private float timer;
    private static float CAMERA_SHIFT_TIME = 2f;

    private void Awake()
    {
        // Extract text components
        menuTextComponents = new Text[MenuItems.Length];
        for (int i = 0; i < MenuItems.Length; i++)
        {
            menuTextComponents[i] = MenuItems[i].GetComponentInChildren<Text>();
        }

        // Load existing profiles
        if(!Directory.Exists(Utils.PERSISTANCE_PATH + "Data/Profiles"))
        {
            Directory.CreateDirectory(Utils.PERSISTANCE_PATH + "Data/Profiles");
        }

        var profiles = Directory.GetDirectories(Utils.PERSISTANCE_PATH + "Data/Profiles");
        numberOfProfiles = profiles.Length;
        for (int i = 0; i < numberOfProfiles; i++)
        {
            MenuItems[i].SetActive(true);
            menuTextComponents[i].text = new DirectoryInfo(profiles[i]).Name;
        }
        // Add button for creating a new profile
        MenuItems[profiles.Length].SetActive(true);
        menuTextComponents[profiles.Length].text = "Create new profile";

        EventManager.PointerEntry += OnPointerEntry;

        menuTextComponents[selectedItem].color = Color.red;
        MenuItems[selectedItem].transform.localScale = Vector3.one * 1.1f;
    }

    private void OnDestroy()
    {
        EventManager.PointerEntry -= OnPointerEntry;
    }

    private void OnPointerEntry(object sender, EventArgs e)
    {
        ClickableText textComponent;
        if ((textComponent = ((GameObject)sender).GetComponent<ClickableText>()) == null)
            return;

        // Set all (other) items to unselected (white)
        for (int i = 0; i < menuTextComponents.Length; i++)
        {
            menuTextComponents[i].color = Color.white;
            MenuItems[i].transform.localScale = Vector3.one;
        }

        for (int i = 0; i < MenuItems.Length; i++)
        {
            if (MenuItems[i] == (GameObject)sender)
                selectedItem = i;
        }

        menuTextComponents[selectedItem].color = Color.red;
        MenuItems[selectedItem].transform.localScale = Vector3.one * 1.1f;
    }

    void Update()
    {
        if (MainMenuController.IS_ACTIVE)
            return;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            for (int i = 0; i < menuTextComponents.Length; i++)
            {
                menuTextComponents[i].color = Color.white;
                MenuItems[i].transform.localScale = Vector3.one;
            }

            if (selectedItem + 1 < MenuItems.Length)
                selectedItem++;
            menuTextComponents[selectedItem].GetComponent<Text>().color = Color.red;
            MenuItems[selectedItem].transform.localScale = Vector3.one * 1.1f;

            PlaySound(ScrollSound);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            for (int i = 0; i < menuTextComponents.Length; i++)
            {
                menuTextComponents[i].color = Color.white;
                MenuItems[i].transform.localScale = Vector3.one;
            }

            if (selectedItem > 0)
                selectedItem--;
            menuTextComponents[selectedItem].GetComponent<Text>().color = Color.red;
            MenuItems[selectedItem].transform.localScale = Vector3.one * 1.1f;

            PlaySound(ScrollSound);
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnItemClicked(selectedItem);
        }

    }

    public void OnItemClicked(int selectedItem)
    {
        if (selectedItem < numberOfProfiles) {
            PLAYER_PROFILE = MenuItems[selectedItem].GetComponentInChildren<Text>().text;
            StartCoroutine(MoveToMainMenu());
        }
        else
        {
            CreateNewProfile();
        }
    }

    private void CreateNewProfile()
    {
        // Open Confirm Dialog
        GameObject SubMenu = GameObject.Instantiate(UIElements.InputDialog, DialogCanvas.transform);
        // Reposition submenu
        RectTransform rt = SubMenu.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 0);

        PopupInputMenuController confirmSaleMenu = SubMenu.GetComponent<PopupInputMenuController>();
        confirmSaleMenu.TextInput.contentType = InputField.ContentType.Alphanumeric;
        confirmSaleMenu.HeaderText.text = "Enter profile name";

        confirmSaleMenu.AcceptButton.onClick.AddListener(() => {
            MenuItems[numberOfProfiles].SetActive(true);
            menuTextComponents[numberOfProfiles].text = confirmSaleMenu.TextInput.text;
            numberOfProfiles++;
            if (numberOfProfiles < MenuItems.Length)
                MenuItems[numberOfProfiles].SetActive(true);
            GameObject.Destroy(confirmSaleMenu.gameObject);
        });
        confirmSaleMenu.CancelButton.onClick.AddListener(() => {
            GameObject.Destroy(confirmSaleMenu.gameObject);
        });
    }

    private IEnumerator MoveToMainMenu()
    {
        MainMenuController.IS_ACTIVE = true;
        timer = 0;
        while (timer < CAMERA_SHIFT_TIME) { 
            Camera.main.transform.position = Vector3.Lerp(ProfilePosition.position, MainPosition.position, curve.Evaluate(timer / CAMERA_SHIFT_TIME));
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();
    }

}
}