using UnityEngine;

public class CharacterAnimationEvent : MonoBehaviour
{
    private Rigidbody rigid;
    private Animator animator;
    private PlayerDataBox box;
    private LayerMask targetLayer;


    #region Skill변수
    private IFireSkill currentSkill;
    private Transform skillObjectUseTransform;
    #endregion

    private void OnEnable()
    {
        rigid = transform.parent.GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        skillObjectUseTransform = transform.parent.FindTarget("SkillObjectUsePos");
        targetLayer = LayerMask.GetMask("EnemyDamage");

        EventBus.Sub<IFireSkill>("ChracterAnimationEvent_SetSkillAttacker", SetSkillAttacker);
    }

    private void OnDisable()
    {
        EventBus.UnSub<IFireSkill>("ChracterAnimationEvent_SetSkillAttacker", SetSkillAttacker);
    }

    public void Initialize(PlayerDataBox box) => this.box = box;

    #region AnimatorApplyRootMotion
    public void ApplyRootMotion()
    {
        box.animator.applyRootMotion = true;    
    }

    public void DeniedRootMotion()
    {
        box.animator.applyRootMotion = false;
    }

    #endregion

    #region Weapon

    public void OnWeaponActive_Keep(float keepTime)
    {
        box.weapon?.OnActive_Keep(keepTime);
    }
    public void OnWeaponActive()
    {
        box.weapon?.OnActive();
    }

    #endregion

    #region Volume
    public void OnChromaticAberration(float amount) => EventBus.Invoke<float>("Volume_Chroma", amount);
    
    public void OnLensDistortion(float amount) => EventBus.Invoke<float>("Volume_LensDistortion", amount);

    public void OnVignette(float amount) => EventBus.Invoke<float>("Volume_Vignette", amount);

    public void OnScreenFlare(float amount) => EventBus.Invoke<float>("Volume_LensFlare", amount);

    public void OnBloom(float amount) => EventBus.Invoke<float>("Volume_Bloom", amount);
    #endregion
    
    #region Attack

    public void OnAttack(float weight = 1f)
    {
        box.weapon.OnAttack(weight);
    }

    public void SetProjectileType(ProjectileType type = ProjectileType.Straight)
    {
        (box.weapon as IRange).SetProjectileType(type);
    }

    public void OnAttackVoice(int index)
    {
        float randomPercent = Random.Range(0f, 1f);
        if(randomPercent >= 0.3f) return;

        int characterId = box.stat.StatData.characterDataId;
        
        var so = DataLoader.GetData<VoiceDataSO>(DataType.Voice, characterId);
        if(so == null || so.attackVoiceClips == null || index > so.attackVoiceClips.Length) return;

        int randomIndex = Random.Range(0, so.attackVoiceClips[index].clips.Length);

        AudioClip currentVoice = so.attackVoiceClips[index].clips[randomIndex];

        EventBus.Invoke<Vector3, AudioClip>("Play_Voice", transform.position, currentVoice);
    }

    public void EnableNextAttackFlag() {}

    public void EndAttackFlag() {}
    #endregion

    #region JumpAttack
    public void OnJumpAttackStart()
    {
        rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, 5f, rigid.linearVelocity.z);
    }

    public void OnJumpAttackEnd()
    {
        rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, -40f, rigid.linearVelocity.z);
    }

    public void OnJumpAttackVoice()
    {
        int characterId = box.stat.StatData.characterDataId;
        
        var so = DataLoader.GetData<VoiceDataSO>(DataType.Voice, characterId);
        if(so == null || so.jumpAttackVoiceClips == null) return;

        int curIndex = Random.Range(0, so.jumpAttackVoiceClips.Length);
        
        AudioClip currentVoice = so.jumpAttackVoiceClips[curIndex];

        EventBus.Invoke<Vector3, AudioClip>("Play_Voice", transform.position, currentVoice);
    }
    #endregion
    
    #region Skill
    public void OnSkillVoice(int index)
    {
        int characterId = box.stat.StatData.characterDataId;

        var so = DataLoader.GetData<VoiceDataSO>(DataType.Voice, characterId);
        if(so == null || so.skillVoiceClips == null || index > so.skillVoiceClips.Length) return;

        AudioClip currentVoice = so.skillVoiceClips[index];

        EventBus.Invoke<Vector3, AudioClip>("Play_Voice", transform.position, currentVoice);
    }   

    private void SetSkillAttacker(IFireSkill skill)
    {
        currentSkill = skill;
    }

    public void OnSkill(float damageMultiple = 1)
    {
        currentSkill?.FireSkill(damageMultiple);
    }

    #endregion

    #region SFX
    public void OnSFX(int attackIndex)
    {
        if(box == null 
        || box.stat == null 
        || box.stat.StatData == null 
        || box.stat.StatData.attackSfxIds == null 
        || attackIndex > box.stat.StatData.attackSfxIds.Length) return;

        int currentId = box.stat.StatData.attackSfxIds[attackIndex];

        EventBus.Invoke<Vector3, int>("Play_SFX", transform.position, currentId);
    }

    public void SetHitSFX(int hitSfxId)
    {
        box.weapon.SetHitSFX(hitSfxId);
    }

    #endregion
}
