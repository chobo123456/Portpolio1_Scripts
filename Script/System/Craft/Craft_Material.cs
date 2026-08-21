using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Craft_Material : MonoBehaviour
{
    private Image recipeIcon;
    private TextMeshProUGUI recipeMaterialAmount;
    public bool isAbleToMake = false, isSetData = false;

    public void Initialize()
    {
        recipeIcon = transform.Find("Item_Icon").GetComponent<Image>();
        recipeMaterialAmount = transform.Find("Item_Amount").GetComponent<TextMeshProUGUI>();
    }

    public void SetData(int itemId, int needAmount = 0, int currentHadAmount = 0)
    {
        isAbleToMake = false;

        if(itemId <= 0) 
        {
            isSetData = false;
            return;
        }

        ItemData itemData = DataLoader.GetData<ItemData>(DataType.Item, itemId);
        
        recipeIcon.sprite = itemData.itemInfo.itemIcon;
        recipeMaterialAmount.SetText($"{currentHadAmount} / {needAmount}");

        if(currentHadAmount >= needAmount)
            isAbleToMake = true;

        isSetData = true;
    }
    
    public bool IsSetData() => isSetData;
    public bool IsAbleToMake() => isAbleToMake;
}