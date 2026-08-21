using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public struct EquipmentPayLoad<T>
{
    public Equipment _equipment;
    public T _equipmentType;
    public Sprite _equipmentIcon;    
    public Sprite _characterIcon;
    public bool isUsing;
}

public class EquipmentIcon<T> where T : System.Enum
{
    private Transform mainIconTransform;
    private Image characterIcon, weaponIcon;
    private Button iconButton;
    private T _type;
    private bool isDataSetted = false;

    public void Initialize(Transform uiParent)
    {
        mainIconTransform = uiParent;
        
        iconButton      = uiParent.GetComponent<Button>();
        weaponIcon      = uiParent.FindTarget("Icon").GetComponent<Image>();
        characterIcon   = uiParent.FindTarget("UseCharacter").GetComponent<Image>();
    }

    public void SetWeapon(Sprite weaponSprite, T type)
    {
        weaponIcon.sprite = weaponSprite;
        _type = type;
        isDataSetted = true;
    }

    public void SetCharacterSprite(Sprite characterSprite, bool isUsing)
    {
        if(!isUsing)
        {
            characterIcon.enabled = false;
        }
        else
        {
            characterIcon.sprite = characterSprite;
            characterIcon.enabled = true;
        }
    }

    public void ButtonAddListener(UnityEngine.Events.UnityAction action)
    {
        iconButton.onClick.AddListener(action);
    }

    public void Active(bool isActive)
    {
        mainIconTransform.gameObject.SetActive(isActive);
    }

    public void ResetData()
    {
        iconButton.onClick.RemoveAllListeners();
        isDataSetted = false;
    }

    public bool IsDataSetted() => isDataSetted;
    
    public bool IsMatchType(T type) => type.Equals(_type);
}

public partial class WeaponEquipUI : MonoBehaviour
{
    public Transform parentTr;
    private Dictionary<Equipment, EquipmentIcon<WeaponType>> _activeIcons = new();
    private List<EquipmentIcon<WeaponType>> _iconPool = new();
    private GameObject _mainPanel, _weaponSelectPanel, _weaponIconPrefab;
    private Transform _weaponIconParentTr;
    private Image _targetCharacterWeaponIcon;
    private int _exceptionCount = 0;

    private void OnEnable()
    {
        this.RunRoutine(Booting());
    }    

    private async void LoadButtonAsset()
    {
        _weaponIconPrefab = await AddressableUtil.Load_Instant<GameObject>("Character_Info_WeaponButton", this.GetCancelOnDestroy());
    }

    private void Initialize_Objects()
    {
        _weaponIconParentTr = parentTr.FindTarget("Weapon_Icons"); //button Parent
        _mainPanel = parentTr.gameObject; // mainPanel
        _weaponSelectPanel = parentTr.FindTarget("WeaponView").gameObject; // choosePanel
        _weaponSelectPanel.SetActive(false);

        _targetCharacterWeaponIcon = parentTr.FindTarget("SelectedCharacter_WeaponIcon").GetComponent<Image>();

        Button weaponSlotButton = parentTr.FindTarget("WeaponSlot").GetComponent<Button>();
        weaponSlotButton.onClick.AddListener(() => EventBus.Invoke("Status_UI_Request_OpenWeaponSelectionPanel"));
    }

    IEnumerator Booting()
    {
        LoadButtonAsset();
        Initialize_Objects();
        SubscribeEvent(true);

        yield return new WaitUntil(() => _weaponIconPrefab != null);
        
        EventBus.Invoke("WeaponEquip_UI_Ready");
    }

    #region Pool
    private void InitializeButtonPool()
    {
        for(int i = 0; i < 20; i++)
        {
            GameObject btn = Object.Instantiate(_weaponIconPrefab);
            btn.transform.SetParent(_weaponIconParentTr);
            btn.SetActive(false);

            var newEquipmentIcon = new EquipmentIcon<WeaponType>();
            newEquipmentIcon.Initialize(btn.transform);

            _iconPool.Add(newEquipmentIcon);
        }
    }

    private EquipmentIcon<WeaponType> GetButtonFromPool()
    {
        for(int i = 0; i < _iconPool.Count; i++)
        {
            var icon = _iconPool[i];

            if(!icon.IsDataSetted())
            {
                _exceptionCount = 0;
                return icon;
            }  
        }

        if(_exceptionCount > 3)
        {
            Util.Log($"오류 발생, WeaponEquipUI.cs GetButtonFromPool()","red");
            return (EquipmentIcon<WeaponType>)default;
        }

        _exceptionCount++;
        InitializeButtonPool();

        return GetButtonFromPool();
    }
    #endregion

    private void IntializeButtons(List<EquipmentPayLoad<WeaponType>> equipmentPayLoadList)
    {
        for(int i = 0; i < equipmentPayLoadList.Count; i++)
        {
            EquipmentIcon<WeaponType> icon = GetButtonFromPool();
            if(icon == null) continue;
            
            EquipmentPayLoad<WeaponType> equipmentPayLoad = equipmentPayLoadList[i];

            icon.ButtonAddListener(() => OnClickWeaponIcon(equipmentPayLoad._equipment._id, equipmentPayLoad._equipment._instanceId));
            icon.SetWeapon(equipmentPayLoad._equipmentIcon, equipmentPayLoad._equipmentType);
            icon.SetCharacterSprite(equipmentPayLoad._characterIcon, equipmentPayLoad.isUsing);

            _activeIcons.Add(equipmentPayLoad._equipment, icon);
        }
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<List<EquipmentPayLoad<WeaponType>>>("WeaponEquip_UI_Initialize", IntializeButtons);
            EventBus.Sub<List<EquipmentPayLoad<WeaponType>>>("WeaponEquip_UI_UpdateIcon", UpdateIcon);
            EventBus.Sub<EquipmentPayLoad<WeaponType>>("WeaponEquip_UI_ObtainNewWeapon", ObtainNewWeapon);
            EventBus.Sub<EquipmentPayLoad<WeaponType>>("WeaponEquip_UI_DeleteWeapon", RemoveWeaponIcon);
            EventBus.Sub<WeaponType>("WeaponEquip_UI_OpenWeaponSelectionPanel", OpenWeaponSelectionPanel);
            EventBus.Sub<Sprite>("WeaponEquip_UI_SetSelectedCharacterWeaponSprite", SetSelectedCharacterWeaponSprite);
            EventBus.Sub<bool>("WeaponEquip_UI_SetActiveSelectionPanel", SetActiveWeaponSelectionPanel);

            EventBus.Sub<CharacterGrowthSystemType>("Status_UI_SetActiveMainPanel", SetActiveMainPanel);

            EventBus.Sub_Func<UIRectName, RectTransform>("Weapon_UI_GetRectTransform", GetRectTransform);
        }
        else
        {
            EventBus.UnSub<List<EquipmentPayLoad<WeaponType>>>("WeaponEquip_UI_Initialize", IntializeButtons);
            EventBus.UnSub<List<EquipmentPayLoad<WeaponType>>>("WeaponEquip_UI_UpdateIcon", UpdateIcon);
            EventBus.UnSub<EquipmentPayLoad<WeaponType>>("WeaponEquip_UI_ObtainNewWeapon", ObtainNewWeapon);
            EventBus.UnSub<EquipmentPayLoad<WeaponType>>("WeaponEquip_UI_DeleteWeapon", RemoveWeaponIcon);
            EventBus.UnSub<WeaponType>("WeaponEquip_UI_OpenWeaponSelectionPanel", OpenWeaponSelectionPanel);
            EventBus.UnSub<Sprite>("WeaponEquip_UI_SetSelectedCharacterWeaponSprite", SetSelectedCharacterWeaponSprite);
            EventBus.UnSub<bool>("WeaponEquip_UI_SetActiveSelectionPanel", SetActiveWeaponSelectionPanel);

            EventBus.UnSub<CharacterGrowthSystemType>("Status_UI_SetActiveMainPanel", SetActiveMainPanel);   

            EventBus.UnSub_Func<UIRectName, RectTransform>("Weapon_UI_GetRectTransform", GetRectTransform);
        }
    }

    private void SetActiveMainPanel(CharacterGrowthSystemType type)
    {
        SetActiveWeaponSelectionPanel(false);

        if(CharacterGrowthSystemType.Weapon == type)
        {
            _mainPanel.SetActive(true);
        }
        else
        {
            _mainPanel.SetActive(false);
        }
    }

    private void OnDisable()
    {
        SubscribeEvent(false);
    }

    private RectTransform GetRectTransform(UIRectName rectName)
    {
        switch(rectName)
        {
            case UIRectName.CharacterGrowthUI_WeaponUI_WeaponMainPanel:
                return _mainPanel.GetComponent<RectTransform>();

            case UIRectName.CharacterGrowthUI_WeaponUI_WeaponSelectionPanel:
                return _weaponSelectPanel.GetComponent<RectTransform>();

            case UIRectName.CharacterGrowthUI_WeaponUI_WeaponIcon:
                return _weaponIconParentTr.GetComponent<RectTransform>();
        }

        return null;
    }
}

//Input
public partial class WeaponEquipUI : MonoBehaviour
{
    private void OpenWeaponSelectionPanel(WeaponType type)
    {
        foreach(var iconMap in _activeIcons)
        {
            var icon = iconMap.Value;

            if(!icon.IsDataSetted()) 
            {
                icon.Active(false);
                continue;
            }

            if(icon.IsMatchType(type))
                icon.Active(true);
            else
                icon.Active(false);
        }

        SetActiveWeaponSelectionPanel(true);
    }

    private void OnClickWeaponIcon(int weaponId, int instanceId)
    {
        EventBus.Invoke<int, int>("ChangeWeapon", weaponId, instanceId);
    }

    private void ObtainNewWeapon(EquipmentPayLoad<WeaponType> newPayLoad)
    {
        EquipmentIcon<WeaponType> icon = GetButtonFromPool();
        if(icon == null) return;

        icon.ButtonAddListener(() => OnClickWeaponIcon(newPayLoad._equipment._id, newPayLoad._equipment._instanceId));
        icon.SetWeapon(newPayLoad._equipmentIcon, newPayLoad._equipmentType);
        icon.SetCharacterSprite(newPayLoad._characterIcon, newPayLoad.isUsing);

        _activeIcons.Add(newPayLoad._equipment, icon);
    }
}

//Output
public partial class WeaponEquipUI : MonoBehaviour
{
    private void SetSelectedCharacterWeaponSprite(Sprite weaponIcon)
    {
        _targetCharacterWeaponIcon.sprite = weaponIcon;
    }

    private void SetActiveWeaponSelectionPanel(bool isActive)
    {
        _weaponSelectPanel.SetActive(isActive);
    }

    private void RemoveWeaponIcon(EquipmentPayLoad<WeaponType> deleteWeapon)
    {
        if(_activeIcons.TryGetValue(deleteWeapon._equipment, out var icon))
        {
            icon.Active(false);
            icon.ResetData();
            _activeIcons.Remove(deleteWeapon._equipment);
        }
    }

    private void UpdateIcon(List<EquipmentPayLoad<WeaponType>> payLoadList)
    {
        for(int i = 0; i < payLoadList.Count; i++)
        {
            EquipmentPayLoad<WeaponType> payLoad = payLoadList[i];

            if(_activeIcons.TryGetValue(payLoad._equipment, out var icon))
                icon.SetCharacterSprite(payLoad._characterIcon, payLoad.isUsing);
        } 
    }
}