using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public interface IHpBase
{
    bool IsDie { get; }
    bool IsHit { get; }
    ReactiveProperty<float> curHp { get; }
    ReactiveProperty<float> maxHp { get; }
}

public class HPBarManager : MonoBehaviour
{
    private TextMeshProUGUI hpText;
    private Image fillImage;
    private Image fillImage_white; // hp가 바로 깎이면 이상해보여서 연출효과로서 깎일때 하얀색의 hp바가 깜빡거리게 하기
    private HPBar_ViewModel view_Model;

    private void OnEnable()
    {
        InitializeUIObject();

        EventBus.Sub<HPBar_ViewModel>("SetHpBar_ViewModel", SetViewModel);
    }
    private void OnDestroy()
    {
        EventBus.UnSub<HPBar_ViewModel>("SetHpBar_ViewModel", SetViewModel);
        view_Model?.HP?.UnSubscribe(OnValueChange);
    }

    private void InitializeUIObject()
    {
        if(hpText == null) hpText = GetComponentInChildren<TextMeshProUGUI>();
        if(fillImage == null)
        {
            fillImage = transform.Find("HpBar_Back").Find("Hp_Fill").GetComponent<Image>();
            fillImage.type = Image.Type.Filled;
        } 
        if(fillImage_white == null)
        {
            fillImage_white = transform.Find("HpBar_Back").Find("Hp_Fill_White").GetComponent<Image>();
            fillImage_white.type = Image.Type.Filled;
        } 
    }

    private void SetViewModel(HPBar_ViewModel viewModel)
    {
        if(viewModel != null)
        {
            view_Model?.HP?.UnSubscribe(OnValueChange);
            view_Model?.MaxHP?.UnSubscribe(OnMaxValueChanged);

            view_Model = viewModel;
            SetValueOnSwapOrNew(view_Model.HP.Value);
            view_Model.HP.Subscribe(OnValueChange);
            view_Model.MaxHP.Subscribe(OnMaxValueChanged);
        }
    }

    //이펙트
    private void SetValueOnSwapOrNew(float amount)
    {
        float percent = SetUI(amount);

        fillImage_white.fillAmount = percent;
    }

    private void OnMaxValueChanged(float maxValue)
    {
        float curHp = view_Model.HP.Value;
        float maxHp     = maxValue;

        float percent = curHp / maxHp;

        _ = SetUI(curHp);

        fillImage_white.fillAmount = percent;
    }

    private void OnValueChange(float amount)
    {   
        float recentFill = fillImage.fillAmount;
        float percent = SetUI(amount);

        bool isHeal = percent > recentFill;

        if(percent == 1f || isHeal) return;
        this.RunRoutine(StartWhiteHpBar(percent), "SetCharacterHp");
    }

    private float SetUI(float amount)
    {
        float curHp = amount;
        float maxHp = view_Model.MaxHP.Value;
        float percent = curHp / maxHp;

        SetFill(percent);
        SetText(curHp, maxHp);

        return percent;
    }

    private void SetFill(float percent)
    {
        fillImage.fillAmount = percent;
    }
    
    private void SetText(float curHp, float maxHp)
    {
        int parseCurHp = (int)curHp;
        int parseMaxHp = (int)maxHp;

        hpText.SetText($"{parseCurHp.ToString()}/{parseMaxHp.ToString()}");
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
}

public class HPBar_ViewModel
{
    public ReadOnlyReactiveProperty<float> HP;
    public ReadOnlyReactiveProperty<float> MaxHP;

    public HPBar_ViewModel(IHpBase model)
    {
        HP = model.curHp.ToReadOnlyValue();
        MaxHP = model.maxHp.ToReadOnlyValue();  
    }
}