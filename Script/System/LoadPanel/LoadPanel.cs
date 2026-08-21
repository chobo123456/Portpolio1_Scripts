using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public partial class LoadPanel : MonoBehaviour
{
    private Image loadPanel, spinner, gradient;
    private WaitUntil waitTask;
    private Coroutine initializeRoutine;
    private void OnEnable()
    {
        waitTask = new WaitUntil(() => 
                LoadStatus.IsReady && 
                LoadStatus.IsReady_Inventory && 
                LoadStatus.IsReady_CharacterParty && 
                LoadStatus.IsReady_Quest &&
                LoadStatus.IsReady_GrowthTap &&
                LoadStatus.IsReady_Craft);

        Initialize_Objects();
        Initialize_Event();
        IntializeFinal();
    }

    private void Initialize_Objects()
    {
        Transform parentTr = transform.FindTarget("FadePanel");
        loadPanel = parentTr.GetComponent<Image>();
        spinner    = parentTr.FindTarget("Spinner").GetComponent<Image>();
        gradient   = parentTr.FindTarget("Gradient").GetComponent<Image>();
    }
    private void Initialize_Event()
    {
        EventBus.Sub<float>("LoadIn", StartFadeIn);
        EventBus.Sub<float>("LoadOut", StartFadeOut);
        EventBus.Sub("OnRespawnProcess", OnRespawnProcess);
        EventBus.Sub("ActiveFadePanelForce", ActiveForce);
    }

    private void IntializeFinal()
    {
        initializeRoutine = this.RunRoutine(WaitForInitialize());
    }

    IEnumerator WaitForInitialize()
    {
        loadPanel.gameObject.SetActive(true);
        CursorManager.CursorActive(false);

        yield return waitTask;
    
        yield return YieldUtil.WaitForSecondsRealtime(5f);

        StartFadeIn();

        EventBus.Invoke("CameraUpdatePosition");
        yield return YieldUtil.WaitForSecondsRealtime(0.5f);

        EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState",  GameStateType.Run, GameEnableTimeSet.True);
    }

    private void OnDisable()
    {
        EventBus.UnSub<float>("LoadOut", StartFadeIn);
        EventBus.UnSub<float>("LoadIn", StartFadeOut);
        EventBus.UnSub("OnRespawnProcess", OnRespawnProcess);
        EventBus.UnSub("ActiveFadePanelForce", ActiveForce);
    }
}


public partial class LoadPanel : MonoBehaviour
{
    private void OnRespawnProcess()
    {
        StopCoroutine(initializeRoutine);
    }

    private void ActiveForce()
    {
        loadPanel.gameObject.SetActive(true);
    }

    private void StartFadeOut(float speed = 0.5f)
    {
        if(!loadPanel.gameObject.activeInHierarchy) loadPanel.gameObject.SetActive(true);
        else return;
        
        this.RunRoutine(Fade(0, 1, speed), "LoadPanel");
    }
    
    private void StartFadeIn(float speed = 0.5f)
    {
        if (!loadPanel.gameObject.activeInHierarchy) loadPanel.gameObject.SetActive(true);

        this.RunRoutine(Fade(1, 0, speed, true), "LoadPanel");
    }

    IEnumerator Fade(float startAmount, float endAmount, float speed = 0.5f,bool needDisable = false)
    {
        float percent = 0f, currentTime = 0f, lerpTime = speed;

        while (percent < 1f)
        {
            currentTime += Time.unscaledDeltaTime;
            percent = currentTime / lerpTime;

            float colorA = Mathf.Lerp(startAmount, endAmount, percent);

            SetMainPanelColor(colorA);
            SetSpinnerColor(colorA);
            SetGradientColor(colorA);

            yield return null;
        }

        SetGradientColor(endAmount);
        SetSpinnerColor(endAmount);
        SetMainPanelColor(endAmount);

        if(needDisable) loadPanel.gameObject.SetActive(false);
    }

    private void SetMainPanelColor(float alpha)
    {
        Color c = loadPanel.color;
        c.a = alpha;
        loadPanel.color = c;
    }

    private void SetSpinnerColor(float alpha)
    {
        Color c = spinner.color;
        c.a = alpha;
        spinner.color = c;
    }

    private void SetGradientColor(float alpha)
    {
        Color c = gradient.color;
        c.a = alpha;
        gradient.color = c;
    }
}