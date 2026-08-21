using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BannerUI : MonoBehaviour
{
    public Transform _mainPanel;
    private RectTransform _uiPanel;
    private CanvasGroup _canvasGroup;
    private TextMeshProUGUI _text;
    private float _originY;

    private void OnEnable()
    {
        ReferenceUI();
        SubscribeEvent(true);
    }   

    private void ReferenceUI()
    {
        if(_uiPanel == null)
        {
            _uiPanel = _mainPanel.GetComponent<RectTransform>();
            _originY = _uiPanel.localScale.y;
        }

        if(_canvasGroup == null)
            _canvasGroup = _mainPanel.GetComponent<CanvasGroup>();

        if(_text == null)
            _text = _mainPanel.FindTarget("Text").GetComponent<TextMeshProUGUI>();
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<int, string>("Banner_UI_Start", BannerStart);
        }
        else
        {
            EventBus.UnSub<int, string>("Banner_UI_Start", BannerStart);
        }
    }

    private void OnDisable()
    {
        SubscribeEvent(false);
    }

    private void BannerStart(int tutorialId, string tutorialContext)
    {
        SetText(tutorialContext);
        this.RunRoutine(OpenUI(tutorialId));
    } 

    IEnumerator OpenUI(int tutorialId)
    {
        _uiPanel.gameObject.SetActive(true);
        
        _uiPanel.localScale = new Vector2(_uiPanel.localScale.x, _originY);

        _canvasGroup.alpha = 0f;

        float start = 0f, end = 1f;
        float percent = 0f, delta = 0f;

        while(percent < 1f)
        {
            delta += Time.deltaTime;
            percent = delta / 0.25f;

            float alpha = Mathf.Lerp(start, end, percent);
            _canvasGroup.alpha = alpha;
            yield return null;
        }

        _canvasGroup.alpha = end;

        yield return YieldUtil.WaitForSecondsRealtime(4f);

        this.RunRoutine(CloseUI(tutorialId));
    }

    IEnumerator CloseUI(int tutorialId)
    {
        float interval = 0.05f;

        for(int i = 0; i < 4; i++)
        {
            Flicking(interval);
            yield return YieldUtil.WaitForSecondsRealtime(interval);
        }

        _uiPanel.gameObject.SetActive(true);
        
        float start = _uiPanel.localScale.y, end = 0f;
        float percent = 0f, delta = 0f;

        while(percent < 1)
        {
            delta += Time.deltaTime;
            percent = delta / 0.45f;

            float lerp = Mathf.Lerp(start, end, percent);
            _uiPanel.localScale = new Vector2(_uiPanel.localScale.x, lerp);
            yield return null;
        }

        _uiPanel.localScale = new Vector2(_uiPanel.localScale.x, end);

        _uiPanel.gameObject.SetActive(false);

        EventBus.Invoke<int>("EndTutorial", tutorialId);
    }

    private void SetText(string tutorialContext)
    {
        _text.SetText(tutorialContext);
    }

    private void Flicking(float interval)
    {
        bool active = !_uiPanel.gameObject.activeSelf;
        _uiPanel.gameObject.SetActive(active);
    }
}
