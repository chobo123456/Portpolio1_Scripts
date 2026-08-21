using UnityEngine;
using System.Collections;

public class EnemyHitFeedback
{
    private MaterialPropertyBlock block;
    private Renderer[] renderers;
    private Coroutine dissolveRoutine;
    private readonly LivingEntityDataBox box;
    private readonly PoissonDiskSamplingVector poissonDisk;

    public EnemyHitFeedback(LivingEntityDataBox box)
    {
        this.box = box;

        block = new(); 
        renderers = box.col.GetComponentsInChildren<Renderer>();

        float radius = box.col.radius + 0.2f;

        poissonDisk = new(radius, 1f);
    }   

    public void OnEnable()
    {
        if(renderers != null) Dissolve(0f);
    }

    public void VisualFeedback(DamageSource source)
    {
        int hitVfxId                    = source.hit_vfxId;
        int hitSfxId                    = source.hit_sfxId;
        float damageAmount              = source.damageAmount;
        Element element                 = source.elementType;
        CameraShakeSource shakeSource   = source.cameraShakeSource;

        TryVFX(hitVfxId);
        TrySFX(hitSfxId);
        TryDamageText(damageAmount, element);
        TryCameraShake(shakeSource);
        TrySaturation();
    }

    private void TryVFX(int hitVfxId)
    {
        var obj = EventBus.Invoke_Func<int, GameObject>("Pool_GetGameObject", hitVfxId);

        if (obj != null)
        {
            obj.transform.position = box.col.transform.position;
            obj.transform.rotation = Quaternion.identity;
            obj.SetActive(true);
        }
    }

    private void TrySFX(int hitSfxId)
    {
        EventBus.Invoke<Vector3, int>("Play_SFX", box.col.transform.position, hitSfxId);
    }

    private void TryDamageText(float damageAmount, Element element)
    {
        Vector3 center = box.col.transform.position;

        Vector3 random = poissonDisk.GetRandomRange(center);
        random.y += Random.Range(1f, 1.5f);

        EventBus.Invoke<float, Vector3, Element>("SetDamageText", damageAmount, random, element);
    }

    private void TryCameraShake(CameraShakeSource shakeSource)
    {
        shakeSource.frequency = Mathf.Min(shakeSource.frequency, 0.1f);
        shakeSource.amplitude = Mathf.Min(shakeSource.amplitude, 0.1f);
        shakeSource.duringTime = Mathf.Min(shakeSource.duringTime, 0.1f);

        EventBus.Invoke<(float, float, float)>("CameraShake", (shakeSource.frequency, shakeSource.amplitude, shakeSource.duringTime));
    }
    
    private void TrySaturation()
    {
        EventBus.Invoke<float>("Volume_Saturation", 90f);
    }

    #region Dissolve
    public void ActiveDissolve()
    {
        dissolveRoutine = box.mono.RunRoutine(Fade(), dissolveRoutine);
    }

    IEnumerator Fade()
    {
        float endValue = 1, currentValue = 0f, percent = 0f, lerpTime = 0.85f;

        while(percent < 1)
        {
            currentValue += Time.deltaTime;
            percent = currentValue / lerpTime;

            Dissolve(percent);
            yield return null;
        }
        
        Dissolve(endValue);
    }

    private void Dissolve(float value)
    {
        for(int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            renderer.GetPropertyBlock(block);
            block.SetFloat("_DissolveAmount", value);
            renderer.SetPropertyBlock(block);
        }
    }

    #endregion
}
