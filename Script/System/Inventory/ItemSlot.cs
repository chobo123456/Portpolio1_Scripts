using UnityEngine;
using UnityEngine.UI;
using TMPro;

//데이터 관리자
public abstract class ISlot : MonoBehaviour
{
    public bool IsItemExist {get; private set;}
    public ItemData Slot_ItemData {get; private set;}
    public int Amount {get; private set;}
    public abstract void Initialize();
    public virtual void GetItem(ItemData itemData, int amount = 0)
    {
        if(itemData != null)
        {
            IsItemExist = true;
            Slot_ItemData = itemData;
            Amount = amount;
        }
        else
        {
            IsItemExist = false;
            Slot_ItemData = null;
            Amount = 0;
        }
    }
}

//Ui표시자 
public class ItemSlot : ISlot
{
    private Image itemImage;
    private Image itemTierColor;
    private TextMeshProUGUI itemAmountText;
    
    public override void Initialize()
    {
        itemImage = transform.Find("Item_Image").GetComponent<Image>();
        itemTierColor = transform.Find("Item_Tier").GetComponent<Image>();
        itemAmountText = transform.Find("Item_Amount").GetComponent<TextMeshProUGUI>();
    }

    public override void GetItem(ItemData itemData, int amount)
    {
        base.GetItem(itemData, amount);

        if(Slot_ItemData == null)
        {
            itemImage.enabled = false;
            itemTierColor.enabled = false;
            itemAmountText.enabled = false;
            gameObject.SetActive(false);
        }
        else
        {
             itemImage.sprite = itemData.itemInfo.itemIcon;
             itemImage.enabled = true;
             SetTierColor(itemData.itemTier);
             itemTierColor.enabled = true;

             if(itemData.itemInfo.item_Type != InventoryType.Equipment)
             {
                 itemAmountText.SetText(Amount.ToString());
                 itemAmountText.enabled = true;
             }
             else
             {
                 itemAmountText.enabled = false;
             }
             gameObject.SetActive(true);
        }
    }

    private void SetTierColor(ItemTier tier)
    {
        switch(tier)
        {
            case ItemTier.Legendary :
                itemTierColor.color = Color.orange;
                break;
            case ItemTier.Epic :
                itemTierColor.color = Color.purple;
                break;
            case ItemTier.Rare :
                itemTierColor.color = Color.cyan;
                break;
            case ItemTier.Common :
                itemTierColor.color = Color.gray;
                break;
        }
    }
}
