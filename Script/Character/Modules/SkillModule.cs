using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class SkillModule : Module
{
    private SkillAct _skillActManager;
    private Dictionary<int, List<SkillBase>> _skills = new();
    private Dictionary<int, int> _skillSlotInfo = new();
    public int[] _skillIds;
    private PlayerDataBox _box;
    public bool _isDebugMode = false;

    private void OnEnable()
    {
        EventBus.Sub<int>("ChangeSkillSet", ChangeSkillSet);
    }

    public override void SetModule(PlayerDataBox box)
    {
        _box = box;

        _skillActManager = new SkillAct(box, box.stat.StatData.skillCount);

        _act = _skillActManager;

        if(_isDebugMode) ChangeSkillSet(box.CharacterId);
    }

    private void ChangeSkillSet(int characterId)
    {
        this.RunRoutine(Wait(characterId));
    }

    IEnumerator Wait(int characterId)
    {
        yield return new WaitUntil(() => LoadStatus.IsReady && _act != null);

        var data = DataLoader.GetData<CharacterData>(DataType.Character, characterId);

        int[] newSkillIds = data.characterSkillId;

        _skillIds = newSkillIds;

        int skillCount = newSkillIds.Length;

        EventBus.Invoke<int, int>("SkillUIManager_Initialize", characterId, skillCount);

        ChangeSkillSet();
    }

    private void ChangeSkillSet()
    {
        for(int i = 0; i < _skillIds.Length; i++)
        {
            int skillId = _skillIds[i];
            SetSkill(skillId, i);
        }
    }

    //새로 스킬 설정
    private void SetSkill(int skillId, int index)
    {
        if(!_skills.ContainsKey(_box.CharacterId)) 
            _skills.Add(_box.CharacterId, new List<SkillBase>());

        var skillBase = SkillFactory.GetSkill(skillId);
        if(!_skills[_box.CharacterId].Contains(skillBase)) 
            _skills[_box.CharacterId].Add(skillBase);

        _skillSlotInfo[index] = skillId;

        int transIndex = index + 1;

        SetSkill(transIndex, _skills[_box.CharacterId][index]);

        EventBus.Invoke<UI_Skill_Info>("SkillUIManager_SetSkill", new UI_Skill_Info
        {
            characterId = _box.CharacterId,
            skill_Slot_Index = index,
            skill_Id = skillId,
            skillScript = _skills[_box.CharacterId][index]
        });
    }

    private void SetSkill(int slot, SkillBase skill)
    {
        _skillActManager.OnSkillChange(slot, skill);
    }

    private void OnDisable()
    {
        EventBus.UnSub<int>("ChangeSkillSet", ChangeSkillSet);
    }
}
