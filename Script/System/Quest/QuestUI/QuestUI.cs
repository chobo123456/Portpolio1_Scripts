using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct QuestPreviewPayload
{
    public bool isDrawable; //questId <= 0
    public bool isMainQuest; // mainQuestId == questId
    public string questName;
    public string questDiscription;
    public float questProgress;
    public float maxQuestProgress;
}

public partial class QuestUI : UIClass
{
    public GameObject mainPanel;
    private List<Button> pool = new();
    private Dictionary<int, Button> _currentQuestContexts = new();
    private GameObject contextPrefab;
    private TextMeshProUGUI questName, questDiscription, questProgress, acceptButtonText;
    private Button questAcceptButton;
    private bool isInit = false;
    private int exceptionCount = 0;

    public override void OnEnable()
    {
        base.SetType(UIType.Quest);
        base.OnEnable();
        this.RunRoutine(Booting());
    }
    
    private void ReferenceText()
    {
        Transform parent = mainPanel.transform.FindTarget("QuestInfoPanel");

        questName           = parent.Find("QuestName_Text").GetComponent<TextMeshProUGUI>();
        questDiscription  = parent.Find("QuestDiscription_Text").GetComponent<TextMeshProUGUI>();
        questProgress  = parent.Find("QuestProgress_Text").GetComponent<TextMeshProUGUI>();
    }

    private void ReferenceButton()
    {
        GameObject acceptButtonObj = mainPanel.transform.FindTarget("SetMainQuestButton").gameObject;

        acceptButtonText  = acceptButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        questAcceptButton = acceptButtonObj.GetComponent<Button>();
        questAcceptButton.onClick.AddListener(OnAcceptQuest);

        GameObject closeButtonObj = mainPanel.transform.FindTarget("CloseButton").gameObject;
        Button closeButton = closeButtonObj.GetComponent<Button>();
        closeButton.onClick.AddListener(() => base.OnClickCloseButton());
    }

    private async void ReferencePrefab()
    {
        contextPrefab = await AddressableUtil.Load_Instant<GameObject>("QuestContext", this.GetCancelOnDestroy());
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if (isSubscribe)
        {
            EventBus.Sub<List<int>, int>("Quest_UI_Initialize", Initialize);
            EventBus.Sub<int>("Quest_UI_FinishQuest", OnFinishQuest);
            EventBus.Sub<int>("Quest_UI_GetNewQuest", OnGetNewQuest);
            EventBus.Sub<QuestPreviewPayload>("Quest_UI_DrawQuestPreview", DrawQuestPreview);
            EventBus.Sub<float, float>("Quest_UI_UpdateProgress", UpdateProgress);
        }
        else
        {
            EventBus.UnSub<List<int>, int>("Quest_UI_Initialize", Initialize);
            EventBus.UnSub<int>("Quest_UI_FinishQuest", OnFinishQuest);
            EventBus.UnSub<int>("Quest_UI_GetNewQuest", OnGetNewQuest);
            EventBus.UnSub<QuestPreviewPayload>("Quest_UI_DrawQuestPreview", DrawQuestPreview);
            EventBus.UnSub<float, float>("Quest_UI_UpdateProgress", UpdateProgress);
        }
    }

    private void InitializePool()
    {
        Transform parent = mainPanel.transform.FindTarget("Quest_Context");

        for(int i = 0; i < 5; i++)
        {
            GameObject obj = Instantiate(contextPrefab);
            obj.transform.SetParent(parent);
            obj.SetActive(false);

            Button button = obj.GetComponent<Button>();
            pool.Add(button);
        }
    }

    private Button GetFromPool()
    {
        for(int i = 0; i < pool.Count; i++)
        {
            Button button = pool[i];

            if (!button.gameObject.activeSelf)
            {
                exceptionCount = 0;
                return button;
            }
        }

        if(exceptionCount > 3)
        {
            Util.Log($"QuestUI.cs Error : GetFromPool() | Can't Get Button From Pool ", "red");
            return null;
        } 
        exceptionCount++;

        InitializePool();

        return GetFromPool();
    }

    IEnumerator Booting()
    {
        mainPanel.SetActive(false);
        ReferencePrefab();
        ReferenceText();
        ReferenceButton();
        SubscribeEvent(true);

        yield return new WaitUntil(() => contextPrefab != null);

        InitializePool();

        EventBus.Invoke("Quest_UI_LocalReady");
    }

    private void Initialize(List<int> questIds, int mostSelectableQuestId)
    {
        InitializeQuestContext(questIds);
        OnContextClick(mostSelectableQuestId);

        isInit = true;
    }

    private void InitializeQuestContext(List<int> questIdList)
    {
        for(int i = 0; i < questIdList.Count; i++)
        {
            int questId = questIdList[i];
            OnGetNewQuest(questId);
        }
    }

    private void OnDisable()
    {
        SubscribeEvent(false);
    }

    //GetRectTransform
    public override RectTransform GetRectTransform(UIRectName rectName)
    {
        switch(rectName)
        {
            case UIRectName.QuestUI_ContextButton:
                return mainPanel.transform.FindTarget("Quest_Context").GetComponent<RectTransform>();

            case UIRectName.QuestUI_QuestAcceptButton:
                return questAcceptButton.GetComponent<RectTransform>();
        }

        return null;
    }
}

//Input
public partial class QuestUI : UIClass
{
    //콘텍스트 버튼 클릭시
    private void OnContextClick(int questId)
    {
        EventBus.Invoke("Quest_UI_OnQuestContextClick", questId);
    }

    private void OnAcceptQuest()
    {
        EventBus.Invoke("Quest_UI_OnQuestAccept");
    }
    
    private void OnGetNewQuest(int questId)
    {
        if(!_currentQuestContexts.ContainsKey(questId))
        {
            Button button = GetFromPool();
            button.gameObject.SetActive(true);
            button.onClick.AddListener(() => OnContextClick(questId));

            _currentQuestContexts.Add(questId, button);
        }
    }

    private void OnFinishQuest(int questId)
    {
        if(_currentQuestContexts.TryGetValue(questId, out Button button))
        {
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);

            _currentQuestContexts.Remove(questId);
        }
    }
}

//OutPut
public partial class QuestUI : UIClass
{
    private void DrawQuestPreview(QuestPreviewPayload payload)
    {
        if (!payload.isDrawable) 
        {
            questName.SetText("");
            questDiscription.SetText("You Don't Had Any Quest");
            questProgress.SetText("");
            
            if(questAcceptButton.gameObject.activeSelf) 
                questAcceptButton.gameObject.SetActive(false);
            return;
        }

        questName.SetText(payload.questName);
        questDiscription.SetText(payload.questDiscription);
        questProgress.SetText($"{payload.questProgress} / {payload.maxQuestProgress}");

        if (payload.isMainQuest) acceptButtonText.SetText("Decline Main Quest");
        else acceptButtonText.SetText("Accept To Main");

        if (!questAcceptButton.gameObject.activeSelf) 
            questAcceptButton.gameObject.SetActive(true);
    }

    private void UpdateProgress(float progress, float maxProgress)
    {
        questProgress.SetText($"{progress} / {maxProgress}");
    }
}

public partial class QuestUI : UIClass
{
    public override bool IsReady()
    {
        return isInit;
    }

    public override void Close()
    {
        mainPanel.SetActive(false);
    }
    public override void Open()
    {
        mainPanel.SetActive(true);
    }
}