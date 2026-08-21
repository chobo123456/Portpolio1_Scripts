using UnityEngine;

[CreateAssetMenu(fileName = "LevelScriptable", menuName = "Level/LevelScriptable")]
public class LevelScriptable : ScriptableObject
{
    public LevelStep[] levelSteps;

    public float GetDefensiveUseLevel(int level)
    {
        for(int i = 0; i < levelSteps.Length; i++)
        {
            var levelStep = levelSteps[i];

            if(levelStep.levelAmount == level)
            {
                return levelStep.denfensive;
            }
        }

        return levelSteps[0].denfensive;
    }
   
    public float GetMaxHpUseLevel(int level)
    {
        for(int i = 0; i < levelSteps.Length; i++)
        {
            var levelStep = levelSteps[i];

            if(levelStep.levelAmount == level)
            {
                return levelStep.maxHp;
            }
        }

        return levelSteps[0].maxHp;
    }

    public float GetBaseAttackDamageUseLevel(int level)
    {
        for(int i = 0; i < levelSteps.Length; i++)
        {
            var levelStep = levelSteps[i];

            if(levelStep.levelAmount == level)
            {
                return levelStep.baseAttack;
            }
        }

        return levelSteps[0].baseAttack;
    }

    public float GetLevelUpgradeAmount(int level)
    {
        for(int i = 0; i < levelSteps.Length; i++)
        {
            var levelStep = levelSteps[i];

            if(levelStep.levelAmount == level)
            {
                return levelStep.levelUpAmount;
            }
        }

        return levelSteps[0].levelUpAmount;
    }
}

[System.Serializable]
public struct LevelStep
{
    public int levelAmount;
    public float levelUpAmount;
    public float denfensive;
    public float maxHp;
    public float baseAttack;
}
