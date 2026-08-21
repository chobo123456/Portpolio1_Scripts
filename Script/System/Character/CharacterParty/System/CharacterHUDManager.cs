using UnityEngine;
using System.Collections.Generic;

public struct CharacterHUDPayload
{
    public int index;
    public bool isDisableSlot;
    public Sprite characterIcon;
    public Sprite elementIcon;
}


public class CharacterHUDManager : MonoBehaviour
{
    public Transform HUDTransform;
    private Dictionary<int, CharacterHUD> _slots = new();
    private List<RectTransform> _slotObjList = new();

    private void OnEnable()
    {
        EventBus.Sub<CharacterHUDPayload>("Party_HUD_UpdateSlot", UpdateSlot);
        EventBus.Sub<int>("Party_HUD_OnSelect", OnSelectSlot);

        InitializeSlots();
    }

    private void OnDisable()
    {
        EventBus.UnSub<CharacterHUDPayload>("Party_HUD_UpdateSlot", UpdateSlot);
        EventBus.UnSub<int>("Party_HUD_OnSelect", OnSelectSlot);
    }

    private void InitializeSlots()
    {
        for(int i = 0; i < HUDTransform.childCount; i++)
        {
            Transform childTr = HUDTransform.GetChild(i);

            var comp = childTr.GetComponent<CharacterHUD>();

            if(comp == null) continue;

            comp.Initialize();
            _slots.Add(i, comp);   

            var rect = childTr.GetComponent<RectTransform>();

            if(rect == null) continue;

            _slotObjList.Add(rect);
        }
    }

    private void UpdateSlot(CharacterHUDPayload payLoad)
    {
        if(_slots.TryGetValue(payLoad.index, out var slot))
        {
            if(payLoad.isDisableSlot)
            {
                slot.DisableSlot();   
            }
            else
            {
                slot.EnableSlot(payLoad.characterIcon, payLoad.elementIcon);
            }
        }   

        SortSlot();
    }

    private void OnSelectSlot(int index)
    {
        foreach(var map in _slots)
        {
            int slotIndex = map.Key;
            var slot = map.Value;

            if(slotIndex == index)
            {
                slot.OnSelect(true);
            }
            else
            {
                slot.OnSelect(false);
            }
        }
    }

    private void SortSlot()
    {
        if(_slots.Count != _slotObjList.Count) return;

        List<RectTransform> targetSlot = new();

        for(int i = 0; i < _slots.Count; i++)
        {
            if(_slots[i].IsActive)
                targetSlot.Add(_slotObjList[i]);
        }

        float line = 4;
        float spacing = 10f, cellSize = 120;
        
        for(int i = 1; i < targetSlot.Count + 1; i++)
        {
            int index = i - 1;

            var currentRect = targetSlot[index];
            int col = (int)(i % line);
            float moveX = col * (spacing + cellSize) - 30;

            currentRect.anchoredPosition = new Vector2(moveX, currentRect.anchoredPosition.y);
        }
    }
}
