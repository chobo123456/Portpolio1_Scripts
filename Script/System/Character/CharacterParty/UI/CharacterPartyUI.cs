using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public struct CharacterSlotElementIconPayload
{
    public int index;
    public bool isDisable;
    public Sprite elementIcon;
}

public struct CharacterIconPayLoad
{
    public int _characterId;
    public Sprite _characterIcon;
    public Sprite _elementIcon;

    public bool isUsedRecent;
}


//초기화
public partial class CharacterPartyUI : UIClass
{
    public Transform partyUITransform;
    private Transform _iconParentTr;
    private Dictionary<int, CharacterPartyIcon> _characterPartyIconList = new();
    private Dictionary<int, RectTransform> _characterIconRectList = new();
    private Dictionary<int, Image> _elementIcons = new();
    private Dictionary<int, Image> _characterSlot = new();

    private GameObject _iconPrefab, _mainPanel, _warningPanel;

    private bool _isiconClickLock = false, _isslotClickLock = false;
    private Button _partySaveButton;
    private Coroutine _warningRoutine;
    private TextMeshProUGUI _warningCaseText;


    public override void OnEnable()
    {
        base.SetType(UIType.Party);
        base.OnEnable();
        this.RunRoutine(Booting());
    }
    
    #region Initialize
    private async void LoadIconPrefab()
    {
        _iconPrefab = await AddressableUtil.Load_Instant<GameObject>("Character_Party_Icon", this.GetCancelOnDestroy());
    }

    private void ReferenceUI()
    {
        _mainPanel = partyUITransform.gameObject;
        _warningPanel = partyUITransform.FindTarget("WarningPanel").gameObject;

        _warningCaseText = _warningPanel.transform.FindTarget("WarningText").GetComponent<TextMeshProUGUI>();

        _partySaveButton = partyUITransform.FindTarget("FinishButton").GetComponent<Button>();
        _partySaveButton.onClick.AddListener(() => base.OnClickCloseButton());

        _iconParentTr = partyUITransform.FindTarget("IconsContent");
    }

    private void Initialize_ElementIconList()
    {
        Transform findTr = partyUITransform.FindTarget("SlotButtons");
        for(int i = 0; i < 2; i++)
        {
            int index = i;

            Transform targetChooseImageTr = findTr.Find($"Character_{i + 1}");

            Image image = targetChooseImageTr.FindTarget("ElementIcon").GetComponent<Image>();
            if(image != null)
                _elementIcons.Add(index, image);

            var btn = targetChooseImageTr.GetComponent<Button>();
            if(btn != null) 
                btn.onClick.AddListener(() => OnChooseSlotClick(index));
        }
    }

    private void IntializeChooseImageList()
    {
        Transform findTr = partyUITransform.FindTarget("ChooseImages");

        int slotIndex = 1;

        for(int i = 0; i < findTr.childCount; i++)
        {
            Transform targetChooseImageTr = findTr.Find($"ChooseImage_{i + 1}");

            var comp = targetChooseImageTr.GetComponent<Image>();

            _characterSlot.Add(slotIndex, comp);

            slotIndex++;
        }

        SetChooseImage(false);
    }
    
    private void InitializeCharacterIcons(List<CharacterIconPayLoad> payLoadList)
    {
        Initialize_CharacterIconList(payLoadList);
        RecentUsedCharacterIcon_SetChoosed(payLoadList);
    }

    private void Initialize_CharacterIconList(List<CharacterIconPayLoad> payLoadList)
    {
        _characterPartyIconList.Clear();
        _characterIconRectList.Clear();

        for(int i = 0; i < payLoadList.Count; i++)
        {
            NewCharacterObtain(payLoadList[i]);
        }
    }

    private void RecentUsedCharacterIcon_SetChoosed(List<CharacterIconPayLoad> payLoadList)
    {
        for(int i = 0; i < payLoadList.Count; i++)
        {
            CharacterIconPayLoad payLoad = payLoadList[i];

            if(payLoad.isUsedRecent)
                ShowChooseIcon(payLoad._characterId);
        }
    }

    #endregion

    IEnumerator Booting()
    {
        LoadIconPrefab();
        ReferenceUI();
        Initialize_ElementIconList();
        IntializeChooseImageList();  
        EventSubscribe(true);
        
        yield return new WaitUntil(() => _iconPrefab != null);

        EventBus.Invoke("Party_UI_LocalReady");
    }

    private void EventSubscribe(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<List<CharacterIconPayLoad> >("Party_UI_InitializeIcons", InitializeCharacterIcons);
            EventBus.Sub<CharacterSlotElementIconPayload>("Party_UI_UpdateSlotElementIcon", SlotElementIconUpdate);
            EventBus.Sub<CharacterIconPayLoad>("Party_UI_NewCharacterObtain", NewCharacterObtain);

            EventBus.Sub<int>("Party_UI_UnShowChooseIcon", UnShowChooseIcon);
            EventBus.Sub<int>("Party_UI_ShowChooseIcon", ShowChooseIcon);
            EventBus.Sub<int>("Party_UI_ShowWarningPanel", ShowWarningPanel);
            
            EventBus.Sub<bool>("Party_UI_LockCharacterIconClick", LockIconClick);
            EventBus.Sub<bool>("Party_UI_LockSlotClick", LockSlotClick);

            EventBus.Sub("Party_UI_SelectIcon", OnSelectIcon);
            EventBus.Sub("Party_UI_UnSelectIcon", OnUnSelectIcon);
            EventBus.Sub("Party_UI_OnClose", OnClose);
            
            EventBus.Sub_Func<int, RectTransform>("Party_UI_CharacterIconRect", GetCharacterIconRect);
        }
        else
        {
            EventBus.UnSub<List<CharacterIconPayLoad> >("Party_UI_InitializeIcons", InitializeCharacterIcons);
            EventBus.UnSub<CharacterIconPayLoad>("Party_UI_NewCharacterObtain", NewCharacterObtain);
            EventBus.UnSub<CharacterSlotElementIconPayload>("Party_UI_UpdateSlotElementIcon", SlotElementIconUpdate);

            EventBus.UnSub<int>("Party_UI_UnShowChooseIcon", UnShowChooseIcon);
            EventBus.UnSub<int>("Party_UI_ShowChooseIcon", ShowChooseIcon);
            EventBus.UnSub<int>("Party_UI_ShowWarningPanel", ShowWarningPanel);

            EventBus.UnSub<bool>("Party_UI_LockCharacterIconClick", LockIconClick);
            EventBus.UnSub<bool>("Party_UI_LockSlotClick", LockSlotClick);

            EventBus.UnSub("Party_UI_SelectIcon", OnSelectIcon);
            EventBus.UnSub("Party_UI_UnSelectIcon", OnUnSelectIcon);
            EventBus.UnSub("Party_UI_OnClose", OnClose);

            EventBus.UnSub_Func<int, RectTransform>("Party_UI_CharacterIconRect", GetCharacterIconRect);
        }
    }
   
    private void OnDisable()
    {
        EventSubscribe(false);
    }

    public override RectTransform GetRectTransform(UIRectName rectName)
    {
        switch(rectName)
        {
            case UIRectName.CharacterPartyUI_CharacterSlot1:
                return partyUITransform.FindTarget("SlotButtons").Find($"Character_{1}").GetComponent<RectTransform>();

            case UIRectName.CharacterPartyUI_CharacterSlot2:
                return partyUITransform.FindTarget("SlotButtons").Find($"Character_{2}").GetComponent<RectTransform>();

            case UIRectName.CharacterPartyUI_CharacterIcons:
                return partyUITransform.FindTarget("CharacterIconParent").GetComponent<RectTransform>();

            case UIRectName.CharacterPartyUI_PartySaveButton:
                return _partySaveButton.GetComponent<RectTransform>();
        }

        return null;
    }

    private RectTransform GetCharacterIconRect(int characterId)
    {
        if(_characterIconRectList.TryGetValue(characterId, out RectTransform chracterIconRect))
        {
            return chracterIconRect;
        }

        Util.Log($"Error -- CharacterPartyUI.cs GetCharacterIconRect() Didn't Exist characterId","red");
        return null;
    }
}

//Input
public partial class CharacterPartyUI : UIClass
{
    private void NewCharacterObtain(CharacterIconPayLoad payload)
    {
        GameObject newIcon = Instantiate(_iconPrefab);
        newIcon.name = $"CharacterIcon_{payload._characterId}";

        newIcon.transform.SetParent(_iconParentTr);

        CharacterPartyIcon icon = new CharacterPartyIcon(newIcon.transform, payload._characterIcon, payload._elementIcon);
        
        Button btn = newIcon.GetComponentInChildren<Button>();
        if(btn != null)
            btn.onClick.AddListener(() => OnCharacterIconClick(payload._characterId));

        _characterPartyIconList.Add(payload._characterId, icon);
        _characterIconRectList.Add(payload._characterId, newIcon.GetComponent<RectTransform>());
    }

    private void OnCharacterIconClick(int id)
    {
        if(_isiconClickLock) return;

        EventBus.Invoke<int>("Party_UI_IconClick", id);
    }

    private void OnChooseSlotClick(int index)
    {
        if(_isslotClickLock) return;
        
        EventBus.Invoke<int>("Party_UI_SlotClick", index);
    }

}

//Output
public partial class CharacterPartyUI : UIClass
{
    private void UnShowChooseIcon(int characterId)
    {
        if(_characterPartyIconList.TryGetValue(characterId, out CharacterPartyIcon icon))
            icon.UnShowChooseImage();
    }

    private void ShowChooseIcon(int characterId)
    {
        if(_characterPartyIconList.TryGetValue(characterId, out CharacterPartyIcon icon))
            icon.ShowChooseImage();
    }

    private void OnUnSelectIcon()
    {
        SetChooseImage(false);
    }
    
    private void OnSelectIcon()
    {
        SetChooseImage(true);
    }

    private void SetChooseImage(bool active)
    {
        foreach(var slot in _characterSlot)
        {
            slot.Value.enabled = active;
        }
    }

    private void SlotElementIconUpdate(CharacterSlotElementIconPayload payload)
    {
        if(_elementIcons.TryGetValue(payload.index, out var icon))
        {
            if(payload.isDisable)
            {
                icon.enabled = false;
            }
            else
            {
                icon.sprite = payload.elementIcon;        
                icon.enabled = true;
            }
        }
    }

    //경고 패널
    private void ShowWarningPanel(int warningCase)
    {
        SetChooseImage(false); 
        _warningRoutine = this.RunRoutine(WariningPanelOpen(warningCase), _warningRoutine);
    }

    IEnumerator WariningPanelOpen(int warningCase)
    {
        SetCaseText(warningCase);

        _warningPanel.SetActive(true);

        yield return YieldUtil.WaitForSecondsRealtime(1f);

        _warningPanel.SetActive(false);
    }

    private void SetCaseText(int warningCase)
    {
        if(warningCase == 1)
        {
            _warningCaseText.SetText("Need Any Character In Your Party");
        }
        else if(warningCase == 2)
        {
            _warningCaseText.SetText("Need Any Character Has Hp");
        }
        else
        {
            _warningCaseText.SetText("-- Exeception Detect --");
        }
    }
}

public partial class CharacterPartyUI : UIClass
{
    private void LockIconClick(bool isLock)
    {
        _isiconClickLock = isLock;
    }

    private void LockSlotClick(bool isLock)
    {
        _isslotClickLock = isLock;
    }

    public override bool IsReady()
    {
        return EventBus.Invoke_Func<bool>("Party_System_TryClose") && _mainPanel != null;
    }

    public override void Open()
    {
        _mainPanel.SetActive(true);
        
        EventBus.Invoke<bool>("SetPartyCam", true);
    }

    public override void Close()
    {
        EventBus.Invoke("Party_UI_CloseClick");
    }

    private void OnClose()
    {
        SetChooseImage(false);
        _mainPanel.SetActive(false);
    }
}