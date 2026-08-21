using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public interface ITarget
{
    Vector3 currentPos { get; }
    Vector3 currentVel { get; }
}

public partial class CharacterControl : MonoBehaviour, ITarget 
{
    public Vector3 currentPos { get; private set; }
    public Vector3 currentVel { get; private set; }

    public List<Module> module;
    public int id;
    public bool isDebug = false;

    //필드
    private ActManager actManager;
    private PlayerDataBox box;

    private bool isAlreadyInit = false;

    private WaitUntil waitUntilBox;

    private void OnEnable()
    {
        if(isAlreadyInit && box != null && box.IsReady) 
        {
            actManager.AbortState();
            box.hpComp.InitViewModel();
            return;
        }

        Initialize();
    }
    private void OnDestroy()
    {
        EventBus.UnSub("CharacterStateAbort", AbortState);

        if(box != null)
        {
            box.OnDestroy();
            box.abortState -= AbortState;
        } 
    }
    private void Initialize()
    {
        this.RunRoutine(WaitDataLoader());
    }

    IEnumerator WaitDataLoader()
    {
        yield return new WaitUntil(() => LoadStatus.IsReady);

        SetDataBox();

        yield return waitUntilBox;

        SetActManager();
        SetInitializeEvent();

        EventBus.Invoke<Transform>("SetCharacterTransform", this.transform);
        isAlreadyInit = true;
    }

    private void SetDataBox()
    {
        box = new PlayerDataBox(this.gameObject, id, isDebug);
        if(box != null)
        {
            waitUntilBox = new WaitUntil(() => box.IsReady);
            box.abortState += AbortState;

            currentPos = this.transform.position;
            currentVel = box.rigid.linearVelocity;
        } 
    }

    private void SetActManager()
    {
        List<ActBase> list = new();

        for(int i = 0; i < module.Count; i++)
        {
            var _module = module[i];

            //모듈 세팅
            _module.SetModule(box);

            list.Add(_module._act);
        }

        actManager = new ActManager(list);
    }
    
    private void SetInitializeEvent()
    {
        EventBus.Sub("CharacterStateAbort", AbortState);
    }
}

public partial class CharacterControl : MonoBehaviour, ITarget
{
    //처리부
    private void Update()
    {   
        if(!IsUpdateAble()) return;
        
        currentPos = this.transform.position;
        
        box.input?.UpdateMoveInput();
        box.interacter?.OnUpdate();
        box.stamina?.Stamina_Update();
        actManager?.Update();

        if(isDebug) return;
        
        PlayerMatch.SetPlayerPos(transform.position);
        PlayerMatch.SetPlayerRotate(transform.rotation);
    }

    private void FixedUpdate()
    {
        box?.animatorControl?.UpdateAnimation();

        if(!IsUpdateAble()) return;
        
        box.sensor?.UpdateCheck();
        actManager?.FixedUpdate();
        box.rotate?.UpdateRotate();
        box.surfaceAlignment?.UpdateState();
    }
    
    private void LateUpdate()
    {
        if(!IsUpdateAble()) return;

        actManager?.LateUpdate();
    }

    private bool IsUpdateAble()
    {
        if(!LoadStatus.IsReady)
            return false;

        if(box == null || !box.IsReady)
            return false;
        
        if(isDebug)
            return true;
            
        if(!GameState.IsActive())
            return false;

        return true;
    }
    private void AbortState()
    {
        actManager?.AbortState();
    }
}
