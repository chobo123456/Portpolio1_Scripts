using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum SettingType
{
    Normal,
    Bind,
}

//Start
public partial class SettingManager : UIClass
{
    public List<ISetting_Save_Load> settings            = new();
    private Dictionary<SettingType, GameObject> panels  = new();
    private List<ButtonUtil<SettingType>> buttons = new();
    private ButtonUtil exitButton;

    private BindManager bindSetting;
    private bool isInit = false;

    private GameObject mainPanel;

    public override void OnEnable()
    {
        base.SetType(UIType.Setting);
        base.OnEnable();

        Transform mainSettingPanel = transform.FindTarget("mainSettingPanel");

        Initialize_PanelList(mainSettingPanel);
        Initialize_Button(mainSettingPanel);
        Initialize_Bind();
        Initialize_FindPanel();
        this.RunRoutine(WaitForEventSubFinish());
    }
    
    private void Initialize_PanelList(Transform mainSettingPanel)
    {
        MappingPanel(SettingType.Normal,    mainSettingPanel.Find("NormalPanel").gameObject);
        MappingPanel(SettingType.Bind,      mainSettingPanel.Find("BindPanel").gameObject);
    }
    
    private void MappingPanel(SettingType type, GameObject obj)
    {
        panels.Add(type, obj);
    }
    
    private void Initialize_Button(Transform mainSettingPanel)
    {
        AddButton(mainSettingPanel.Find("NormalSettingButton"), SettingType.Normal);
        AddButton(mainSettingPanel.Find("BindSettingButton"), SettingType.Bind);

        exitButton = new ButtonUtil(mainSettingPanel.Find("ClossButton"), () => base.OnClickCloseButton());
    }
    
    private void AddButton(Transform buttonTr, SettingType type)
    {
        buttons.Add(new ButtonUtil<SettingType>(buttonTr, OnActivePanel, type));
    }
    
    private void Initialize_Bind()
    {
        bindSetting = new(transform.FindTarget("BindingContent"), GetComponent<MonoBehaviour>());
    }
   
    private void Initialize_FindPanel()
    {
        mainPanel = transform.Find("mainSettingPanel").gameObject;
        mainPanel.SetActive(false);
    }
    
    IEnumerator WaitForEventSubFinish()
    {
        yield return new WaitUntil(() => bindSetting.IsReady);

        Initialize_Setting();
    }
    
    private void Initialize_Setting()
    {
        ISetting_Save_Load[] settingComps = GetComponentsInChildren<ISetting_Save_Load>(true);
        
        for(int i = 0; i < settingComps.Length; i++)
        {
            ISetting_Save_Load setting = settingComps[i];

            setting.Load();
            settings.Add(setting);
        }

        isInit = true;
    }
}

//During
public partial class SettingManager : UIClass
{
    private void OnActivePanel(SettingType type)
    {
        foreach(var map in panels)
        {
            GameObject panel = map.Value;

            if(panel.activeSelf || panel.activeInHierarchy) 
                panel.SetActive(false);
        } 
        
        panels.TryGetValue(type, out GameObject targetPanel);

        if(!targetPanel.activeSelf || !targetPanel.activeInHierarchy) 
            targetPanel.SetActive(true);
    }
}

//End
public partial class SettingManager : UIClass
{
    private void OnDisable()
    {
        DisableBindManager();
        OnRemoveSettings();
        RemoveButtonEvents();
    }
    
    private void DisableBindManager()
    {
        bindSetting.OnDisable();
    }
    
    private void OnRemoveSettings()
    {
        for(int i = 0; i < settings.Count; i++)
        {
            ISetting_Save_Load setting = settings[i];
            
            if(setting is BindSetting bind) continue;

            setting.Save();
        }
    }
    
    private void RemoveButtonEvents()
    {
        for(int i = 0; i < buttons.Count; i++)
            buttons[i].Dispose();
    }
}

//Open - Close
public partial class SettingManager : UIClass
{
    public override bool IsReady()
    {
        return isInit;
    }
    
    public override void Close()
    {
        mainPanel.SetActive(false);
        bindSetting.ForceCancel();
    }

    public override void Open()
    {
        mainPanel.SetActive(true);
    }
}