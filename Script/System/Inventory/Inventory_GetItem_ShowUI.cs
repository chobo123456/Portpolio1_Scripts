using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Inventory_GetItem_ShowUI : MonoBehaviour
{
    private Image itemIcon, itemTierImage, backGround;
    private TextMeshProUGUI itemName;
    public bool IsShowing {get; private set;} = false;

    private RectTransform myRect;

    private bool isInitialized = false, isInLoop = false;

    private void Initialize()
    {
        myRect          = transform.GetComponent<RectTransform>();
        myRect.offsetMin = new Vector2(-25f, myRect.offsetMin.y);
        myRect.offsetMax = new Vector2(25f, myRect.offsetMax.y);

        backGround      = transform.FindTarget("BackGround").GetComponent<Image>();
        itemIcon        = transform.FindTarget("GetItemSprite").GetComponent<Image>();
        itemTierImage   = transform.FindTarget("GetItemTierColor").GetComponent<Image>();
        itemName        = transform.Find("GetItemName").GetComponent<TextMeshProUGUI>();

        isInitialized = true;
    }

    public void SetItemInfo(ItemData data, int itemAmount)
    {
        if(!isInitialized)  
            Initialize();

        IsShowing = true;

        itemIcon.sprite = data.itemInfo.itemIcon;
        itemName.SetText($"{data.itemInfo.item_Name} x {itemAmount}");
        itemTierImage.color = GetColor(data.itemTier);
    }
    
    public void SetEnable()
    {
        if(isInLoop) return;
           
        this.gameObject.SetActive(true);
        this.RunRoutine(Waitting());
    }

    private Color GetColor(ItemTier tier)
    {
        switch(tier)
        {
            case ItemTier.Legendary :
                return Color.orange;
            case ItemTier.Epic :
                return  Color.purple;
            case ItemTier.Rare :
                return  Color.cyan;
            case ItemTier.Common :
                return  Color.gray;
            default:
                return  Color.gray;
        }
    }

    private IEnumerator Waitting()
    {
        isInLoop = true;

        yield return YieldUtil.WaitForSeconds(1f);

        IsShowing = false;
        
        isInLoop = false;

        EventBus.Invoke("RemoveItemGetShowUI");
    }

    public void SetPosition(float posX, float posY)
    {
        myRect.anchoredPosition = new Vector2(posX, posY);
    }

    public Vector2 GetAnchorPosition() => myRect.anchoredPosition;
}
