using UnityEngine;

public class Stamina_ViewModel
{
    
    public ReadOnlyReactiveProperty<float> Stamina;
    public ReadOnlyReactiveProperty<float> MaxStamina;
    private readonly CharacterStaminaManager model;
    public Stamina_ViewModel(CharacterStaminaManager model)
    {
        this.model = model;

        Stamina = model.stamina.ToReadOnlyValue();
        MaxStamina = model.maxStamina.ToReadOnlyValue();
    }
}
public class CharacterStaminaManager
{
    public ReactiveProperty<float> stamina {get; private set;}
    public ReactiveProperty<float> maxStamina {get; private set;}
    
    private float waitRecurveTime = 1f, useTime = 0f, addSpeed = 10f;

    //스태미나 사용여부
    public bool CanUseDash  => (stamina.Value / maxStamina.Value) >= 0.2f;
    public bool CanUseRun   =>  (stamina.Value / maxStamina.Value) > 0f;
    //준비됬는지
    private bool isReady    = false;
    private bool staminaLock = false;

    public Stamina_ViewModel ViewModel {get; private set;}

    public CharacterStaminaManager(MonoBehaviour mono)
    {
        isReady = false;

        stamina = new(100f);
        maxStamina = new(stamina.Value);

        ViewModel = new Stamina_ViewModel(this);
        EventBus.Invoke<MonoBehaviour, Stamina_ViewModel>("SetStaminaBar_ViewModel", mono, ViewModel);

        isReady = true;
    }

    public void Stamina_Update()
    {
        if(!isReady) return;

        if(staminaLock) return;

        if(Time.time - useTime <= waitRecurveTime) return;
        
        if(stamina.Value < maxStamina.Value)
            stamina.Value = Mathf.Min(stamina.Value + Time.deltaTime * addSpeed, maxStamina.Value);
    }
    
    public void UseStamina(float value = 10f)
    {
        useTime = Time.time;
        stamina.Value = Mathf.Max(stamina.Value - value, 0f);
    }
}

