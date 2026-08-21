using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyAreaDataBase", menuName = "Enemy/EnemyAreaDataBase")]
public class EnemyAreaDataBase : ScriptableObject
{
    public List<EnemyData> database;
}
