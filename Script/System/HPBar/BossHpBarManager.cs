using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossHpBarManager : MonoBehaviour
{
    private GameObject hpBar;
    private Image fillImage, fillImage_white, poiseFill;
    private HPBar_ViewModel view_Model;
    private TextMeshProUGUI bossName;
    private PoiseBar_ViewModel poise_Model;

    private void OnEnable()
    {
        if (fillImage == null)
        {
            Transform bossHpTr = transform.FindTarget("BossHp");

            fillImage = bossHpTr.Find("Fill").GetComponent<Image>();
            fillImage.type = Image.Type.Filled;

            fillImage_white = bossHpTr.Find("White_Fill").GetComponent<Image>();
            fillImage_white.type = Image.Type.Filled;
        }

        if(hpBar == null)
        {
            Transform bossHpTr = transform.Find("BossHpUI");
            hpBar = bossHpTr.gameObject;
        }

        if(poiseFill == null)
        {
            Transform bossPoiseTr = transform.FindTarget("Poise_Fill");
            poiseFill = bossPoiseTr.GetComponent<Image>();
        }

        if(bossName == null)
        {
            Transform bossNameTr = transform.FindTarget("BossName");
            bossName = bossNameTr.GetComponent<TextMeshProUGUI>();
        }

        hpBar.SetActive(false);

        EventBus.Sub<HPBar_ViewModel, int>("SetBossHpBar_ViewModel", SetViewHpModel);
        EventBus.Sub<PoiseBar_ViewModel>("SetBossPoiseBar_ViewModel", SetPoiseViewModel);
    }

    private void OnDestroy()
    {
        EventBus.UnSub<HPBar_ViewModel, int>("SetBossHpBar_ViewModel", SetViewHpModel);
        EventBus.UnSub<PoiseBar_ViewModel>("SetBossPoiseBar_ViewModel", SetPoiseViewModel);
        view_Model?.HP?.UnSubscribe(OnValueChange);
        poise_Model?.CurrentPoise?.UnSubscribe(OnPoiseValueChange);
    }

    
    #region Poise
    private void SetPoiseViewModel(PoiseBar_ViewModel viewModel)
    {
        if(viewModel != null)
        {
            poise_Model?.CurrentPoise?.UnSubscribe(OnPoiseValueChange);

            //새 보스의 뷰모델 장착
            poise_Model = viewModel;
            poise_Model.CurrentPoise.Subscribe(OnPoiseValueChange);
        }
        else
        {
            poise_Model?.CurrentPoise?.UnSubscribe(OnPoiseValueChange);
        }
    }

    private void OnPoiseValueChange(float amount)
    {   
        poiseFill.fillAmount = amount;
    }

    #endregion

    #region HP

    private void SetViewHpModel(HPBar_ViewModel viewModel, int bossId)
    {
        if(viewModel != null)
        {
            SetBossName(DataLoader.GetData<EnemyData>(DataType.Enemy, bossId).enemyName);

            view_Model?.HP?.UnSubscribe(OnValueChange);

            //새 보스의 뷰모델 장착
            view_Model = viewModel;
            view_Model.HP.Subscribe(OnValueChange);

            hpBar.SetActive(true);

            this.RunRoutine(BossAwaken());
        }
        else
        {
            hpBar.SetActive(false);
            view_Model?.HP?.UnSubscribe(OnValueChange);
        }
    }

    IEnumerator BossAwaken()
    {
        fillImage_white.fillAmount = 0f;

        float percent = 0f, currentDelta = 0f, lerpTime = 0.5f;
        float currentHp = view_Model.HP.Value, maxHp = view_Model.MaxHP.Value;

        while(percent <= 1f)
        {
            currentDelta += Time.deltaTime;
            percent = currentDelta / lerpTime;

            float gage = Mathf.Lerp(currentHp, maxHp, percent);

            fillImage.fillAmount = gage;

            yield return null;
        }

        fillImage_white.fillAmount = 1f;
    }

    private void OnValueChange(float amount)
    {   
        float percent = SetHp(amount);

        if(percent == 1f) return;
        this.RunRoutine(StartWhiteHpBar(percent), "SetBossHp");
    }

    private float SetHp(float amount)
    {
        float curHp = amount;
        float maxHp = view_Model.MaxHP.Value;
        float percent = curHp / maxHp;

        SetFill(percent);
        return percent;
    }

    private void SetFill(float percent)
    {
        fillImage.fillAmount = percent;
    }
  
    IEnumerator StartWhiteHpBar(float percent)
    {
        Color c = fillImage_white.color;

        float blinkTime = 0.75f, Timer = 0f, interval = 0.2f;
        bool blink = false;

        while(Timer < blinkTime)
        {
            blink = !blink;

            c.a = blink ? 0.5f : 1;
            fillImage_white.color = c;

            yield return YieldUtil.WaitForSeconds(interval);

            Timer += interval;
        }

        float fillpercent = 0f, currentTime = 0f, targetTime = 0.45f;
        float startFillAmount = fillImage_white.fillAmount;

        while(fillpercent < 1f)
        {
            currentTime += Time.deltaTime;
            fillpercent = currentTime / targetTime;

            fillImage_white.fillAmount = Mathf.Lerp(startFillAmount, percent, fillpercent);

            yield return null;
        }

        fillImage_white.fillAmount = percent;
    }
    #endregion

    private void SetBossName(string dataBossName)
    {
        bossName.SetText(dataBossName);   
    }
}

public interface IPoiseBase
{
    public ReactiveProperty<float> poise { get; }
    public ReactiveProperty<float> maxPoise { get; }
}

public class PoiseBar_ViewModel
{
    private ReadOnlyReactiveProperty<float> Poise;
    private ReadOnlyReactiveProperty<float> MaxPoise;

    private ReactiveProperty<float> currentPoise;
    public ReadOnlyReactiveProperty<float> CurrentPoise {get; private set;}

    public PoiseBar_ViewModel(IPoiseBase model)
    {
        Poise = model.poise.ToReadOnlyValue();
        Poise?.Subscribe(CalculatePoise);
        MaxPoise = model.maxPoise.ToReadOnlyValue();  

        currentPoise = new(MaxPoise.Value);
        CurrentPoise = currentPoise.ToReadOnlyValue();
    }

    private void CalculatePoise(float poise)
    {
        float currentPoiseValue = poise / MaxPoise.Value;

        currentPoise.Value = currentPoiseValue;
    }
}