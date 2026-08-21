using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QualitySetting : SettingBase<int>
{
    private TMP_Dropdown dropdown;
    private void OnValueChanged(int value)
    {
        int choosedValue = value;

        EventBus.Invoke<TerrainQuality>("SetTerrainQuality", GetTerrainQualityType(choosedValue));

        base.value = choosedValue;
        base.Save();
    }

    protected override void Initialize()
    {
        base.baseValue = 1;
        base.saveName  = "Quality";

        if(dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
            dropdown.onValueChanged.AddListener(OnValueChanged);
            AddOption();
        }
    }

    protected override void OnLoad()
    {
        int choosedValue = value;

        dropdown.value = choosedValue;

        EventBus.Invoke<TerrainQuality>("SetTerrainQuality", GetTerrainQualityType(choosedValue));
    }

    private void AddOption()
    {
        List<TMP_Dropdown.OptionData> options = new();

        options.Add(GetOptionData("High"));
        options.Add(GetOptionData("Normal"));
        options.Add(GetOptionData("Low"));

        foreach(TMP_Dropdown.OptionData option in options)
            dropdown.options.Add(option);
    }

    private TMP_Dropdown.OptionData GetOptionData(string text)
    {
        dropdown.options.Clear();
        
        TMP_Dropdown.OptionData newData = new TMP_Dropdown.OptionData();
        newData.text = text;

        return newData;
    }

    private TerrainQuality GetTerrainQualityType(int index)
    {
        switch(index)
        {
            case 0:
                return TerrainQuality.High;
            case 1:
                return TerrainQuality.Normal;
            case 2:
                return TerrainQuality.Low;
            default:
                return TerrainQuality.Low;
        }
    }
}
