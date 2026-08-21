using UnityEngine;

public enum ItemType
{
    UnDestroy,
    Destroy,
    Story,    
}

public enum ItemTier
{
    Legendary   = 0,
    Epic        = 1,
    Rare        = 2,
    Common      = 3,
}

[System.Serializable]
public struct ItemInfo
{
    public int itemId;
    public int itemSellAmount;
    public InventoryType item_Type;    
    public Sprite itemIcon;
    public string item_Name;
    public string item_Description;
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Item/Data")]
public class ItemData : ScriptableObject
{
    public ItemInfo itemInfo;
    public ItemType itemType;
    public ItemTier itemTier;
}
