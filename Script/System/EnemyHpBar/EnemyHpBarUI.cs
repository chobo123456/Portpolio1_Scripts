using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHpBarUI : MonoBehaviour
{
    private HPBar_ViewModel viewModel;
    
    private Transform active;
    private RectTransform rect;
    private Image redFill, whiteFill;
    public bool IsUnUsable {get; private set;} = false;
    private Coroutine whireRoutine, activeRoutine;
    
    public void Initialize()
    {
        active = transform.Find("Active");
        redFill = active.Find("RedFill").GetComponent<Image>();
        whiteFill = active.Find("WhiteFill").GetComponent<Image>();
        rect = transform.GetComponent<RectTransform>();
    }

    public void SetViewModel(HPBar_ViewModel viewModel)
    {
        Active(false);

        IsUnUsable = true;

        this.viewModel = viewModel;
        OnHpChange(this.viewModel.MaxHP.Value);
        this.viewModel.HP.Subscribe(OnHpChange);
    }

    public void SetPosition(Vector2 pos)
    {
        rect.anchoredPosition = pos;
    }

    public void OnHpChange(float hp)
    {  
        bool isMaxHp = hp == viewModel.MaxHP.Value;

        if(!isMaxHp)
            activeRoutine = this.RunRoutine(Loop(), activeRoutine);

        float maxHp = this.viewModel.MaxHP.Value;
        float currentHp = hp / maxHp;

        redFill.fillAmount = currentHp;

        if(isMaxHp)
            whiteFill.fillAmount = hp / maxHp;
        else
            whireRoutine = this.RunRoutine(WhiteFull(currentHp), whireRoutine);
    }

    IEnumerator Loop()
    {
        float showTime = 3f;

        Active(true);   

        yield return YieldUtil.WaitForSecondsRealtime(showTime);

        Active(false);
    }

    IEnumerator WhiteFull(float changedFill)
    {
        float currentFill = whiteFill.fillAmount;
        
        float percent = 0f, delta = 0f, lerpTime = 0.25f;

        while(percent < 1f)
        {
            delta += Time.deltaTime;
            percent = delta / lerpTime;

            float lerpAmount = Mathf.Lerp(currentFill, changedFill, percent);

            whiteFill.fillAmount = lerpAmount;

            yield return null;
        }

        whiteFill.fillAmount = changedFill;
    }

    private void Active(bool isActive)
    {
        active.gameObject.SetActive(isActive);
    }

    public void OnDisable()
    {
        viewModel?.HP.UnSubscribe(OnHpChange);
        IsUnUsable = false;
    }
}