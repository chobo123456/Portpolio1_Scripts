using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ItemInteractManager : MonoBehaviour
{
    private List<ItemInteractUI> slots = new();
    private Dictionary<int, ItemInteractUI> currentUsingSlots = new();
    private List<ItemInteractUI> activeSlots = new();

    private int exceptionPhase = 0;
    private GameObject prefab;
    public float rectX = 0f;

    private Coroutine moveRoutine;

    private void OnEnable()
    {
        this.RunRoutine(Initialize());

        EventBus.Sub<int, int>("ShowItemInteractBar", OnInteractDetect);
        EventBus.Sub<int>("UnShowItemInteractBar", OnInteractUnDetect);
    }

    private void OnDisable()
    {
        EventBus.UnSub<int, int>("ShowItemInteractBar", OnInteractDetect);
        EventBus.UnSub<int>("UnShowItemInteractBar", OnInteractUnDetect);
    }

    IEnumerator Initialize()
    {
        LoadPrefab();

        yield return new WaitUntil(() => prefab != null);

        InitializeSlot();
    }

    private void OnInteractDetect(int itemId, int instanceId)
    {
        if(currentUsingSlots.ContainsKey(instanceId)) return;

        var interactUISlot = GetInteractUISlot();
        interactUISlot.SetShow(itemId);

        currentUsingSlots.Add(instanceId, interactUISlot);

        AddToActive(interactUISlot);
    }

    private void AddToActive(ItemInteractUI slot)
    {
        if(activeSlots.Contains(slot)) return;

        activeSlots.Add(slot);

        int currentLine = activeSlots.Count - 1;
        float desiredSpace = 40f;
        float desiredMoveY = -(currentLine * desiredSpace);

        slot.SetRectPosition(rectX, desiredMoveY);
    }

    private void OnInteractUnDetect(int instanceId)
    {
        if(currentUsingSlots.TryGetValue(instanceId, out var slot))
        {
            slot.SetUnShow();
            currentUsingSlots.Remove(instanceId);
        }

        if(moveRoutine != null) return;
        moveRoutine = this.RunRoutine(MoveLoop());
    }

    IEnumerator MoveLoop()
    {
        while(activeSlots.Count > 0)
        {
            yield return this.RunRoutine(ReleaseSlot());
        }

        moveRoutine = null;
    }

    IEnumerator ReleaseSlot()
    {
        var targetRemoveSlot = GetReadyToMoveUI();
        if(targetRemoveSlot == null) yield break;

        yield return this.RunRoutine(SlotRight(targetRemoveSlot));
        yield return YieldUtil.WaitForSeconds(0.05f);

        targetRemoveSlot.gameObject.SetActive(false);

        if(activeSlots.Count <= 0) yield break;

        int startIndex = activeSlots.FindIndex(x => x == targetRemoveSlot);
        activeSlots.Remove(targetRemoveSlot);

        yield return this.RunRoutine(SlotUp(startIndex, activeSlots.Count));
        yield return null;
    }

    private IEnumerator SlotRight(ItemInteractUI slot)
    {
        RectTransform rect = slot.GetRect();
        float startX    = rect.anchoredPosition.x;
        float originY   = rect.anchoredPosition.y;
        float endX = startX + 200f;

        float percent = 0f, deltaTime = 0f, lerpTime = 0.15f;

        //실제 이동
        while(percent < 1)
        {
            deltaTime += Time.deltaTime;
            percent = deltaTime / lerpTime;
            
            float moveX = Mathf.Lerp(startX, endX, percent);

            slot.SetRectPosition(moveX, originY);
            yield return null;
        }

        slot.SetRectPosition(endX, originY);
    }

    private IEnumerator SlotUp(int startIndex, int loopCount)
    {
        int arrayCount = Mathf.Max(activeSlots.Count, 0);

        float[] activeSlotMoveY = new float[arrayCount];
        for(int i = 0; i < arrayCount; i++)
            activeSlotMoveY[i] = activeSlots[i].GetRect().anchoredPosition.y;

        float percent = 0f, lerpTime = 0.15f, deltaTime = 0f;

        while(percent < 1)
        {
            deltaTime += Time.deltaTime;
            percent = deltaTime / lerpTime;

            for(int i = startIndex; i < loopCount; i++) // 제거 대상 인덱스 ~ 마지막 활성된 슬롯까지
            {
                var slot = activeSlots[i];

                float startY = activeSlotMoveY[i];
                float endY = startY + 40f;

                float moveY = Mathf.Lerp(startY, endY, percent);
                slot.SetRectPosition(rectX, moveY);
            }

            yield return null;
        }
    }

    private ItemInteractUI GetReadyToMoveUI()
    {
        for(int i = 0; i < activeSlots.Count; i ++)
        {
            var slot = activeSlots[i];

            if(slot.IsReadyToMove())
                return slot;
        }

        return null;
    }

    private async void LoadPrefab()
    {
        prefab = await AddressableUtil.Load_Instant<GameObject>("ItemInteract");
    }

    private void InitializeSlot()
    {
        Transform parentTr = transform.FindTarget("Content");

        for(int i = 0; i < 20; i++)
        {
            GameObject slot = GameObject.Instantiate(prefab);
            slot.transform.SetParent(parentTr, false);
            slot.SetActive(false);

            var comp = slot.GetComponent<ItemInteractUI>();
            if(comp == null) continue;

            comp.Initialize();
            slots.Add(comp);
        }
    }

    private ItemInteractUI GetInteractUISlot()
    {
        if(exceptionPhase >= 3) return null;

        int currentSlotsCount = slots.Count;

        for(int i = 0; i <= currentSlotsCount; i++)
        {
            var slot = slots[i];
            if(!slot.IsAlreadyShow())
            {
                slot.gameObject.SetActive(true);
                return slot;
            }
        }

        InitializeSlot();

        if(slots.Count == currentSlotsCount)
            exceptionPhase++;

        return GetInteractUISlot();
    }
}
