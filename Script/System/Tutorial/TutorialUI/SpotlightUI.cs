using UnityEngine;
using System.Collections;


public class SpotlightUI : MonoBehaviour
{
    private GameObject _parentPanel;
    private RectTransform _focus;
    
    private void OnEnable()
    {
        ReferenceUI();
        SubscribeEvent(true);
    }

    private void ReferenceUI()
    {
        _parentPanel = transform.FindTarget("SpotLightPanel").gameObject;
        _parentPanel.SetActive(false);
        
        _focus   = transform.FindTarget("SpotLight").GetComponent<RectTransform>();
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<RectTransform>("FocusStart", Focus);
            EventBus.Sub("FocusEnd", FocusEnd);
        }
        else
        {
            EventBus.UnSub<RectTransform>("FocusStart", Focus);
            EventBus.UnSub("FocusEnd", FocusEnd);
        }
    }

    private void OnDisable()
    {
        SubscribeEvent(false);
    }

    private void Focus(RectTransform target)
    {
        this.RunRoutine(ForceLoop(target));
    }

    private IEnumerator ForceLoop(RectTransform target)
    {
        yield return new WaitForEndOfFrame();

        _parentPanel.SetActive(true);

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector2 center = (corners[0] + corners[2]) * 0.5f;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, center);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _focus.parent as RectTransform,
            screenPos,
            null,
            out Vector2 localPoint
        );

        _focus.anchoredPosition = localPoint;

        float padding = 15f;
        Vector2 scale = target.rect.size + (new Vector2(padding, padding) * 2f);
        _focus.sizeDelta = scale;
    }

    private void FocusEnd()
    {
        _parentPanel.SetActive(false);
    }
}
