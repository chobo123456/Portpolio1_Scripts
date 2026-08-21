using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditorInternal;
#endif

[System.Serializable]
public struct EnemyDropItemList
{
    public EnemyDropItemInfo[] itemList;
}

[System.Serializable]
public struct EnemyDropItemInfo
{
    public int dropItemId;
    public int dropItemAmount;
}

public enum EnemyType
{
    Dummy,
    Clone,
    Normal,
    Elite, 
    Boss,   
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/EnemyStatData")]
public class EnemyData : ScriptableObject
{
    public int enemyId;
    public string enemyName;
    public EnemyType enemyType;

    [Header("적 속성")]
    public Element element;

    public float enemyHp;
    
    [Header("Patrol")]
    public float enemyPatrolSpeed;
    public float standTime;
    public float patrolStopDistance;

    [Header("Chase")]
    public float enemyChaseSpeed;
    public float chaseStopDistance;
    public float chaseDetectRadius;

    [Header("Beware")]
    public float bewareStopTime;

    [Header("Attack")]
    public float attackRange;
    public float attackDamage;

    [Tooltip("해당 변수는 본인이 공격을 했을때 상대방에게서 뜰 vfx의 아이디값을 의미함")]

    public int hit_VfxId;

    [Tooltip("강인도 : 피격시 일정수준 깍이면 크게 밀려나는 애니메이션 실행")]
    public float poiseAmount;

    [Range(0f, 1f)]
    public float[] phaseConditionHp;

    [Header("DropItemList")]
    public EnemyDropItemList dropItemList;
}

#if UNITY_EDITOR

[CustomEditor(typeof(EnemyData))]
public class EnemyDataEditor : Editor
{
    ReorderableList reorderableList;

    private void OnEnable()
    {
        reorderableList = new ReorderableList(
            serializedObject,
            FindProp("pattern"),
            true,
            true,
            true,
            true
            );
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EnemyData data = (EnemyData)target;

        ShowProp("enemyId", "Enemy Id");
        ShowProp("enemyName", "Enemy Name");
        ShowProp("enemyType", "Enemy Type");
        ShowProp("element", "Enemy Element Type");
        ShowProp("enemyHp", "Enemy Base Hp");

        switch(data.enemyType)
        {
            case EnemyType.Dummy:
                break;
            case EnemyType.Clone:
                OnClone();
                break;
            case EnemyType.Normal:
                OnNormal();
                break;
            case EnemyType.Elite:
                OnElite();
                break;
            case EnemyType.Boss:
                OnBoss();
                break;
        }

        ShowProp("hit_VfxId", "Enemy Base Vfx Id");
        ShowProp("poiseAmount", "Enemy Base Poise Amount");

        serializedObject.ApplyModifiedProperties();
    }

    private void OnNormal()
    {
        //Patrol
        ShowProp("enemyPatrolSpeed", "-Enemy Patrol Speed-");
        ShowProp("standTime", "-Enemy Patrol Stand Time-");
        ShowProp("patrolStopDistance", "-Enemy Patrol Reach Stop Distance-");

        //Chase
        ShowProp("enemyChaseSpeed", "-Enemy Chase Speed-");
        ShowProp("chaseStopDistance", "-Enemy Chase Reach Stop Distance-");
        ShowProp("chaseDetectRadius", "-Enemy Chase Detect Radius-");

        //Beware
        ShowProp("bewareStopTime", "-Enemy Beware StopTime-");

        //Attack
        ShowProp("attackRange", "-Enemy Attack Range-");  
        ShowProp("attackDamage", "-Enemy Attack Damage-");  

        ShowProp("dropItemList", "-Enemy Drop Item List-");  
    }

    private void OnElite()
    {
        ShowProp("enemyPatrolSpeed", "-Elite Enemy Patrol Speed-");
        ShowProp("standTime", "-Elite Enemy Patrol Stand Time-");
        ShowProp("patrolStopDistance", "-Elite Enemy Patrol Reach Stop Distance-");

        //Chase
        ShowProp("enemyChaseSpeed", "-Elite Enemy Chase Speed-");
        ShowProp("chaseStopDistance", "-Elite Enemy Chase Reach Stop Distance-");
        ShowProp("chaseDetectRadius", "-Elite Enemy Chase Detect Radius-");

        ShowProp("attackRange", "-Elite Enemy Attack Range-");  
        ShowProp("attackDamage", "-Elite Enemy Attack Damage-");  

        ShowProp("dropItemList", "-Elite Enemy Drop Item List-");  
    }

    private void OnClone()
    {
        ShowProp("attackRange", "-Boss Enemy Attack Range-");
        ShowProp("attackDamage", "-Boss Enemy Attack Damage-");
    }

    private void OnBoss()
    {
        ShowProp("phaseConditionHp", "-Boss Change Phase Hp Percent-");
        ShowProp("enemyChaseSpeed", "-Boss Chase Speed-");
        ShowProp("attackRange", "-Boss Enemy Attack Range-");
        ShowProp("attackDamage", "-Boss Enemy Attack Damage-");
    }

    private SerializedProperty FindProp(string propName)
    {
        return serializedObject.FindProperty(propName);
    }

    private void ShowProp(string propName, string textName)
    {
        EditorGUILayout.PropertyField(FindProp(propName), new GUIContent(textName));
    }

    private void ListOnGUI()
    {
        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocus) =>
        {
            SerializedProperty elementProp = reorderableList.serializedProperty.GetArrayElementAtIndex(index);

            rect.height = -4f;
            rect.y += 2f;

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                elementProp, 
                new GUIContent($"Pattern {index + 1}"),
                true);
        };

        reorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, new GUIContent("Pattern"));
        };

        reorderableList.elementHeightCallback = (index) =>
        {
            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + 4f;
        };

        reorderableList?.DoLayoutList();
    }
}

#endif