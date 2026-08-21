using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;
public class BindManager
{
    private readonly Transform contentTr;
    private static string bindSavePathName = "BindingInfo"; 
    public bool IsReady = false;
    private List<BindSetting> settings = new();
    private BindSetting currentChooseSetting, previousChooseSetting;
    public BindManager(Transform targetContentTr, MonoBehaviour mono)
    {
        EventBus.Sub("BindManager_FinishVisual", FinishVisual);
        EventBus.Sub<BindSetting>("BindManager_OnChoose", OnChooseSetting);

        this.contentTr = targetContentTr;   
        mono.RunRoutine(WaitForEvent(mono), "BindManager_WaitForEvent");
    }

    IEnumerator WaitForEvent(MonoBehaviour mono)
    {
        yield return new WaitUntil(() => EventBus.HasEvent("CharacterInputOverride"));
        yield return new WaitUntil(() => EventBus.HasEvent("GetCharacterInputAction"));
        yield return new WaitUntil(() => EventBus.HasEvent("SkillUIManager_OnChangedKeyInput"));
        

        Intialize_BindList(EventBus.Invoke_Func<CharacterInput>("GetCharacterInputAction"), mono);
    }   

    public void OnDisable()
    {
        ForceCancel();

        EventBus.UnSub("BindManager_FinishVisual", FinishVisual);
        EventBus.UnSub<BindSetting>("BindManager_OnChoose", OnChooseSetting);
    }

    private async void Intialize_BindList(CharacterInput inputAction, MonoBehaviour mono)
    {
        string savedJsonPath = PlayerPref.GetPlayerPref<string>(bindSavePathName);

        if(!string.IsNullOrEmpty(savedJsonPath))
        {
            EventBus.Invoke<string>("CharacterInputOverride", savedJsonPath);
            inputAction = EventBus.Invoke_Func<CharacterInput>("GetCharacterInputAction");
        }

        GameObject prefab = await AddressableUtil.Load_Instant<GameObject>("SettingUI_Bind", mono.GetCancelOnDestroy());

        InputActionAsset asset = inputAction.asset;
        InputActionMap actionMap = asset.actionMaps[0];

        List<(string inputName, string bindKeyName, string bindActionName, int bindingIndex)> bindList = new();

        for(int i = 0; i < actionMap.actions.Count; i++)
        {
            InputAction action = actionMap.actions[i];
            List<InputBinding> bindings = action.bindings.ToList();

            for(int j = 0; j < bindings.Count; j++)
            {
                InputBinding binding = bindings[j];
                int bindIndex = action.bindings.IndexOf(b => b.id == binding.id);

                if(binding.isPartOfComposite)
                    bindList.Add((binding.name, binding.ToDisplayString(), action.name, bindIndex));
                else if(!binding.isComposite)
                    bindList.Add((action.name,  binding.ToDisplayString(), "", bindIndex));   
            }
        }

        for(int i = 0; i < bindList.Count; i++)
        {
            var list = bindList[i];

            string inputText        = list.inputName;
            string bindingKeyName   = list.bindKeyName;
            string actionName       = list.bindActionName;
            int bindIndex           = list.bindingIndex;

            GameObject newObj = Object.Instantiate(prefab);
            newObj.transform.SetParent(contentTr);

            BindSetting comp = newObj.GetComponent<BindSetting>();

            if(comp == null) continue;

            comp.InitializeSavePath(bindSavePathName);
            comp.Initialize_BindSetting(inputText, bindingKeyName, actionName, bindIndex);

            settings.Add(comp);
        }

        FinishVisual();

        IsReady = true;
    }

    private void FinishVisual()
    {
        CharacterInput input = EventBus.Invoke_Func<CharacterInput>("GetCharacterInputAction");

        for(int i = 0; i < settings.Count; i++)
        {
            var setting = settings[i];
            setting.FindMatchKeyExist(input);
            setting.TryChangeSkillUIText();
        }
    }

    private void OnChooseSetting(BindSetting newSetting)
    {
        previousChooseSetting = currentChooseSetting;
        previousChooseSetting?.ForceCancelling();

        currentChooseSetting = newSetting;        
    }

    public void ForceCancel()
    {
        previousChooseSetting?.ForceCancelling();
        currentChooseSetting?.ForceCancelling();
    }
}
