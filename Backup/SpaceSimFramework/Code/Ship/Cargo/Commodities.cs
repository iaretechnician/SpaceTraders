using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "DataHolders/Commodities")]
public class Commodities: SingletonScriptableObject<Commodities> {

    public List<WareType> CommodityTypes;
    public int NumberOfWares {
        get {
            if (_numberOfWares == 0)
                _numberOfWares = CommodityTypes.Count;

            return _numberOfWares;
        }
    }
    private int _numberOfWares;

    [Serializable]
    public class WareType
    {
        public string Name;
        public int MinPrice, MaxPrice;

        public WareType(string name, int min, int max)
        {
            Name = name;
            MinPrice = min;
            MaxPrice = max;
        }
    }

    /// <summary>
    /// Finds a ware by its name and returns it
    /// </summary>
    /// <param name="item">Ware name</param>
    /// <returns>Ware if found, null otherwise</returns>
    public WareType GetWareByName(string item)
    {
        foreach (WareType ware in CommodityTypes)
        {
            if (ware.Name == item)
                return ware;
        }

        return null;
    }

    /// <summary>
    /// Return a number between 0 and 1, 0 being expensive, 1 being cheap
    /// </summary>
    public float GetWareBuyRating(string wareName, float warePrice)
    {
        WareType cargo = GetWareByName(wareName);
        return 1f - (warePrice - cargo.MinPrice) / (cargo.MaxPrice - cargo.MinPrice);
    }

    /// <summary>
    /// Return a number between 0 and 1, 0 being cheap, 1 being expensive
    /// </summary>
    public float GetWareSellRating(string wareName, float warePrice)
    {
        WareType cargo = GetWareByName(wareName);
        return (warePrice - cargo.MinPrice) / (cargo.MaxPrice - cargo.MinPrice);
    }

    /// <summary>
    /// Return a color between red and green, red being expensive, green being cheap
    /// </summary>
    public Color GetWareBuyColor(string wareName, float warePrice)
    {
        float rating = (GetWareBuyRating(wareName, warePrice)-0.5f)*2f;
        float absRating = Mathf.Abs(rating);

        return new Color(
            rating < 0 ? 1 : absRating,
            rating > 0 ? 1 : absRating,
            absRating
        );
    }

    /// <summary>
    /// Return a color between red and green, red being cheap, green being expensive
    /// </summary>
    public Color GetWareSellColor(string wareName, float warePrice)
    {
        float rating = (GetWareSellRating(wareName, warePrice) - 0.5f) * 2f;
        float absRating = Mathf.Abs(rating);

        return new Color(
            rating < 0 ? 1 : absRating,
            rating > 0 ? 1 : absRating,
            absRating
        );
    }
}