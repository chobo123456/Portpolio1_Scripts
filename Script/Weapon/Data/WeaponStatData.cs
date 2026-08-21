using UnityEngine;
using UnityEditor;

[System.Serializable]
[CreateAssetMenu(fileName = "WeaponStatData", menuName = "Weapon/Data")]
public class WeaponStatData : ScriptableObject
{
    public int weaponId;
    [Tooltip("무기의 종류")]
    public WeaponType type;
    public float damage;

    [Header("기본 취약도")]
    public float baseImpactForce;

    [Header("--Melee projectile Id--")]
    [Tooltip("무기의 길이(오버랩 박스로 찍을 위치) / 계산 : 현재 위치 + 보고있는 회전방향에서의 앞쪽방향 * 무기 길이")]
    public float weaponRange;
    public float length;

    public WeaponVisualData visualData;

    [Header("--Range projectile Id--")]
    public int arrowProjectileId;
}

[System.Serializable]
public struct WeaponVisualData
{
    [Tooltip("히트 vfx ID")]
    public int vfxId;

    [Tooltip("공격 vfx ID")]
    public int attack_vfxId;

    [Tooltip("마지막 공격 vfx ID")]
    public int final_attack_vfxId;

    [Tooltip("무기 Mesh")]
    public Mesh weaponMesh;

    [Tooltip("무기 Material")]
    public Material weaponMaterial;

    [Tooltip("무기 Sprite")]
    public Sprite weaponSprite;
}

#if UNITY_EDITOR

[CustomEditor(typeof(WeaponStatData))]
public class WeaponDataScriptableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WeaponStatData soData = (WeaponStatData)target;

        ShowProp("weaponId", "Weapon_Id");
        ShowProp("type", "Weapon_Type");
        ShowProp("damage", "Weapon_Damage");
        ShowProp("baseImpactForce", "WeaponBaseImpactForce");
        ShowProp("visualData", "Weapon_VisualData");

        switch(soData.type)
        {
            case WeaponType.Melee:
                Melee();
                break;

            case WeaponType.Range:
                Range();
                break;

            default :
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowProp(string propName, string guiName)
    {
        SerializedProperty prop = serializedObject.FindProperty(propName);
        EditorGUILayout.PropertyField(prop, new GUIContent(guiName));
    }

    private void Melee()
    {
        ShowProp("weaponRange", "WeaponRange");
        ShowProp("length", "Melee_Check_StartPoint_Length");
    }

    private void Range()
    {
        ShowProp("arrowProjectileId", "Range_ProjectileId");
    }
}

#endif