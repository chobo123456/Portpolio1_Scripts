using UnityEngine;
using System.Collections;
public interface ISetting_Save_Load
{
    void Save(bool debug = false);
    void Load();
}

public abstract class SettingBase<T> : MonoBehaviour, ISetting_Save_Load
{
    protected string saveName;
    protected T value;
    protected T baseValue;
    public virtual void Save(bool debug = false)
    {
        PlayerPref.SetPlayerPref<T>(saveName, value);
        PlayerPref.PlayerPrefSave();

        if(debug) Util.Log($"{saveName} / {value} 저장");
    }

    public virtual void Load()
    {
        Initialize();
        value = PlayerPref.GetPlayerPref<T>(saveName, baseValue);
        OnLoad();
    }

    protected abstract void OnLoad();
    protected virtual void Initialize() {}
}