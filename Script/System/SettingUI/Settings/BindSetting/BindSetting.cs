using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class BindSetting : SettingBase<string>
{
    private Image warningPanel;
    private TextMeshProUGUI targetText, keyText, waitText;
    private Button btn;
    private InputActionRebindingExtensions.RebindingOperation oper;
    private string compositePartActionName, displayName, key, bindSavePathName;
    private int bindIndex;
    private bool isInProcess = false;

    #region Initialize
    public void InitializeSavePath(string path)
    {
        bindSavePathName = path;
        base.saveName   = bindSavePathName;
    }
    public void Initialize_BindSetting(string displayName, string key, string compositePartActionName, int bindIndex)
    {
        this.displayName            = displayName;
        this.key                    = key;
        this.bindIndex              = bindIndex;

        if(!string.IsNullOrEmpty(compositePartActionName)) 
            this.compositePartActionName = compositePartActionName;

        InitializeComp();          
    }
    private void InitializeComp()
    {
        if(targetText == null) {
            targetText = transform.Find("Input_Text").GetComponent<TextMeshProUGUI>();
            targetText.SetText(displayName);
        }

        if(btn == null) 
        {
            btn = transform.Find("Input").GetComponent<Button>();
            btn.onClick.AddListener(OnClick);
        }

        if(keyText == null)
            keyText = transform.Find("KeyText").GetComponent<TextMeshProUGUI>();

        if(waitText == null)
            waitText = transform.Find("WaitText").GetComponent<TextMeshProUGUI>();

        if(warningPanel == null)
            warningPanel = transform.Find("WarningPanel").GetComponent<Image>();
    }
    protected override void OnLoad()
    {
        keyText.text = key;
        keyText.enabled = true;
        waitText.enabled = false;
    }
    #endregion

    private InputAction FindAction(CharacterInput inputAction)
    {
        InputAction action = inputAction.asset.FindAction(displayName);

        if(action == null) {
            action = inputAction.asset.FindAction(compositePartActionName);
            
            if(action == null)
            {
                Util.Log("액션을 찾아보았으나 검색안됨");
                return null;
            }
        }

        return action;
    }

    #region rebinding
    private void OnClick()
    {
        if(isInProcess) return;
        
        EventBus.Invoke<BindSetting>("BindManager_OnChoose", this);

        isInProcess = true;

        keyText.enabled  = false;
        waitText.enabled     = true;    

        CharacterInput inputAction = EventBus.Invoke_Func<CharacterInput>("GetCharacterInputAction");
        InputAction action = FindAction(inputAction);

        if(action == null) {
            isInProcess = false;
            return;
        }

        action.Disable();

        oper = action.PerformInteractiveRebinding(bindIndex)
            .WithTargetBinding(bindIndex)
            .WithCancelingThrough("<Mouse>/rightButton")
            .OnMatchWaitForAnother(0.1f);

        if(displayName != "Attack")
            oper.WithControlsExcluding("<Mouse>");
            
        oper.OnComplete(operation => OnSave(inputAction, action))
            .OnCancel(operation => OnCanceled(action))
            .Start();
    }

    private void OnSave(CharacterInput inputAction, InputAction action)
    {
        string json = inputAction.SaveBindingOverridesAsJson();

        base.value = json;
        base.Save();
        
        key = GetCurrentKeyText(action);
        keyText.SetText(key);

        keyText.enabled         = true;
        waitText.enabled        = false;

        oper.Dispose();
        action.Enable();

        EventBus.Invoke("BindManager_FinishVisual");

        isInProcess = false;
    }

    private void OnCanceled(InputAction action)
    {
        key = GetCurrentKeyText(action);
        keyText.SetText(key);

        keyText.enabled     = true;
        waitText.enabled    = false;

        oper.Dispose();
        action.Enable();

        isInProcess = false;
    }

    public void ForceCancelling()
    {
        CharacterInput inputAction = EventBus.Invoke_Func<CharacterInput>("GetCharacterInputAction");
        InputAction action = FindAction(inputAction);
        OnCanceled(action);
    }

    private string GetCurrentKeyText(InputAction action)
    {
        return action.bindings[bindIndex].ToDisplayString();
    }
    #endregion

    #region visual
    public void FindMatchKeyExist(CharacterInput inputAction)
    {
        bool isExistMatchKey = false;
        
        InputAction curAction = FindAction(inputAction);;
        InputActionMap actionMap = curAction.actionMap;

        for(int i = 0; i < actionMap.actions.Count; i++)
        {
            InputAction action = actionMap.actions[i];
            
            for(int j = 0; j < action.bindings.Count; j++)
            {
                InputBinding binding = action.bindings[j];

                if(binding.isComposite) continue;
                if(action == curAction && j == bindIndex) continue;

                if(binding.ToDisplayString() == key)
                {
                    isExistMatchKey = true;
                    break;
                }
            }

            if(isExistMatchKey) break;
        }

        warningPanel.enabled = isExistMatchKey;
    }
    
    public void TryChangeSkillUIText()
    {
        if(displayName.Contains("Skill"))
        {
            string indexStr = displayName.Replace("Skill", "");
            
            int.TryParse(indexStr, out int skillIndex);

            int calculateMatchListIndex = skillIndex - 1;
            if(calculateMatchListIndex < 0)
            {
                Util.Log("주의 스킬의 인풋키 텍스트 변경시도중 인덱스가 0 미만이 됨", "red");
                return;
            }

            EventBus.Invoke<(int, string)>("SkillUIManager_OnChangedKeyInput", (calculateMatchListIndex, key));
        }
    }
    #endregion

    
}
