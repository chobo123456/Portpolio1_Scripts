using UnityEngine;
using UnityEditor;

[System.Serializable]
public struct TutorialUIConfig
{
    public UIType uiType;
    public UIEntryState state;
}

#region Banner
[System.Serializable]
public struct BannerTutorialConfig
{
    public string context;
}
#endregion


#region SpotLight

#region -StartEvent

public enum TutorialStartEventName
{
    None,
    Inventory_System_TryReceiveItem,
}

[System.Serializable]
public struct TutorialStartEvent
{
    public TutorialStartEventName eventName;
    public int value1;
    public int value2;
}

#endregion

#region -Condition

public enum TutorialEvent
{
    On_Input_UI,

    On_Craft_UI_RecipeClick,
    On_Craft_UI_CraftClick,

    On_Inventory_UI_CategoryClick,
    On_Inventory_UI_ItemIconClick,
    On_Inventory_UI_ItemUseClick,
    On_UseItem,

    On_Party_UI_ClickCharacterIcon,
    On_Party_UI_ClickCharacterSlot,
    On_Party_UI_ClickCharacterSettingFinish,

    On_Growth_UI_ClickCharacterIcon,
    On_Growth_UI_ClickedExpItem,
    On_LevelUpgrade,
}

[System.Serializable]
public class TutorialCondition
{
    public TutorialEvent eventName;
    public SlotEnum slotValue;
    public int conditionTargetInt1;
    public int conditionTargetInt2;
    public UIType conditionTargetUIInteract;
    public InventoryType conditionTargetInventoryTarget;
}
#endregion

#region -SpotlightTarget
public enum SpotlightTarget
{
    HUD,
    UI
}

[System.Serializable]
public class SpotlightTargetRef
{
    public SpotlightTarget spotlightTarget;

    public UIType spotLightUIType;
    public UIRectName uiName;

    public bool isNeedIntValue;
    public int spotLightValue;
}

#endregion

#region -FinishEvent

public enum SlotEnum
{
    Index1 = 0,
    Index2 = 1,
}


public enum TutorialFinishEventName
{
    None,
    UILock,

    Craft_UI_Lock_RecipeButton,
    Craft_UI_Lock_CraftButton,
    
    Inventory_UI_Lock_ItemUsePanel,
    Inventory_UI_Lock_CategoryPanel,
    Party_UI_LockCharacterIconClick,
    Party_UI_LockSlotClick,
    Party_System_SetCheckAcceptedCharacterIdFlag,
    Party_System_SetCheckAcceptedSlotFlag,

    Growth_System_SetCheckAcceptedCharacterIdFlag,
}

[System.Serializable]
public struct TutorialFinishEvent
{
    public TutorialFinishEventName eventName;
    public UIType uiType;
    public bool isLock;    
    public int intValue1;
    public SlotEnum slotValue;
}

#endregion

#region -FinalStruct
[System.Serializable]
public class SpotlightTutorialStepData
{
    public TutorialStartEvent startEvent;
    public TutorialCondition condition;
    public SpotlightTargetRef targetRef;
    public TutorialFinishEvent[] finishEvent;
}

[System.Serializable]
public class SpotLightTutorialConfig
{
    public SpotlightTutorialStepData[] steps;
}

#endregion

#endregion

public enum TutorialType
{
    Banner,
    Spotlight
}

[CreateAssetMenu(fileName = "TutorialData", menuName = "Tutorial/Data")]
public class TutorialData : ScriptableObject
{
    public int tutorialId;
    public bool isNeedInvokeQuestEvent;
    public GameStateType tutorialEndGameState;
    public GameEnableTimeSet enableTime;

    public TutorialType tutorialType;

    public BannerTutorialConfig bannerConfig;
    public SpotLightTutorialConfig spotlightConfig;
    public TutorialUIConfig[] config;
}

#if UNITY_EDITOR

#region TutorialData
[CustomEditor(typeof(TutorialData))]
public class TutorialDataCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        TutorialData tutorialData = (TutorialData)target;

        ShowProp("tutorialId", "튜토리얼 아이디");
        ShowProp("tutorialType", "튜토리얼 타입");
        ShowProp("config", "UI 잠금/숨김 설정");
        
        switch(tutorialData.tutorialType)
        {
            case TutorialType.Banner:
                OnBanner();
                break;
            
            case TutorialType.Spotlight:
                OnSpotLight();
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnBanner()
    {
        ShowProp("bannerConfig", "배너 튜토리얼");
    }

    private void OnSpotLight()
    {
        ShowProp("isNeedInvokeQuestEvent", "퀘스트 완료 이벤트 필요?");
        ShowProp("spotlightConfig", "강조 튜토리얼");
        ShowProp("tutorialEndGameState", "강조 튜토리얼 완료후 게임 상태");
        ShowProp("enableTime", "시간 속도를 1(기본값)로 변경?");
    }

    private void ShowProp(string propName, string contextName)
    {
        SerializedProperty prop = serializedObject.FindProperty(propName);
        EditorGUILayout.PropertyField(prop, new GUIContent(contextName));
    }
}
#endregion

#region SpotlightTarget
[CustomPropertyDrawer(typeof(SpotlightTargetRef))]
public class SpotLightDataCustomEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        
        SerializedProperty spotlightTargetProp = property.FindPropertyRelative("spotlightTarget");
        EditorGUI.PropertyField(rect, spotlightTargetProp, new GUIContent("강조 대상"));
        SpotlightTarget targetType = (SpotlightTarget)spotlightTargetProp.enumValueIndex;

        float line = EditorGUIUtility.singleLineHeight + 2f;
        rect.y += line;

        SerializedProperty typeProp = property.FindPropertyRelative("spotLightUIType");
        EditorGUI.PropertyField(rect, typeProp, new GUIContent("강조 UI 타입"));
        
        rect.y += line;
        if(targetType == SpotlightTarget.UI)
        {
            SerializedProperty uiNameProp = property.FindPropertyRelative("uiName");
            EditorGUI.PropertyField(rect, uiNameProp, new GUIContent("강조 UI 이름"));

            if(uiNameProp.enumValueIndex <= 0) return;

            string uiName = uiNameProp.enumNames[uiNameProp.enumValueIndex];
            UIRectName uiRectName = (UIRectName)System.Enum.Parse(typeof(UIRectName), uiName);

            rect.y += line;

            if(uiRectName == UIRectName.Inventory_UI_InventorySlotRect || 
                uiRectName == UIRectName.CharacterPartyUI_CharacterIcon ||
                uiRectName == UIRectName.CharacterGrowthUI_CharacterIcon)
            {
                ShowProp(rect, property, "spotLightValue", "강조 정수값(아이디, 배열등등)");
            }
        }

        EditorGUI.EndProperty();
    }

    private void ShowProp(Rect rect, SerializedProperty prop, string propName, string name)
    {
        EditorGUI.PropertyField(
            rect,
            prop.FindPropertyRelative(propName),
            new GUIContent(name)
        );
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty spotlightTargetProp = property.FindPropertyRelative("spotlightTarget");
        SpotlightTarget targetType = (SpotlightTarget)spotlightTargetProp.enumValueIndex;

        float line = EditorGUIUtility.singleLineHeight + 2f;
        int lineCount = 2; // 항상그려지는 대상 2개 포함

        if(targetType == SpotlightTarget.UI)
        {
            lineCount += 2;
        }

        SerializedProperty isIntValueUIProp = property.FindPropertyRelative("isNeedIntValue");
        bool needIntValue = (bool)isIntValueUIProp.boolValue;

        if(needIntValue)
        {
            lineCount++;
        }
        
        return (line * lineCount) + 10f;
    }
}
#endregion

#region TutorialCondition
[CustomPropertyDrawer(typeof(TutorialCondition))]
public class TutorialConditionDataCustomEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        
        SerializedProperty eventNameProp = property.FindPropertyRelative("eventName");
        EditorGUI.PropertyField(rect, eventNameProp, new GUIContent("조건 이름"));

        float line = EditorGUIUtility.singleLineHeight + 2f;
        rect.y += line;

        TutorialEvent targetType = (TutorialEvent)eventNameProp.enumValueIndex;
        switch(targetType)
        {
            case TutorialEvent.On_Craft_UI_CraftClick:
            case TutorialEvent.On_Inventory_UI_ItemUseClick:
            case TutorialEvent.On_Party_UI_ClickCharacterSettingFinish:
            case TutorialEvent.On_Growth_UI_ClickedExpItem:
            case TutorialEvent.On_LevelUpgrade:
                //아무 타입도 필요없음
                break;

            case TutorialEvent.On_Craft_UI_RecipeClick:
            case TutorialEvent.On_Inventory_UI_ItemIconClick:
            case TutorialEvent.On_Party_UI_ClickCharacterIcon:
            case TutorialEvent.On_Growth_UI_ClickCharacterIcon:
                IntEventExist(rect, property);
                break;

            case TutorialEvent.On_UseItem:
                IntIntEventExist(rect, property);
                break;

            case TutorialEvent.On_Party_UI_ClickCharacterSlot:
                SlotEventExist(rect, property);
                break;

            case TutorialEvent.On_Input_UI:
                UIOpenEventExist(rect, property);
                break;

            case TutorialEvent.On_Inventory_UI_CategoryClick:
                InventoryEventExist(rect, property);
                break;
        }

        EditorGUI.EndProperty();
    }

    private void IntEventExist(Rect rect, SerializedProperty prop)
    {
        ShowProp(rect, prop, "conditionTargetInt1", "목표 입력 정수");
    }

    private void IntIntEventExist(Rect rect, SerializedProperty prop)
    {
        ShowProp(rect, prop, "conditionTargetInt1", "목표 입력 정수1");

        float line = EditorGUIUtility.singleLineHeight + 2f;
        rect.y += line;

        ShowProp(rect, prop, "conditionTargetInt2", "목표 입력 정수2");
    }

    private void SlotEventExist(Rect rect, SerializedProperty prop)
    {
        ShowProp(rect, prop, "slotValue", "목표 슬롯");
    }

    private void UIOpenEventExist(Rect rect, SerializedProperty prop)
    {
        ShowProp(rect, prop, "conditionTargetUIInteract", "목표 입력 UI");
    }

    private void InventoryEventExist(Rect rect, SerializedProperty prop)
    {
        ShowProp(rect, prop, "conditionTargetInventoryTarget", "목표 입력 인벤토리 타입");
    }

    private void ShowProp(Rect rect, SerializedProperty prop, string propName, string name)
    {
        EditorGUI.PropertyField(
            rect,
            prop.FindPropertyRelative(propName),
            new GUIContent(name)
        );
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty eventNameProp = property.FindPropertyRelative("eventName");
        TutorialEvent targetType = (TutorialEvent)eventNameProp.enumValueIndex;

        float lineSpace = 60f;

        switch(targetType)
        {
            case TutorialEvent.On_Craft_UI_CraftClick:
            case TutorialEvent.On_Inventory_UI_ItemUseClick:
            case TutorialEvent.On_Party_UI_ClickCharacterSettingFinish:
            case TutorialEvent.On_Growth_UI_ClickedExpItem:
            case TutorialEvent.On_LevelUpgrade:
                lineSpace = 2f;
                break;

            case TutorialEvent.On_Craft_UI_RecipeClick:
            case TutorialEvent.On_Inventory_UI_ItemIconClick:
            case TutorialEvent.On_Party_UI_ClickCharacterIcon:
            case TutorialEvent.On_Party_UI_ClickCharacterSlot:
            case TutorialEvent.On_Growth_UI_ClickCharacterIcon:
                lineSpace = 30f;
                break;

            case TutorialEvent.On_UseItem:
                lineSpace = 60f;
                break;

            case TutorialEvent.On_Input_UI:
                lineSpace = 30f;
                break;

            case TutorialEvent.On_Inventory_UI_CategoryClick:
                lineSpace = 30f;
                break;
        }

        float line = EditorGUIUtility.singleLineHeight + lineSpace;
        return line;
    }
}
#endregion

#region TutorialFinishEvent
[CustomPropertyDrawer(typeof(TutorialFinishEvent))]
public class TutorialFinishEventDataCustomEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        SerializedProperty eventNameProp = property.FindPropertyRelative("eventName");
        EditorGUI.PropertyField(rect, eventNameProp, new GUIContent("튜토리얼 완료시 발생 이벤트"));
        TutorialFinishEventName eventName = (TutorialFinishEventName)eventNameProp.enumValueIndex;
        
        rect.y += EditorGUIUtility.singleLineHeight + 2f;

        switch(eventName)
        {
            case TutorialFinishEventName.None:
                break;

            case TutorialFinishEventName.UILock:
                OnUIType(rect, property);
                break;

            case TutorialFinishEventName.Craft_UI_Lock_CraftButton:
            case TutorialFinishEventName.Craft_UI_Lock_RecipeButton:
            case TutorialFinishEventName.Inventory_UI_Lock_ItemUsePanel:
            case TutorialFinishEventName.Inventory_UI_Lock_CategoryPanel:
            case TutorialFinishEventName.Party_UI_LockCharacterIconClick:
            case TutorialFinishEventName.Party_UI_LockSlotClick:
                OnSingleLock(rect, property);
                break;

            case TutorialFinishEventName.Party_System_SetCheckAcceptedCharacterIdFlag:
            case TutorialFinishEventName.Growth_System_SetCheckAcceptedCharacterIdFlag:
                OnBool_IntEvent(rect, property);
                break;

            case TutorialFinishEventName.Party_System_SetCheckAcceptedSlotFlag:
                OnSlotEvent(rect, property);
                break;
        }
    }

    private void OnUIType(Rect rect, SerializedProperty prop)
    {
        ShowProp(rect, prop, "uiType", "UI타입");
        rect.y += EditorGUIUtility.singleLineHeight + 2f;

        ShowProp(rect, prop, "isLock", "활성?");
    }

    private void OnSingleLock(Rect rect, SerializedProperty prop)
    {
        ShowProp(rect, prop, "isLock", "활성?");
    }

    private void OnBool_IntEvent(Rect rect, SerializedProperty prop)
    {
        ShowProp(rect, prop, "isLock", "활성?");
        rect.y += EditorGUIUtility.singleLineHeight + 2f;

        ShowProp(rect, prop, "intValue1", "정수값");
    }

    private void OnSlotEvent(Rect rect, SerializedProperty prop)
    {
        ShowProp(rect, prop, "isLock", "활성?");
        rect.y += EditorGUIUtility.singleLineHeight + 2f;

        ShowProp(rect, prop, "slotValue", "슬롯");
    }

    private void ShowProp(Rect rect, SerializedProperty prop, string propName, string labelName)
    {
        EditorGUI.PropertyField(
            rect,
            prop.FindPropertyRelative(propName),
            new GUIContent(labelName)
            );
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty eventNameProp = property.FindPropertyRelative("eventName");
        TutorialFinishEventName eventName = (TutorialFinishEventName)eventNameProp.enumValueIndex;

        float lineSpace = 50f;

        switch(eventName)
        {
            case TutorialFinishEventName.None:
                lineSpace = 10f;
                break;

            case TutorialFinishEventName.UILock:
                lineSpace = 50f;
                break;

            case TutorialFinishEventName.Craft_UI_Lock_CraftButton:
            case TutorialFinishEventName.Craft_UI_Lock_RecipeButton:
            case TutorialFinishEventName.Inventory_UI_Lock_ItemUsePanel:
            case TutorialFinishEventName.Inventory_UI_Lock_CategoryPanel:
            case TutorialFinishEventName.Party_UI_LockCharacterIconClick:
            case TutorialFinishEventName.Party_UI_LockSlotClick:
                lineSpace = 50f;
                break;

            case TutorialFinishEventName.Party_System_SetCheckAcceptedCharacterIdFlag:
            case TutorialFinishEventName.Party_System_SetCheckAcceptedSlotFlag:
            case TutorialFinishEventName.Growth_System_SetCheckAcceptedCharacterIdFlag:
                lineSpace = 70f;
                break;
        }

        float line = EditorGUIUtility.singleLineHeight + lineSpace;
        return line;
    }
}
#endregion

#region TutorialStartEvent
[CustomPropertyDrawer(typeof(TutorialStartEvent))]
public class TutorialStartEventDataCustomEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        SerializedProperty eventNameProp = property.FindPropertyRelative("eventName");
        EditorGUI.PropertyField(rect, eventNameProp, new GUIContent("튜토리얼 시작시 발생 이벤트"));
        TutorialStartEventName eventName = (TutorialStartEventName)eventNameProp.enumValueIndex;
        
        rect.y += EditorGUIUtility.singleLineHeight + 2f;

        switch(eventName)
        {
            case TutorialStartEventName.None:
                break;

            case TutorialStartEventName.Inventory_System_TryReceiveItem:
                OnItemReceiveEvent(rect, property);
                break;
        }
    }

    private void OnItemReceiveEvent(Rect rect, SerializedProperty prop)
    {
        ShowProp(rect, prop, "value1", "아이템 아이디");
        rect.y += EditorGUIUtility.singleLineHeight + 2f;

        ShowProp(rect, prop, "value2", "아이템 갯수");
    }

    private void ShowProp(Rect rect, SerializedProperty prop, string propName, string labelName)
    {
        EditorGUI.PropertyField(
            rect,
            prop.FindPropertyRelative(propName),
            new GUIContent(labelName)
            );
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty eventNameProp = property.FindPropertyRelative("eventName");
        TutorialStartEventName eventName = (TutorialStartEventName)eventNameProp.enumValueIndex;

        float lineSpace = 10f;
        
        switch(eventName)
        {
            case TutorialStartEventName.None:
                lineSpace = 10f;
                break;

            case TutorialStartEventName.Inventory_System_TryReceiveItem:
                lineSpace = 60f;
                break;
        }

        float line = EditorGUIUtility.singleLineHeight + lineSpace;
        return line;
    }
}
#endregion

#endif

