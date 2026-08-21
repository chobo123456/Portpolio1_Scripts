using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterDataBase", menuName = "Character/Stat/StatDataBase")]
public class CharacterDataBase : ScriptableObject
{
    public List<CharacterData> list;
}
