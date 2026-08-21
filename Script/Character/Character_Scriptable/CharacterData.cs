using UnityEngine;
using UnityEditor;
public enum WeaponType
{
    Melee,
    Range,
}

public enum Element
{
    Light = 1,
    Dark = 2,
    Fire = 3,
    Water = 4,
    Wind = 5,
    Ground = 6,
}

[System.Serializable]
[CreateAssetMenu(fileName = "CharacterData", menuName = "Character/Data")]
public class CharacterData : ScriptableObject
{
    public int characterDataId;

    [Header("캐릭터 속성")]
    public Element element;

    [Tooltip("기본적으로 캐릭터가 가진 이동 속도")] 
    public float moveSpeed;

    [Tooltip("플레이어가 기본적으로 가진 스킬 갯수")]
    public int skillCount;

    [Tooltip("플레이어의 무기 종류")]
    public WeaponType weaponType;

    [Tooltip("캐릭터 스프라이트")]
    public Sprite characterSprite;
    public Sprite characterIcon;

    [Tooltip("캐릭터 공격 데이터")]
    public int[] attackSfxIds;
    public CharacterAutoTargeting autoTargeting;
    public LevelScriptable levelStep;

    [Tooltip("캐릭터 스킬보유 데이터")]
    public int[] characterSkillId;
}    


[System.Serializable]
public struct CharacterAutoTargeting
{
    [Range(1f, 20f)]
    public float priximityRange;
}


#if UNITY_EDITOR
[CustomEditor(typeof(CharacterData))]
public class CharacterDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        CharacterData script = (CharacterData)target;
        
        ShowProp(GetProp("characterDataId"), "CharacterID");
        ShowProp(GetProp("element"), "CharacterElement");
        ShowProp(GetProp("moveSpeed"), "CharacterSpeed");
        ShowProp(GetProp("skillCount"), "CharacterSkillCount");
        ShowProp(GetProp("weaponType"), "CharacterWeaponType");
        ShowProp(GetProp("characterSprite"), "CharacterSprite");
        ShowProp(GetProp("characterIcon"), "CharacterIconSprite");
        ShowProp(GetProp("attackSfxIds"), "CharacterAttackSfxIds");
        ShowProp(GetProp("autoTargeting"), "CharacterAutoTargeting");
        ShowProp(GetProp("levelStep"), "CharacterLevelStep");
        ShowProp(GetProp("characterSkillId"), "CharacterSkillSet");

        serializedObject.ApplyModifiedProperties();
    }

    private SerializedProperty GetProp(string name)
    {
        return serializedObject.FindProperty(name);
    }

    private void ShowProp(SerializedProperty prop, string name)
    {
        EditorGUILayout.PropertyField(prop, new GUIContent(name));
    }
}

#endif