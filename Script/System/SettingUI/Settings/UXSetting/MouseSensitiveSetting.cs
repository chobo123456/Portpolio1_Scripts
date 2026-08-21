using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MouseSensitiveSetting : SettingBase<float>
{
    private TMP_InputField field;

    private void OnValueChanged(string value)
    {
        float.TryParse(value, out float parseValue);

        EventBus.Invoke<float>("SetCameraSensitivity", parseValue);
        
        base.value = parseValue;
        base.Save();
    }

    protected override void Initialize()
    {
        base.baseValue = 0.5f;
        base.saveName  = "Sensitivity";

        if(field == null)
        {
            field = GetComponent<TMP_InputField>();
            field.onValueChanged.AddListener(OnValueChanged);
        }
    }
    protected override void OnLoad()
    {
        field.text = $"{value}";

        EventBus.Invoke<float>("SetCameraSensitivity", value);
    }
}
