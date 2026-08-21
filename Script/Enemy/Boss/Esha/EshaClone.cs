using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
public partial class EshaClone : CloneBase<CloneEnemyDataBox>
{
    private static int _dashParam = Animator.StringToHash("Dash"), _onInPlaceParam = Animator.StringToHash("InPlace");
    private MaterialPropertyBlock _block;
    private Renderer[] _renderers;
    private WaitUntil _onDash, _onInPlaceAttack, _waitInit;
    public Indicator _indicator;
    private Transform _meshTr;
    private Vector3 _localOriginPos;
    private int _cloneLayer = 0, _invisibleLayer;
    private bool _finishInitialize = false;
    private Coroutine _routine, _renderRoutine, _disableRoutine;
    private GameObject _indicatorObj;

    public override void OnEnable()
    {
        base.OnEnable();

        if(livingEntityDataBox != null) livingEntityDataBox.col.enabled = true;

        if(!_finishInitialize)
        {
            _meshTr = transform.Find("Mesh");
            _localOriginPos = _meshTr.localPosition;
            _cloneLayer = this.gameObject.layer;
            _invisibleLayer = LayerMask.NameToLayer("InvisibleLayer");
            _block = new();
            _renderers = transform.GetComponentsInChildren<Renderer>();
            _waitInit = new WaitUntil(() => _finishInitialize);
        }

        this.RunRoutine(FadingLoop(1f, 0f), _renderRoutine);
    }
    
    protected override void Initialize(int enemyDataId)
    {
        base.Initialize(enemyDataId);

        livingEntityDataBox.col.isTrigger = true;
        livingEntityDataBox.damageComp?.curHp.Subscribe(OnHit);
        livingEntityDataBox.animator.enabled = false;

        InitializeWaitUntil();
    }

    private void InitializeWaitUntil()
    {
        Animator animator = livingEntityDataBox.animator;

        _onDash          = pattern.GetWaitUntil(animator, "Dash", 0.9f, 0);
        _onInPlaceAttack = pattern.GetWaitUntil(animator, "InPlace", 0.9f, 0);
        _finishInitialize = true;
    }

    private void OnDisable()
    {
        _indicatorObj?.SetActive(false);
    }

    private void OnDestroy()
    {
        if(livingEntityDataBox != null && livingEntityDataBox.damageComp != null)
            livingEntityDataBox.damageComp.curHp.UnSubscribe(OnHit);
    }
}


public partial class EshaClone : CloneBase<CloneEnemyDataBox>
{
    public override void Exception()
    {
        this.RunRoutine(DisableLoop(), _disableRoutine);
    }

    #region Execute
    public override void Execute() 
    {
        this.RunRoutine(ExecuteLoop(), _routine);
    }

    IEnumerator ExecuteLoop()
    {
        yield return _waitInit;

        SetLayer(_invisibleLayer);

        int indicatorId = GetIndicatorCase();
        Vector3 indicatorPos = transform.TransformPoint(livingEntityDataBox.col.center + ((livingEntityDataBox.col.height * 0.5f) * Vector3.down));
        indicatorPos.y += 0.2f;

        _indicatorObj = _indicator.GetIndicator(indicatorId);

        yield return this.RunRoutine(_indicator.IndicatorLoop(_indicatorObj, indicatorPos, transform.rotation));

        WaitUntil currentWaitUntil = GetWaitUntilCase();

        if(currentWaitUntil == null) yield break;

        int param = GetParamCase();
        Animator animator = livingEntityDataBox.animator;
        
        livingEntityDataBox.animator.applyRootMotion = true;
        yield return this.RunRoutine(pattern.Strategy(animator, param, currentWaitUntil));
        
        this.RunRoutine(DisableLoop());
    }

    private int GetParamCase()
    {
        switch(currentStrategy)
        {
            case CloneCommand.Dash:
                return _dashParam;
            case CloneCommand.InPlace:
                return _onInPlaceParam;
            default:
                return 0;
        }
    }

    private WaitUntil GetWaitUntilCase()
    {
        switch(currentStrategy)
        {
            case CloneCommand.Dash:
                return _onDash;
            case CloneCommand.InPlace:
                return _onInPlaceAttack;
            default:
                return null;
        }
    }
    
    private int GetIndicatorCase()
    {
        switch(currentStrategy)
        {
            case CloneCommand.Dash:
                return 1000001;
            case CloneCommand.InPlace:
                return 1000000;
            default:
                return 1000000;
        }
    }

    #endregion

    private void SetLayer(int layer)
    {
        this.gameObject.layer = layer;
    }

    private void OnHit(float curHp)
    {
        if(livingEntityDataBox.damageComp.IsDie) return;

        if(curHp <= 0f) 
        {
            EventBus.Invoke<EshaClone>("EshaCloneDie", this);
            this.RunRoutine(DisableLoop(), _disableRoutine);
        }
    }

    IEnumerator DisableLoop()
    {
        yield return this.RunRoutine(FadingLoop(0f, 1f), _renderRoutine);
        
        livingEntityDataBox.animator.applyRootMotion = false;
        livingEntityDataBox.animator.Play("Idle", 0, 0f);
        livingEntityDataBox.animator.Update(0f);
        livingEntityDataBox.animator.enabled = false;

        _indicatorObj?.SetActive(false);
        gameObject.SetActive(false);
        SetLayer(_cloneLayer);

        _meshTr.localPosition = _localOriginPos;
        _meshTr.localRotation = Quaternion.identity;
    }

    #region Dissolve
    IEnumerator FadingLoop(float start, float end)
    {
        float curTime = 0f, per = 0f, fadeTime = 0.5f;

        while(per < 1f)
        {
            curTime += Time.deltaTime;
            per = curTime / fadeTime;

            float curFadeVal = Mathf.Lerp(start, end, per);
            SetDissolve(curFadeVal);
            yield return null;
        }

        SetDissolve(end);
    }

    private void SetDissolve(float amount)
    {
        for(int i = 0; i < _renderers.Length; i++)
        {
            Renderer renderer = _renderers[i];

            renderer.GetPropertyBlock(_block);
            _block.SetFloat("_DissolveAmount", amount);
            renderer.SetPropertyBlock(_block);
        }
    }
    #endregion

    
}