using System;
using UnityEngine;

[Serializable]
public class ShopItem
{
    public string itemName = "Item";
    public Sprite itemIcon;
    public CurrencyType costType = CurrencyType.COIN;
    public int price = 10;
    public int levelCost = 1;
}
