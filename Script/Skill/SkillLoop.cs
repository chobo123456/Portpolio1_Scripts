using UnityEngine;
using System.Collections.Generic;

public class SkillLoop
{
    //스킬 루핑
    private SkillLooper skillLooper;

    public SkillLoop()
    {
        skillLooper = new();
    }

    public void AddSkillLoop(SkillBase skill)
    {
        skillLooper.AddSkillLoopTarget(skill);
    }
    public void UpdateLoop()
    {
        skillLooper.SkillLoop();
    }
}

public class SkillLooper
{
    private List<SkillBase> skillSet = new();
    private int count = 0;
    public void AddSkillLoopTarget(SkillBase skill)
    {
        if(!skillSet.Contains(skill))
            skillSet.Add(skill);
        
        SkillLoop();
    }

    public void SkillLoop()
    {
        if(skillSet.Count <= 0) return;

        count = (count + 1) % skillSet.Count;

        if(skillSet[count] == null) return;

        SkillBase currentSkill = skillSet[count];

        float amount = 0;
        amount = currentSkill.CalculateCoolDown(true);

        if(!currentSkill.WasActived())
        {
            skillSet.Remove(currentSkill);
        }
    }
}
