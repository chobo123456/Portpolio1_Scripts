using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ItemInteractUI : MonoBehaviour
{
    private RectTransform myRect;
    private Image itemIcon, itemTierColor;
    private TextMeshProUGUI itemName;

    private bool isShow = false, readyToMove = false;
    public void Initialize()
    {
        itemIcon        = transform.Find("itemIcon").GetComponentInChildren<Image>();
        itemTierColor   = transform.Find("TierImage").GetComponentInChildren<Image>();
        itemName        = transform.Find("itemName").GetComponentInChildren<TextMeshProUGUI>();
        
        myRect = GetComponent<RectTransform>();
    }

    public void SetShow(int itemId)
    {
        var itemData = DataLoader.GetData<ItemData>(DataType.Item, itemId);
        isShow = true;

        itemIcon.sprite = itemData.itemInfo.itemIcon;
        itemName.SetText($"{itemData.itemInfo.item_Name}");
        itemTierColor.color = GetTierColor(itemData.itemTier);
    }

    private Color GetTierColor(ItemTier tier)
    {
        switch(tier)
        {
            case ItemTier.Legendary :
                return Color.orange;
            case ItemTier.Epic :
                return Color.purple;
            case ItemTier.Rare :
                return Color.cyan;
            case ItemTier.Common :
                return Color.gray;
            default :
                return Color.gray;
        }
    }

    public void SetUnShow()
    {
        isShow = false;
        readyToMove = true;
    }

    public void SetRectPosition(float rectX, float rectY)
    {
        myRect.anchoredPosition = new Vector2(rectX, rectY);
    }

    
    public RectTransform GetRect() => myRect;
    
    public bool IsReadyToMove() 
    {
        bool isReady = readyToMove;
        readyToMove = false;

        return isReady;
    }

    public bool IsAlreadyShow() => isShow;
}
