using UnityEngine;
using System.Collections.Generic;
public class ShopUI_ViewPanel
{
    private List<GameObject> lines                          = new();
    private Dictionary<int, List<ShopUI_ItemIcon>> icons    = new();
    private List<InventoryType> sellTypes                   = new();
    private List<InventoryType> buyTypes                   = new();
    private int lineInitializeCount = 5;
    private List<bool> readyList = new();
    public bool IsReady {get; private set;} = false;

    private GameObject linePrefab, iconPrefab;
    public ShopUI_ViewPanel(Transform tr)
    {
        Initialize(tr);
    }

    public void Initialize(Transform ownerTr)
    {
        InitializeItemType();
        MonoBehaviour mono = ownerTr.GetComponent<MonoBehaviour>();
        WaitForPrefab(ownerTr, mono);
    }

    private async void WaitForPrefab(Transform ownerTr, MonoBehaviour mono)
    {
        linePrefab  = await AddressableUtil.Load_Instant<GameObject>("ShopUI_ItemLine", mono.GetCancelOnDestroy());
        iconPrefab  = await AddressableUtil.Load_Instant<GameObject>("ShopUI_Icon", mono.GetCancelOnDestroy());

        Initialize_Line(ownerTr, mono);
    }

    private void Initialize_Line(Transform ownerTr, MonoBehaviour mono)
    {
        Transform tr = ownerTr.transform.FindTarget("mainViewPanel");
        for(int i = 0; i < lineInitializeCount; i++)
        {
            GameObject newObj = Object.Instantiate(linePrefab);
            newObj.transform.SetParent(tr);
            newObj.SetActive(false);

            lines.Add(newObj);

            Transform targetLineTr = newObj.transform;

            Initialize_Icon(i, targetLineTr, iconPrefab); 
        }

        mono.RunRoutine(WaitForReady(), "ShopUI_ViewPanel_WaitForReady");
    }
    System.Collections.IEnumerator WaitForReady()
    {
        yield return new WaitUntil(()=> readyList.Count >= lineInitializeCount);

        IsReady = true;
    }

    private void Initialize_Icon(int index, Transform targetTr, GameObject prefabArgs)
    {
        GameObject prefab = prefabArgs;

        Transform targetParent = targetTr.FindTarget("Content");

        for(int i = 0; i < 30; i++)
        {
            GameObject newObj = Object.Instantiate(prefab);

            newObj.transform.SetParent(targetParent);
            newObj.SetActive(false);

            ShopUI_ItemIcon comp = newObj.GetComponent<ShopUI_ItemIcon>();
            
            if(comp != null)
            {
                comp.Initialize();

                if(!icons.ContainsKey(index))
                    icons.Add(index, new());

                icons[index].Add(comp);
            }
        }

        readyList.Add(true);
    }
    
    private void InitializeItemType()
    {
        SetSellItemType(InventoryType.Usable);
        SetSellItemType(InventoryType.Material);
        SetSellItemType(InventoryType.ETC);

        SetBuyItemType(InventoryType.Usable);
        SetBuyItemType(InventoryType.Material);
        SetBuyItemType(InventoryType.ETC);
        SetBuyItemType(InventoryType.Equipment);
    }
    private void SetSellItemType(InventoryType type)
    {
        if(!sellTypes.Contains(type)) sellTypes.Add(type);
    }
    private void SetBuyItemType(InventoryType type)
    {
        if(!buyTypes.Contains(type)) buyTypes.Add(type);
    }

    public void SetPanel(ShopType type)
    {
        switch(type)
        {
            case ShopType.Buy:
                OnBuy();
                break;

            case ShopType.Sell:
                OnSell();
                break;
            default :
                Util.Log("ShopUI_ViewPanel.cs 주의 선택된 상점 타입이 이상함","yellow");
                break;
        }
    }

    private void OnBuy()
    {
        for(int i = 0; i < lines.Count; i++)
        {
            GameObject line     = lines[i];

            if(i < buyTypes.Count)
            {
                InventoryType type  = buyTypes[i];
                List<ItemHasInfo> infos = EventBus.Invoke_Func<InventoryType, List<ItemHasInfo>>("ShopUI_GetBuyList", type);

                if(infos == null || infos.Count <= 0) 
                    line.SetActive(false);
                else 
                {
                    line.SetActive(true);
                    SetIcons(i, line.transform.FindTarget("Content"), infos);
                }
            }
        }
    }

    private void OnSell()
    {
        for(int i = 0; i < lines.Count; i++)
        {
            GameObject line     = lines[i];

            if(i < sellTypes.Count)
            {
                InventoryType type  = sellTypes[i];

                List<ItemHasInfo> infos = EventBus.Invoke_Func<InventoryType, List<ItemHasInfo>>("Inventory_System_GetInventory", type);

                if(infos == null || infos.Count <= 0) 
                    line.SetActive(false);
                else 
                {
                    line.SetActive(true);
                    SetIcons(i, line.transform.FindTarget("Content"), infos);
                }
            }
            else
            {
                line.SetActive(false);
            }
        }
    }

    private void SetIcons(int index, Transform parentTr, List<ItemHasInfo> infos)
    {
        if(icons.TryGetValue(index, out var iconList))
        {
            //아이콘 갯수가 아이템 갯수보다도 적을시
            if(iconList.Count <= infos.Count)
            {
                int count = (infos.Count - iconList.Count) + 2;

                Transform targetTr = parentTr;

                for(int i = 0; i < count; i++)
                {
                    GameObject newObj = Object.Instantiate(iconPrefab);
                    newObj.transform.SetParent(targetTr);
                    newObj.SetActive(false);

                    ShopUI_ItemIcon comp = newObj.GetComponent<ShopUI_ItemIcon>();
            
                    if(comp != null) 
                    {
                        comp.Initialize();
                        icons[index].Add(comp);
                    }
                }
            }

            //아이콘 갯수만큼 실행하되 아이템 갯수에 맞춰서 아이콘의 꺼짐 여부 결정
            for(int i = 0; i < iconList.Count; i++)
            {
                ShopUI_ItemIcon icon    = iconList[i];
            
                if(i < infos.Count)
                {
                    ItemHasInfo info        = infos[i];
                    
                    if(info.itemAmount <= 0)
                    {
                        icon.gameObject.SetActive(false);
                    }
                    else
                    {
                        icon.SetItem(info.data, info.itemAmount);
                        icon.gameObject.SetActive(true);
                    }
                }
                else
                {
                    icon.gameObject.SetActive(false);
                }
            }
        }
    }
}
