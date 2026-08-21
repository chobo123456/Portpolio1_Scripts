using UnityEngine;
using TMPro;
using System.Collections;

public struct QuestPointPayload
{
    public bool isDrawable;
    public Vector3 questPoint;
}

public partial class QuestPoint : MonoBehaviour
{
    public Transform questPointTransform;
    private RectTransform questPointerRect, parentRect;
    private TextMeshProUGUI questPointDistanceText;
    private Vector3 currrentQuestPoint;
    private Vector2 currentVelocity;
    private Transform character;
    private bool isChase = false;
    private float smoothTime = 0.02f;

    private void OnEnable()
    {
        ReferenceUI();
        SubscribeEvent(true);
    }
    
    private void ReferenceUI()
    {
        parentRect = questPointTransform.GetComponent<RectTransform>();

        Transform uiParent = questPointTransform.FindTarget("QuestPointUI");
        questPointerRect        = uiParent.GetComponent<RectTransform>();
        questPointDistanceText  = uiParent.GetComponentInChildren<TextMeshProUGUI>();
        questPointerRect.gameObject.SetActive(false);

    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<Transform>("SetCharacterTransform", SetCharacter);
            EventBus.Sub<bool>("MainCanvasActive", MainCanvasCase);
            EventBus.Sub<QuestPointPayload>("Quest_Point_DrawUI", ChaseStart);
            EventBus.Sub("Quest_Point_FinishQuest", ChaseEnd);
        }
        else
        {
            EventBus.UnSub<Transform>("SetCharacterTransform", SetCharacter);
            EventBus.UnSub<bool>("MainCanvasActive", MainCanvasCase);
            EventBus.UnSub<QuestPointPayload>("Quest_Point_DrawUI", ChaseStart);
            EventBus.UnSub("Quest_Point_FinishQuest", ChaseEnd);
        }   
    }

    private void OnDisable()
    {
        SubscribeEvent(false);
    }
}


//Output
public partial class QuestPoint : MonoBehaviour
{
    private void MainCanvasCase(bool active)
    {
        if(!isChase) return;
        
        questPointerRect.gameObject.SetActive(active);
    }

    private void SetCharacter(Transform character)
    {
        this.character = character;
    }

    private void ChaseStart(QuestPointPayload payload)
    {   
        if(!payload.isDrawable)
        {
            ChaseEnd();
            return;
        }

        currrentQuestPoint = payload.questPoint;
        
        questPointerRect.anchoredPosition = GetCurrentQuestPointCalculate();
        Text();

        questPointerRect.gameObject.SetActive(true);
        isChase = true;
    }

    private void ChaseEnd()
    {
        isChase = false;
        questPointerRect.gameObject.SetActive(false);
        currrentQuestPoint = Vector3.zero;
    }

    private void Update()
    {
        if(isChase && GameState.IsActive())
        {
            Point();
            Text();
        }
    }

    private void Point()
    {
        Vector2 result = GetCurrentQuestPointCalculate();
        questPointerRect.anchoredPosition = Vector2.SmoothDamp(questPointerRect.anchoredPosition, result, ref currentVelocity, smoothTime); 
    }

    private void Text()
    {
        if(character == null) return;

        float distance = (currrentQuestPoint - character.position).magnitude;

        questPointDistanceText.SetText($"{distance:F1} m");
    }

    private Vector2 GetCurrentQuestPointCalculate()
    {
        Vector3 viewPortVector = Camera.main.WorldToViewportPoint(currrentQuestPoint);
        
        if(viewPortVector.z <= 0f)
        {
            viewPortVector.x = 1f - viewPortVector.x;
            viewPortVector.y = 0f;
        }

        float margin = 0.2f;

        float Min = margin;
        float Max = 1 - margin;

        viewPortVector.x = Mathf.Clamp(viewPortVector.x, Min, Max);
        viewPortVector.y = Mathf.Clamp(viewPortVector.y, Min, Max);

        Vector3 screenVector = Camera.main.ViewportToScreenPoint(viewPortVector);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenVector,
            null,
            out Vector2 result
        );

        return result;
    }
}