using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public struct WalkieTalkiePayload
{
    public bool canPlay;
    public WalkieTalkieData walkieTalkieData;
}

public class WalkieTalkieManager : MonoBehaviour
{
    private int walkieTalkieIndex = 0;
    private CanvasGroup alphaGroup;
    private TextMeshProUGUI context;
    private GameObject active;
    private Image icon;

    private void OnEnable()
    {
        ReferenceUI();
        SubscribeEvent(true);
    }

    private void ReferenceUI()
    {
        alphaGroup = GetComponent<CanvasGroup>();
        alphaGroup.alpha = 0f;

        active = transform.Find("Active").gameObject;
        active.SetActive(false);

        icon = active.transform.Find("NpcIcon").GetComponent<Image>();
        context = active.transform.Find("Context").GetComponent<TextMeshProUGUI>();
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<WalkieTalkiePayload>("PlayWalkieTalkie", PlayWalkieTalkie);
        }
        else
        {
            EventBus.UnSub<WalkieTalkiePayload>("PlayWalkieTalkie", PlayWalkieTalkie);
        }
    }

    private void OnDisable()
    {
        SubscribeEvent(false);
    }
    
    private void PlayWalkieTalkie(WalkieTalkiePayload payload)
    {
        if(!payload.canPlay) return;

        walkieTalkieIndex = 0;
        this.RunRoutine(ActiveLoop(payload.walkieTalkieData), "WalkieTalkieOn");
    }

    IEnumerator ActiveLoop(WalkieTalkieData walkieTalkieData)
    {
        yield return new WaitUntil(() => GameState.IsActive());
        yield return YieldUtil.WaitForSeconds(0.5f);

        SetImage(walkieTalkieData.walkieTalkieDatas[0].speekNpc);
        context.SetText("");

        active.SetActive(true);
        yield return this.RunRoutine(Fade(0f, 1f, 0.25f));
        yield return this.RunRoutine(PlayLoop(walkieTalkieData));
        yield return this.RunRoutine(Fade(1f, 0f, 0.3f));
        active.SetActive(false);
    }

    IEnumerator Fade(float start, float end, float speed)
    {
        float percent = 0f, delta = 0f;

        while(percent < 1)
        {
            delta += Time.deltaTime;
            percent = delta / speed;

            float alpha = Mathf.Lerp(start, end, percent);

            alphaGroup.alpha = alpha;

            yield return null;
        }

        alphaGroup.alpha = end;
    }

    IEnumerator PlayLoop(WalkieTalkieData walkieTalkieData)
    {
        WalkieTalkieTalkData[] walkieTalkieTalkData = walkieTalkieData.walkieTalkieDatas;

        while(walkieTalkieIndex < walkieTalkieTalkData.Length)
        {
            WalkieTalkieTalkData walkieDataTalk = walkieTalkieTalkData[walkieTalkieIndex];

            string currentContext = walkieDataTalk.speekContext;

            SetImage(walkieDataTalk.speekNpc);

            yield return this.RunRoutine(ShowText(currentContext));

            yield return YieldUtil.WaitForSeconds(0.75f);
            walkieTalkieIndex++;
        }

        yield return null;
    }

    IEnumerator ShowText(string currentText)
    {
        string currentContext = "";

        int stringIndex = 0;

        while(stringIndex < currentText.Length)
        {
            currentContext += currentText[stringIndex];

            context.SetText(currentContext);

            stringIndex++;

            yield return YieldUtil.WaitForSeconds(0.05f);
        }
    }

    private void SetImage(Sprite sprite)
    {
        icon.sprite = sprite;
    }
}
