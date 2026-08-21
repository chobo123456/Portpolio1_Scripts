using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "QuestDataBase", menuName = "Quest/QuestDataBase")]
public class QuestDataBase : ScriptableObject
{
    public QuestData[] questDatas;
}