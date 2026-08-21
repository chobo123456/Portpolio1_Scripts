using UnityEngine;

public class CharacterItemUseRelay : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus.Sub<int, int>("On_UseItem", OnUseItem);
    }

    private void OnDisable()
    {
        EventBus.UnSub<int, int>("On_UseItem", OnUseItem);
    }

    private void OnUseItem(int characterId, int itemId)
    {
        ItemData itemData = DataLoader.GetData<ItemData>(DataType.Item, itemId);

        UseItemData useItem = GetUsableClass(itemData);
        UseCase(useItem, characterId);
    }

    private UseItemData GetUsableClass(ItemData itemData)
    {
        return itemData as UseItemData;
    }

    private void UseCase(UseItemData useItemData, int characterId = 0)
    {
        if(useItemData == null) return;

        if(useItemData is HealthItem healItem)
        {
            healItem.UseItem(characterId);
        }
    }
}
