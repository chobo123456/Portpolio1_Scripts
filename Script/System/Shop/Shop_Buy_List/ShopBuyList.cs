using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ShopBuyList", menuName = "Shop/ShopBuyList")]
public class ShopBuyList : ScriptableObject
{
    public List<ShopBuyItemInfo> list;
}


[System.Serializable]
public struct ShopBuyItemInfo
{
    public int itemId;
    public ItemData data;
    public int itemAmount;
}
