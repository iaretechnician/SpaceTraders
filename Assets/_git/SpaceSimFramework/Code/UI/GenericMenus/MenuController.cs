using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public abstract class MenuController : MonoBehaviour
{

    public Text HeaderText;
    public Transform OptionContainer;

    [HideInInspector]
    public int SelectedOption
    {
        get { return selectedOption; }
    }
    [HideInInspector]
    public int selectedOption = 0;
    protected List<ClickableText> _availableOptions;

    [HideInInspector]
    public GameObject SubMenu;
    [HideInInspector]
    public bool IsSubMenu = false;

    private System.EventHandler _delegateInstance;

    [HideInInspector]
    public bool DisableKeyInput = false;

    protected void Awake()
    {
        _availableOptions = new List<ClickableText>();
    }

    protected void Start()
    {
        _delegateInstance = new System.EventHandler(OnPointerEntry);
        EventManager.PointerEntry += _delegateInstance;
    }

    protected void Update()
    {
        if (_availableOptions.Count > 0 && selectedOption >= _availableOptions.Count)
            selectedOption = _availableOptions.Count-1;
           
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Destroy submenu if one exists
            if (SubMenu != null)
                return;

            _availableOptions[selectedOption].SetColor(Color.white);
            if (selectedOption + 1 < _availableOptions.Count)
                selectedOption++;

            MusicController.Instance.PlaySound(AudioController.Instance.ScrollSound);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (SubMenu != null)
                return;

            _availableOptions[selectedOption].SetColor(Color.white);
            if (selectedOption > 0)
                selectedOption--;

            MusicController.Instance.PlaySound(AudioController.Instance.ScrollSound);
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Submenu should catch enter keypresses
            if (SubMenu == null)
                OnOptionSelected(selectedOption);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsSubMenu)
            {
                GameObject.Destroy(this.gameObject);
                return;
            }
            // Submenu should catch esc keypresses
            if (SubMenu == null)
                EventManager.OnCloseClicked(System.EventArgs.Empty, gameObject);
           
        }

        if (_availableOptions.Count > 0)
            _availableOptions[selectedOption].SetColor(Color.red);
    }

    protected void OnEnable()
    {
        selectedOption = 0;
        EventManager.PointerEntry += _delegateInstance;

        if (_availableOptions.Count == 0)
            return;

        _availableOptions[0].SetColor(Color.red);
        for (int i = 1; i < _availableOptions.Count; i++)
            _availableOptions[i].SetColor(Color.white);
    }

    protected void OnDisable()
    {
        EventManager.PointerEntry -= _delegateInstance;
        GameObject.Destroy(SubMenu);
    }

    public void OnCloseClicked()
    {
        // If submenu is open, let it catch the close
        if (SubMenu != null)
            return;

        if (IsSubMenu)
        {
            GameObject.Destroy(this.gameObject);
            return;
        }

        EventManager.OnCloseClicked(System.EventArgs.Empty, gameObject);
    }

    private void OnPointerEntry(object sender, System.EventArgs e)
    {
        //Debug.Log("1 " + gameObject.name + ": OnPointerEntry of " + ((GameObject)sender).name +
        //    " owned by " + ((EventManager.MenuEventArgs)e).Menu.name);

        if (((GameObject)sender).GetComponent<ClickableText>() == null)
            return;

        ClickableText textComponent;
        if ((textComponent = ((GameObject)sender).GetComponent<ClickableText>()) == null)
            return;

        // Set all (other) items to unselected (white)
        foreach (ClickableText ct in _availableOptions)
        {
            if (ct == null)
                return;
            else
                ct.SetColor(Color.white);
        }


        //Debug.Log("2 " + gameObject.name + ": OnPointerEntry of " + ((GameObject)sender).name +
        //    " owned by " + ((EventManager.MenuEventArgs)e).Menu.name);

        EventManager.MenuEventArgs args = (EventManager.MenuEventArgs)e;
        if (args.Menu != gameObject) {
            DisableKeyInput = true;
            return;
        }
        else
        {
            DisableKeyInput = false;
        }

        if (SubMenu != null)
            return;


        textComponent.SetColor(Color.red);
        selectedOption = _availableOptions.IndexOf(textComponent);

    }

    /// <summary>
    /// This method is invoked by the element that populates this menu. The method 
    /// returns an onclick handler which can be used by the invoker to process the 
    /// option click/selection events.
    /// </summary>
    /// <param name="text">Title/text of the menu option</param>
    /// <returns>OnClick event handler to be used by the invoker</returns>
    public Button.ButtonClickedEvent AddMenuOption(string text)
    {
        GameObject listItem = Instantiate(UIElements.Instance.ClickableText);
        listItem.name = text;

        RectTransform rt = listItem.GetComponent<RectTransform>();
        rt.SetParent(OptionContainer.transform);

        ClickableText t = listItem.GetComponent<ClickableText>();
        t.SetText(text);
        t.OwnerMenu = this.gameObject;

        _availableOptions.Add(t);

        return t.GetButtonOnClick();
    }

    public Button.ButtonClickedEvent AddMenuChoice(string text, string[] options)
    {
        GameObject listItem = Instantiate(UIElements.Instance.ClickableTextChoice);
        listItem.name = text;

        RectTransform rt = listItem.GetComponent<RectTransform>();
        rt.SetParent(OptionContainer.transform);

        ClickableTextChoice t = listItem.GetComponent<ClickableTextChoice>();
        t.SetOptions(options);
        t.SetText(text);
        t.OwnerMenu = this.gameObject;

        _availableOptions.Add(t);

        return t.GetButtonOnClick();
    }

    public void RemoveMenuOption(string text)
    {
        int numOfOptions = _availableOptions.Count;

        for (int i=0; i<_availableOptions.Count; i++)
        {
            if (_availableOptions[i].Text.text == text)
            {
                GameObject.Destroy(_availableOptions[i].gameObject);
                _availableOptions.RemoveAt(i);
                numOfOptions--;
                return;
            }
        }

        if (selectedOption > numOfOptions && numOfOptions > 0)
            selectedOption = numOfOptions - 1;
    }

    public void ClearMenuOptions()
    {
        for (int i = 0; i < _availableOptions.Count; i++)
        {
            GameObject.Destroy(_availableOptions[i].gameObject);           
        }
        _availableOptions.Clear();
        if(selectedOption > 0)
            selectedOption--;
    }


    #region abstract memebers

    /// <summary>
    /// This method contains the functionality of the menu, and performs the 
    /// desired operation depending on which option was selected by the user.
    /// </summary>
    /// <param name="option">Index of the selected option</param>
    abstract protected void OnOptionSelected(int option);

    #endregion abstract memebers
    
}
}