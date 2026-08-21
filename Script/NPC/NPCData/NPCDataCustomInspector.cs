using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(NPCData))]
public class NPCDataCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        ShowProp("npcId", "NPC Id :");
        ShowProp("npcName", "NPC Name :");
        ShowProp("type", "NPC TYPE :");
        ShowProp("startTalk", "StartTalk :");

        SerializedProperty typeProp = serializedObject.FindProperty("type");

        NPCType type = (NPCType)typeProp.enumValueIndex;

        switch(type)
        {
            case NPCType.QuestNPC:
                OnQuestNPC();
                break;
            case NPCType.ShopNPC:
                OnShopNPC();
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowProp(string propName, string showName)
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty(propName), new GUIContent(showName));
    }
    
    private void OnQuestNPC()
    {
        ShowProp("duringTalk", "QuestDuringTalk :");
        ShowProp("endTalk", "QuestEndTalk :");
    }

    private void OnShopNPC()
    {
        ShowProp("shopData", "ShopData :");
    }
}

#endif
