using System.Collections;
using UnityEngine;
using System;

//초기화, 데이터 보유 담당
public abstract class WeaponBase : MonoBehaviour 
{
    private int _characterId;
    private CharacterVFXSetter _vfxSetter;
    private bool _isInit = false;

    protected WeaponStatData _info;
    protected PlayerDataBox _box;
    protected int _weaponId = 1;
    protected LayerMask _targetLayer;
    
    private Coroutine _routine;

    public bool IsEnable {get; private set;} = false;
    public bool _isDebugMode = false;

    private void OnEnable()
    {
        _targetLayer = LayerMask.GetMask("EnemyDamage");

        Initialize();
        EventBus.Sub("ReloadEquipment", SettingWeaponData);
    }

    private void OnDisable()
    {
        EventBus.UnSub("ReloadEquipment", SettingWeaponData);
    }

    public void Initialize(PlayerDataBox box, int characterId, CharacterVFXSetter vfxSetter)
    {
        _box            = box;
        _characterId    = characterId;
        _vfxSetter      = vfxSetter;
        _isInit = true;

        SettingWeaponData();
    }

    private void SettingWeaponData()
    {
        IsEnable = false;

        if(LoadStatus.IsReady && LoadStatus.IsReady_GrowthTap && _isInit)
        {
            int newWeaponId = EventBus.Invoke_Func<int, int>("GetEquipmentIdToUseCharacterId", _characterId);
    
            if (newWeaponId == -1)
            {
                newWeaponId = 1;
                _weaponId = newWeaponId;
                ChangeWeapon();
            } 
            else 
            {
                _weaponId = newWeaponId;
                ChangeWeapon();
            }
        }
        else
        {
            if(_routine != null) return;
            _routine = this.RunRoutine(DelayChangeWeapon());
        }
    }

    IEnumerator DelayChangeWeapon()
    {
        yield return new WaitUntil(() => LoadStatus.IsReady);

        if(_isDebugMode)
        {
            _weaponId = 1;
            ChangeWeapon();
            _routine = null;

            yield break;
        }

        yield return new WaitUntil(() => LoadStatus.IsReady_GrowthTap);

        int newWeaponId = EventBus.Invoke_Func<int, int>("GetEquipmentIdToUseCharacterId", _characterId);

        if(newWeaponId == -1)
        {
            newWeaponId = 1;
            _weaponId = newWeaponId;
        }
        else
        {
            _weaponId = newWeaponId;
        }

        ChangeWeapon();

        _routine = null;
    }

    private void ChangeWeapon()
    {
        _info = DataLoader.GetData<WeaponStatData>(DataType.Weapon, _weaponId);
        if (_info == null || _vfxSetter == null) return;

        _vfxSetter.SetVFX((int)_info.visualData.attack_vfxId, (int)_info.visualData.final_attack_vfxId);

        InitializePool(_info.visualData.vfxId);
        SetWeaponMesh(_info.visualData.weaponMesh);
        SetWeaponMaterial(_info.visualData.weaponMaterial);   
        InitializeOnChange();

        IsEnable = true;
    }

    private void InitializePool(int vfxId)
    {
        _ = EventBus.Invoke_Func<int, GameObject>("Pool_GetGameObject", vfxId);
    }    

    public virtual WeaponStatData GetInfo()
    {
        if(_info == null || _info.damage.Equals(0))
        {
            var dataSo = DataLoader.GetData<WeaponStatData>(DataType.Weapon, _weaponId);
            _info = dataSo;
        }

        return _info;
    }
    protected virtual void Initialize() {}
    protected virtual void InitializeOnChange() {}
    protected virtual void SetWeaponMesh(Mesh mesh) {}
    protected virtual void SetWeaponMaterial(Material material) {}
    public virtual void SetHitSFX(int hitSfxId) {}
    public virtual void OnAttack(float weight = 1f) {}
}

//비쥬얼 담당
public abstract class Weapon : WeaponBase
{
    private Material _material;
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;

    protected override void Initialize()
    {
        if(_meshFilter == null || _meshRenderer == null)
        {
            Transform meshParent = transform.FindTarget("Mesh");

            _meshFilter = meshParent.GetComponent<MeshFilter>();
            _meshRenderer = meshParent.GetComponent<MeshRenderer>();
        }

        _meshRenderer.enabled = false;
    }

    protected override void InitializeOnChange()
    {
        SetDissolve(2f);

        _meshRenderer.enabled = true;
    }
    
    public virtual void OnActive_Keep(float keepTime)
    {
        SetDissolve(0);

        this.RunRoutine(StartFade(keepTime), "Weapon_WeaponActive");
    }

    public virtual void OnActive()
    {
        SetDissolve(0);

        this.RunRoutine(StartFade(), "Weapon_WeaponActive");
    }

    public virtual void OnInactive()
    {
        SetDissolve(2f);
    }

    IEnumerator StartFade(float waitTime = 1.45f)
    {
        yield return YieldUtil.WaitForSeconds(waitTime);

        float percent = 0f, currentTime = 0f, lerpTime = 0.75f;

        while(percent < 1f)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / lerpTime;

            float lerpValue = Mathf.Lerp(0f, 1f, percent);

            SetDissolve(lerpValue);

            yield return null;
        }
    }

    protected override void SetWeaponMaterial(Material material)
    {
        _meshRenderer.material = material;

        _material = material;
    }

    private void SetDissolve(float intensity)
    {
        _material.SetFloat("_DissolveAmount", intensity);
    }

    protected override void SetWeaponMesh(Mesh mesh)
    {
        _meshFilter.mesh = mesh;
    }
}