using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public enum ShopType
{
    Buy,
    Sell
}
//initialize
public partial class ShopUI_MainSystem : MonoBehaviour
{
    private List<ShopType> types = new();
    private ShopUI_ViewPanel viewPanel;
    private ShopUI_CalculateSystem calculateSystem;
    private ShopUI_SelectChoicePanel amountSetPanel;

    private ShopType currentShopType = ShopType.Buy;
    private Dictionary<ShopType, string> typeButtonNames = new();

    private ShopBuyList dummyList; // 파는 아이템 목록
    private CurrencyUI currencyUI;
    private GameObject mainPanel;

    private bool isInit = false;

    private void OnEnable()
    {
        this.RunRoutine(WaitForDataLoaderAndInventory(), "ShopUI_MainSystem_WaitForDataLoaderAndInventory");
    }
    IEnumerator WaitForDataLoaderAndInventory()
    {
        yield return new WaitUntil(() => LoadStatus.IsReady && LoadStatus.IsReady_Inventory);

        Initialize();
    }
    private void Initialize()
    {
        InitializeMakeDummyData();
        InitializeClass();
        InitializeTypeButtonName();
        InitializeShopTypes();
        InitializeObject();
        InitializeEvent();
        InitializeCurrencyUI();

        this.RunRoutine(WaitForViewPanel(), "ShopUI_MainSystem_WaitForViewPanel");
    }
    private void InitializeMakeDummyData()
    {
        dummyList = (ShopBuyList)ScriptableObject.CreateInstance("ShopBuyList");
    }

    private void InitializeClass()
    {
        calculateSystem = new();
        amountSetPanel  = new(this.transform);
        viewPanel       = new(this.transform);
    }
    private void InitializeTypeButtonName()
    {
        typeButtonNames.Add(ShopType.Buy, "Buy");
        typeButtonNames.Add(ShopType.Sell, "Sell");
    }
    private void InitializeShopTypes()
    {
        SetShopType(ShopType.Buy);
        SetShopType(ShopType.Sell);
    }
    private async void InitializeObject()
    {
        GameObject prefab = await AddressableUtil.Load_Instant<GameObject>("ShopType_ButtonPrefab", this.GetCancelOnDestroy());

        if(transform == null) return;
        Transform parentTr = transform.FindTarget("TypeButtonContext");   

        for(int i = 0; i < types.Count; i++)
        {
            ShopType type = types[i];
            
            GameObject newObj = Instantiate(prefab);
            newObj.transform.SetParent(parentTr);

            ShopUI_TypeButton comp = newObj.GetComponent<ShopUI_TypeButton>();

            if(comp != null)
            {
                typeButtonNames.TryGetValue(type, out string name);
                comp.Initialize(type, name);
            }
            else
            {
                Object.DestroyImmediate(newObj);
                Util.Log("주의! ShopUI_MainSystem.cs에서 버튼의 컴포넌트가 존재하지않음 -> 삭제처리시킴","yellow");
            }
        }

        Button finishButton = transform.FindTarget("FinishButton").GetComponent<Button>();
        finishButton.onClick.AddListener(FinishCalculate);  

        Button exitButton = transform.FindTarget("ExitButton").GetComponent<Button>();
        exitButton.onClick.AddListener(Close);  

        mainPanel = transform.FindTarget("mainPanel").gameObject; 
        mainPanel.SetActive(false);
    }
    private void InitializeEvent()
    {
        EventBus.Sub<ShopType>("ShopUI_OnClick_ShopTypeButton", OnClickShopTypeButton);
        EventBus.Sub<ShopType>("ShopUI_Reload", DelayLoad);
        EventBus.Sub<(ShopUI_ItemIcon, int)>("ShopUI_AddList", AddList);
        EventBus.Sub<(ShopUI_ItemIcon, bool)>("ShopUI_OnClickIcon", OnClickItemIcon);
        EventBus.Sub_Func<InventoryType, List<ItemHasInfo>>("ShopUI_GetBuyList", GetBuyList);
        EventBus.Sub<int>("ShopUI_ChangeList", ChangeList);
        EventBus.Sub("ShowShop", Open);
    }
    private void InitializeCurrencyUI()
    {
        currencyUI = new(transform.FindTarget("CurrencyText").GetComponent<TextMeshProUGUI>());
    }
    private void OnDisable()
    {
        EventBus.UnSub<ShopType>("ShopUI_OnClick_ShopTypeButton", OnClickShopTypeButton);
        EventBus.UnSub<ShopType>("ShopUI_Reload", DelayLoad);
        EventBus.UnSub<(ShopUI_ItemIcon, int)>("ShopUI_AddList", AddList);
        EventBus.UnSub<(ShopUI_ItemIcon, bool)>("ShopUI_OnClickIcon", OnClickItemIcon);
        EventBus.UnSub_Func<InventoryType, List<ItemHasInfo>>("ShopUI_GetBuyList", GetBuyList);
        EventBus.UnSub<int>("ShopUI_ChangeList", ChangeList);
        EventBus.UnSub("ShowShop", Open);

        if(currencyUI != null) currencyUI.Inactive();
    }

    IEnumerator WaitForViewPanel()
    {
        yield return new WaitUntil(() => viewPanel.IsReady);

        OnClickShopTypeButton(currentShopType);

        isInit = true;
    }
}

//system
public partial class ShopUI_MainSystem : MonoBehaviour
{
    private void ChangeList(int npcId)
    {
        var soData = DataLoader.GetData<NPCData>(DataType.NPC, npcId);

        if(soData.type == NPCType.ShopNPC)
        {
            ShopBuyList shopList = soData.shopData;
            calculateSystem.InitializeBuyList(shopList);
        }
    }

    private void SetShopType(ShopType type)
    {
        if(!types.Contains(type)) types.Add(type);
    }
    private void OnClickShopTypeButton(ShopType type)
    {
        if(viewPanel == null || !viewPanel.IsReady) {
            Util.Log("주의 ShopUI_MainSystem.cs viewPanel 미상","yellow");
            return;
        }

        amountSetPanel.ForceInactive();
        calculateSystem.ClearList();
        
        viewPanel.SetPanel(type);

        currentShopType = type;
    }
    private void OnClickItemIcon((ShopUI_ItemIcon icon, bool isInteract) tuple)
    {
        if(tuple.isInteract)
        {
            if(tuple.icon.itemAmount >= 2)
            {
                amountSetPanel.ShowAmountPanel(tuple.icon);
            }
            else
            {
                calculateSystem.AddSelectList(tuple.icon, 1);
            }
        }
        else
        {
            calculateSystem.RemoveSelectList(tuple.icon);
        }
    }
    private void AddList((ShopUI_ItemIcon icon, int amount) tuple)
    {
        calculateSystem.AddSelectList(tuple.icon, tuple.amount);
    }
    private void FinishCalculate()
    {
        calculateSystem.FinishCalculate(currentShopType);
    }
    private void DelayLoad(ShopType type)
    {
        this.RunRoutine(ReloadPanel(type), "ShopUI_MainSystem_ReloadPanel");
    }
    IEnumerator ReloadPanel(ShopType type)
    {
        yield return new WaitUntil(() => LoadStatus.IsReady && LoadStatus.IsReady_Inventory);

        viewPanel.SetPanel(type); 
    }

    private List<ItemHasInfo> GetBuyList(InventoryType type)
    {
        var dc = calculateSystem.GetList();

        dc.TryGetValue(type, out var list);

        return list;
    }

    public void Close()
    {
        if(!isInit) return;

        amountSetPanel.ForceInactive();
        calculateSystem.ClearList();
        
        if(!GameState.IsTutorial()) 
            EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState", GameStateType.Run, GameEnableTimeSet.True);

        CursorManager.CursorActive(false);

        EventBus.Invoke<bool>("SetCameraRotateLock", false);
        EventBus.Invoke<bool>("Lock_All_UI", false);
        EventBus.Invoke<bool>("MainCanvasActive", true);

        mainPanel.SetActive(false);
    }

    public void Open()
    {
        if(!isInit) return;

        OnClickShopTypeButton(currentShopType);
        
        EventBus.Invoke<bool>("SetCameraRotateLock", true);
        EventBus.Invoke<bool>("Lock_All_UI", true);
        EventBus.Invoke<bool>("MainCanvasActive", false);

        if(!GameState.IsTutorial()) 
            EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState", GameStateType.Stop, GameEnableTimeSet.True);
        CursorManager.CursorActive(true);
        mainPanel.SetActive(true);
    }
}

public class ShopUI_CalculateSystem
{
    private Dictionary<InventoryType, List<ItemHasInfo>> buyList = new();
    private Dictionary<int, (ShopUI_ItemIcon ui, int amount)> itemCalList = new();
    public void InitializeBuyList(ShopBuyList shopList)
    {
        buyList.Clear();

        for(int i = 0; i < shopList.list.Count; i++)
        {
            var info = shopList.list[i];

            int itemId = info.itemId;
            ItemData data = null;

            if(info.data != null) data = info.data;
            else data = DataLoader.GetData<ItemData>(DataType.Item, itemId);

            if(data == null) Util.Log("ShopUI_MainSystem.cs 주의 아이템 데이터 미상임","yellow");

            InventoryType type = data.itemInfo.item_Type;

            if(!buyList.ContainsKey(type))
                buyList.Add(type, new());

            ItemHasInfo newInfo = new();
            newInfo.itemAmount = info.itemAmount;
            newInfo.data = data;

            buyList[type].Add(newInfo);
        }
    }

    public Dictionary<InventoryType, List<ItemHasInfo>> GetList() => buyList;
    public void ClearList()
    {
        foreach(var list in itemCalList)
        {
            var icon = list.Value.ui;

            icon.OnSelectedText(icon.itemAmount, false);
        }

        itemCalList.Clear();
    }
    private void SetList(int itemId, int itemAmount)
    {
        foreach(var map in buyList)
        {
            List<ItemHasInfo> list  = map.Value;

            for(int i = 0; i < list.Count; i++)
            {
                ItemHasInfo info = list[i];

                if(itemId == info.data.itemInfo.itemId)
                {
                    info.itemAmount = Mathf.Max(0, info.itemAmount - itemAmount);

                    if(info.itemAmount <= 0)
                    {
                        list.RemoveAt(i);
                    }
                    else
                    {
                        list[i] = info;
                    }
                    
                    return;
                }
            }
        }
    }
    public void AddSelectList(ShopUI_ItemIcon itemIcon, int amount)
    {
        itemCalList.Add(itemIcon.itemData.itemInfo.itemId, (itemIcon, amount));
        itemIcon.OnSelectedText(amount);
    }
    public void RemoveSelectList(ShopUI_ItemIcon itemIcon)
    {
        if(itemCalList.ContainsKey(itemIcon.itemData.itemInfo.itemId)) itemCalList.Remove(itemIcon.itemData.itemInfo.itemId);
        itemIcon.OnSelectedText(itemIcon.itemAmount, false);
    }

    public void FinishCalculate(ShopType type)
    {
        switch(type)
        {
            case ShopType.Buy:
                Buy();
                break;
            case ShopType.Sell:
                Sell();
                break;
            default :
                Util.Log("ShopUI_CalculateSystem.cs 주의 선택된 상점 타입이 이상함","yellow");
                break;
        }
    }

    private void Buy()
    {
        if(itemCalList.Count <= 0) return;
        
        List<(int itemId, int amount)> items = new();
        int calAmount = 0;

        foreach(var map in itemCalList)
        {
            int amount              = map.Value.amount;
            int itemId              = map.Key;

            ItemData itemData = DataLoader.GetData<ItemData>(DataType.Item, itemId);

            calAmount += Mathf.Max(0, calAmount + (itemData.itemInfo.itemSellAmount * amount));

            items.Add((itemId, amount));
        }   

        int currentCurrency = EventBus.Invoke_Func<int>("GetCurrentCurrency");

        if(calAmount <= currentCurrency)
        {
            for(int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                int itemId = item.itemId;
                int amount = item.amount;

                SetList(itemId, amount);

                //아이템 지급
                EventBus.Invoke<int, int, bool>("GetItem", itemId, amount, false);
            }

            int calculateCurrency = currentCurrency - calAmount;

            EventBus.Invoke<int>("SetCurrency", -calculateCurrency);
        }

        EventBus.Invoke<ShopType>("ShopUI_Reload", ShopType.Buy);

        //선택데이터 초기화
        itemCalList.Clear();
    }

    private void Sell()
    {
        if(itemCalList.Count <= 0) return;

        int getAmount = 0;

        foreach(var map in itemCalList)
        {
            int amount              = map.Value.amount;
            int itemId              = map.Key;

            EventBus.Invoke<int, int, bool>("GetItem", itemId, -amount, false);

            //돈 또는 재화 지급
            ItemData itemData = DataLoader.GetData<ItemData>(DataType.Item, itemId);

            getAmount += Mathf.Max(0, getAmount + (itemData.itemInfo.itemSellAmount * amount));
        }

        EventBus.Invoke<int>("SetCurrency", getAmount);
        
        EventBus.Invoke<ShopType>("ShopUI_Reload", ShopType.Sell);

        //선택데이터 초기화
        itemCalList.Clear();
    }
}