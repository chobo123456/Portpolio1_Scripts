using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HUDManager : MonoBehaviour
{
    private RectTransform panel;
    private Vector3 startPos, targetPos;
    private RectTransform craftIcon, settingIcon, questIcon, statusIcon, inventoryIcon, partyIcon;
    private Dictionary<UIType, RectTransform> uiTypeMap = new();

    private void OnEnable()
    {
        Transform buttonParent = transform.Find("Buttons");

        GetPosition(buttonParent);
        GetIcons(buttonParent);
        SettingList();
        SubscribeEvents(true);
    }

    private void SubscribeEvents(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<bool>("OnBattle",     OnBattle);
            EventBus.Sub<UIType, bool>("UIShow", ShowUI);
            EventBus.Sub_Func<UIType, RectTransform>("GetUIIconRect", GetIconRect);
        }
        else
        {
            EventBus.UnSub<bool>("OnBattle",     OnBattle);
            EventBus.UnSub<UIType, bool>("UIShow", ShowUI);
            EventBus.UnSub_Func<UIType, RectTransform>("GetUIIconRect", GetIconRect);
        }
    }

    private void OnDisable()
    {
        SubscribeEvents(false);
    }

    private void GetPosition(Transform parent)
    {
        panel       = parent.GetComponent<RectTransform>();
        startPos    = panel.anchoredPosition;
        targetPos   = transform.Find("MovePos").GetComponent<RectTransform>().anchoredPosition;
    }

    private void GetIcons(Transform parent)
    {
        craftIcon       = parent.Find("Craft").GetComponent<RectTransform>();
        settingIcon     = parent.Find("Setting").GetComponent<RectTransform>();
        questIcon       = parent.Find("Quest").GetComponent<RectTransform>();
        statusIcon      = parent.Find("Status").GetComponent<RectTransform>();
        inventoryIcon   = parent.Find("Inventory").GetComponent<RectTransform>();
        partyIcon       = parent.Find("Party").GetComponent<RectTransform>();
    }

    private void SettingList()
    {
        uiTypeMap.Add(UIType.Setting, settingIcon);
        uiTypeMap.Add(UIType.Quest, questIcon);
        uiTypeMap.Add(UIType.Inventory, inventoryIcon);
        uiTypeMap.Add(UIType.Craft, craftIcon);
        uiTypeMap.Add(UIType.CharacterStatus, statusIcon);
        uiTypeMap.Add(UIType.Party, partyIcon);
    }

    private void OnBattle(bool isStart)
    {
        EventBus.Invoke<bool>("EnableTutorialShowStep", false);

        Vector3 target = isStart ? targetPos : startPos;
        this.RunRoutine(PanelMove(panel.anchoredPosition, target), "UIMove_PanelMove");
    }
    
    IEnumerator PanelMove(Vector3 startPos, Vector3 endPos)
    {
        float percent = 0f, currentTime = 0f, lerpTime = 0.5f;

        while(percent < 1f)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / lerpTime;

            Vector3 movePos = Vector3.Lerp(startPos, endPos, percent);
            panel.anchoredPosition = movePos;

            yield return null;
        }

        panel.anchoredPosition = endPos;

        EventBus.Invoke<bool>("EnableTutorialShowStep", true);
    }

    private void ShowUI(UIType type, bool active)
    {
        if(uiTypeMap.TryGetValue(type, out RectTransform icon))
            icon.gameObject.SetActive(active);

        UIReload();
    }

    private void UIReload()
    {
        List<RectTransform> activedUI = new();

        foreach(var ui in uiTypeMap.Values)
        {
            if(ui.gameObject.activeSelf || ui.gameObject.activeInHierarchy)
                activedUI.Add(ui);
        }

        float horizontalSpace = 67.5f;

        for(int i = 0; i < activedUI.Count; i++)
        {
            RectTransform iconRect = activedUI[i];
            float col = -i * horizontalSpace;

            iconRect.anchoredPosition = new Vector2(col, iconRect.anchoredPosition.y);
        }
    }

    private RectTransform GetIconRect(UIType type)
    {
        if(uiTypeMap.TryGetValue(type, out RectTransform rect))
            return rect;   

        return null;
    }
}
