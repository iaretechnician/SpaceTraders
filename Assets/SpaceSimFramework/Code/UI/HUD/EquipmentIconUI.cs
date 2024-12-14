using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
/// <summary>
/// Handles the UI icons for equipment mounted on the player ship
/// </summary>
public class EquipmentIconUI : Singleton<EquipmentIconUI>
{

    public Image[] EquipmentSlots;
    public GameObject[] HotkeyTexts;
    public Transform[] ActiveIndicators;

    private Equipment[] setItems;
    private bool[] itemActive;

    private void Update()
    {
        // Listen for item activation
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleItem(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ToggleItem(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ToggleItem(2);
        }
    }

    /// <summary>
    /// Enables the icon on the HUD with the appropriate image.
    /// </summary>
    /// <param name="iconIndex">Index of the equipment slot</param>
    private void SetIcon(int iconIndex, Equipment item)
    {
        if(setItems == null)
        {
            setItems = new Equipment[3];
            itemActive = new bool[3];
        }

        EquipmentSlots[iconIndex].sprite = IconManager.Instance.GetEquipmentIcon(item.name);
        if (item.isActivateable)
            HotkeyTexts[iconIndex].SetActive(true);
        EquipmentSlots[iconIndex].gameObject.SetActive(true);

        setItems[iconIndex] = item;
    }

    /// <summary>
    /// Hides the icon on the HUD.
    /// </summary>
    private void RemoveIcon(int iconIndex)
    {
        setItems = new Equipment[3];
        itemActive = new bool[3];

        EquipmentSlots[iconIndex].gameObject.SetActive(false);
    }

    // Mark the item as activated
    private void ToggleItem(int iconIndex)
    {
        if (setItems.Length >= iconIndex && setItems[iconIndex] != null && setItems[iconIndex].isActivateable)
        {
            // Try activating the item (item may not allow it)
            bool success = ((ActivatableEquipment)setItems[iconIndex]).SetActive(!itemActive[iconIndex], Ship.PlayerShip);
            if (!success)
                return;

            ActiveIndicators[iconIndex].gameObject.SetActive(!itemActive[iconIndex]);
            itemActive[iconIndex] = !itemActive[iconIndex];
        }
    }

    // Unmark the item as activated
    private void DeactivateItem(int iconIndex)
    {
        ActiveIndicators[iconIndex].gameObject.SetActive(false);
    }

    /// <summary>
    /// Get playership's equipment and populate the UI icons.
    /// </summary>
    public void SetIconsForShip(Ship newShip)
    {
        for (int i = 0; i < 3; i++)
        {
            RemoveIcon(i);
            ActiveIndicators[i].gameObject.SetActive(false);
            HotkeyTexts[i].SetActive(false);
        }

        for (int i = 0; i < newShip.Equipment.MountedEquipment.Count; i++)
        {
            SetIcon(i, newShip.Equipment.MountedEquipment[i]);
            if (newShip.Equipment.MountedEquipment[i].isActivateable)
            {
                bool isActive = ((ActivatableEquipment)newShip.Equipment.MountedEquipment[i]).IsActive();
                ActiveIndicators[i].gameObject.SetActive(isActive);
                HotkeyTexts[i].SetActive(true);
            }
        }
    }
}
}