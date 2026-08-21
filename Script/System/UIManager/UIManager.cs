using UnityEngine;
using System.Collections.Generic;

public abstract class UIClass : MonoBehaviour
{
    private UIType uiType;
    protected void SetType(UIType type) => uiType = type;
    public virtual void OnEnable()
    {
        EventBus.Invoke<UIType, UIClass>("RegisterUI", uiType, this);
    }

    public abstract void Close();
    public abstract void Open();
    public abstract bool IsReady();
    protected virtual void OnClickCloseButton()
    {
        EventBus.Invoke<UIType>("OnClose_UI", uiType);
    }

    public virtual RectTransform GetRectTransform(UIRectName rectName) { return null; }
}

public enum UIType
{
    None,
    Setting,
    
    Quest,
    Craft,
    Inventory,
    Party,
    CharacterStatus,
}

public enum UIRectName
{
    CraftUI_RecipeButton = 0,
    CraftUI_AmountSlider = 1,
    CraftUI_CraftButton = 2,
    CraftUI_MaterialList = 3,

    QuestUI_ContextButton = 10,
    QuestUI_QuestAcceptButton = 11,

    CharacterGrowthUI_CharacterIcons = 20,
    CharacterGrowthUI_CharacterIcon = 21,
    CharacterGrowthUI_LevelUI_Level = 22,
    CharacterGrowthUI_LevelUI_ProgressBar = 23,
    CharacterGrowthUI_LevelUI_LevelUpButton = 24,
    CharacterGrowthUI_LevelUI_ExpItem = 25,
    CharacterGrowthUI_WeaponUI_WeaponMainPanel = 26,
    CharacterGrowthUI_WeaponUI_WeaponSelectionPanel = 27,
    CharacterGrowthUI_WeaponUI_WeaponIcon = 28,

    InventoryUI_QuestInventoryButton = 41,
    InventoryUI_UsableInventoryButton = 42,
    InventoryUI_MaterialInventoryButton = 43,
    InventoryUI_EquipmentInventoryButton = 44,
    InventoryUI_ETCInventoryButton = 45,
    InventoryUI_ItemUseButton = 46,
    InventoryUI_ItemUseTargetChoosePanel = 47,
    Inventory_UI_InventorySlotRect = 48,

    CharacterPartyUI_CharacterSlot1 = 60,
    CharacterPartyUI_CharacterSlot2 = 61,
    CharacterPartyUI_CharacterIcons = 62,
    CharacterPartyUI_CharacterIcon = 63,
    CharacterPartyUI_PartySaveButton = 64,
}

public class UIManager : MonoBehaviour
{
    private Dictionary<UIType, UIClass> uis = new();
    private Dictionary<UIType, bool> lockedUIs = new();
    private UIType _currentActiveUIType = UIType.None;
    private bool _globalLock = false;

    private void OnEnable()
    {
        EventBus.Sub<UIType, UIClass>("RegisterUI", OnRegister);
        EventBus.Sub<UIType>("On_Input_UI", OnInput);
        EventBus.Sub<UIType>("OnClose_UI", OnCloseUI);
        EventBus.Sub<UIType, bool>("UILock", OnLockUI);
        EventBus.Sub<bool>("Lock_All_UI", LockAll);

        EventBus.Sub_Func<UIType, UIRectName, RectTransform>("Get_UI_RectTransform", GetRectTransform);
    }

    private void OnDisable()
    {
        EventBus.UnSub<UIType, UIClass>("RegisterUI", OnRegister);
        EventBus.UnSub<UIType>("On_Input_UI", OnInput);
        EventBus.UnSub<UIType>("OnClose_UI", OnCloseUI);
        EventBus.UnSub<UIType, bool>("UILock", OnLockUI);
        EventBus.UnSub<bool>("Lock_All_UI", LockAll);

        EventBus.UnSub_Func<UIType, UIRectName, RectTransform>("Get_UI_RectTransform", GetRectTransform);
    }

    private void OnRegister(UIType type, UIClass ui)
    {
        if(!uis.ContainsKey(type))
        {
            uis.Add(type, ui);
            OnLockUI(type, false);
        }
    }

    private void OnInput(UIType type)
    {
        if(!_globalLock && _currentActiveUIType != UIType.None && _currentActiveUIType == type)
        {
            OnCloseUI(type);
        }
        else if(!_globalLock && _currentActiveUIType == UIType.None)
        {
            OnOpenUI(type);
        }
    }

    private void OnLockUI(UIType type, bool active)
    {
        lockedUIs[type] = active;
    }

    private void LockAll(bool isLock)
    {
        _globalLock = isLock;
    }
    
    private void OnOpenUI(UIType type)
    {
        if (lockedUIs.TryGetValue(type, out bool isLock) && isLock) return;

        if(uis.TryGetValue(type, out UIClass uiClass))
        {
            if(uiClass.IsReady())
            {
                uiClass.Open();

                _currentActiveUIType = type;

                if(!GameState.IsTutorial()) 
                    EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState",  GameStateType.Stop, GameEnableTimeSet.True);

                CursorManager.CursorActive(true);
            }
        } 
    }


    private void OnCloseUI(UIType type)
    {
        if(lockedUIs.TryGetValue(type, out bool isLock) && isLock) return;

        if(uis.TryGetValue(type, out UIClass uiClass))
        {
            if(uiClass.IsReady())
            {
                uiClass.Close();

                _currentActiveUIType = UIType.None;

                if(!GameState.IsTutorial())
                    EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState",  GameStateType.Run, GameEnableTimeSet.True);

                CursorManager.CursorActive(false);
            }
        }
    }

    private RectTransform GetRectTransform(UIType type, UIRectName rectName)
    {
        if(uis.TryGetValue(type, out UIClass uiClass))
        {
            RectTransform rectTransform = uiClass.GetRectTransform(rectName);

            if(rectTransform == null)
                Util.Log($"Error -- UIManager.cs GetRectTransform() UIType : {type} Can't Return Transform","red");

            return rectTransform;
        }

        Util.Log($"Error -- UIManager.cs GetRectTransform() Didn't Exist UIType","red");
        return null;
    }
}
