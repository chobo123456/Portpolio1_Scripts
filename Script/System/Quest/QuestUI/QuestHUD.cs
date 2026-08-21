using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public struct QuestHUDPayload
{
    public bool isDrawable;
    public string questName;
    public string questHUDDiscription;
    public float questProgress;
    public float maxQuestProgress;
}

public partial class QuestHUD : MonoBehaviour
{
    public Transform _questHUDPanel;
    private RectTransform _HUDPanel;
    private Vector2 startPos, endPos;
    private TextMeshProUGUI questName, questDiscription, questProgress;
    private int currentQuestId;
    
    private void OnEnable()
    {
        ReferenceMainPanel();
        ReferencePosition();
        ReferenceText();
        SubscribeEvent(true);
    }   

    private void ReferenceMainPanel()
    {
        _HUDPanel = _questHUDPanel.FindTarget("Main_HUD_Panel").GetComponent<RectTransform>();
        _HUDPanel.gameObject.SetActive(false);
    }
    private void ReferencePosition()
    {
        startPos    = _questHUDPanel.FindTarget("StartPoint").GetComponent<RectTransform>().anchoredPosition;   
        endPos      = _questHUDPanel.FindTarget("EndPoint").GetComponent<RectTransform>().anchoredPosition;    
    }   
    private void ReferenceText()
    {
        questName           = _questHUDPanel.FindTarget("QuestName").GetComponent<TextMeshProUGUI>();
        questDiscription    = _questHUDPanel.FindTarget("QuestDiscription").GetComponent<TextMeshProUGUI>();
        questProgress       = _questHUDPanel.FindTarget("QuestProgress").GetComponent<TextMeshProUGUI>();
    }
    private void SubscribeEvent(bool isSubscribe)
    {
        if (isSubscribe)
        {
            EventBus.Sub<QuestHUDPayload>("Quest_HUD_DrawUI", DrawUI);
            EventBus.Sub<float, float>("Quest_HUD_UpdateProgress", UpdateProgress);
            EventBus.Sub("Quest_HUD_FinishQuest", OnFinishQuest);
        }
        else
        {
            EventBus.UnSub<QuestHUDPayload>("Quest_HUD_DrawUI", DrawUI);
            EventBus.UnSub<float, float>("Quest_HUD_UpdateProgress", UpdateProgress);
            EventBus.UnSub("Quest_HUD_FinishQuest", OnFinishQuest);
        }
    }

    private void OnDisable()
    {
        SubscribeEvent(false);
    }
}

//Output
public partial class QuestHUD : MonoBehaviour
{
    private void DrawUI(QuestHUDPayload payload) //id <= 0
    {
        bool isHUDPanelActive = _HUDPanel.gameObject.activeSelf;

        if (payload.isDrawable)
        {
            questName.SetText(payload.questName);
            questDiscription.SetText(payload.questHUDDiscription);
            questProgress.SetText($"{payload.questProgress} / {payload.maxQuestProgress}");

            if(!isHUDPanelActive) _HUDPanel.gameObject.SetActive(true);

            this.RunRoutine(Move(startPos, endPos), "Quest_HUD_Move");
        }
        else
        {
            if(isHUDPanelActive) _HUDPanel.gameObject.SetActive(false);
        }
    }

    private void OnFinishQuest()
    {
        this.RunRoutine(Move(endPos, startPos), "Quest_HUD_Move");

        if(_HUDPanel.gameObject.activeSelf)
            _HUDPanel.gameObject.SetActive(false);
    }

    IEnumerator Move(Vector2 start, Vector2 end)
    {
        float currentTime = 0f, percent = 0f, lerpTime = 0.45f;

        while(percent < 1)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / lerpTime;

            Vector2 lerpPos = Vector2.Lerp(start, end, percent);

            _HUDPanel.anchoredPosition = lerpPos;

            yield return null;
        }

        _HUDPanel.anchoredPosition = end;
    }

    private void UpdateProgress(float progress, float maxProgress)
    {
        questProgress.SetText($"{progress} / {maxProgress}");
    }
}