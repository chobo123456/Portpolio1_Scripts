using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FrameSetting : SettingBase<int>
{
    private List<int> frames = new();
    private TMP_Dropdown dropdown;

    private void OnValueChange(int value)
    {
        int choosedValue = value;

        Setframe(GetFrame(choosedValue));

        base.value = choosedValue;
        base.Save();
    }

    protected override void Initialize()
    {
        base.baseValue = 3;
        base.saveName  = "Frame";

        if(dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
            dropdown.onValueChanged.AddListener(OnValueChange);
            AddOptions();
            AddFrames();
        } 
    }

    protected override void OnLoad()
    {
        int choosedValue = value;

        dropdown.value = choosedValue;

        Setframe(GetFrame(choosedValue));
    }

    private void AddOptions()
    {
        dropdown.options.Clear();
        
        List<TMP_Dropdown.OptionData> options = new();

        options.Add(GetOptionData("30"));
        options.Add(GetOptionData("60"));
        options.Add(GetOptionData("120"));
        options.Add(GetOptionData("240"));

        foreach(TMP_Dropdown.OptionData option in options)
            dropdown.options.Add(option);
    }
    private TMP_Dropdown.OptionData GetOptionData(string text)
    {
        TMP_Dropdown.OptionData newData = new TMP_Dropdown.OptionData();
        newData.text = text;
        return newData;
    }

    private void AddFrames()
    {
        AddFrame(30);
        AddFrame(60);
        AddFrame(120);
        AddFrame(240);
    }
    private void AddFrame(int frame)
    {
        frames.Add(frame);
    }

    private int GetFrame(int index)
    {
        return frames[index];
    }

    private void Setframe(int frame)
    {
        Application.targetFrameRate = frame;
    }
}
