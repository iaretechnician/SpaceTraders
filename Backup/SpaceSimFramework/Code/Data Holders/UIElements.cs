using UnityEngine;

/// <summary>
/// Class containing prefab references
/// </summary>
[CreateAssetMenu(menuName = "DataHolders/UIElements")]
public class UIElements : SingletonScriptableObject<UIElements>{

    public GameObject FlashingText;

    [Header("Menus")]
    public GameObject ScrollMenu;
    public GameObject ScrollText;
    public GameObject SimpleMenu;
    
    [Header("Game Menus")]
    public GameObject UniverseMap;
    public GameObject TargetMenu;
    public GameObject StationMainMenu;
    public GameObject StationTradeMenu;
    public GameObject StationEquipmentMenu;
    public GameObject StationDealershipMenu;

    [Header("Dialogs")]
    public GameObject SliderDialog;
    public GameObject InputDialog;
    public GameObject ConfirmDialog;

    [Header("Elements")]
    public GameObject ClickableText;
    public GameObject ClickableTextChoice;
    public GameObject ClickableImageText;
    public GameObject TextPanel;
    public GameObject TwoTextPanel;


}
