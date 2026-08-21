using UnityEngine;
using System.Collections.Generic;

public class EnemyIDFinder
{
    private static Dictionary<string, int> name_IdMap = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeMap()
    {
        foreach(string value in System.Enum.GetNames(typeof(EnemyNameToID)))
        {
            if(System.Enum.TryParse(value, true, out EnemyNameToID type))
            {
                if(!name_IdMap.ContainsKey(value)) 
                    name_IdMap.Add(value, (int)type);
            }
        }
    }

    public static int GetEnemyIDFromName(string name)
    {
        name_IdMap.TryGetValue(name, out int id);

        return id;
    }
}
public enum EnemyNameToID
{
    Knight = 1,
    Esha = 100,
    Esha_Clone = 101,
    Esha_SwordClone = 102,
    Dummy = 100000,
}

public class EnemySpawnManager : MonoBehaviour
{
    
    public bool[] ignorePatrol;
    public int[] pathIds;
    public int spawnEnemyId;
    public int spawnCount = 1;

    private bool isSpawned = false;

    private void OnEnable()
    {
        if(!isSpawned)
            this.RunRoutine(Spawn());

        isSpawned = true;
    }

    System.Collections.IEnumerator Spawn()
    {
        yield return new WaitUntil(() => LoadStatus.IsReady);

        EnemyData enemyData = DataLoader.GetData<EnemyData>(DataType.Enemy, spawnEnemyId);

        for(int i = 0; i < spawnCount;  i++)
        {
            Entity entity = EventBus.Invoke_Func<int, Entity>("Get_Enemy", spawnEnemyId);
            entity.gameObject.name = enemyData.enemyName;
            entity.gameObject.SetActive(false);

            var spawn = entity.GetComponent<ISpawnable>();
            spawn?.SetSpawn();

            #region Path
            var patrolComp = entity.GetComponentInChildren<PatrolNode_Module>();

            if(pathIds.Length == spawnCount && ignorePatrol.Length > i && ignorePatrol[i] == true)
            {
                Vector3[] path = DataLoader.GetData<Vector3[]>(DataType.AIPath, pathIds[i]);

                if(patrolComp != null)
                    Destroy(patrolComp);

                entity.gameObject.transform.position = path[0];

                continue;
            }
            
            if(pathIds.Length == spawnCount)
            {
                Vector3[] path = DataLoader.GetData<Vector3[]>(DataType.AIPath, pathIds[i]);

                entity.gameObject.transform.position = path[0] + Vector3.up * 1f;

                if(patrolComp != null)
                    patrolComp.SetPatrolPoint(path);
            }

            #endregion
        }
        yield return null;
    }
}
