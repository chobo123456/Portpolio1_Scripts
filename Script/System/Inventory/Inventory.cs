using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public struct ItemHasInfo
{
    public ItemData data;
    public int itemAmount;    
    public int instanceId;
}
public struct GiveItemInfo
{
    public int itemId;
    public int amount;
}

public enum InventoryType
{
    Quest,
    Usable,
    Material,
    Equipment,
    ETC,
}

public enum SortType
{
    RareUp,
    RareDown,
    FirstName,
}
//뷰 (초기화)
public partial class Inventory : UIClass
{
    private List<ISlot> slots = new();
    private List<RectTransform> slotLineRects = new();
    private List<RectTransform> slotRects = new();
    private Transform targetParent;
    private ScrollRect scrollRect;
    private RectTransform content;

    //아이템 정보칸
    private Image itemInfoIcon, itemInfoColorImage;
    private TextMeshProUGUI itemName, itemDiscription, itemType;
    private Button itemUseButton;

    //인벤토리 나가기
    private GameObject inventoryMainPanel, itemSlotPrefab, itemUseTargetChoosePanel;

    //라인 갯수
    private int line = 6;

    //인벤토리 버튼
    private Dictionary<InventoryType, Button> inventoryButtons = new();

    //인벤토리 버튼 이미지
    private Dictionary<InventoryType, Image> inventoryButtonImages = new();
    
    //인벤토리 매핑
    private Dictionary<InventoryType, ITypeInventory> inventories = new();

    private InventoryType currentInventoryType = InventoryType.ETC;

    //현재 선택중인 아이템
    private ISlot currentChoose_Slot;

    //현재 정렬타입
    private SortType currentSortType = SortType.RareUp;

    private bool isInit = false, isLockItemUsePanel = false, isLockItemTypePanel = false;

    //인벤토리에서 보이는 재화ui
    private CurrencyUI currencyUISystem;
    private InventoryItemUseSystem useItemSystem;

    public override void OnEnable()
    {
        base.SetType(UIType.Inventory);
        base.OnEnable();

        SubscribeEvent(true);
        this.RunRoutine(Booting());
    }

    #region Initialize
    private void Initialize_ItemIconParent(Transform middle)
    {
        Transform inventoryPanel = middle.Find("Inventory");
        inventoryMainPanel = middle.FindTarget("InventoryConnecter").gameObject;

        scrollRect = inventoryPanel.FindTarget("Scroll View").GetComponent<ScrollRect>();
        content = inventoryPanel.FindTarget("Content").GetComponent<RectTransform>();
        targetParent = inventoryPanel.FindTarget("Content");
    }

    private void Initialize_Objects(Transform middle)
    {
        Button inventoryExitButton = middle.FindTarget("Inventory_ExitButton").GetComponent<Button>();
        inventoryExitButton.onClick.AddListener(() => base.OnClickCloseButton());

        var inventorySortDropDown = middle.FindTarget("Inventroy_SortDropDown").GetComponent<TMP_Dropdown>();
        inventorySortDropDown.onValueChanged.AddListener(ChangedSortType);

        itemInfoColorImage = middle.FindTarget("Item_Tier_Color").GetComponent<Image>();
        itemInfoIcon = middle.FindTarget("Item_Image").GetComponent<Image>();
        itemName = middle.FindTarget("Item_Name").GetComponent<TextMeshProUGUI>();
        itemDiscription = middle.FindTarget("Item_Discription").GetComponent<TextMeshProUGUI>();
        itemType = middle.FindTarget("Item_Type").GetComponent<TextMeshProUGUI>();
        itemUseButton = middle.FindTarget("Item_Use_Button").GetComponent<Button>();

        itemInfoColorImage.enabled = false;
        itemInfoIcon.enabled = false;
        itemName.enabled = false;
        itemDiscription.enabled = false;
        itemType.enabled = false;
        itemUseButton.gameObject.SetActive(false);

        itemUseButton.onClick.AddListener(OpenUsePanel);
    }
    private void Initialize_Inventory()
    {
        AddInventory(InventoryType.Quest);
        AddInventory(InventoryType.Usable);
        AddInventory(InventoryType.Material);
        AddInventory(InventoryType.Equipment);
        AddInventory(InventoryType.ETC);
    }
    private void Initialize_InventoryButton()
    {
        Transform mainCanvas    = GameObject.Find("MainCanvas").transform;
        Transform middle        = mainCanvas.Find("Middle");

        //퀘스트
        SubscribeAndInitializeButton(middle, InventoryType.Quest, "Quest_InventoryButton");

        //사용
        SubscribeAndInitializeButton(middle, InventoryType.Usable, "Useable_InventoryButton");

        //재료
        SubscribeAndInitializeButton(middle, InventoryType.Material, "Material_InventoryButton");

        //장비
        SubscribeAndInitializeButton(middle, InventoryType.Equipment, "Equipment_InventoryButton");

        //기타
        SubscribeAndInitializeButton(middle, InventoryType.ETC, "ETC_InventoryButton");
    }   
    private void Initialize_CurrencyUI()
    {
        currencyUISystem = new(this.transform.FindTarget("CurrencyAmount").GetComponent<TextMeshProUGUI>());
    }
    private void Initialize_UseItemSystem()
    {
        itemUseTargetChoosePanel = this.transform.FindTarget("UseCharacterChoosePanel").gameObject;
        useItemSystem = new(itemUseTargetChoosePanel, OnClickCharacterIcon, IsUsePanelLock);
        useItemSystem.Initialize();
    }
    private void AddInventory(InventoryType type)
    {
        var inventory = this.GetInventory(type);
        inventories.Add(type, inventory);
    }
    private void SubscribeAndInitializeButton(Transform findTargetTr, InventoryType type, string buttonName)
    {
        var button    = findTargetTr.FindTarget(buttonName).GetComponent<Button>();
        button.onClick.AddListener(() => Show(type));  
        inventoryButtons.Add(type, button);

        var buttonImage = button.GetComponent<Image>();
        inventoryButtonImages.Add(type, buttonImage);
    }
    private async void Initialize_Slots()
    {
        itemSlotPrefab = await AddressableUtil.Load_Instant<GameObject>("InventorySlots", this.GetCancelOnDestroy());

        Transform mainCanvas = GameObject.Find("MainCanvas").transform;
        Transform middle = mainCanvas.Find("Middle");

        float ySize = 150f;
        float spacing = 20f;
        float yMove = 400f;

        List<Button> buttons = new();

        for(int i = 0; i < line; i++)
        {
            float col = i % line; // 열수

            float yPos = -col * (ySize + spacing) + yMove;
            
            GameObject newSlot = Instantiate(itemSlotPrefab);
            newSlot.transform.SetParent(targetParent);

            for(int j = 0; j < newSlot.transform.childCount; j ++)
            {
                Transform tr = newSlot.transform.Find($"Slot_{j}");

                var comp = tr.GetComponent<ISlot>();
                comp?.Initialize();

                RectTransform slotRect = tr.GetComponent<RectTransform>();

                slots.Add(comp);
                slotRects.Add(slotRect);

                buttons.Add(newSlot.transform.Find($"Slot_{j}").GetComponent<Button>());
            }

            newSlot.SetActive(false);
            RectTransform rect = newSlot.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, yPos);     

            slotLineRects.Add(rect);
        }

        for(int i = 0; i < buttons.Count; i++)
        {
            int index = i;

            buttons[i].onClick.AddListener(() => ShowInfo(index));
        }
    }
    
    #endregion

    IEnumerator Booting()
    {
        Transform mainCanvas = GameObject.Find("MainCanvas").transform;
        Transform middle = mainCanvas.Find("Middle");

        Initialize_ItemIconParent(middle);
        Initialize_Objects(middle);
        Initialize_Inventory();
        Initialize_InventoryButton();
        Initialize_Slots();
        Initialize_CurrencyUI();
        Initialize_UseItemSystem();

        yield return new WaitUntil(() => LoadStatus.IsReady && itemSlotPrefab != null);

        inventoryMainPanel.SetActive(false);

        Load();

        isInit = true;

        LoadStatus.SetStatus(ManagerType.Inventory, true);

        //테스트 용도
        TestItemCode();
    }

    private void OnDisable()
    {
        LoadStatus.SetStatus(ManagerType.Inventory, false);

        SubscribeEvent(false);

        currencyUISystem.Inactive();
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<int, int, bool>("GetItem", GetItem);
            EventBus.Sub<bool>("Inventory_UI_Lock_ItemUsePanel", LockItemUsePanel);
            EventBus.Sub<bool>("Inventory_UI_Lock_CategoryPanel", LockTypePanel);

            EventBus.Sub<int, int>("Inventory_System_TryReceiveItem", TryReceiveItem);
            EventBus.Sub_Func<InventoryType, List<ItemHasInfo>>("Inventory_System_GetInventory", GetInventoryItem);
            EventBus.Sub_Func<InventoryType, Dictionary<int, ItemHasInfo>>("Inventory_System_GetInventory_Dic", GetInventoryItem_Dic);
            EventBus.Sub_Func<int, bool>("Inventory_System_IsExistItem", IsItemExist);

            EventBus.Sub_Func<int, RectTransform>("Inventory_UI_InventorySlotRect", GetInventorySlotRect);
            EventBus.Sub_Func<int, int>("InventorySlotIndex", GetItemSlotIndex); //Delete Target
        }
        else
        {
            EventBus.UnSub<int, int, bool>("GetItem", GetItem);
            EventBus.UnSub<bool>("Inventory_UI_Lock_ItemUsePanel", LockItemUsePanel);
            EventBus.UnSub<bool>("Inventory_UI_Lock_CategoryPanel", LockTypePanel);

            EventBus.UnSub<int, int>("Inventory_System_TryReceiveItem", TryReceiveItem);
            EventBus.UnSub_Func<InventoryType, List<ItemHasInfo>>("Inventory_System_GetInventory", GetInventoryItem);
            EventBus.UnSub_Func<InventoryType, Dictionary<int, ItemHasInfo>>("Inventory_System_GetInventory_Dic", GetInventoryItem_Dic);
            EventBus.UnSub_Func<int, bool>("Inventory_System_IsExistItem", IsItemExist);

            EventBus.UnSub_Func<int, RectTransform>("Inventory_UI_InventorySlotRect", GetInventorySlotRect);
            EventBus.UnSub_Func<int, int>("InventorySlotIndex", GetItemSlotIndex); //Delete Target
        }
    }

    private void TestItemCode()
    {
        //GetItem(100000, 2);
        //GetItem(1000000, 15);
        //GetItem(1000001, 15);
        //GetItem(1000002, 15);
        //GetItem(1000003, 15);
    }

    public override RectTransform GetRectTransform(UIRectName rectName)
    {
        switch(rectName)
        {
            case UIRectName.InventoryUI_QuestInventoryButton :
                return inventoryButtons[InventoryType.Quest].GetComponent<RectTransform>();

            case UIRectName.InventoryUI_UsableInventoryButton :
                return inventoryButtons[InventoryType.Usable].GetComponent<RectTransform>();

            case UIRectName.InventoryUI_MaterialInventoryButton :
                return inventoryButtons[InventoryType.Material].GetComponent<RectTransform>();

            case UIRectName.InventoryUI_EquipmentInventoryButton :
                return inventoryButtons[InventoryType.Equipment].GetComponent<RectTransform>();

            case UIRectName.InventoryUI_ETCInventoryButton :
                return inventoryButtons[InventoryType.ETC].GetComponent<RectTransform>();

            case UIRectName.InventoryUI_ItemUseButton :
                return itemUseButton.GetComponent<RectTransform>();

            case UIRectName.InventoryUI_ItemUseTargetChoosePanel :
                return itemUseTargetChoosePanel.GetComponent<RectTransform>();
        }

        return null;
    }

    private RectTransform GetInventorySlotRect(int itemId)
    {
        int itemIndex = GetItemSlotIndex(itemId);

        if(itemIndex < 0) return null;

        if(slotRects.Count > itemIndex)
            return slotRects[itemIndex];

        return null;
    }
}

//뷰 (시스템)
public partial class Inventory : UIClass
{

    #region GetItem
    private void TryReceiveItem(int targetItemId, int targetAmount)
    {
        if(targetItemId <= 0) return;

        ItemData data = DataLoader.GetData<ItemData>(DataType.Item, targetItemId);
        Dictionary<int, ItemHasInfo> inventoryItem = GetInventoryItem_Dic(data.itemInfo.item_Type);

        if(inventoryItem.TryGetValue(targetItemId, out var item))
        {
            if(item.itemAmount < targetAmount)
            {
                GetItem(targetItemId, targetAmount, false);
            }
        }
    }
    
    private void GetItem(int itemId, int amount, bool isShowRightSide_ItemGetPanel = true)
    {
        ItemData data = DataLoader.GetData<ItemData>(DataType.Item, itemId);

        if (inventories.TryGetValue(data.itemInfo.item_Type, out var inventory))
        {
            GiveItemInfo getInfo = new GiveItemInfo();
            getInfo.itemId = itemId;
            getInfo.amount = amount;
            inventory.GetItem(getInfo, isShowRightSide_ItemGetPanel);

            if (currentInventoryType == data.itemInfo.item_Type)
                this.RunRoutine(DelayShow());
        }

        if(data.itemInfo.item_Type == InventoryType.Material) 
            EventBus.Invoke("InventoryReload");
    }
    #endregion
    
    #region Show
    IEnumerator DelayShow()
    {
        yield return YieldUtil.WaitForSecondsRealtime(0.05f);

        Show(currentInventoryType);
    }

    private void ShowInfo(int index, bool forceExit = false)
    {
        if(forceExit)
        {
            CloseUsePanel();

            itemInfoIcon.enabled        = false;
            itemInfoColorImage.enabled  = false;
            itemName.enabled            = false;
            itemDiscription.enabled     = false;
            itemType.enabled            = false;
            itemUseButton.gameObject.SetActive(false);

            return;
        }
        
        currentChoose_Slot = slots[index];

        var info = currentChoose_Slot.Slot_ItemData;

        if(info != null)
        {
            if(GameState.IsTutorial())
                EventBus.Invoke<int>("On_Inventory_UI_ItemIconClick", info.itemInfo.itemId);

            if(info.itemInfo.itemIcon != null)
            {
                itemInfoIcon.sprite         = info.itemInfo.itemIcon;
                itemInfoIcon.enabled        = true;
            }   
            
            switch(info.itemTier)
            {
                case ItemTier.Legendary :
                    itemInfoColorImage.color = Color.orange;
                    break;
                case ItemTier.Epic :
                    itemInfoColorImage.color = Color.purple;
                    break;
                case ItemTier.Rare :
                    itemInfoColorImage.color = Color.cyan;
                    break;
                case ItemTier.Common :
                    itemInfoColorImage.color = Color.gray;
                    break;
            }

            switch(currentInventoryType)
            {
                case InventoryType.Quest :
                    itemUseButton.gameObject.SetActive(false);
                    break;
                case InventoryType.Usable :
                    itemUseButton.gameObject.SetActive(true);
                    break;
                case InventoryType.Material :
                    itemUseButton.gameObject.SetActive(false);
                    break;
                case InventoryType.Equipment :
                    itemUseButton.gameObject.SetActive(false);
                    break;
            }

            itemInfoColorImage.enabled  = true;
            
            itemName.SetText(info.itemInfo.item_Name);
            itemName.enabled            = true;

            itemDiscription.SetText(info.itemInfo.item_Description);
            itemDiscription.enabled     = true;
            
            itemType.SetText(info.itemType.ToString());
            itemType.enabled            = true;
        }
        else
        {
            itemInfoIcon.enabled        = false;
            itemInfoColorImage.enabled  = false;
            itemName.enabled            = false;
            itemDiscription.enabled     = false;
            itemType.enabled            = false;
            itemUseButton.gameObject.SetActive(false);
        }
    }

    private void SetButtonColor(InventoryType type)
    {
        foreach(var image in inventoryButtonImages)
        {
            var key = image.Key;
            var value = image.Value;

            if(key == type) {
                value.color = Color.yellow;
                continue;
            }

            value.color = Color.white;
        }
    }

    private void Show(InventoryType type)
    {
        if(isLockItemTypePanel) return;

        if (inventories.TryGetValue(type, out var inventory))
        {
            List<ItemHasInfo> list = inventory.OnInventory();
            list = SetSort(list);
            Set_ShowCurrentUI(list);
            SetButtonColor(type);
            
            if(currentInventoryType != type) ShowInfo(-999, true);
            currentInventoryType = type;

            if(GameState.IsTutorial())
                EventBus.Invoke<InventoryType>("On_Inventory_UI_CategoryClick", type);
        }
    }

    private List<ItemHasInfo> SetSort(List<ItemHasInfo> list)
    {
        switch (currentSortType)
        {
            case SortType.RareUp:
                list = list.OrderBy((item) => (int)item.data.itemTier).ToList();
                return list;

            case SortType.RareDown:
                list = list.OrderByDescending((item) => (int)item.data.itemTier).ToList();
                return list;

            default:
                return list;
        }
    }
    private void Set_ShowCurrentUI(List<ItemHasInfo> slotArgs)
    {
        int existItemCount = 0;
        int count = 0;

        for(int i = 0; i < slots.Count; i++)
        {
            if(i >= slotArgs.Count)
            {
                slots[i].GetItem(null);

                continue;
            }

            ItemHasInfo info = slotArgs[i];
            
            if(info.data == null || info.itemAmount <= 0)
            {
                slots[i].GetItem(null);
                continue;
            }

            slots[i].GetItem(info.data, info.itemAmount);
        }

        foreach(var slot in slots)
        {
            if(slot.IsItemExist)
            {
                existItemCount++;
            }

            count++;
        }

        float multiple = existItemCount / 9f;

        int LineCount = Mathf.Clamp(Mathf.Max((int)Mathf.Ceil(multiple), 1), 1, line);

        float ySize = 150f;
        float spacing = 10f;
        float topPadding = 100f;
        float bottomPadding = 20f;

        float calculateValue = (ySize * LineCount) + (spacing * (LineCount - 1)) + topPadding + bottomPadding;

        content.sizeDelta = new Vector2(content.sizeDelta.x, calculateValue);

        for(int i = 0; i < line; i++)
        {
            if(i < LineCount)
            {
                float yPos = -topPadding - (i * (ySize + spacing));
                
                slotLineRects[i].anchoredPosition = new Vector2(0, yPos);  
                slotLineRects[i].gameObject.SetActive(true);
            }
            else
            {
                slotLineRects[i].gameObject.SetActive(false);
            }
        }
    }
    private void ChangedSortType(int index)
    {
        currentSortType = (SortType)index;

        Show(currentInventoryType);
    }

    private void Load()
    {
        foreach (var keyAndValue in inventories)
        {
            var inventorySc = keyAndValue.Value;

            inventorySc.Load();
        }

        Show(currentInventoryType);
        return;
    }

    #endregion

    #region ItemUse
    
    private void OnClickCharacterIcon(int characterId)
    {
        if(!currentChoose_Slot.IsItemExist) return;

        int currentUseItemId = currentChoose_Slot.Slot_ItemData.itemInfo.itemId;
        
        EventBus.Invoke<int, int>("On_UseItem", characterId, currentUseItemId);
        GetItem(currentUseItemId, -1);

        if(inventories.TryGetValue(InventoryType.Usable, out var inventory))
        {
            List<ItemHasInfo> lists = inventory.OnInventory();

            var itemInfo = inventory.GetItemById(currentUseItemId);
            
            if(itemInfo.data == null || itemInfo.itemAmount <= 0)
            {
                CloseUsePanel();

                ItemHasInfo nextSlot = default;
                int index = 0;

                for(int i = 0; i < lists.Count; i++)
                {
                    ItemHasInfo slot = lists[i];

                    if(slot.itemAmount > 0)
                    {
                        nextSlot = slot;
                        index = i;
                    }
                }

                if(nextSlot.itemAmount > 0)
                {
                    ShowInfo(index);  
                }
                else
                {
                    ShowInfo(-999, true);    
                    CloseUsePanel();
                }
            }
        }
    }
    #endregion

    #region Getter
    private bool IsItemExist(int instanceId)
    {
        inventories.TryGetValue(InventoryType.Equipment, out var inventory);

        return inventory.ItemExist(instanceId);
    }
    
    public List<ItemHasInfo> GetInventoryItem(InventoryType type)
    {   
        if (inventories.TryGetValue(type, out var inventory))
        {
            return inventory.OnInventory().ToList();
        }

        return null;
    }

    private Dictionary<int, ItemHasInfo> GetInventoryItem_Dic(InventoryType type)
    {   
        if (inventories.TryGetValue(type, out var inventory))
        {
            Dictionary<int, ItemHasInfo> newDic = new();
            List<ItemHasInfo> itemList = inventory.OnInventory();

            for(int i = 0; i < itemList.Count; i++)
            {
                ItemHasInfo info = itemList[i];
                newDic.Add(info.data.itemInfo.itemId, info);
            }

            return newDic;
        }

        return null;
    }

    private int GetItemSlotIndex(int itemId)
    {
        int itemIndex = -1;

        for(int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];

            if(slot.Slot_ItemData != null && slot.Slot_ItemData.itemInfo.itemId == itemId)
            {
                return i;
            }
        }

        return itemIndex;
    }
    #endregion
}

public partial class Inventory : UIClass
{
    private void LockItemUsePanel(bool isLock)
    {
        isLockItemUsePanel = isLock;
    }

    private void OpenUsePanel()
    {
        if(isLockItemUsePanel) return;

        if(currentChoose_Slot == null) return;

        useItemSystem.ActivePanel();

        if(GameState.IsTutorial())
            EventBus.Invoke("On_Inventory_UI_ItemUseClick");     
    }

    private void CloseUsePanel()
    {
        if(isLockItemUsePanel) return;
        
        useItemSystem.ClosePanel();
    }

    private bool IsUsePanelLock() => isLockItemUsePanel;

    private void LockTypePanel(bool isLock)
    {
        isLockItemTypePanel = isLock;
    }
    
    public override bool IsReady()
    {
        return isInit;
    }
    public override void Open()
    {
        inventoryMainPanel.SetActive(true);
    }

    public override void Close()
    {
        CloseUsePanel();
        inventoryMainPanel.SetActive(false);
    }
}

public abstract class ITypeInventory
{
    private List<int> savedInstanceIds = new();

    protected List<ItemHasInfo> slots = new();
    protected InventorySaver inventorySaver;
    public ITypeInventory(string path, string InventoryDirectoryPath = "")
    {
        if (string.IsNullOrEmpty(InventoryDirectoryPath) || InventoryDirectoryPath == "")
        {
            InventoryDirectoryPath = JsonUtil.Combine_Path(Application.persistentDataPath, "PlayerInventoryData");
        }

        inventorySaver = new(path, InventoryDirectoryPath);
    }
    public abstract List<ItemHasInfo> OnInventory();
    public virtual void GetItem(GiveItemInfo giveItemInfo, bool isShowItemGetPanel = true)
    {
        var itemData = DataLoader.GetData<ItemData>(DataType.Item, giveItemInfo.itemId);

        bool hasItemAlready = false;

        if(itemData.itemInfo.item_Type != InventoryType.Equipment)//이미 얻었다면 무기제외하고 쌓기
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var item = slots[i];

                if (item.data != null && item.data.itemInfo.itemId == giveItemInfo.itemId)
                {
                    item.itemAmount = Mathf.Max(0, item.itemAmount + giveItemInfo.amount);

                    if(item.itemAmount <= 0)
                    {
                        slots.RemoveAt(i);
                    }
                    else
                    {
                        slots[i] = item;
                        hasItemAlready = true;
                    }

                    break;
                }
            }
        }

        if (!hasItemAlready) //한번도 얻지않았을경우 
        {
            if(giveItemInfo.amount > 0)
            {
                ItemHasInfo newSlot = new ItemHasInfo();

                newSlot.data = itemData;

                newSlot.itemAmount = giveItemInfo.amount;

                if(itemData.itemInfo.item_Type == InventoryType.Equipment)
                {
                    newSlot.instanceId = RandomNumber.GetNumber(savedInstanceIds);

                    EventBus.Invoke<ItemHasInfo>("ObtainNewEquipment", newSlot);
                }
                else
                {
                    newSlot.instanceId = 0;
                }
                
                slots.Add(newSlot);
            }
        }
        
        if(isShowItemGetPanel) EventBus.Invoke<int, int>("ShowItemGetUI", giveItemInfo.itemId, giveItemInfo.amount);

        Save();
    }

    public virtual bool ItemExist(int instanceId)
    {
        return slots.FindIndex(item => item.instanceId == instanceId) != -1;    
    }

    public virtual ItemHasInfo GetItemById(int itemId)
    {
        for(int i = 0; i < slots.Count; i++)
        {
            ItemHasInfo curInfo = slots[i];

            if(curInfo.data.itemInfo.itemId == itemId)
                return curInfo;
        }

        return default;
    }

    public virtual void Save()
    {
        List<InventoryData> dataList = new();

        for(int i = 0; i < slots.Count; i++)
        {
            int id = slots[i].data.itemInfo.itemId;
            int itemAmount = slots[i].itemAmount;
            int instanceId = slots[i].instanceId;

            if(itemAmount <= 0) continue;

            InventoryData data = new()
            {
                itemId = id,
                itemAmount = itemAmount,
                instanceId = instanceId
            };

            dataList.Add(data);
        }

        inventorySaver.InventorySave(new InventoryDatas{
            data_List = dataList,
        });
    }

    public virtual void Load()
    {
        if (!inventorySaver.IsFileExist()) return;
        bool isHasException = false;
        
        List<InventoryData> dataList = inventorySaver.InventoryLoad();

        for(int i = 0; i < dataList.Count; i++)
        {
            InventoryData data = dataList[i];
            int itemId = data.itemId;
            int itemAmount = data.itemAmount;
            int instanceId = data.instanceId;

            ItemHasInfo newSlot = new ItemHasInfo();

            newSlot.data = DataLoader.GetData<ItemData>(DataType.Item, itemId);
            newSlot.itemAmount = itemAmount;

            if(newSlot.data.itemInfo.item_Type == InventoryType.Equipment && instanceId == 0)
            {
                isHasException = true;
                instanceId = RandomNumber.GetNumber(savedInstanceIds);
            }

            newSlot.instanceId = instanceId;
            
            savedInstanceIds.Add(instanceId);
            slots.Add(newSlot);
        }

        if(isHasException)
        {
            Save();
        }
    }
}

public class QuestInventory : ITypeInventory
{
    public QuestInventory(string path) : base(path) { }

    public override List<ItemHasInfo> OnInventory()
    {
        return slots;
    }

    public override void GetItem(GiveItemInfo giveItemInfo, bool isActivePanel = true)
    {
        base.GetItem(giveItemInfo, isActivePanel);
    }
}

public class UsableInventory : ITypeInventory
{
    public UsableInventory(string path) : base(path) { }
    public override List<ItemHasInfo> OnInventory()
    {
        return slots;
    }

    public override void GetItem(GiveItemInfo giveItemInfo, bool isActivePanel = true)
    {
        base.GetItem(giveItemInfo, isActivePanel);
    }
}

public class MaterialInventory : ITypeInventory
{
    public MaterialInventory(string path) : base(path) { }
    public override List<ItemHasInfo> OnInventory()
    {
        return slots;
    }

    public override void GetItem(GiveItemInfo giveItemInfo, bool isActivePanel = true)
    {
        base.GetItem(giveItemInfo, isActivePanel);
    }
}

public class EquipmentInventory : ITypeInventory
{
    public EquipmentInventory(string path) : base(path) { }
    public override List<ItemHasInfo> OnInventory()
    {
        return slots;
    }

    public override void GetItem(GiveItemInfo giveItemInfo,  bool isActivePanel = true)
    {
        base.GetItem(giveItemInfo, isActivePanel);
    }
}

public class ETCInventory : ITypeInventory
{
    public ETCInventory(string path) : base(path) { }
    public override List<ItemHasInfo> OnInventory()
    {
        return slots;
    }

    public override void GetItem(GiveItemInfo giveItemInfo,  bool isActivePanel = true)
    {
        base.GetItem(giveItemInfo, isActivePanel);
    }
}
public static class InventoryClassFactory
{
    public static ITypeInventory GetInventory(this object obj, InventoryType type)
    {
        switch(type)
        {
            case InventoryType.Quest :
                return new QuestInventory("Quest");
                
            case InventoryType.Usable :
                return new UsableInventory("Usable");

            case InventoryType.Material :
                return new MaterialInventory("Material");

            case InventoryType.Equipment :
                return new EquipmentInventory("Equipment");

            case InventoryType.ETC :
                return new ETCInventory("ETC");

            default :
                return null;
        }
    }
}