using UnityEngine;
using UnityEngine.UI;

public class Craft_Recipe_Button : MonoBehaviour
{
    private bool initStatus = false;
    private Image recipeIcon;
    public void Initialize(int recipeId)
    {
        CraftRecipe recipeData = DataLoader.GetData<CraftRecipe>(DataType.Recipe, recipeId);
        ItemData resultItemData = DataLoader.GetData<ItemData>(DataType.Item, recipeData.result_item_Id);
        
        recipeIcon = transform.Find("Recipe_Result_Icon").GetComponent<Image>();
        recipeIcon.sprite = resultItemData.itemInfo.itemIcon;

        initStatus = true;
    }

    public bool IsInitialized() => initStatus;
}