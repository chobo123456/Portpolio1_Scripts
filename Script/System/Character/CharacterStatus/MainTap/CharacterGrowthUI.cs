using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System.Linq;


public enum CharacterGrowthSystemType
{
    Upgrade,
    Weapon
}

//초기화 
public partial class CharacterGrowthUI : UIClass
{
    private Dictionary<int, RectTransform> _characterIconRectList = new();
    public Transform mainPanelTr;
    private Transform _iconParent;
    private GameObject _characterIconButtonPrefab, _mainPanel;
    private Image _characterSprite;
    private bool _isAbleToShow = false, _isLockChangePanel = false, _isLockSelectCharacter = false;

    public override void OnEnable()
    {
        base.SetType(UIType.CharacterStatus);
        base.OnEnable();
        this.RunRoutine(Booting());
    }

    private async void LoadPrefabs()
    {
        _characterIconButtonPrefab =  await AddressableUtil.Load_Instant<GameObject>("Character_Info_Button", this.GetCancelOnDestroy());
    }

    private void Initialize_Objects()
    {
        _mainPanel = mainPanelTr.gameObject;
        _mainPanel.SetActive(false);

        _iconParent = mainPanelTr.FindTarget("Character_Icons");

        _characterSprite = mainPanelTr.FindTarget("Character_Sprite").GetComponent<Image>();

        Button weaponSettingUIPanelButton = mainPanelTr.FindTarget("EquipWeaponButton").GetComponent<Button>();
        weaponSettingUIPanelButton.onClick.AddListener(() => ChangePanelType(CharacterGrowthSystemType.Weapon));

        Button levelUpgradUIPanelButton = mainPanelTr.FindTarget("LevelUpgradeButton").GetComponent<Button>();
        levelUpgradUIPanelButton.onClick.AddListener(() => ChangePanelType(CharacterGrowthSystemType.Upgrade));

        Button closeButton = mainPanelTr.FindTarget("CloseButton").GetComponent<Button>();
        closeButton.onClick.AddListener(() => base.OnClickCloseButton());

        ChangePanelType(CharacterGrowthSystemType.Upgrade);
    }
    
    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<Dictionary<int, Sprite>>("Growth_UI_Initialize", InitializePool);
            EventBus.Sub<int, Sprite>("Growth_UI_ObtainNewCharacter", ObtainNewCharacter);
            EventBus.Sub<Sprite>("Growth_UI_SetCharacterIcon", SetCharacterIcon);

            //Tutorial
            EventBus.Sub<bool>("Growth_UI_Lock_ChangeTypeSystem", LockChangePanel);
            EventBus.Sub<bool>("Growth_UI_Lock_ChangeCharacterSystem", LockSelect);

            EventBus.Sub_Func<int, RectTransform>("Growth_UI_CharacterIconRect", GetCharacterIconRect);
        }
        else
        {
            EventBus.UnSub<Dictionary<int, Sprite>>("Growth_UI_Initialize", InitializePool);
            EventBus.UnSub<int, Sprite>("Growth_UI_ObtainNewCharacter", ObtainNewCharacter);
            EventBus.UnSub<Sprite>("Growth_UI_SetCharacterIcon", SetCharacterIcon);

            //Tutorial
            EventBus.UnSub<bool>("Growth_UI_Lock_ChangeTypeSystem", LockChangePanel);
            EventBus.UnSub<bool>("Growth_UI_Lock_ChangeCharacterSystem", LockSelect);

            EventBus.UnSub_Func<int, RectTransform>("Growth_UI_CharacterIconRect", GetCharacterIconRect);
        }
    }

    IEnumerator Booting()
    {
        LoadPrefabs();  
        Initialize_Objects();
        SubscribeEvent(true);

        yield return new WaitUntil(() => _characterIconButtonPrefab != null);

        EventBus.Invoke("Growth_UI_Ready");

        _isAbleToShow = true;
    }

    private void OnDisable()
    {
        LoadStatus.SetStatus(ManagerType.GrowthTap, false);

        SubscribeEvent(false);
    }

    //GetRectTransform
    public override RectTransform GetRectTransform(UIRectName rectName)
    {
        switch(rectName)
        {
            case UIRectName.CharacterGrowthUI_CharacterIcon:
                return mainPanelTr.FindTarget("CharacterIcons").GetComponent<RectTransform>();

            case UIRectName.CharacterGrowthUI_LevelUI_Level:
            case UIRectName.CharacterGrowthUI_LevelUI_ProgressBar:
            case UIRectName.CharacterGrowthUI_LevelUI_LevelUpButton:
            case UIRectName.CharacterGrowthUI_LevelUI_ExpItem:
                return EventBus.Invoke_Func<UIRectName, RectTransform>("Level_UI_GetRectTransform", rectName);

            case UIRectName.CharacterGrowthUI_WeaponUI_WeaponMainPanel:
            case UIRectName.CharacterGrowthUI_WeaponUI_WeaponSelectionPanel:
            case UIRectName.CharacterGrowthUI_WeaponUI_WeaponIcon:
                return EventBus.Invoke_Func<UIRectName, RectTransform>("Weapon_UI_GetRectTransform", rectName);
        }
        
        return null;
    }

    private RectTransform GetCharacterIconRect(int characterId)
    {
        if(_characterIconRectList.TryGetValue(characterId, out RectTransform chracterIconRect))
            return chracterIconRect;

        Util.Log($"Error -- CharacterGrowthUI.cs GetCharacterIconRect() Didn't Exist characterId","red");
        return null;
    }
}

public partial class CharacterGrowthUI
{
    private void InitializePool(Dictionary<int, Sprite> characterId_spriteMap)
    {
        _characterIconRectList.Clear();

        foreach(var map in characterId_spriteMap)
        {
            int id = map.Key;
            Sprite characterIcon = map.Value;

            if(id <= 0) continue;

            ObtainNewCharacter(id, characterIcon);
        }
    }

    private void ObtainNewCharacter(int newCharacterId, Sprite characterIcon)
    {
        GameObject newObj = Instantiate(_characterIconButtonPrefab);
        newObj.transform.SetParent(_iconParent);

        Button button = newObj.GetComponent<Button>();
        if (button == null) return;

        newObj.transform.FindTarget("Icon").GetComponent<Image>().sprite = characterIcon;

        _characterIconRectList.Add(newCharacterId, newObj.GetComponent<RectTransform>());

        button.onClick.AddListener(() => OnSelectCharacter(newCharacterId));
    }

    private void OnSelectCharacter(int id)
    {
        if(_isLockSelectCharacter) return;
        
        EventBus.Invoke<int>("Growth_UI_TryChangeSelectCharacter", id);  
    }

    private void SetCharacterIcon(Sprite characterIcon)
    {
        _characterSprite.sprite = characterIcon;
    }

    private void ChangePanelType(CharacterGrowthSystemType type)
    {
        if(_isLockChangePanel) return;

        EventBus.Invoke<CharacterGrowthSystemType>("Status_UI_SetActiveMainPanel", type);
    }
}

public partial class CharacterGrowthUI
{
    private void LockSelect(bool isLock)
    {
        _isLockSelectCharacter = isLock;
    }

    private void LockChangePanel(bool isLock)
    {
        _isLockChangePanel = isLock;
    }

    public override bool IsReady()
    {
        return _isAbleToShow;
    }
    
    public override void Open()
    {
        EventBus.Invoke("Growth_UI_Open");

        ActiveMainPanel(true);
    }

    public override void Close()
    {
        ActiveMainPanel(false);
    }

    private void ActiveMainPanel(bool isActive)
    {
        _mainPanel.SetActive(isActive);
        EventBus.Invoke("Status_UI_SelectionPanelClose");
    }
}