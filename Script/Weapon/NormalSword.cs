using UnityEngine;
public class NormalSword : Weapon
{
    private int hitSfxId = 0;
    public override void SetHitSFX(int hitSfxId) 
    {
        this.hitSfxId = hitSfxId;
    }

    public override void OnAttack(float weight = 1f)
    {
        Vector3 startPos = _box.rigid.position + (_box.rigid.rotation * Vector3.forward * _info.length);

        float range = _info.weaponRange / 1.5f;

        Collider[] cols = Physics.OverlapBox(
            startPos,
            new Vector3(range, range, range),
            Quaternion.identity,
            _targetLayer
        );

        if (cols.Length > 0)
        {
            for(int i = 0; i < cols.Length; i++)
            {
                var col = cols[i];

                var comp = col.GetComponent<IDamageable>();

                if(comp != null)
                {
                    Vector3 knockBackDir = col.transform.position - _box.rigid.position;
                    float hitTime = weight >= 1.5f ? 0.1f : 0.05f;
                    
                    DamageSource damageSource = new DamageSource
                    {
                        hit_vfxId           = _info.visualData.vfxId,
                        hit_sfxId           = hitSfxId,
                        knockbackDir        = knockBackDir,
                        damageAmount        = _box.stat.player.GetAttackDamage() * weight,
                        hitTime             = hitTime,
                        cameraShakeSource   = 
                        new CameraShakeSource{frequency = 0.005f * weight, amplitude = 0.05f * weight, duringTime = 0.05f},
                        impactForce         = _info.baseImpactForce * weight,
                        elementType         = _box.stat.StatData.element,
                    };

                    comp.TakeDamage(damageSource);
                }
            }
        }
    }
}

