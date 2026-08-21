using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPrefabData", menuName = "Enemy/EnemyPrefabData")]
public class EnemyPrefabData : ScriptableObject
{
    public int prefabId;
    public GameObject prefab;
}
