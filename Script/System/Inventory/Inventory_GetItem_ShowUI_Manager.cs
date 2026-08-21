using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public partial class Inventory_GetItem_ShowUI_Manager : MonoBehaviour
{
    private List<Inventory_GetItem_ShowUI> uiList = new();
    private GameObject showUiPrefab;
    public float rectX = 0f;
    private WaitUntil wait;
    private int currentUseIndex = 0;
    private bool isInitialized = false;

    private void OnEnable()
    {
        wait = new WaitUntil(() => LoadStatus.IsReady && isInitialized);
        EventBus.Sub<int, int>("ShowItemGetUI", OnShow);
        EventBus.Sub("RemoveItemGetShowUI", UnShowTargetSlot);
        Initialize();
    }

    private void OnDisable()
    {
        EventBus.UnSub<int, int>("ShowItemGetUI", OnShow);
        EventBus.UnSub("RemoveItemGetShowUI", UnShowTargetSlot);
    }

    private void Initialize()
    {
        IntializePrefab();
    }

    private async void IntializePrefab()
    {
        showUiPrefab = await AddressableUtil.Load_Instant<GameObject>("ItemInteract_Show", this.GetCancelOnDestroy());
        Initialize_List();

        isInitialized = true;
    }

    private void Initialize_List()
    {
        Transform parentTr = this.transform.FindTarget("ShowUIContent");

        for(int i = 0; i < 20; i ++)
        {
            GameObject newObj = Instantiate(showUiPrefab);
            newObj.transform.SetParent(parentTr, false);
            newObj.SetActive(false);

            var comp = newObj.GetComponent<Inventory_GetItem_ShowUI>();
            uiList.Add(comp);
        }
    }
}

//시스템
public partial class Inventory_GetItem_ShowUI_Manager : MonoBehaviour
{
    private Coroutine moveLoopRoutine;
    private List<Inventory_GetItem_ShowUI> activedUI = new();
    private void OnShow(int id, int amount)
    {
        if(amount <= 0) return;

        this.RunRoutine(DelayShow(id, amount));
    }

    IEnumerator DelayShow(int itemId, int itemAmount)
    {
        yield return wait;

        ItemData data = DataLoader.GetData<ItemData>(DataType.Item, itemId);

        var showUI = GetFromPool();
        showUI.SetItemInfo(data, itemAmount);

        yield return null;
        AddToActive(showUI);
    }

    private void AddToActive(Inventory_GetItem_ShowUI slot)
    {
        if(activedUI.Contains(slot)) return;

        activedUI.Add(slot);
        slot.SetPosition(rectX, -(activedUI.Count - 1) * 55f);
        slot.SetEnable();
    }

    private void UnShowTargetSlot()
    {
        if(moveLoopRoutine != null) return;
        moveLoopRoutine = this.RunRoutine(MoveLoop());
    }

    IEnumerator MoveLoop()
    {
        while(activedUI.Count > 0)
        {
            yield return this.RunRoutine(MoveUpSlot());
        }    

        moveLoopRoutine = null;
    }

    IEnumerator MoveUpSlot()
    {
        var removeSlot = activedUI[0];
        activedUI.Remove(removeSlot);

        float percent = 0f, deltaTime = 0f, lerpTime = 0.25f;

        float startY = removeSlot.GetAnchorPosition().y;        
        float endY   = startY + 55f;

        int currentActiveUICount = activedUI.Count;

        float[] activeSlotsY = new float[currentActiveUICount];

        for(int i = 0; i < currentActiveUICount; i++)
            activeSlotsY[i] = activedUI[i].GetAnchorPosition().y;

        while(percent <= 1f)
        {
            deltaTime += Time.deltaTime;
            percent = deltaTime / lerpTime;

            float moveY = Mathf.Lerp(startY, endY, percent);
            removeSlot.SetPosition(rectX, moveY);

            for(int i = 0; i < currentActiveUICount; i++)
            {
                var slot = activedUI[i];

                float slotStartY = activeSlotsY[i];
                float slotEndY   = slotStartY + 55f;

                float slotMoveY = Mathf.Lerp(slotStartY, slotEndY, percent);

                slot.SetPosition(rectX, slotMoveY);
            }

            yield return null;
        }
        
        removeSlot.gameObject.SetActive(false);
        removeSlot.SetPosition(rectX, endY);

        yield return YieldUtil.WaitForSeconds(1f);
    }
}

//풀
public partial class Inventory_GetItem_ShowUI_Manager : MonoBehaviour
{
    private Inventory_GetItem_ShowUI GetFromPool()
    {
        if(currentUseIndex >= uiList.Count)
        {
            Initialize_List();
            return GetFromPool();
        } 

        var slot = uiList[currentUseIndex];
        currentUseIndex = (currentUseIndex + 1) % uiList.Count;

        return slot;
    }
}