using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public class ScrollTextController : MonoBehaviour
{
    // Reference to scroll content used for adding objects
    public GameObject ScrollContentContainer;
    public Scrollbar ScrollBar;
    public Text HeaderText;

    private RectTransform textContainer;
    private float keyPressTimer;
    private bool fastScrollDown, fastScrollUp;

    public void Awake()
    {
        textContainer = ScrollContentContainer.GetComponent<RectTransform>();
    }

    protected void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ScrollBar.value -= 0.1f * ScrollBar.size;

            keyPressTimer = 0.5f;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            keyPressTimer -= Time.deltaTime;
            if (keyPressTimer < 0 && !fastScrollDown)
            {
                fastScrollDown = true;
                keyPressTimer = 0.05f;
            }
            if (keyPressTimer < 0 && fastScrollDown)
            {
                ScrollBar.value -= 0.1f * ScrollBar.size;
                keyPressTimer = 0.05f;
            }
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            fastScrollDown = false;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ScrollBar.value += 0.1f * ScrollBar.size;

            keyPressTimer = 0.5f;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            keyPressTimer -= Time.deltaTime;
            if (keyPressTimer < 0 && !fastScrollUp)
            {
                fastScrollUp = true;
                keyPressTimer = 0.05f;
            }
            if (keyPressTimer < 0 && fastScrollUp)
            {

                ScrollBar.value += 0.1f * ScrollBar.size;

                keyPressTimer = 0.05f;
            }
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            fastScrollUp = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {            
            EventManager.OnCloseClicked(System.EventArgs.Empty, gameObject);
        }
    }

    /// <summary>
    /// This method is invoked by the element that populates this menu. The method 
    /// allows a text item to be added with optional bolding and color choice.
    /// </summary>
    /// <param name="text">Title/text of the menu option</param>
    /// <returns>OnClick event handler to be used by the invoker</returns>
    public void AddMenuItem(string text, bool isBold, Color color)
    {
        GameObject listItem = Instantiate(UIElements.Instance.TextPanel);
        listItem.name = text;

        RectTransform rt = listItem.GetComponent<RectTransform>();
        rt.SetParent(textContainer.transform);
        rt.sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, rt.rect.height);

        Text t = listItem.GetComponentInChildren<Text>();
        t.text = text;

        if(isBold)
            t.fontStyle = FontStyle.Bold;

        t.color = color;
    }

    /// <summary>
    /// This method is invoked by the element that populates this menu. The method 
    /// allows a text item to be added with optional bolding and color choice. This text item
    /// used two text fields, one on the left and one on the right of the panel.
    /// </summary>
    /// <param name="text">Title/text of the menu option</param>
    /// <returns>OnClick event handler to be used by the invoker</returns>
    public void AddMenuItem(string text1, string text2, bool isBold, Color color)
    {
        GameObject listItem = Instantiate(UIElements.Instance.TwoTextPanel);
        listItem.name = text1;

        RectTransform rt = listItem.GetComponent<RectTransform>();
        rt.SetParent(textContainer.transform);
        rt.sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, rt.rect.height);

        Text[] t = listItem.GetComponentsInChildren<Text>();
        t[0].text = text1;
        t[1].text = text2;

        if (isBold) { 
            t[0].fontStyle = FontStyle.Bold;
            t[1].fontStyle = FontStyle.Bold;
        }

        t[0].color = color;
        t[1].color = color;
    }

    /// <summary>
    /// This method is invoked by the element that populates this menu. The method 
    /// returns an onclick handler which can be used by the invoker to process the 
    /// option click/selection events.
    /// </summary>
    /// <param name="text">Title/text of the menu option</param>
    /// <param name="color">Color of the item in the list</param>
    /// <param name="icon">Icon of the item</param>
    /// <param name="iconAspect">Aspect ratio of the icon image</param>
    /// <returns>OnClick event handler to be used by the invoker</returns>
    public void AddMenuItem(string text, Color color, Sprite icon, float iconAspect = 1, int itemHeight = 40)
    {
        GameObject listItem = Instantiate(UIElements.Instance.ClickableImageText);
        listItem.name = text;

        RectTransform rt = listItem.GetComponent<RectTransform>();
        rt.SetParent(textContainer.transform);

        var le = listItem.GetComponent<LayoutElement>();
        le.minHeight = le.preferredHeight = itemHeight;

        NavigationListItem nli = listItem.GetComponent<NavigationListItem>();
        nli.SetText(text);
        nli.Icon.color = color;
        nli.Icon.sprite = icon;
        nli.Icon.GetComponent<AspectRatioFitter>().aspectRatio = iconAspect;
        nli.OwnerMenu = this.gameObject;
    }

    public void ClearItems()
    {
        if(textContainer == null)
            textContainer = ScrollContentContainer.GetComponent<RectTransform>();

        for (int i = 0; i < textContainer.childCount; i++)
            GameObject.Destroy(textContainer.GetChild(i).gameObject);
    }


    // EXAMPLE only
    private void PopulateMenuExample()
    {
        HeaderText.text = "Title";

        // Clickable Text
        string[] options = { "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                "Nunc rutrum, felis et lobortis fringilla, mauris odio dapibus urna, non vulputate" +
                " neque diam quis lorem. Duis vehicula non justo a maximus. Nullam quis urna eu libero" +
                " facilisis vulputate. Morbi iaculis erat in magna dapibus, sed dictum tellus lacinia. ",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " ,
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                "Nunc rutrum, felis et lobortis fringilla, mauris odio dapibus urna, non vulputate" +
                " neque diam quis lorem. Duis vehicula non justo a maximus. Nullam quis urna eu libero" +
                " facilisis vulputate. Morbi iaculis erat in magna dapibus, sed dictum tellus lacinia. ",
        "Ut volutpat mollis massa, id mattis ligula tincidunt eu. Mauris interdum magna mi," +
        " eu luctus justo consequat at. Integer rhoncus quis velit sit amet interdum. " +
        "Donec vitae dui nec velit interdum iaculis sit amet a augue. Pellentesque at dui quis mi fermentum viverra eget in lacus." +
        " Etiam aliquam ipsum vitae scelerisque sollicitudin. Aliquam erat volutpat. ",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                "Nunc rutrum, felis et lobortis fringilla, mauris odio dapibus urna, non vulputate" +
                " neque diam quis lorem. Duis vehicula non justo a maximus. Nullam quis urna eu libero" +
                " facilisis vulputate. Morbi iaculis erat in magna dapibus, sed dictum tellus lacinia. ",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " ,
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                "Nunc rutrum, felis et lobortis fringilla, mauris odio dapibus urna, non vulputate" +
                " neque diam quis lorem. Duis vehicula non justo a maximus. Nullam quis urna eu libero" +
                " facilisis vulputate. Morbi iaculis erat in magna dapibus, sed dictum tellus lacinia. ",
        "Ut volutpat mollis massa, id mattis ligula tincidunt eu. Mauris interdum magna mi," +
        " eu luctus justo consequat at. Integer rhoncus quis velit sit amet interdum. " +
        "Donec vitae dui nec velit interdum iaculis sit amet a augue. Pellentesque at dui quis mi fermentum viverra eget in lacus." +
        " Etiam aliquam ipsum vitae scelerisque sollicitudin. Aliquam erat volutpat. "};

        AddMenuItem(options[1], false, Color.yellow);
        for (int index = 0; index < options.Length*20; ++index)
        {
            AddMenuItem(options[index%options.Length], false, Color.white);
        }
        
    }

    public void OnCloseClicked()
    {
        EventManager.OnCloseClicked(System.EventArgs.Empty, gameObject);
    }
}
}