using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public class NavigationListItem : ClickableText, IPointerExitHandler
{
    public Color NormalColor, HoverColor;

    public Image Icon;
    private Image background;

    protected new void Awake()
    {
        background = GetComponent<Image>();
        text = GetComponentInChildren<Text>();
        button = GetComponent<Button>();
    }

    private void Start()
    {
        background.color = NormalColor;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        background.color = HoverColor;
        background.transform.localScale = Vector3.one*1.05f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        background.color = NormalColor;
        background.transform.localScale = Vector3.one;
    }

}
}