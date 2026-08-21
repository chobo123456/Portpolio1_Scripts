using UnityEngine;

[CreateAssetMenu(fileName = "PoolInfo", menuName = "Pool/PoolInfo")]
public class PoolInfo : ScriptableObject
{
    public int pool_id;
    public GameObject pool_targetObject;
}
