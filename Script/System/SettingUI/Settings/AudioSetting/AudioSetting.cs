using UnityEngine;
using UnityEngine.UI;

public enum AudioType
{
    Master,
    BGM,
    SFX,
    Voice,
}
public abstract class AudioSetting : SettingBase<float>
{
    protected AudioType type;  
    protected Slider slider;
    protected void OnSlider(float value)
    {
        value = Mathf.Max(0.01f, value);
        float volumeValue = Mathf.Max(20 * Mathf.Log10(value));
        // (10 * 2) * Log10(진폭) => 기존 데시벨 공식은 10 * Log10(전력) 
        // 허나 게임에서 조절하는 슬라이더의 주목표는 전력이 아닌 진폭 그래서 물리법칙상으로 전력의 비례해 진폭 제곱값이 나옴
        // 그리고 로그의 공식중 log(x^y) == y * log(x)라는 공식존재 그래서 제곱값(2)가 앞으로 나오며 10과 곱해짐
        //결과 20 * log10(진폭)

        EventBus.Invoke<AudioType, float>("SetVolume", type, volumeValue);

        base.value = value;
        base.Save();
    }

    protected override void Initialize()
    {
        base.baseValue = 0.8f;

        SetSaveName();

        if(slider == null) 
        {
            slider = GetComponent<Slider>();
            slider.onValueChanged.AddListener(OnSlider);
        }
    }

    protected override void OnLoad()
    {
        slider.value = value;

        float volumeValue = 20 * Mathf.Log10(value);
        EventBus.Invoke<AudioType, float>("SetVolume", type, volumeValue);
    }

    protected abstract void SetSaveName();
}
