using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public partial class Enemy : EnemyBase<NormalEnemyDataBox>
{
    private bool isRegistCull = false;
    public override void OnEnable()
    {
        base.OnEnable();

        if(isRegistCull) return;

        isRegistCull = true;

        EventBus.Invoke<ICullable>("SubCull", this);
    }

    public virtual void OnDestroy()
    {
        EventBus.Invoke<ICullable>("UnSubCull", this);

        isRegistCull = false;
    }

    private void Update()
    {
        if (!isInitializeOnce) return;
        if(!GameState.IsActive()) return;
    
        if(!updateEnabled)
        {
            if(livingEntityDataBox.nav.isStopped)
                livingEntityDataBox.nav.isStopped = true;
            
            return;
        }

        livingEntityDataBox.nav.nextPosition = livingEntityDataBox.rigid.position;

        if(livingEntityDataBox.damageComp.IsDie || livingEntityDataBox.damageComp.IsHit)
        {
            Undo();
            return;
        }

        mainSelecter.Execute();
    }

    protected override void Undo()
    {
        if(mainSelecter != null) mainSelecter.Undo(true);
    }
}

//Cull
public partial class Enemy
{
    private float deadTime = -999f, deadRespawnTime = 120f;

    protected override void OnDie()
    {
        deadTime = Time.time;

        DropItem();
    }

    private void DropItem()
    {
        EnemyDropItemInfo[] itemList = livingEntityDataBox.enemyData.dropItemList.itemList;

        for(int i = 0; i < itemList.Length; i++)
        {
            EnemyDropItemInfo itemInfo = itemList[i];

            ItemObject itemObj = EventBus.Invoke_Func<int, int, ItemObject>("Pool_GetItemObject", itemInfo.dropItemId, itemInfo.dropItemAmount);
            itemObj.MoveStart(this.transform.position);
        }
    }

    public override void ToggleOn()
    {
        if(Time.time - deadTime < deadRespawnTime) return;
        
        updateEnabled = true;
        this.gameObject.SetActive(true);
    }

    public override void DisableUpdate()
    {
        updateEnabled = false;
    }

    public override void ToggleOff()
    {
        EventBus.Invoke<MonoBehaviour, bool>("EnemyUnDetect", this, true);
        this.gameObject.SetActive(false);
        updateEnabled = false;
    }
}