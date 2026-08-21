using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct RuntimeCharacterModelMap
{
    public int characterId;
    public GameObject chracterModel;
}

public class CharacterModelChanger
{
    private Transform modelParentTransform, modelRestoreParentTransform;
    private Dictionary<int, RuntimeCharacterModelMap> character_Models = new();
    private CharacterModelMaker _modelMaker;
    private CharacterAnimationOverrider _characterAnimationOverrider;
    private GameObject _character;
    private MonoBehaviour _mono;
    
    public void InjectParameter(
        CharacterModelMaker modelMaker, 
        CharacterAnimationOverrider characterAnimationOverrider, 
        GameObject character, 
        MonoBehaviour mono)
    {
        _modelMaker                         = modelMaker;
        _characterAnimationOverrider        = characterAnimationOverrider;
        _character                          = character;
        _mono                               = mono;

        modelParentTransform = character.transform.FindTarget("Model");
        modelRestoreParentTransform = character.transform.FindTarget("RestoreModel");

        ClearObjectInTransform();
    }

    private void ClearObjectInTransform()
    {
        for(int i = modelParentTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = modelParentTransform.GetChild(i);
            UnityEngine.Object.Destroy(child.gameObject);
        }

        for(int i = modelRestoreParentTransform.childCount - 1; i >= 0; i++)
        {
            Transform child = modelRestoreParentTransform.GetChild(i);
            UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    public void LoadRecentParty(Dictionary<int, int> recentPartyInfo, int recentIndex)
    {
        _mono.RunRoutine(Loop(recentPartyInfo, recentIndex));        
    }  

    IEnumerator Loop(Dictionary<int, int> recentPartyInfo, int recentIndex)
    {
        SetCharacterPosition_RecentPosition();
        ClearModels(recentPartyInfo);

        yield return null;
        
        SetModelList(recentPartyInfo);
        ActiveCharacter(recentIndex);
    }

    private void SetCharacterPosition_RecentPosition()
    {
        _character.transform.position = PlayerMatch.GetPlayerPos();
    }
    
    private void ClearModels(Dictionary<int, int> recentPartyInfo)
    {
        foreach(var map in recentPartyInfo)
        {
            int index = map.Key;
            int characterId = map.Value;

            if(character_Models.TryGetValue(index, out var recentInfo))
            {
                if(characterId <= 0 || recentInfo.characterId != characterId)
                {
                    GameObject.Destroy(recentInfo.chracterModel);
                    character_Models.Remove(index);
                }
            }
        }
    }

    private void SetModelList(Dictionary<int, int> recentPartyInfo)
    {
        foreach(var info in recentPartyInfo)
        {
            int slotIndex   = info.Key;
            int characterId = info.Value;

            if (characterId <= 0) continue; 

            if(!character_Models.ContainsKey(slotIndex) || (character_Models.TryGetValue(slotIndex, out var recentInfo) && recentInfo.characterId != characterId))
            {
                var data = DataLoader.GetData<Character_Prefab_Data>(DataType.CharacterETC, characterId);

                GameObject characterModel = data.chracterModel;
                GameObject madeModel = _modelMaker.GetModel(modelRestoreParentTransform, characterModel);
                madeModel.SetActive(false);
                
                RuntimeCharacterModelMap changeInfo = new RuntimeCharacterModelMap
                {
                    characterId = characterId,
                    chracterModel = madeModel,
                };

                character_Models[slotIndex] = changeInfo;
            }
        }
    }

    //특정 모델을 활성
    public void ActiveCharacter(int recentIndex)
    {
        foreach(var model in character_Models)
        {
            int modelIndex = model.Key;
            var info = model.Value;

            if(modelIndex == recentIndex)
            {
                info.chracterModel.transform.SetParent(modelParentTransform);
                info.chracterModel.transform.localPosition = Vector3.zero;
                info.chracterModel.transform.localRotation = Quaternion.identity;

                if(!info.chracterModel.activeSelf)
                {
                    info.chracterModel.SetActive(true);
                    _characterAnimationOverrider.OverrideAnimation(info.characterId);
                    EventBus.Invoke<int, int>("InitializeDataBox", info.characterId, recentIndex);
                    EventBus.Invoke<int>("ChangeSkillSet", info.characterId); 
                } 
            }
            else
            {
                info.chracterModel.transform.SetParent(modelRestoreParentTransform);
                info.chracterModel.SetActive(false);
            }
        }
    }
}