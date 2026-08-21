using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class ExpItemSlotData
{
    public float expValue;
    public int selectedAmount;
    public int maxItemAmount;
}

public struct CharacterVariableStat
{
    public float defensive;
    public float maxHp;
    public float baseAttack;
}

public class LevelSystem
{
    private List<(int id, int amount)> runtimeUsingItemList = new();
    private Dictionary<int, ExpItemSlotData> expItemSlots = new();

    private int runTimeUpgradeLevel = 0;
    private float runTimeUpgradeProgress = 0f;

    private const string 
        GetInventoryEventName = "Inventory_System_GetInventory", 
        GetCharacterLevelEventName = "GetCharacterLevel", 
        GetCharacterLevelProgressEventName = "GetCharacterLevelProgress", 
        CharacterLevelUpgradeEventName = "OnCharacterLevelUpgrade";

    public void OnEnable()
    {
        InitializeList();
    }

    private void InitializeList()
    {
        expItemSlots = new()
        {
            {1000000, new ExpItemSlotData{ expValue = (DataLoader.GetData<ItemData>(DataType.Item, 1000000) as ExpItemData).expValue,   selectedAmount = 0 }},
            {1000001, new ExpItemSlotData{ expValue = (DataLoader.GetData<ItemData>(DataType.Item, 1000001) as ExpItemData).expValue,   selectedAmount = 0 }},
            {1000002, new ExpItemSlotData{ expValue = (DataLoader.GetData<ItemData>(DataType.Item, 1000002) as ExpItemData).expValue,   selectedAmount = 0 }},
            {1000003, new ExpItemSlotData{ expValue = (DataLoader.GetData<ItemData>(DataType.Item, 1000003) as ExpItemData).expValue,   selectedAmount = 0 }},
        };
    }

    private float GetCalculateExp()
    {
        runtimeUsingItemList.Clear();

        float calAllExpAmount = 0;
        
        foreach(var map in expItemSlots)
        {
            var item = map.Value;
            int choosedAmount   = item.selectedAmount; 
        
            if(choosedAmount > 0)
            {
                var itemId = map.Key;

                float expValue  = item.expValue;
                float expAmount = expValue * choosedAmount;
                calAllExpAmount += expAmount;

                runtimeUsingItemList.Add((itemId, -choosedAmount));
            }
        }

        return calAllExpAmount;
    }

    private (int level, float currentProgress, float maxProgress) CalculateLevelAndProgress(int characterId)
    {
        int originLevel         = GetCurrentCharacterLevel(characterId);
        int maxLevel            = GetCharacterMaxLevel(characterId);
        float originProgress    = GetCharacterLevelProgress(characterId);

        //현재 사용한 아이템의 최종 경험치 수 + 이월된 값
        float totalExp = GetCalculateExp() + originProgress;

        int tempUpgradeLevel = 0;
        float remainExp = totalExp;
        float currentLevelMaxExp = 0f;

        var characterData = DataLoader.GetData<CharacterData>(DataType.Character, characterId);

        while(true)
        {
            int checkLevel = originLevel + tempUpgradeLevel;

            //올리는값이 최대 레벨이라면 최대레벨의 최대 경험치수를 강제로 맞춤
            if (checkLevel >= maxLevel) 
            {
                currentLevelMaxExp = characterData.levelStep.GetLevelUpgradeAmount(maxLevel);
                remainExp = currentLevelMaxExp;
                break;
            }

            // 다음 업글까지 필요한 경험치 수를 가져옴
            currentLevelMaxExp = characterData.levelStep.GetLevelUpgradeAmount(checkLevel); 

            // 남은 경험치가 필요 경험치 이상일시 업그레이드 값을 올리고 남은경험치에서 값을 뺌
            if (remainExp >= currentLevelMaxExp) 
            {
                remainExp -= currentLevelMaxExp;
                tempUpgradeLevel++;
            }
            else // 남은 경험치가 이제 없다면 멈춤
            {
                break;
            }
        }
        
        runTimeUpgradeLevel = tempUpgradeLevel;
        runTimeUpgradeProgress = remainExp;

        return (tempUpgradeLevel, remainExp, currentLevelMaxExp);
    }

    private void ApplyLevel(int characterId, int upgradeLevel, float upgradeProgress)
    {
        EventBus.Invoke<int, int, float>(CharacterLevelUpgradeEventName, characterId, upgradeLevel, upgradeProgress);
        EventBus.Invoke("On_LevelUpgrade");
    }

    private void DeleteSelectedItem()
    {
        for(int i = 0; i < runtimeUsingItemList.Count; i++)
        {
            int id      = runtimeUsingItemList[i].id;
            int amount  = runtimeUsingItemList[i].amount;
            EventBus.Invoke<int, int, bool>("GetItem", id, amount, false);
        }
    }

    private int GetCharacterMaxLevel(int characterId)
    {
        var data = DataLoader.GetData<CharacterData>(DataType.Character, characterId);
        int lastIndex = data.levelStep.levelSteps.Length - 1;
        var lastStep = data.levelStep.levelSteps[lastIndex];
        return lastStep.levelAmount;
    }

    private int GetCurrentCharacterLevel(int characterId)
    {
        return EventBus.Invoke_Func<int, int>(GetCharacterLevelEventName, characterId);
    }

    private float GetCharacterLevelProgress(int characterId)
    {
        return EventBus.Invoke_Func<int, float>(GetCharacterLevelProgressEventName, characterId);
    }

    private float GetMaxProgressAmount(int characterId)
    {
        int level = GetCurrentCharacterLevel(characterId);
        var characterData = DataLoader.GetData<CharacterData>(DataType.Character, characterId);
        float maxProgress = characterData.levelStep.GetLevelUpgradeAmount(level);

        return maxProgress;
    }

    private CharacterVariableStat GetCharacterStat(int characterId)
    {
        int level = GetCurrentCharacterLevel(characterId);
        var characterData = DataLoader.GetData<CharacterData>(DataType.Character, characterId);

        float currentDefensive = characterData.levelStep.GetDefensiveUseLevel(level);
        float currentMaxHp = characterData.levelStep.GetMaxHpUseLevel(level);
        float currentbaseAttack = characterData.levelStep.GetBaseAttackDamageUseLevel(level);

        return new CharacterVariableStat{ defensive = currentDefensive, maxHp = currentMaxHp, baseAttack = currentbaseAttack };
    }


    //Payload
    public LevelViewPayLoad GetViewPayLoad(int characterId)
    {
        int maxLevel = GetCharacterMaxLevel(characterId);
        int originLevel = GetCurrentCharacterLevel(characterId);

        float currentProgressAmount = GetCharacterLevelProgress(characterId);
        float maxProgressAmount = GetMaxProgressAmount(characterId);

        CharacterVariableStat currentStat = GetCharacterStat(characterId);

        LevelViewPayLoad payLoad = new()
        {
            level = originLevel,

            currentExpAmount = currentProgressAmount,
            maxExpAmount = maxProgressAmount,
            expPercentGage = currentProgressAmount / maxProgressAmount,

            defensive = currentStat.defensive,
            maxHp = currentStat.maxHp,
            baseAttack = currentStat.baseAttack,

            isMaxLevel = originLevel >= maxLevel,
        };

        return payLoad;
    }

    public LevelPreviewPayload GetPreViewPayLoad(int characterId)
    {
        int originLevel = GetCurrentCharacterLevel(characterId);
        int maxLevel = GetCharacterMaxLevel(characterId);

        (int upgradedLevel, float currentProgress, float maxProgress) = CalculateLevelAndProgress(characterId);
        
        LevelPreviewPayload payLoad = new()
        {
            originLevel = originLevel,
            upgradeLevel = upgradedLevel,

            remainExp = currentProgress,
            currentLevelMaxExp = maxProgress,
            currentProgress = currentProgress / maxProgress,
            
            isOverMaxLevel = originLevel + upgradedLevel >= maxLevel,
            isUpgrading = upgradedLevel > 0
        };

        return payLoad;
    }

    public List<ExpItemSlotInitPayload> GetExpItemSlotInitializePayLoad()
    {
        List<ExpItemSlotInitPayload> payLoadList = new();

        foreach(var slot in expItemSlots)
        {
            int itemId = slot.Key;
            var itemData = DataLoader.GetData<ItemData>(DataType.Item, itemId);

            payLoadList.Add(new()
            {
                itemId = itemId,
                expItemIcon = itemData.itemInfo.itemIcon,
                itemTier =  itemData.itemTier 
            });
        }

        return payLoadList;
    }

    
    //Publics
    public void UpdateExpItemMaxAmounts()
    {
        Dictionary<int, ItemHasInfo> items = 
            EventBus.Invoke_Func<InventoryType, List<ItemHasInfo>>(GetInventoryEventName, InventoryType.ETC).ToDictionary(key => key.data.itemInfo.itemId, value => value);

        foreach(var slot in expItemSlots)
        {
            int expItemId = slot.Key;
            var expItemSlot = slot.Value;

            if(items.TryGetValue(expItemId, out ItemHasInfo info))
                expItemSlot.maxItemAmount = info.itemAmount;
            else
                expItemSlot.maxItemAmount = 0;
        }
    }

    public void OnChangeLevelUpTarget()
    {
        runTimeUpgradeLevel = 0;
        runTimeUpgradeProgress = 0f;
        runtimeUsingItemList.Clear();

        foreach(var map in expItemSlots)
        {
            var slot = map.Value;
            slot.selectedAmount = 0;
        }
    }

    public void AddExp(int expItemId)
    {
        if(expItemSlots.TryGetValue(expItemId, out ExpItemSlotData slot))
        {
            slot.selectedAmount = Mathf.Min(slot.selectedAmount + 1, slot.maxItemAmount);
        }
    }

    public void RemoveExp(int expItemId)
    {
        if(expItemSlots.TryGetValue(expItemId, out ExpItemSlotData slot))
        {
            slot.selectedAmount = Mathf.Max(0, slot.selectedAmount - 1);
        }
    }

    public bool TryUpgrade(int characterId)
    {
        if(runTimeUpgradeProgress > 0f || runTimeUpgradeLevel > 0)
        {
            int originalLevel = GetCurrentCharacterLevel(characterId);
            int upgradeTargetLevel  = originalLevel + runTimeUpgradeLevel;
            float finalExpProgress = runTimeUpgradeProgress;

            ApplyLevel(characterId, upgradeTargetLevel, finalExpProgress);
            DeleteSelectedItem();
            UpdateExpItemMaxAmounts();

            return true;
        }

        return false;
    }

    public ExpItemSlotData GetItemSlot(int itemId)
    {
        if(expItemSlots.TryGetValue(itemId, out var item))
        {
            return item;
        }

        return (ExpItemSlotData) default;
    }
    
    public List<int> GetExpItemIds()
    {
        List<int> itemIds = new();

        foreach(var slot in expItemSlots)
        {
            int itemId = slot.Key;
            itemIds.Add(itemId);
        }

        return itemIds;
    }

}

public class LevelUIHandler
{
    private int runTimeSelectedCharacterId = 0;
    private LevelSystem _levelSystem;

    public void InjectParameter(LevelSystem levelSystem)
    {
        _levelSystem = levelSystem;
    }

    public void OnEnable()
    {
        SubscribeEvent(true);
    }

    public void OnDisable()
    {
        SubscribeEvent(false);
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {   
            EventBus.Sub<int>("Growth_UI_ChangeSelectedCharacter", SelectedCharacterId);  
            EventBus.Sub<int>("AddExp", OnAddExp);
            EventBus.Sub<int>("RemoveExp", OnRemoveExp);
            EventBus.Sub("UpgradeLevel", OnUpgrade);
        }
        else
        {
            EventBus.UnSub<int>("Growth_UI_ChangeSelectedCharacter", SelectedCharacterId);  
            EventBus.UnSub<int>("AddExp", OnAddExp);
            EventBus.UnSub<int>("RemoveExp", OnRemoveExp);
            EventBus.UnSub("UpgradeLevel", OnUpgrade);
        }
    }

    private void SelectedCharacterId(int characterId)
    {
        _levelSystem.OnChangeLevelUpTarget();
        _levelSystem.UpdateExpItemMaxAmounts();

        UpdateSlots();

        runTimeSelectedCharacterId = characterId;
        
        EventBus.Invoke<LevelViewPayLoad>("Level_UI_ClickCharacterIcon", _levelSystem.GetViewPayLoad(runTimeSelectedCharacterId));
    }

    private void OnAddExp(int expItemId)
    {
        _levelSystem.AddExp(expItemId);
        ExpItemSlotData slotData = _levelSystem.GetItemSlot(expItemId);

        InvokeUpdateUI(expItemId, slotData.selectedAmount, slotData.maxItemAmount);
        UpdatePreview();
    }

    private void OnRemoveExp(int expItemId)
    {
        _levelSystem.RemoveExp(expItemId);
        ExpItemSlotData slotData = _levelSystem.GetItemSlot(expItemId);

        InvokeUpdateUI(expItemId, slotData.selectedAmount, slotData.maxItemAmount);
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        EventBus.Invoke<LevelPreviewPayload>("Level_UI_UpdatePreview", _levelSystem.GetPreViewPayLoad(runTimeSelectedCharacterId));
    }

    private void OnUpgrade()
    {
        if(_levelSystem.TryUpgrade(runTimeSelectedCharacterId))
        {
            SelectedCharacterId(runTimeSelectedCharacterId);   
        }
    }

    public void UpdateSlots()
    {
        List<int> itemIds = _levelSystem.GetExpItemIds();
        for(int i = 0; i < itemIds.Count; i++)
        {
            int expItemId = itemIds[i];
            ExpItemSlotData slotData = _levelSystem.GetItemSlot(expItemId);
            InvokeUpdateUI(expItemId, slotData.selectedAmount, slotData.maxItemAmount);
        }
    }

    private void InvokeUpdateUI(int expItemId, int selectedAmount, int currentAmount)
    {
        EventBus.Invoke<int, int, int>("Level_UI_UpdateSlot", expItemId, selectedAmount, currentAmount);
    }
}

public class LevelManager : MonoBehaviour
{
    private LevelSystem _levelSystem;
    private LevelUIHandler _levelUIHandler;

    private bool isReadyLocalUI = false;

    private void OnEnable()
    {
        SubscribeEvent(true);
        this.RunRoutine(Booting());
    }

    private IEnumerator Booting()
    {
        _levelSystem   = new();
        _levelUIHandler = new();

        yield return new WaitUntil(() => 
            LoadStatus.IsReady_Inventory
            && EventBus.Invoke_Func<bool>("FinishLoadCharacterData"));
       
        _levelSystem.OnEnable();
        
        _levelUIHandler.InjectParameter(_levelSystem);
        _levelUIHandler.OnEnable();

        yield return new WaitUntil(() => isReadyLocalUI);

        _levelSystem.UpdateExpItemMaxAmounts();
        EventBus.Invoke<List<ExpItemSlotInitPayload>>("Level_UI_Initialize", _levelSystem.GetExpItemSlotInitializePayLoad());
        _levelUIHandler.UpdateSlots();
    }   

    private void SubscribeEvent(bool isSubcribe)
    {
        if(isSubcribe)
        {
            EventBus.Sub("Level_UI_Ready", OnLocalUIReady);
        }
        else
        {
            EventBus.UnSub("Level_UI_Ready", OnLocalUIReady);
        }
    }

    private void OnLocalUIReady()
    {
        isReadyLocalUI = true;
    }
}
