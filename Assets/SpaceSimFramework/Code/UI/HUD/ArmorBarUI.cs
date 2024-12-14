using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public class ArmorBarUI : MonoBehaviour {

    private Text armorText; 

    void Start()
    {
        armorText = GetComponentInChildren<Text>();
    }

    void Update () {
        float value = Ship.PlayerShip.Armor / (float)Ship.PlayerShip.MaxArmor * 100f;
        armorText.text = "HULL \n" + (int)Ship.PlayerShip.Armor + "/" + (int)Ship.PlayerShip.MaxArmor;

    }

}
}