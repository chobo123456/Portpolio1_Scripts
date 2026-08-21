using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StaminaUI : MonoBehaviour
{
    private MonoBehaviour player;
    private CanvasGroup canvasGroup;
    private RectTransform parent, active;
    private Stamina_ViewModel viewModel;
    private Coroutine activeRoutine, chaseRoutine;
    private Image fill;

    private float currentFill = 0f;


    private void OnEnable()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        parent = GetComponent<RectTransform>();

        Transform fillParent = transform.Find("Active");
        active = fillParent.GetComponent<RectTransform>();

        fill = fillParent.Find("Fill").GetComponent<Image>();

        EventBus.Sub<MonoBehaviour, Stamina_ViewModel>("SetStaminaBar_ViewModel", StaminaViewModelSetting);
        EventBus.Sub<bool>("MainCanvasActive", MainCanvasCase);
    }

    private void OnDisable()
    {
        viewModel?.Stamina.UnSubscribe(OnValueChanged);

        EventBus.UnSub<MonoBehaviour, Stamina_ViewModel>("SetStaminaBar_ViewModel", StaminaViewModelSetting);
        EventBus.UnSub<bool>("MainCanvasActive", MainCanvasCase);
    }

    private void StaminaViewModelSetting(MonoBehaviour mono, Stamina_ViewModel viewModel)
    {
        player = mono;

        Active(false);

        this.viewModel = viewModel;
        OnValueChanged(viewModel.MaxStamina.Value);
        this.viewModel.Stamina.Subscribe(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        float amount = value / viewModel.MaxStamina.Value;
        fill.fillAmount = amount;

        bool isUp = currentFill < amount;

        currentFill = fill.fillAmount;

        if(value == viewModel.MaxStamina.Value) return;

        if(!isUp) 
        {
            activeRoutine = this.RunRoutine(Loop(), activeRoutine);
            chaseRoutine  = this.RunRoutine(Chase(), chaseRoutine);
        }
    }

    IEnumerator Loop()
    {
        if(!active.gameObject.activeSelf)
        {
            Active(true);
            yield return this.RunRoutine(Fade(0f, 1f, 0.15f));
        }

        yield return YieldUtil.WaitForSeconds(5f);
        yield return this.RunRoutine(Fade(1f, 0f, 0.25f));

        Active(false);
    }

    IEnumerator Chase()
    {
        Vector3 offset = Vector3.up * 0.5f;

        while(active.gameObject.activeSelf)
        {
            Vector3 world = player.transform.position + offset;

            Vector3 screen = Camera.main.WorldToScreenPoint(world);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                screen,
                null,
                out Vector2 local
            );

            local.x += 10f;
            
            active.anchoredPosition = local;

            yield return null;
        }
    }

    IEnumerator Fade(float start, float end, float speed = 0.25f)
    {
        float percent = 0f, delta = 0f, lerpTime = speed;

        while(percent < 1f)
        {
            delta += Time.deltaTime;
            percent = delta / lerpTime;

            float alpha = Mathf.Lerp(start, end, percent);
            canvasGroup.alpha = alpha;

            yield return null;
        }

        canvasGroup.alpha = end;
    }

    private void MainCanvasCase(bool active)
    {
        if(!active)
        {
            if(activeRoutine != null) this.StopRoutine(activeRoutine);
            if(chaseRoutine != null) this.StopRoutine(chaseRoutine);
            Active(active);
        }
    }

    private void Active(bool isActive)
    {
        active.gameObject.SetActive(isActive);
    }
}
