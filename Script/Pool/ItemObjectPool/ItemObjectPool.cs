using UnityEngine;
using System.Collections.Generic;


public class ItemObjectPool
{
    private readonly Transform _parentTr;
    private Dictionary<ItemTier, List<ItemObject>> _itemPool;

    private Dictionary<ItemTier, int> _item_PrefabList = new();
    
    public ItemObjectPool(Transform parentTr)
    {
        _parentTr = parentTr;

        _item_PrefabList.Add(ItemTier.Common, 1000_000_0);
        _item_PrefabList.Add(ItemTier.Rare, 1000_000_1);
        _item_PrefabList.Add(ItemTier.Epic, 1000_000_2);
        _item_PrefabList.Add(ItemTier.Legendary, 1000_000_3);

        EventBus.Sub_Func<int, int, ItemObject>("Pool_GetItemObject", GetFromPool);
    }

    public void OnDisable()
    {
        EventBus.UnSub_Func<int, int, ItemObject>("Pool_GetItemObject", GetFromPool);
    }
    
    private ItemObject GetFromPool(int itemId, int itemAmount)
    {
        if(_itemPool == null) _itemPool = new();

        if(itemId <= 0) 
        {
            Util.Log("ItemObjectPool.cs GetFromPool() Not Found Item Object");
            return null;
        }

        ItemData itemData = DataLoader.GetData<ItemData>(DataType.Item, itemId);
        
        if(_itemPool.TryGetValue(itemData.itemTier, out List<ItemObject> itemPool))
        {
            for(int i = 0; i < itemPool.Count; i++)
            {
                ItemObject item = itemPool[i];

                if(!item.gameObject.activeSelf)
                {
                    Initialize_Object_Transform(item.gameObject);
                    item.InitializeItem(itemId, itemAmount);
                    return item;
                }
            }

            return CreateNew(itemData.itemTier, itemId, itemAmount);
        }
        else
        {
            return CreateNew(itemData.itemTier, itemId, itemAmount);
        }
    }

    private ItemObject CreateNew(ItemTier tier, int itemId, int itemAmount)
    {
        if(_item_PrefabList.TryGetValue(tier, out int itemPrefabId))
        {
            GameObject itemPrefab = DataLoader.GetData<GameObject>(DataType.Pool, itemPrefabId);
            GameObject newObj = GameObject.Instantiate(itemPrefab);
            newObj.transform.SetParent(_parentTr);
            newObj.SetActive(false);

            Initialize_Object_Transform(newObj);

            ItemObject itemComponent = newObj.GetComponent<ItemObject>();
            itemComponent.InitializeItem(itemId, itemAmount);
            
            if(!_itemPool.ContainsKey(tier))
            {
                _itemPool[tier] = new List<ItemObject>();
            }
            
            _itemPool[tier].Add(itemComponent);
            
            return itemComponent;
        }

        Util.Log("ItemObjectPool.cs CreateNew() Not Found Prefab");
        return null;
    }

    private void Initialize_Object_Transform(GameObject obj)
    {
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
    }
}