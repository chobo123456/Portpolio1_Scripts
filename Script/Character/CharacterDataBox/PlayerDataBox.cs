using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum CharacterMoveState
{
    Idle,
    Walk,
    Running,
}

public class PlayerDataBox
{
    //boolean
    public bool IsReady {get; private set;} = false;

    //Physic
    public Rigidbody rigid {get; private set;}
    public CharacterSurfaceAlignment surfaceAlignment {get; private set;} 

    //Mono
    public MonoBehaviour mono {get; private set;}

    //Input
    public ICharacterInput input {get; private set;}

    //Sensor
    public Sensor sensor {get; private set;}

    //Animation
    public Animator animator {get; private set;}
    public CharacterAnimator animatorControl {get; private set;}
    public CharacterAnimationEvent animationEvent {get; private set;}
    public CharacterAnimationProxy animationProxy {get; private set;}

    //Stat
    public CharacterStatContainer stat {get; private set;}
    public CharacterHpBase hpComp {get; private set;}

    //ETC
    public CharacterInteracter interacter {get; private set;}
    public CharacterVFXSetter vfxSetter {get; private set;} 

    //Rotate
    public RotationManager rotate {get; private set;}

    public CharacterStaminaManager stamina {get; private set;}
    
    public Weapon weapon {get; private set;}

    //int
    public int CharacterIndex {get; private set;} = 0;
    public int CharacterId {get; private set;}
    
    //ETC
    private CharacterMoveState walkState;
    public System.Action abortState;
    public SingleShotEvent disableCall;

    public PlayerDataBox(GameObject owner, int characterId, bool isDebugMode = false)
    {
        CharacterId = characterId;

        IsReady = false;

        mono = owner.GetComponent<MonoBehaviour>();
        rigid = owner.GetComponent<Rigidbody>();

        mono.RunRoutine(Initialize(owner, isDebugMode));

        EventBus.Sub<int, int>("InitializeDataBox", Initialize);
    }
    
    public void Initialize(int characterId, int characterIndex)
    {
        CharacterId = characterId;
        CharacterIndex = characterIndex;

        mono.RunRoutine(ReInitialize());
    }

    IEnumerator ReInitialize()
    {
        yield return new WaitUntil(() => IsReady);
        Initialize_VFXSetter();
        stat.Initialize();

        yield return new WaitUntil(() => stat.isReady);
        Initialize_Weapon();
        hpComp.Initialize(this);

        EventBus.Invoke("ReloadEquipment");
        
        EventBus.Invoke<DecoKey>("SetCharaterIndex", new DecoKey{ 
            index = CharacterIndex, 
            characterId = CharacterId});
    }

    #region Initialize

    private void Initialize_Input(bool isDebugMode = false)
    {
        input = new CharacterInputManager(mono, isDebugMode);
    }

    private void Initialize_Sensor(GameObject owner)
    {
        sensor = new Sensor(owner.transform);
    }

    private void Initialize_VFXSetter()
    {
        vfxSetter = new CharacterVFXSetter(mono.transform.FindTarget("VFX"), mono.transform.FindTarget("VFX_Final"));
    }

    private void Initialize_Interacter()
    {
        interacter = new CharacterInteracter(this);
    }

    private void Initialize_SurfaceAlign()
    {
        surfaceAlignment = new CharacterSurfaceAlignment(this);
    }

    private void Initialize_AnimationSystem(GameObject owner)
    {
        animator        = owner.transform.Find("Mesh").GetComponent<Animator>();
        
        animationEvent  = owner.GetComponentInChildren<CharacterAnimationEvent>();
        animationEvent.Initialize(this);

        animationProxy  = owner.GetComponentInChildren<CharacterAnimationProxy>();
        animationProxy.Initialize(this);

        animatorControl = new CharacterAnimator(this);
    }
    
    private void Initialize_Weapon()
    {
        Transform weaponTr          = mono.transform.FindTarget("Weapon");
        weapon = weaponTr.GetComponent<Weapon>();
        weapon.Initialize(this, CharacterId, vfxSetter);
    }

    private void Initialize_Stat()
    {
        stat = new CharacterStatContainer(this);
    }

    private void Initialzie_Stamina()
    {
        stamina = new CharacterStaminaManager(mono);
    }
    
    private void Initialize_Rotate(bool isDebugMode = false)
    {
        rotate = new RotationManager(this, isDebugMode);
    }

    private void Initialize_HpComp(GameObject owner) 
    {
        hpComp = owner.GetComponentInChildren<CharacterDamageComponent>();
        hpComp.Initialize(this);
    }

    #endregion

    IEnumerator Initialize(GameObject owner, bool isDebugMode = false)
    {
        Initialize_Input(isDebugMode);
        Initialize_VFXSetter();
        Initialize_Sensor(owner);
        Initialize_Interacter();
        Initialize_SurfaceAlign();
        Initialize_AnimationSystem(owner);
        Initialize_Weapon();
        Initialize_Stat();
        Initialzie_Stamina();

        yield return new WaitUntil(() => stat.isReady);

        Initialize_Rotate(isDebugMode);
        Initialize_HpComp(owner);    

        IsReady = true;
    }

    public void OnDisable()
    {
        disableCall.Invoke();

        EventBus.UnSub<int, int>("InitializeDataBox", Initialize);
    }
    public void OnDestroy()
    {
        EventBus.Invoke<DecoKey>("OnRemoveCharacter_Deco", new DecoKey{ 
            index = CharacterIndex, 
            characterId = CharacterId});
    }
    
    public void SetMoveState(CharacterMoveState state) => walkState = state;
    public CharacterMoveState GetMoveState() => walkState;
}
