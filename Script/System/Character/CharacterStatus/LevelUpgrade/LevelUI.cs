using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public struct ExpItemSlotInitPayload
{
    public int itemId;
    public Sprite expItemIcon;
    public ItemTier itemTier;
}

public struct LevelViewPayLoad
{
    public float level;

    public float currentExpAmount;
    public float maxExpAmount;
    public float expPercentGage;

    public float defensive;
    public float maxHp;
    public float baseAttack;

    public bool isMaxLevel;
}

public struct LevelPreviewPayload
{
    public int originLevel;
    public int upgradeLevel;

    public float remainExp;
    public float currentLevelMaxExp;
    public float currentProgress;

    public bool isOverMaxLevel;
    public bool isUpgrading;
}

public partial class LevelUI : MonoBehaviour
{
    public Transform parentTr;
    private TextMeshProUGUI _level, _currentProgressText, _maxProgressText;
    private TextMeshProUGUI _defensive, _maxHp, _atk;
    private Slider _progressBar;
    private Button _levelUpButton;
    private Dictionary<int, ExpItemSlot> _slots = new();
    private GameObject _slotPrefab, _slotPanel, _canNotUpgradeAbleTextPanel, _progressBarPanel, _upgradePanel;
    private Transform _slotParentTr;

    private void OnEnable()
    {   
        this.RunRoutine(Booting());
    }

    private IEnumerator Booting()
    {
        LoadExpItemSlotPrefab();
        ReferenceUI();
        AddButtonListener();
        SubscribeEvent(true);

        yield return new WaitUntil(() => _slotPrefab != null);

        EventBus.Invoke("Level_UI_Ready");
    }

    private async void LoadExpItemSlotPrefab()
    {
        _slotPrefab = await AddressableUtil.Load_Instant<GameObject>("expItemSlot", this.GetCancelOnDestroy());
    }
    
    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<List<ExpItemSlotInitPayload>>("Level_UI_Initialize", InitializeSlots);
            EventBus.Sub<LevelPreviewPayload>("Level_UI_UpdatePreview", UpdatePreview);
            EventBus.Sub<LevelViewPayLoad>("Level_UI_ClickCharacterIcon", OnSelectCharacterIcon);
            EventBus.Sub<int, int, int>("Level_UI_UpdateSlot", UpdateSlot);
            EventBus.Sub<CharacterGrowthSystemType>("Status_UI_SetActiveMainPanel", SetActiveMainPanel);

            EventBus.Sub_Func<UIRectName, RectTransform>("Level_UI_GetRectTransform", GetRectTransform);
        }
        else
        {
            EventBus.UnSub<List<ExpItemSlotInitPayload>>("Level_UI_Initialize", InitializeSlots);
            EventBus.UnSub<LevelPreviewPayload>("Level_UI_UpdatePreview", UpdatePreview);
            EventBus.UnSub<LevelViewPayLoad>("Level_UI_ClickCharacterIcon", OnSelectCharacterIcon);
            EventBus.UnSub<int, int, int>("Level_UI_UpdateSlot", UpdateSlot);
            EventBus.UnSub<CharacterGrowthSystemType>("Status_UI_SetActiveMainPanel", SetActiveMainPanel);

            EventBus.UnSub_Func<UIRectName, RectTransform>("Level_UI_GetRectTransform", GetRectTransform);
        }
    }

    private void ReferenceUI()
    {
        _level = parentTr.FindTarget("LevelAmount").GetComponent<TextMeshProUGUI>();
        _currentProgressText = parentTr.FindTarget("CurrentLevelUpAmount").GetComponent<TextMeshProUGUI>();
        _maxProgressText = parentTr.FindTarget("NextLevelUpAmount").GetComponent<TextMeshProUGUI>();
        _defensive   =   parentTr.FindTarget("Def").GetComponent<TextMeshProUGUI>();
        _maxHp       =   parentTr.FindTarget("Hp").GetComponent<TextMeshProUGUI>();
        _atk         =   parentTr.FindTarget("Atk").GetComponent<TextMeshProUGUI>();
        _progressBar = parentTr.FindTarget("LevelSlider").GetComponent<Slider>();
        _levelUpButton = parentTr.FindTarget("LevelUpButton").GetComponent<Button>();

        Transform slotPanelTr = parentTr.FindTarget("ExpItemSlot");
        _slotParentTr = slotPanelTr.FindTarget("ExpItemSlots");
        _slotPanel = slotPanelTr.gameObject;

        _upgradePanel = parentTr.gameObject;
        _canNotUpgradeAbleTextPanel = parentTr.FindTarget("CannotUpgradePanel").gameObject;
        _progressBarPanel = parentTr.FindTarget("ProgressPanel").gameObject;
    }

    private void AddButtonListener()
    {
        _levelUpButton.onClick.AddListener(OnClickLevelUpgradeButton);
    }

    private void InitializeSlots(List<ExpItemSlotInitPayload> payLoads)
    {
        for(int i = 0; i < payLoads.Count; i++)
        {
            GameObject newSlot = Object.Instantiate(_slotPrefab);
            newSlot.transform.SetParent(_slotParentTr);
            
            var comp = newSlot.GetComponent<ExpItemSlot>();

            if(comp != null)
            {
                ExpItemSlotInitPayload payLoad = payLoads[i];

                comp.Initialize(payLoad.itemId, payLoad.expItemIcon, payLoad.itemTier);
                _slots.Add(payLoad.itemId, comp);
            } 
        }
    }

    private void SlotClickLock(bool isLock)
    {
        foreach(var slotMap in _slots)
        {
            var slot = slotMap.Value;
            slot.LockClick(isLock);
        }
    }

    private void SetActiveMainPanel(CharacterGrowthSystemType type)
    {
        if(CharacterGrowthSystemType.Upgrade == type)
        {
            _upgradePanel.SetActive(true);
        }
        else
        {
            _upgradePanel.SetActive(false);
        }
    }

    private RectTransform GetRectTransform(UIRectName rectName)
    {
        switch(rectName)
        {
            case UIRectName.CharacterGrowthUI_LevelUI_Level:
                return _level.GetComponent<RectTransform>();

            case UIRectName.CharacterGrowthUI_LevelUI_ProgressBar:
                return _progressBar.GetComponent<RectTransform>();

            case UIRectName.CharacterGrowthUI_LevelUI_LevelUpButton:
                return _levelUpButton.GetComponent<RectTransform>();

            case UIRectName.CharacterGrowthUI_LevelUI_ExpItem:
                return parentTr.FindTarget("ExpItemSlot").GetComponent<RectTransform>();
        }

        return null;
    }
}

//Input
public partial class LevelUI : MonoBehaviour
{
    private void OnClickLevelUpgradeButton()
    {
        EventBus.Invoke("UpgradeLevel");

        foreach(var map in _slots)
        {
            var slot = map.Value;
            slot.DisableRemoveButton();
        }
    }
}

//Output
public partial class LevelUI : MonoBehaviour
{
    private void OnSelectCharacterIcon(LevelViewPayLoad payLoad)
    {
        _level.SetText($"Lv {payLoad.level}");
        _currentProgressText.SetText($"{payLoad.currentExpAmount}");
        _maxProgressText.SetText($"{payLoad.maxExpAmount}");
        _defensive.SetText($"Def : {payLoad.defensive}");
        _maxHp.SetText($"Hp : {payLoad.maxHp}");
        _atk.SetText($"Atk : {payLoad.baseAttack}");

        if(payLoad.isMaxLevel)
        {
            _canNotUpgradeAbleTextPanel.gameObject.SetActive(true);
            _progressBarPanel.gameObject.SetActive(false);
            _slotPanel.gameObject.SetActive(false);
            _levelUpButton.gameObject.SetActive(false);
            _progressBar.gameObject.SetActive(false);

            SlotClickLock(true);
        }
        else
        {
            _canNotUpgradeAbleTextPanel.gameObject.SetActive(false);
            _progressBarPanel.gameObject.SetActive(true);
            _slotPanel.gameObject.SetActive(true);
            _levelUpButton.gameObject.SetActive(true);
            _progressBar.gameObject.SetActive(true);
            _progressBar.value = payLoad.expPercentGage;

            SlotClickLock(false);
        }
    }

    private void UpdatePreview(LevelPreviewPayload uiPayLoad)
    {
        _progressBar.value = uiPayLoad.currentProgress;

        if(uiPayLoad.isUpgrading) _level.SetText($"Lv {uiPayLoad.originLevel} + {uiPayLoad.upgradeLevel}");
        else _level.SetText($"Lv {uiPayLoad.originLevel}");

        _maxProgressText.SetText($"{uiPayLoad.currentLevelMaxExp}");
        _currentProgressText.SetText($"{uiPayLoad.remainExp}");

        if(uiPayLoad.isOverMaxLevel)
            SlotClickLock(true);
        else
            SlotClickLock(false);
    }

    private void UpdateSlot(int expItemId, int selectedAmount, int currentAmount)
    {
        if(_slots.TryGetValue(expItemId, out var slot))
        {
            slot.UpdateUI(selectedAmount, currentAmount);
        }   
    }
}