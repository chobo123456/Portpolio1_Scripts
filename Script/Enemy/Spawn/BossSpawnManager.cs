using UnityEngine;

public class BossSpawnManager : MonoBehaviour
{
    private GameObject _bossBarrier;
    public Transform _bossSpawnPoint;

    private void OnEnable()
    {
        EventBus.Sub<int>("SpawnBoss", SpawnBoss);
        
        _bossBarrier = transform.FindTarget("BossBarrier").gameObject;
        _bossBarrier.SetActive(false);
    }
    
    private void OnDisable()
    {
        EventBus.UnSub<int>("SpawnBoss", SpawnBoss);
    }

    private void SpawnBoss(int bossId)
    {
        EnemyData bossData = DataLoader.GetData<EnemyData>(DataType.Enemy, bossId);

        Entity entity = EventBus.Invoke_Func<int, Entity>("Get_Enemy", bossId);
        entity.gameObject.name = bossData.enemyName;
        entity.transform.position = _bossSpawnPoint.position;
        
        entity.gameObject.SetActive(true);

        _bossBarrier.SetActive(true);
        _bossBarrier.transform.position = _bossSpawnPoint.position;
    }

    private void BossDefeated()
    {
        _bossBarrier.SetActive(false);
    }
}
