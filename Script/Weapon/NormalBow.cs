using UnityEngine;

public interface IRange
{
    void SetProjectileType(ProjectileType type);    
}
public class NormalBow : Weapon, IRange
{
    private int hitSfxId = 0;
    private Transform startArrowTr;
    private ProjectileType projectileType;
    //풀 초기화
    protected override void InitializeOnChange()
    {
        base.InitializeOnChange();
         
        startArrowTr = transform.FindTarget("StartArrowPos");
        _ = EventBus.Invoke_Func<int, ProjectileBase>("Pool_GetProjectile", _info.arrowProjectileId);   
    }
    
    public override void SetHitSFX(int hitSfxId) 
    {
        this.hitSfxId = hitSfxId;
    }

    public override void OnAttack(float weight = 1f)
    {
        ProjectileBase projectile = EventBus.Invoke_Func<int, ProjectileBase>("Pool_GetProjectile", _info.arrowProjectileId);

        if(projectile != null)
        {
            float hitTime = weight >= 2f ? 0.1f : 0.05f;

            DamageSource damageSource = new DamageSource
            {
                hit_vfxId           = _info.visualData.vfxId,
                hit_sfxId           = hitSfxId,
                damageAmount        = _box.stat.player.GetAttackDamage() * weight,
                hitTime             = hitTime,
                cameraShakeSource   = 
                        new CameraShakeSource{frequency = 0.005f * weight, amplitude = 0.05f * weight, duringTime = 0.05f},
                impactForce         = _info.baseImpactForce * weight,
                elementType         = _box.stat.StatData.element,
            };
            
            Initialize_Projectile(projectile, damageSource);
        }
    }

    private void Initialize_Projectile(ProjectileBase projectile, DamageSource source)
    {
        projectile.gameObject.transform.position = startArrowTr.position;

        Vector3 targetPos = startArrowTr.position + (_box.rigid.rotation * Vector3.forward * 8.5f);

        Transform target = _box.sensor.LookTarget;

        if(target != null)
        {
            float distance = (target.position - _box.rigid.position).magnitude;

            if(distance <= _box.stat.StatData.autoTargeting.priximityRange)
                targetPos = target.position;
        }

        projectile.Initialize(
                _targetLayer,
                source, 
                startArrowTr.position, 
                targetPos,
                projectileType);

        projectile.gameObject.SetActive(true);
        projectile.Execute();
    }

    public void SetProjectileType(ProjectileType type)
    {
        projectileType = type;
    }
}

