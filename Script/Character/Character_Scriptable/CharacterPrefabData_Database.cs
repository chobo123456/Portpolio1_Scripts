using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterPrefabData_Database", menuName = "Character/Prefab/Character_Prefab_DataBase")]
public class CharacterPrefabData_Database : ScriptableObject
{
    public List<Character_Prefab_Data> list;
}
