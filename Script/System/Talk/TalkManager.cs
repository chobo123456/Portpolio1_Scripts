using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

//initialize
public partial class TalkManager : MonoBehaviour
{
    private TextMeshProUGUI context, characterNameContext;
    private GameObject mainPanel;
    private TalkData currentTalkData;
    private bool isLoop = false;
    private int count;

    private void OnEnable()
    {
        Initialize();
    }
    private void Initialize()
    {
        Initialize_Object();
        Initialize_Event();
        //Invoke("TestCode", 1f);
    }
    private void Initialize_Object()
    {
        mainPanel               = transform.FindTarget("mainPanel").gameObject;
        mainPanel.SetActive(false);

        Transform textBarParent = mainPanel.transform.FindTarget("TextBar");

        context                 = textBarParent.FindTarget("Context").GetComponent<TextMeshProUGUI>();
        Button btn              = textBarParent.GetComponent<Button>();

        characterNameContext    = mainPanel.transform.FindTarget("TalkCharacterName").FindTarget("Context").GetComponent<TextMeshProUGUI>();

        btn.onClick.AddListener(TalkProgress);
    }
    private void Initialize_Event()
    {
        EventBus.Sub<int>("OnTalk", TalkStart);
        EventBus.Sub<TalkData>("OnTalk_UseData", TalkStart);
    }
    private void OnDisable()
    {
        EventBus.UnSub<int>("OnTalk", TalkStart);
        EventBus.UnSub<TalkData>("OnTalk_UseData", TalkStart);
    }

    private void TestCode()
    {
        TalkStart(1);
    }
}

//system
public partial class TalkManager : MonoBehaviour
{
    private void TalkStart(TalkData talkData)
    {
        //TalkData설정 및 Talk시작
        currentTalkData = talkData;

        EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState",  GameStateType.Stop, GameEnableTimeSet.False);

        SetPanel(true);
        SetEvent(false);
        TalkProgress();
    }

    private void TalkStart(int talkId)
    {
        EventBus.Invoke<bool, float>("BlackPanelFade", true, 0.35f);

        //TalkData설정 및 Talk시작
        currentTalkData = DataLoader.GetData<TalkData>(DataType.Talk, talkId);

        EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState",  GameStateType.Stop, GameEnableTimeSet.False);

        SetPanel(true);
        SetEvent(false);
        TalkProgress();
    }

    private void TalkProgress()
    {
        if(currentTalkData == null || isLoop) return;
        
        if(count >= currentTalkData.infos.Count)
        {
            OnEndTalk();
            count = 0;
            isLoop = false;
            return;
        }

        isLoop = true;

        var currentInfo = currentTalkData.infos[count];

        string characterName    = currentInfo.talkCharacterName;
        string context          = currentInfo.talkContext;
        
        if(currentInfo.timeLineId > 0)
        {
            int timeLineId      = currentInfo.timeLineId;
            EventBus.Invoke("OnTimeLineStart");
            EventBus.Invoke<int>("PlayTimeLine", timeLineId);
        }

        count++;

        characterNameContext.SetText(characterName);

        this.RunRoutine(ContextLoop(context), "TalkManager_ContextLoop");       
    }

    IEnumerator ContextLoop(string contextArgs)
    {
        int counter = 0;
        string targetContext = "";
        context.SetText(targetContext);

        while(counter < contextArgs.Length)
        {
            targetContext += contextArgs[counter];

            context.SetText(targetContext);

            if((float)targetContext.Length / contextArgs.Length >= 0.7f) isLoop = false;

            counter++;

            yield return YieldUtil.WaitForSecondsRealtime(0.1f);
        }

        isLoop = false;
    }

    private void OnEndTalk()
    {
        InvokeEvents();
        SetPanel(false);
    }

    private void InvokeEvents()
    {
        this.RunRoutine(DelayFadeIn(), "TalkEndDelayEvents");
    }

    IEnumerator DelayFadeIn()
    {
        EventBus.Invoke("OnTimeLineEnd");
        EventBus.Invoke("DisableCutSceneCam");
        EventBus.Invoke("ResumeCamera");

        EventBus.Invoke<bool, float>("BlackPanelFade", true, 0.45f);

        yield return new WaitUntil(() => EventBus.Invoke_Func<bool>("FadeFinish"));

        if(currentTalkData.talkType == TalkType.Quest && currentTalkData.etcInfo.finishQuestId > 0)
        {
            EventBus.Invoke<QuestType, int>("QuestManager_OnAskQuestFinish", QuestType.Interact, currentTalkData.etcInfo.finishQuestId);
        }
        
        if(!string.IsNullOrEmpty(currentTalkData.shopInfo.invokeEventName) && currentTalkData.talkType == TalkType.Shop)
        {
            EventBus.Invoke($"{currentTalkData.shopInfo.invokeEventName}");
        }
        else
        {
            SetEvent(true);
        }  

        EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState",  GameStateType.Run, GameEnableTimeSet.False);

        if(currentTalkData.talkType == TalkType.Quest && currentTalkData.etcInfo.getQuestId > 0)
        {
            EventBus.Invoke<int>("ReceiveQuest", currentTalkData.etcInfo.getQuestId);
        }
    }

    private void SetPanel(bool active)
    {
        mainPanel.SetActive(active);
    }
    private void SetEvent(bool active)
    {
        EventBus.Invoke("CharacterStateAbort");
    
        CursorManager.CursorActive(!active);
        EventBus.Invoke<bool>("SetCameraRotateLock", !active);
        EventBus.Invoke<bool>("Lock_All_UI", !active);
        EventBus.Invoke<bool>("MainCanvasActive", active);
    }
}