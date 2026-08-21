using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NPCDataBase", menuName = "NPC/NPCDataBase")]
public class NPCDataBase : ScriptableObject
{
    public List<NPCData> database;
}