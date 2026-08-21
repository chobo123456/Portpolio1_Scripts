using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Equipment
{
    public int _id;
    public int _instanceId;
}


[System.Serializable]
public struct Character_EquipmentMap
{
    public int characterId;
    public Equipment _equipment;
}

[System.Serializable]
public struct Characters_EquipmentsMap
{
    public List<Character_EquipmentMap> datas;
}

public interface IEquipmentSetting
{
    int GetEquipmentId(int characterId);
}

public class EquipmentStore : IEquipmentSetting
{
    private Save<Characters_EquipmentsMap> _equipmentData;
    private Dictionary<int, Equipment> _equipmentMap = new();
    private Dictionary<int, int> _equipmentInstanceMap = new();
    
    public EquipmentStore(string equipmentFolderName, string equipmentFileName)
    {
        _equipmentData = new(equipmentFolderName, equipmentFileName);
    }

    public void SetEquipment(int characterId, int equipmentId, int equipmentInstanceId)
    {
        Equipment equipmentInfo = new Equipment{ _id = equipmentId, _instanceId = equipmentInstanceId};

        if(_equipmentMap.ContainsKey(characterId))
        {
            _equipmentMap[characterId] = equipmentInfo;
        }
        else
        {
            _equipmentMap.Add(characterId, equipmentInfo);
        }

        if(_equipmentInstanceMap.ContainsKey(equipmentInstanceId))
        {
            _equipmentInstanceMap[equipmentInstanceId] = characterId;
        }
        else
        {
            _equipmentInstanceMap.Add(equipmentInstanceId, characterId);
        }
            
        SaveEquipment();
    }

    public void RemoveInstanceId(int instanceId)
    {
        _equipmentInstanceMap.Remove(instanceId);
    }

    private void SaveEquipment()
    {
        Characters_EquipmentsMap newMap = new();

        newMap.datas = new();

        foreach(var map in _equipmentMap)
        {
            Equipment equipmentInfo = map.Value;
            int characterId = map.Key;

            Character_EquipmentMap newSetData = new();
            newSetData._equipment = equipmentInfo;
            newSetData.characterId = characterId;

            newMap.datas.Add(newSetData);
        }

        _equipmentData.Saving(newMap);
    }   

    public int GetEquipmentId(int characterId)
    {
        if(_equipmentMap.TryGetValue(characterId, out var equipmentInfo))
        {
            return equipmentInfo._id;
        }
        else
        {
            return -1;    
        }
    }

    public int GetCharacterIdToUseInstance(int instanceId)
    {
        if(_equipmentInstanceMap.TryGetValue(instanceId, out int characterId))
        {
            return characterId;
        }

        return -1;
    }

    public bool IsExistData() => _equipmentData.IsExist();
    
    public bool IsAlreadyEquipped(int characterId) => _equipmentMap.ContainsKey(characterId);
    
    public Characters_EquipmentsMap GetData() => _equipmentData.savedData;
    
    public Equipment GetEquipment(int characterId)
    {
        _equipmentMap.TryGetValue(characterId, out Equipment currentEquipment);

        return currentEquipment;
    }
}

