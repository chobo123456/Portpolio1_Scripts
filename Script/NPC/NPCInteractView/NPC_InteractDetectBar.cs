using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class NPC_InteractDetectBar : MonoBehaviour
{
    private Vector3 startPos, endPos;
    private RectTransform rect;
    private TextMeshProUGUI context;
    private Coroutine routine;

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        EventBus.UnSub<(int, bool)>("OnInteractNpc", OnInteract);
    }

    private void Initialize()
    {
        FindObjectsAndGetComponent();
        EventSub();
    }

    private void FindObjectsAndGetComponent()
    {
        Transform detectBarPr = transform.FindTarget("TalkDetectBar");
        rect        = detectBarPr.GetComponent<RectTransform>();
        context     = detectBarPr.FindTarget("InteractContext").GetComponent<TextMeshProUGUI>();

        startPos    = transform.FindTarget("StartPos").GetComponent<RectTransform>().anchoredPosition;
        endPos      = transform.FindTarget("EndPos").GetComponent<RectTransform>().anchoredPosition;

        rect.gameObject.SetActive(false);
    }

    private void EventSub()
    {
        EventBus.Sub<(int, bool)>("OnInteractNpc", OnInteract);
    }

    private void OnInteract((int npcId, bool active) tuple)
    {
        if(tuple.npcId >= 1)
        {
            NPCData data = DataLoader.GetData<NPCData>(DataType.NPC, tuple.npcId);

            context.SetText(data.npcName);

            if(tuple.active)
                routine = this.RunRoutine(MoveTo(startPos, endPos, tuple.active), routine);
            else
                routine = this.RunRoutine(MoveTo(endPos, startPos, tuple.active), routine);
        }
    }

    System.Collections.IEnumerator MoveTo(Vector3 start, Vector3 end, bool active)
    {
        if(active) rect.gameObject.SetActive(active);
        
        float currentTime = 0f, percent = 0f, lerpTime = 0.2f;

        while(percent < 1f)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / lerpTime;

            Vector3 lerpPos = Vector3.Lerp(start, end, percent);
            rect.anchoredPosition = lerpPos;

            yield return null;
        }

        if(!active) rect.gameObject.SetActive(active);
    }
}
