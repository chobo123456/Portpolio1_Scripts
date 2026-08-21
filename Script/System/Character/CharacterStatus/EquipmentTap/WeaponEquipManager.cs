using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WeaponSystem
{
    private readonly EquipmentStore _weaponStore;
    private const string 
        GetCharacterIdsEventName = "GetCharacterIds",
        GetInventoryItemEventName = "Inventory_System_GetInventory";
    private Dictionary<int, bool> _runTimeWeaponUseMap = new();

    public WeaponSystem(string folderName, string fileName)
    {
        _weaponStore = new(folderName, fileName);
    }

    //public
    public void OnEnable()
    {
        EventBus.Sub_Func<int, int>("GetEquipmentIdToUseCharacterId", GetEquipmentIdUseCharacterId);
    }

    public void OnDisable()
    {
        EventBus.UnSub_Func<int, int>("GetEquipmentIdToUseCharacterId", GetEquipmentIdUseCharacterId);
    }

    public void LoadWeapon()
    {
        if (!_weaponStore.IsExistData()) 
        {
            //데이터가 없을때

            List<int> characterIds = EventBus.Invoke_Func<List<int>>("GetCharacterIds");

            int amount = 1;

            for(int i = 0; i < characterIds.Count; i++)
            {
                int equipmentId = GetBaseWeaponId(characterIds[i]);
                if(equipmentId <= 0) continue;

                EventBus.Invoke<int, int, bool>("GetItem", equipmentId, amount, false);
            }
            
            List<ItemHasInfo> equipmentItems = EventBus.Invoke_Func<InventoryType, List<ItemHasInfo>>(GetInventoryItemEventName, InventoryType.Equipment);

            SetWeaponsFromInventory(equipmentItems, characterIds);

            return;
        } 
        else if(_weaponStore.IsExistData()
            && EventBus.Invoke_Func<InventoryType, List<ItemHasInfo>>(GetInventoryItemEventName, InventoryType.Equipment).Count < _weaponStore.GetData().datas.Count)
        {
            //데이터는 존재 하지만 실제 인벤토리 아이템갯수와 일치하지않을때

            Characters_EquipmentsMap map = _weaponStore.GetData();
            
            int amount = 1;

            foreach(Character_EquipmentMap structInfo in map.datas)
            {
                Equipment equipmentInfo    = structInfo._equipment;
                int itemId              = equipmentInfo._id;
                int instanceId          = equipmentInfo._instanceId;

                if(!EventBus.Invoke_Func<int, bool>("Inventory_System_IsExistItem", instanceId))
                    EventBus.Invoke<int, int, bool>("GetItem", itemId, amount, false);
            }

            List<int> characterIds = EventBus.Invoke_Func<List<int>>(GetCharacterIdsEventName);
            List<ItemHasInfo> equipmentItems = EventBus.Invoke_Func<InventoryType, List<ItemHasInfo>>(GetInventoryItemEventName, InventoryType.Equipment);

            if(equipmentItems.Count >= map.datas.Count)
                SetWeaponsFromInventory(equipmentItems, characterIds);
  
            return;
        }
        else 
        {
            //데이터가 존재시

            Characters_EquipmentsMap map = _weaponStore.GetData();

            foreach(Character_EquipmentMap structInfo in map.datas)
            {
                Equipment equipmentInfo    = structInfo._equipment;
                int characterId         = structInfo.characterId;

                int itemId              = equipmentInfo._id;
                int instanceId          = equipmentInfo._instanceId;

                SetWeapon_InCase(characterId, itemId, instanceId);
            }
        }
    }

    public void ChangeWeapon(int characterId, int weaponId, int instanceId)
    {
        SetWeapon_InCase(characterId, weaponId, instanceId);
        ApplyWeapon();
    }

    //캐릭터 실제 무기 데이터 변경
    public void ApplyWeapon()
    {
        EventBus.Invoke("ReloadEquipment");
    }

    public List<EquipmentPayLoad<WeaponType>> GetPayLoadList()
    {
        List<ItemHasInfo> inventoryItems = EventBus.Invoke_Func<InventoryType, List<ItemHasInfo>>(GetInventoryItemEventName, InventoryType.Equipment);

        List<EquipmentPayLoad<WeaponType>> payLoad = new();

        for(int i = 0; i < inventoryItems.Count; i++)
        {
            var inventoryItem = inventoryItems[i];

            if(inventoryItem.data is WeaponItemData weapon)
            {
                int instanceId = inventoryItem.instanceId;
                if(instanceId <= 0) continue;

                int weaponId = weapon.GetEquipmentId();

                payLoad.Add(GetPayLoad(weaponId, instanceId));
            }
        }

        return payLoad;
    }

    public EquipmentPayLoad<WeaponType> GetPayLoad(int weaponId, int instanceId)
    {
        EquipmentPayLoad<WeaponType> newPayLoad = new();

        int characterId = _weaponStore.GetCharacterIdToUseInstance(instanceId);
                
        bool isUsing = characterId >= 1;

        Sprite characterIcon = null;
        if(isUsing)
            characterIcon = DataLoader.GetData<CharacterData>(DataType.Character, characterId).characterSprite;

        var weaponData = DataLoader.GetData<WeaponStatData>(DataType.Weapon, weaponId);

        newPayLoad = new EquipmentPayLoad<WeaponType>
        {
            _equipment = new Equipment{ _id = weaponId, _instanceId = instanceId},
            _equipmentType = weaponData.type,
            _equipmentIcon = weaponData.visualData.weaponSprite,
            _characterIcon = characterIcon,
            isUsing = isUsing,
        };

        return newPayLoad;
    }

    public WeaponType GetWeaponTypeUseCharacterId(int characterId)
    {
        var characterDataSo = DataLoader.GetData<CharacterData>(DataType.Character, characterId);
        return characterDataSo.weaponType;
    }

    public Sprite GetEquipmentSpriteUseCharacterId(int characterId)
    {
        int weaponId = _weaponStore.GetEquipmentId(characterId);
        WeaponStatData weaponData = DataLoader.GetData<WeaponStatData>(DataType.Weapon, weaponId);
        return weaponData.visualData.weaponSprite;
    }


    //private
    private void SetWeaponsFromInventory(List<ItemHasInfo> list, List<int> characterIdList)
    {
        for(int i = 0; i < characterIdList.Count; i++)
        {
            ItemHasInfo curItem = list[i];
            if(curItem.data is WeaponItemData itemData)
            {
                int instanceId  = curItem.instanceId;
                int equipmentId    = itemData.GetEquipmentId();

                SetWeapon_InCase(characterIdList[i], equipmentId, instanceId);
            }
        }
    }

    private void SetWeapon_InCase(int selectedCharacterId, int selectedWeaponId, int selectedWeaponInstanceId)
    {
        if(_runTimeWeaponUseMap.ContainsKey(selectedWeaponInstanceId))
        {
            int selectedWeaponUsingCharacterId = _weaponStore.GetCharacterIdToUseInstance(selectedWeaponInstanceId);

            if(selectedCharacterId == selectedWeaponUsingCharacterId) return;
                
            Equipment selectedCharacterWeapon = _weaponStore.GetEquipment(selectedCharacterId);
            Equipment selectedWeapon          = _weaponStore.GetEquipment(selectedWeaponUsingCharacterId);

            _weaponStore.SetEquipment(selectedCharacterId, selectedWeapon._id, selectedWeapon._instanceId);
            _weaponStore.SetEquipment(selectedWeaponUsingCharacterId, selectedCharacterWeapon._id, selectedCharacterWeapon._instanceId); 
        }
        else // 선택한 장비가 아무 캐릭터에게도 장착이 되지않았을경우
        {
            if(_weaponStore.IsAlreadyEquipped(selectedCharacterId)) // 선택한 캐릭터가 이미 다른장비를 장착했다면 
            {
                Equipment currentEquipment = _weaponStore.GetEquipment(selectedCharacterId);

                //기존의 장비 장착해제
                _runTimeWeaponUseMap.Remove(currentEquipment._instanceId);
                _weaponStore.RemoveInstanceId(currentEquipment._instanceId);

                _weaponStore.SetEquipment(selectedCharacterId, selectedWeaponId, selectedWeaponInstanceId);
                _runTimeWeaponUseMap.Add(selectedWeaponInstanceId, true);
            }
            else //선택한 캐릭터가 아무런 장비도 장착하지 않았을 경우
            {  
                _weaponStore.SetEquipment(selectedCharacterId, selectedWeaponId, selectedWeaponInstanceId);
                _runTimeWeaponUseMap.Add(selectedWeaponInstanceId, true);
            }
        }
    }

    private int GetBaseWeaponId(int characterId)
    {
        WeaponType weaponType = DataLoader.GetData<CharacterData>(DataType.Character, characterId).weaponType;

        switch(weaponType)
        {
            case WeaponType.Melee:
                return 100;
            case WeaponType.Range:
                return 1000;
            default:
                return 100;
        }
    }

    private int GetEquipmentIdUseCharacterId(int characterId) => _weaponStore.GetEquipmentId(characterId);
}

public class WeaponUIHandler
{
    private int _runtimeSelectedCharacterId = 0;
    private WeaponSystem _equipmentSystem;

    public void InjectParameter(WeaponSystem equipmentSystem)
    {
        _equipmentSystem = equipmentSystem;
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
            EventBus.Sub<int, int>("ChangeWeapon", ChangeWeapon);
            EventBus.Sub<int>("Growth_UI_ChangeSelectedCharacter", OnSelectCharacter);
            EventBus.Sub("Status_UI_Request_OpenWeaponSelectionPanel", OpenWeaponSelectionPanel);
            EventBus.Sub("Status_UI_SelectionPanelClose", CloseWeaponSelectionPanel);
        }   
        else
        {
            EventBus.UnSub<int, int>("ChangeWeapon", ChangeWeapon);
            EventBus.UnSub<int>("Growth_UI_ChangeSelectedCharacter", OnSelectCharacter);
            EventBus.UnSub("Status_UI_Request_OpenWeaponSelectionPanel", OpenWeaponSelectionPanel);
            EventBus.UnSub("Status_UI_SelectionPanelClose", CloseWeaponSelectionPanel);
        } 
    }

    private void ChangeWeapon(int equipmentId, int instanceId)
    {
        _equipmentSystem.ChangeWeapon(_runtimeSelectedCharacterId, equipmentId, instanceId);

        CloseWeaponSelectionPanel();
        EventBus.Invoke<List<EquipmentPayLoad<WeaponType>>>("WeaponEquip_UI_UpdateIcon", _equipmentSystem.GetPayLoadList());
    } 

    private void OnSelectCharacter(int characterId)
    {
        _runtimeSelectedCharacterId = characterId;

        CloseWeaponSelectionPanel();
        EventBus.Invoke<Sprite>("WeaponEquip_UI_SetSelectedCharacterWeaponSprite", _equipmentSystem.GetEquipmentSpriteUseCharacterId(characterId));
    }

    private void OpenWeaponSelectionPanel()
    {
        EventBus.Invoke<WeaponType>("WeaponEquip_UI_OpenWeaponSelectionPanel", _equipmentSystem.GetWeaponTypeUseCharacterId(_runtimeSelectedCharacterId));
    }

    private void CloseWeaponSelectionPanel()
    {
        EventBus.Invoke<bool>("WeaponEquip_UI_SetActiveSelectionPanel", false);
    }
}

public class WeaponEquipManager : MonoBehaviour
{
    private WeaponSystem _equipmentSystem;
    private WeaponUIHandler _equipmentUIHandler;

    private bool isReadyLocalUI = false, isReadyManager = false;

    private void OnEnable()
    {
        SubscribeEvent(true);
        this.RunRoutine(Booting());
    }

    private IEnumerator Booting()
    {
        _equipmentSystem = new("Player/Equip", "Weapon");
        _equipmentSystem.OnEnable();

        _equipmentUIHandler = new();
        _equipmentUIHandler.InjectParameter(_equipmentSystem);
        _equipmentUIHandler.OnEnable();

        yield return new WaitUntil(() => 
            LoadStatus.IsReady_Inventory
            && EventBus.Invoke_Func<bool>("FinishLoadCharacterData"));

        _equipmentSystem.LoadWeapon();
        _equipmentSystem.ApplyWeapon();

        yield return new WaitUntil(() => isReadyLocalUI);

        EventBus.Invoke<List<EquipmentPayLoad<WeaponType>>>("WeaponEquip_UI_Initialize", _equipmentSystem.GetPayLoadList());

        isReadyManager = true;
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub("WeaponEquip_UI_Ready", OnLocalUIReady);
            EventBus.Sub<ItemHasInfo>("ObtainNewEquipment", ObtainNewWeapon);
            EventBus.Sub<ItemHasInfo>("DeleteWeapon", DeleteWeapon);
        }   
        else
        {
            EventBus.UnSub("WeaponEquip_UI_Ready", OnLocalUIReady);
            EventBus.UnSub<ItemHasInfo>("ObtainNewEquipment", ObtainNewWeapon);
            EventBus.UnSub<ItemHasInfo>("DeleteWeapon", DeleteWeapon);
        } 
    }

    private void OnLocalUIReady()
    {
        isReadyLocalUI = true;
    }

    private void OnDisable()
    {
        SubscribeEvent(false);
        _equipmentSystem?.OnDisable();
        _equipmentUIHandler?.OnDisable();
    }

    private void ObtainNewWeapon(ItemHasInfo newEquipment)
    {
        if(!isReadyManager || newEquipment.instanceId <= 0) return;

        if(newEquipment.data is WeaponItemData weaponData)
            EventBus.Invoke<EquipmentPayLoad<WeaponType>>("WeaponEquip_UI_ObtainNewWeapon", _equipmentSystem.GetPayLoad(weaponData.GetEquipmentId(), newEquipment.instanceId));
    }

    private void DeleteWeapon(ItemHasInfo deleteEquipment)
    {
        if(!isReadyManager || deleteEquipment.instanceId <= 0) return;

        if(deleteEquipment.data is WeaponItemData weaponData)
            EventBus.Invoke<EquipmentPayLoad<WeaponType>>("WeaponEquip_UI_DeleteWeapon", _equipmentSystem.GetPayLoad(weaponData.GetEquipmentId(), deleteEquipment.instanceId));
    }
}


