using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;

public enum EffectType
{
    ChromaticAberration,
    LensDistortion,
    Vignette,
    LensFlare,
}

public class VolumeManager : MonoBehaviour
{
    private Volume volume;
    private VolumeProfile profile;
    private Dictionary<EffectType, VolumeComponent> compMaps = new();
    
    private void OnEnable()
    {
        volume = GetComponent<Volume>();
        profile = volume.profile;

        InitializeList();
        InitializeEvent();
    }

    private void InitializeList()
    {
        AddEffectInList<ChromaticAberration>(EffectType.ChromaticAberration);
        AddEffectInList<LensDistortion>(EffectType.LensDistortion);
    }

    private void InitializeEvent()
    {
        EventBus.Sub<(EffectType, bool)>("EnabledEffect", EnabledEffect);
        EventBus.Sub<float>("Volume_Chroma", SetChromaticAberration_Lerp);
        EventBus.Sub<float>("Volume_Vignette", SetVignette_Lerp);
        EventBus.Sub<float>("Volume_LensFlare", SetScreenFlare_Lerp);
        EventBus.Sub<float>("Volume_LensDistortion", SetLensDistortion_Lerp);
        EventBus.Sub<float>("Volume_Saturation", SetSaturation_Lerp);
        EventBus.Sub<float>("Volume_Bloom", SetBloom_Lerp);
    }

    private void OnDisable()
    {
        EventBus.UnSub<(EffectType, bool)>("EnabledEffect", EnabledEffect);
        EventBus.UnSub<float>("Volume_Chroma", SetChromaticAberration_Lerp);
        EventBus.UnSub<float>("Volume_Vignette", SetVignette_Lerp);
        EventBus.UnSub<float>("Volume_LensFlare", SetScreenFlare_Lerp);
        EventBus.UnSub<float>("Volume_LensDistortion", SetLensDistortion_Lerp);
        EventBus.UnSub<float>("Volume_Saturation", SetSaturation_Lerp);
        EventBus.UnSub<float>("Volume_Bloom", SetBloom_Lerp);
    }
    
    private void EnabledEffect((EffectType type, bool isOn) tuple)
    {
        if(compMaps.TryGetValue(tuple.type, out var comp))
            comp.active = tuple.isOn;
    }

    private void AddEffectInList<T>(EffectType type) where T : VolumeComponent
    {
        profile.TryGet<T>(out var comp);
        compMaps.Add(type, comp);
    }

    private void SetChromaticAberration_Lerp(float targetAmount)
    {
        profile.TryGet<ChromaticAberration>(out var comp);

        StartingRoutine(
            "EffectType.ChromaticAberration", 
            Start_Linear(comp.intensity, targetAmount));
    }

    private void SetVignette_Lerp(float targetAmount)
    {
        profile.TryGet<Vignette>(out var comp);

        StartingRoutine(
            "EffectType.Vignette", 
            Start_Linear(comp.intensity, targetAmount, 0.25f, 0.1f));
    }

    private void SetScreenFlare_Lerp(float targetAmount)
    {
        profile.TryGet<ScreenSpaceLensFlare>(out var comp);

        StartingRoutine(
            "EffectType.LensFlare", 
            Start_Linear(comp.intensity, targetAmount));
    }

    private void SetLensDistortion_Lerp(float targetAmount)
    {
        profile.TryGet<LensDistortion>(out var comp);

        StartingRoutine(
            "EffectType.LensDistortion", 
            Start_Linear(comp.intensity, targetAmount));
    }

    private void SetSaturation_Lerp(float targetAmount)
    {
        profile.TryGet<ColorAdjustments>(out var comp);

        StartingRoutine(
            "EffectType.Saturation", 
            Start_Linear(comp.saturation, targetAmount, 45f, 0.1f));
    }

    private void SetBloom_Lerp(float targetAmount)
    {
        profile.TryGet<Bloom>(out var comp);

        StartingRoutine(
            "EffectType.Bloom", 
            Start_Linear(comp.intensity, targetAmount, 0.2f));
    }

    IEnumerator Start_Linear(VolumeParameter<float> parameter, float targetAmount, float originValue = 0f, float lerpTime = 0.25f)
    {
        float currentTime = 0f, percent = 0f;

        float startValue = parameter.value;

        while(percent < 1)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / lerpTime;

            float value = Mathf.Lerp(startValue, targetAmount, percent);
            parameter.value = value;

            yield return null;
        }

        parameter.value = targetAmount;

        currentTime = 0f;
        percent     = 0f;
        startValue  = parameter.value;
        lerpTime    = 0.8f;

        while(percent < 1)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / lerpTime;

            float value = Mathf.Lerp(startValue, originValue, percent);
            parameter.value = value;

            yield return null;
        }

        parameter.value = originValue;
    }

    private void StartingRoutine(string routineName, IEnumerator startMethod)
    {
        this.RunRoutine(startMethod, routineName);
    }
}
