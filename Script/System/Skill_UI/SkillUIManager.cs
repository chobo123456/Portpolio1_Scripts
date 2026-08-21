using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public struct UI_Skill_Info : IEquatable<UI_Skill_Info>
{
    public int characterId;
    public int skill_Slot_Index;
    public int skill_Id;
    public SkillData skillData;
    public SkillBase skillScript;

    public bool Equals(UI_Skill_Info targetCompareInfo)
    {
        return this.characterId == targetCompareInfo.characterId 
            && this.skill_Id == targetCompareInfo.skill_Id;
    }

    public override bool Equals(object targetCompareInfo)
    {
        return Equals((UI_Skill_Info)targetCompareInfo);
    }

    public override int GetHashCode()
    {
        return this.GetHashCode();
    }

    public static bool operator !=(UI_Skill_Info a, UI_Skill_Info b)
    {
        return a.characterId != b.characterId 
            || a.skill_Id != b.skill_Id || a.skill_Slot_Index != b.skill_Slot_Index;
    }

    public static bool operator ==(UI_Skill_Info a, UI_Skill_Info b)
    {
        return a.characterId == b.characterId 
            && a.skill_Id == b.skill_Id &&  a.skill_Slot_Index == b.skill_Slot_Index;
    }
}

//초기화
public partial class SkillUIManager : MonoBehaviour
{
    private List<GameObject> skill_slot = new();
    private List<Image> skill_Icon = new();
    private List<Image> skill_Cooldown_Icon = new();
    private List<TextMeshProUGUI> skill_CoolDownText = new();
    private List<TextMeshProUGUI> skill_InputText = new();

    private Dictionary<int, List<SkillUI_ViewModel>> viewModels = new();
    private Dictionary<int, List<Coroutine>> routines = new();
    private Dictionary<int, List<bool>> isSkillUsed = new();
    private Dictionary<int, bool> list_Initialized = new();

    private SkillLoop skillLoop;

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        IntializeUI();
        InitializeLoop();
        InitializeEvent();
    }

    private void IntializeUI()
    {
        for (int i = 0; i < 3; i++)
        {
            Transform tr = transform.FindTarget($"Skill_Slot{i + 1}");

            skill_slot.Add(tr.gameObject);
            skill_Icon.Add(tr.Find("SkillIcon").GetComponent<Image>());
            skill_Cooldown_Icon.Add(tr.Find("CoolDown").GetComponent<Image>());
            skill_InputText.Add(tr.Find("Input_KeyName").GetComponent<TextMeshProUGUI>());
            skill_CoolDownText.Add(tr.FindTarget("CoolDownTime").GetComponent<TextMeshProUGUI>());

            skill_slot[i].gameObject.SetActive(false);
            skill_Cooldown_Icon[i].fillAmount = 0f;      
        }
    }

    private void InitializeLoop()
    {
        skillLoop = new();
    }
    private void InitializeEvent()
    {
        EventBus.Sub<int, int>("SkillUIManager_Initialize", ListInitialize);
        EventBus.Sub<UI_Skill_Info>("SkillUIManager_SetSkill", SetSkill);
        EventBus.Sub<int, int>("SkillUIManager_UseSkill", UseSkill);
        EventBus.Sub<(int, string)>("SkillUIManager_OnChangedKeyInput", OnChangedKeyInput);
    }

    private void ListInitialize(int characterId, int skillCount)
    {
        if(list_Initialized.ContainsKey(characterId)) return;

        list_Initialized[characterId] = false;

        viewModels[characterId]  = new List<SkillUI_ViewModel>();
        routines[characterId]    = new List<Coroutine>();
        isSkillUsed[characterId] = new List<bool>();

        for (int i = 0; i < skillCount; i++)
        {
            viewModels[characterId].Add(new SkillUI_ViewModel());
            routines[characterId].Add(null);      
            isSkillUsed[characterId].Add(false);
        }

        list_Initialized[characterId] = true;
    }   

    public void OnDestroy()
    {
        EventBus.UnSub<int, int>("SkillUIManager_Initialize", ListInitialize);
        EventBus.UnSub<UI_Skill_Info>("SkillUIManager_SetSkill", SetSkill);
        EventBus.UnSub<int, int>("SkillUIManager_UseSkill", UseSkill);

        foreach(var view in viewModels)
        {
            var list = view.Value;
            for(int i = 0; i < list.Count; i++)
                list[i]?.readonlySkill_ImageProperties?.UnSubscribe(UpdateIcon);
        }
    }
}

//시스템
public partial class SkillUIManager : MonoBehaviour
{
    private void SetSkill(UI_Skill_Info skill_Info)
    {
        this.RunRoutine(Delay_Setting(skill_Info));
    }

    //쿨타임바 설정(캐릭터 변경 또는 맨처음 세팅)

    #region Delay_ShowUI_Apply_Process

    IEnumerator Delay_Setting(UI_Skill_Info skill_Info)
    {
        int characterId = skill_Info.characterId;
        int skillSlotIndex = skill_Info.skill_Slot_Index;

        if(skill_Info.skill_Id <= 0)
        {
            UpdateIcon((skillSlotIndex, null));
            yield break;
        }

        SetCoroutine(characterId);
        
        yield return new WaitUntil(() => list_Initialized[characterId] == true);

        SkillUI_ViewModel viewModel = viewModels[characterId][skillSlotIndex];

        SetViewModel(viewModel, skillSlotIndex, skill_Info);
        SetUI(viewModel, characterId, skillSlotIndex);
    }
    private void SetCoroutine(int characterIndex)
    {
        foreach(var routine in routines)
        {
            var key = routine.Key;
            var coroutineList = routine.Value;

            if(key == characterIndex) continue;
            
            if(coroutineList != null)
            {
                for(int i = 0; i < coroutineList.Count; i++)
                {
                    if(coroutineList[i] != null)
                        StopCoroutine(coroutineList[i]);
                }
            }
        }
    }

    private void SetUI(SkillUI_ViewModel viewModel, int characterIndex, int skillSlotIndex)
    {
        Image targetImage = skill_Cooldown_Icon[skillSlotIndex];

        //캐릭터 변경시 이전의 캐릭터가 이미 스킬을 사용했을때
        if(viewModel.WasActived())
        {
            targetImage.fillAmount = viewModel.CalculateCoolDown();
            
            StartRoutine(characterIndex, skillSlotIndex);
        }
        else // 아무런 스킬도 사용하지 않았을때
        {
            skill_CoolDownText[skillSlotIndex].enabled = false;
            targetImage.fillAmount = 0f;
        }
    }
    #endregion

    #region OnViewModel
    private void SetViewModel(SkillUI_ViewModel viewModel, int skillSlotIndex, UI_Skill_Info skill_Info)
    {
        viewModel.readonlySkill_ImageProperties.Subscribe(UpdateIcon);
        viewModel.SetSkill(skillSlotIndex, skill_Info);
    }
    
    //새로 스킬을 설정할시
    private void UpdateIcon((int index, Sprite sprite) tuple)
    {
        int index = tuple.index;
        Sprite sprite = tuple.sprite;

        if (skill_Icon.Count < index) return;

        if (sprite == null)
        {
            skill_slot[index].SetActive(false);
            skill_Cooldown_Icon[index].fillAmount = 0f;
            skill_CoolDownText[index].SetText("");
        }
        else
        {
            skill_slot[index].SetActive(true);
            skill_Icon[index].sprite = sprite;
            skill_Cooldown_Icon[index].fillAmount = 0f;
        }
    }

    #endregion
    
    #region OnUseSkill
    //스킬 사용시
    private void UseSkill(int skill_Index, int characterId)
    {
        skill_Index -= 1;
        
        SkillUI_ViewModel viewModel = viewModels[characterId][skill_Index];
        TextMeshProUGUI text = skill_CoolDownText[skill_Index];

        if(!isSkillUsed[characterId][skill_Index])
        {
            isSkillUsed[characterId][skill_Index] = true;

            text.enabled = true;
            text.SetText(viewModel.CoolDownText());

            skill_Cooldown_Icon[skill_Index].fillAmount = 1f;
        }

        skillLoop.AddSkillLoop(viewModel.GetSkillBase());

        StartRoutine(characterId, skill_Index);
    }

    #endregion

    private void StartRoutine(int characterId, int skillIndex)
    {
        if(routines.TryGetValue(characterId, out List<Coroutine> routineList))
        {
            routineList[skillIndex] = this.RunRoutine(StartUpdate(skillIndex, characterId), routineList[skillIndex]);
        }
    }

    IEnumerator StartUpdate(int skill_Index, int characterId)
    {
        TextMeshProUGUI text        = skill_CoolDownText[skill_Index];
        Image icon                  = skill_Cooldown_Icon[skill_Index];
        SkillUI_ViewModel viewModel = viewModels[characterId][skill_Index];

        if(!text.enabled) text.enabled = true;

        while (viewModel.WasActived())
        {
            icon.fillAmount = viewModel.CalculateCoolDown();
            text.SetText($"{viewModel.CurrentCoolDown():F1}");

            yield return null;
        }

        icon.fillAmount = 0f;
        text.enabled = false;
        isSkillUsed[characterId][skill_Index] = false;
    }

    #region keyText
    private void OnChangedKeyInput((int index, string key) tuple)
    {
        skill_InputText[tuple.index].SetText(tuple.key);
    }

    #endregion
}

public partial class SkillUIManager : MonoBehaviour
{
    private void Update() => skillLoop.UpdateLoop();
}

public class SkillUI_ViewModel
{
    private ReactiveProperty<(int, Sprite)> skill_ImageProperties;
    public ReadOnlyReactiveProperty<(int, Sprite)> readonlySkill_ImageProperties => skill_ImageProperties.ToReadOnlyValue();

    private UI_Skill_Info skill_Datas;
    public SkillUI_ViewModel()
    {
        (int, Sprite) args = (0, null);
        skill_ImageProperties = new(args);
    }

    public void SetSkill(int index, UI_Skill_Info infos)
    {
        if (infos != null)
        {
            SkillData data = DataLoader.GetData<SkillData>(DataType.Skill, infos.skill_Id);

            infos.skillData = data;

            if (data == null) return;

            skill_Datas = infos;

            skill_ImageProperties.Value = (index, infos.skillData.skillImage);
        }
    }

    public float CalculateCoolDown()
    {
        return skill_Datas.skillScript.CalculateCoolDown();
    }

    public float CurrentCoolDown()
    {
        return skill_Datas.skillScript.CurrentCoolDown();
    }

    public string CoolDownText()
    {
        return skill_Datas.skillData.coolDown.ToString();
    }

    public bool WasActived()
    {
        return skill_Datas.skillScript.WasActived();
    }

    public SkillBase GetSkillBase()
    {
        return skill_Datas.skillScript;
    }
}

