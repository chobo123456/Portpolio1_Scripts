using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public struct DecoKey : IEquatable<DecoKey>
{
    public int index;
    public int characterId;

    public bool Equals(DecoKey deco)
    {
        return index == deco.index && characterId == deco.characterId;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
/// <summary>
///  초기화
/// </summary>
public partial class DecoUI : MonoBehaviour
{
    private Dictionary<DecoKey, List<DecoInfo>> currentDecoList = new();
    private List<DecoUI_Slot> slotObj = new();
    private List<GameObject> objs = new();
    private GameObject uiPrefab;
    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        Initialize_DecoPrefab();
        Initialize_EventBus();
    }

    private async void Initialize_DecoPrefab()
    {
        uiPrefab = await AddressableUtil.Load_Instant<GameObject>("DecoUI", this.GetCancelOnDestroy());

        Initialize_DecoUIList(); 
    }
    private void Initialize_DecoUIList()
    {
        Transform parentTr = this.transform.FindTarget("Content");

        for(int i = 0; i < 20; i++)
        {
            var obj = Instantiate(uiPrefab);
            obj.name = $"Deco_Content_{i}";
            obj.transform.SetParent(parentTr);
            obj.SetActive(false);

            objs.Add(obj);

            var uiSlot = obj.GetComponent<DecoUI_Slot>();

            if(uiSlot != null)
            {
                uiSlot.Initialize();
                slotObj.Add(uiSlot);
            }
        }
    }

    private void Initialize_EventBus()
    {
        EventBus.Sub<DecoKey, DecoInfo>("SetDecoUI", SetDeco);
        EventBus.Sub<DecoKey>("SetCharaterIndex", OnCharacterSet);
        EventBus.Sub<DecoKey>("OnRemoveCharacter_Deco", OnCharacterRemove);
        EventBus.Sub<DecoUI_Slot>("OnRemoveUI_Deco", OnRemoveList);
    }

    private void OnDestroy()
    {
        EventBus.UnSub<DecoKey, DecoInfo>("SetDecoUI", SetDeco);
        EventBus.UnSub<DecoKey>("SetCharaterIndex", OnCharacterSet);
        EventBus.UnSub<DecoKey>("OnRemoveCharacter", OnCharacterRemove);
        EventBus.UnSub<DecoUI_Slot>("OnRemoveUI_Deco", OnRemoveList);
    }
}

/// <summary>
/// 시스템
/// </summary>
public partial class DecoUI : MonoBehaviour
{
    private DecoKey currentPlayerDecoKey;
    private void SetDeco(DecoKey decoKey, DecoInfo info)
    {
        currentPlayerDecoKey = decoKey;

        if (currentDecoList.TryGetValue(currentPlayerDecoKey, out var list))
        {
            bool isNotIn = true;

            for(int i = 0; i < list.Count; i++)
            {
                int instanceId = list[i].instanceId;

                if (instanceId == info.instanceId)
                {
                    isNotIn = false;
                    break;
                }                    
            }

            if(isNotIn)
            {
                currentDecoList[currentPlayerDecoKey].Add(info);
            }
        }
        else
        {
            currentDecoList[currentPlayerDecoKey].Add(info);
        }
            
        ShowUi();
    }

    private void OnCharacterSet(DecoKey decoKey)
    {
        if(!currentDecoList.ContainsKey(decoKey))
        {
            currentDecoList[decoKey] = new();
        }

        currentPlayerDecoKey = decoKey;

        ShowUi();    
    }

    private void OnCharacterRemove(DecoKey decoKey)
    {
        if(currentDecoList.ContainsKey(decoKey))
        {
            currentDecoList.Remove(decoKey);
        }
    }

    private void ShowUi()
    {
        if(currentDecoList.TryGetValue(currentPlayerDecoKey, out var list))
        {
            if(list.Count > slotObj.Count) Initialize_DecoUIList();

            for(int i = 0; i < slotObj.Count; i++)
            {
                var obj = objs[i];

                if (i >= list.Count) {
                    obj.SetActive(false);
                    continue;
                }
                
                var slot = slotObj[i];
                var info = list[i];

                Util.Log($"{slot.IsSetted()}");

                if(slot.IsSetted())
                {
                    if(slot.IsFinish(info.startTime, info.activeTime))
                    {
                        OnRemoveList(slot);
                    }
                }
                else
                {
                    slot.SetSetting(info);
                    obj.SetActive(true);
                    slot.LoopStart(info.startTime, info.activeTime);
                }
            }
        }
    }

    private void OnRemoveList(DecoUI_Slot slot)
    {
        DecoInfo info = slot.GetDecoInfo();

        currentDecoList[currentPlayerDecoKey].Remove(info);   
        slot.gameObject.SetActive(false);
    }
}