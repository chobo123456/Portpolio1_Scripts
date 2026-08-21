using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.Mathematics;
public class ResolutionSetting : SettingBase<int>
{
    private List<int2> resolutions = new();
    private TMP_Dropdown dropdown;
    
    private void OnValueChanged(int value)
    {
        int choosedValue = value;

        SetResolution(GetResolutionInList(choosedValue));

        base.value = choosedValue;
        base.Save();
    }

    protected override void Initialize()
    {
        base.baseValue = 2;
        base.saveName  = "Resolution";

        if(dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
            dropdown.onValueChanged.AddListener(OnValueChanged);
            AddOptions();
            AddResolutions();
        }
    }

    protected override void OnLoad()
    {
        int choosedValue = base.value;
        dropdown.value = choosedValue;

        SetResolution(GetResolutionInList(choosedValue));
    }

    private void AddOptions()
    {
        dropdown.options.Clear();
        
        List<TMP_Dropdown.OptionData> options = new();

        options.Add(GetOptionData("1280 x 720"));
        options.Add(GetOptionData("1920 x 1080"));
        options.Add(GetOptionData("2560 x 1440"));

        foreach(TMP_Dropdown.OptionData option in options)
            dropdown.options.Add(option);
    }
    private TMP_Dropdown.OptionData GetOptionData(string text)
    {
        TMP_Dropdown.OptionData newData = new TMP_Dropdown.OptionData();
        newData.text = text;

        return newData;
    }

    private int2 GetResolutionInList(int index)
    {
        return resolutions[index];
    }
    private void AddResolutions()
    {
        AddResolution(GetResolution(1280, 720));
        AddResolution(GetResolution(1920, 1080));
        AddResolution(GetResolution(2560, 1440));
    }
    private int2 GetResolution(int width, int height)
    {
        return new int2(width, height);
    }
    private void AddResolution(int2 resolution)
    {
        resolutions.Add(resolution);
    }

    private void SetResolution(int2 resolution)
    {
        Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreen);
    }
}
