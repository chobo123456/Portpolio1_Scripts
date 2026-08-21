using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Skill/SkillData")]
public class SkillData : ScriptableObject
{
    public int skillId;
    public float damageMultiple;
    public float coolDown;
    public float duringTime;
    public Sprite skillImage;
}
