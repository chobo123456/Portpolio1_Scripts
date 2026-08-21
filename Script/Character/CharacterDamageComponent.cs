using UnityEngine;
using System.Collections;

public abstract class CharacterHpBase : MonoBehaviour, IHpBase, IDamageable
{
    public bool IsDie { get; protected set; } = false;
    public bool IsHit { get; protected set; } = false;
    public bool Evade { get; private set; } = false;

    public ReactiveProperty<float> curHp { get; private set;  }
    public ReactiveProperty<float> maxHp { get; private set; }
    protected PlayerDataBox box;
    
    private HPBar_ViewModel viewModel;
    public DamageSource CurrentSource {get; protected set;}

    public bool IgnoreDamage { get; set; }
    

    private float evadeInputTime = 0f, minEvadeActiveTime = 0.4f;

    protected virtual void OnEnable()
    {
        IsDie = false;

        EventBus.Sub("CharacterReloadHp", ReloadHp);
    }
    protected virtual void OnDisable()
    {
        EventBus.UnSub("CharacterReloadHp", ReloadHp);
    }

    public void Initialize(PlayerDataBox box)
    {
        this.box = box;

        maxHp = new(box.stat.GetHP());
        curHp = new(maxHp.Value);

        ReloadHp();

        viewModel = new HPBar_ViewModel(this);
        EventBus.Invoke<HPBar_ViewModel>("SetHpBar_ViewModel", viewModel);

        box.stat.onValueChanged += OnUpdateMaxHp;

        EventBus.Invoke<int, float>("OnCharacterHpChanged", box.CharacterId, curHp.Value);
    }    

    public void OnDestroy()
    {
        if(box != null && box.stat != null)
            box.stat.onValueChanged -= OnUpdateMaxHp;
    }

    public void InitViewModel()
    {
        if(viewModel != null) EventBus.Invoke<HPBar_ViewModel>("SetHpBar_ViewModel", viewModel);
    }

    public void OnUpdateMaxHp()
    {
        maxHp.Value = box.stat.GetHP();
    }

    private void Update()
    {
        if(box == null) return;

        if(box.input.IsPressed(InputType.Dash_Evade))
            evadeInputTime = Time.time;
    }

    public virtual void TakeDamage(DamageSource source)
    {
        if (IsDie || IgnoreDamage || Evade) return;

        if(Time.time - evadeInputTime <= minEvadeActiveTime)
        {
            Evade = true;
            return;
        }

        float elementCalDamage = this.OnElementCase(box.stat.StatData.element, source.elementType, source.damageAmount);

        if(GameState.IsBossFight())
        {
            curHp.Value = Mathf.Max(curHp.Value - elementCalDamage, 1f);
        }
        else
            curHp.Value = Mathf.Max(curHp.Value - elementCalDamage, 0f);
        
        EventBus.Invoke<int, float>("OnCharacterHpChanged", box.CharacterId, curHp.Value);

        if(curHp.Value <= 0f) OnDie(source);
        else OnHit(source);
    }
    protected abstract void OnDie(DamageSource source);
    protected abstract void OnHit(DamageSource source);

    private void ReloadHp()
    {
        if(curHp == null) return;        

        if(EventBus.Invoke_Func<int, bool>("IsExistRecentInfo", box.CharacterId))
        {
            float recentHp = EventBus.Invoke_Func<int, float>("GetCharacterRecentHp", box.CharacterId);
            curHp.Value = Mathf.Clamp(recentHp, 0f, maxHp.Value); 

            Util.Log($"체력 참, 최대체력 : {maxHp.Value}, 현재 체력 : {curHp.Value}");

            if(curHp.Value > 0f) IsDie = false;
        }
        else
            curHp.Value = maxHp.Value;
    }

    public void EvadeFinish()
    {
        Evade = false;
    }
}

public class CharacterDamageComponent : CharacterHpBase
{
    private CapsuleCollider col;
    private Coroutine routine;

    protected override void OnEnable()
    {
        col = GetComponent<CapsuleCollider>();
        base.OnEnable();
    }

    protected override void OnDie(DamageSource source)
    {
        IsDie = true;
        box.abortState.Invoke();
    }

    protected override void OnHit(DamageSource source)
    {
        EventBus.Invoke<(float, float, float)>("CameraShake", (0.005f, 0.05f, 0.05f));

        CurrentSource = source;

        GameObject obj = EventBus.Invoke_Func<int, GameObject>("Pool_GetGameObject", source.hit_vfxId);

        if(obj != null)
        {
            obj.transform.position = transform.TransformPoint(col.center + (Vector3.up * 0.3f));
            obj.transform.rotation = Quaternion.identity;
            obj.SetActive(true);
        }

        routine = this.RunRoutine(HitProcess(), routine);
    }

    IEnumerator HitProcess()
    {
        IsHit = true;

        yield return YieldUtil.WaitForSecondsRealtime(0.2f);

        IsHit = false;
    }
}