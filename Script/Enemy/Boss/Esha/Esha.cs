using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Esha : BossBase<EshaDataBox>
{
    public override void OnEnable()
    {
        EventBus.Sub<EshaClone>("EshaCloneDie", OnCloneDie);

        EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState", GameStateType.BossBattle, GameEnableTimeSet.False);

        base.OnEnable();
    }

    protected override void OnInitializeTask()
    {
        if(livingEntityDataBox.currentTarget == null)
            livingEntityDataBox.currentTarget = EventBus.Invoke_Func<GameObject>("GetCharacterObject").GetComponent<ITarget>();

        EventBus.Invoke<CameraCase>("SetCameraCase", CameraCase.Boss);
        EventBus.Invoke<MonoBehaviour, int, bool>("EnemyDetect", this, livingEntityDataBox.livingEntityId, false);

        EventBus.Invoke<Vector3, int>("Play_BGM", this.transform.position, 3);
    }

    public void OnDisable()
    {
        EventBus.UnSub<EshaClone>("EshaCloneDie", OnCloneDie);
        EventBus.Invoke<CameraCase>("SetCameraCase", CameraCase.None);
        EventBus.Invoke<MonoBehaviour, bool>("EnemyUnDetect", this, false);

        EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState", GameStateType.Run, GameEnableTimeSet.False);
    }

    private void OnCloneDie(EshaClone clone)
    {
        livingEntityDataBox.clones.Remove(clone);
    }

    public override void SomeTask()
    {
        if (livingEntityDataBox.nav != null)
        {
            livingEntityDataBox.nav.updateRotation = false;
            livingEntityDataBox.nav.updatePosition = false;
        } 
    }

    private void Update()
    {
        if (!isInitializeOnce || livingEntityDataBox.damageComp.IsDie || !GameState.IsActive()) return;
        
        livingEntityDataBox.nav.SetDestination(livingEntityDataBox.currentTarget.currentPos);
        mainSelecter.Execute();
    }
}
