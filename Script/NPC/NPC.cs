using UnityEngine;

public abstract class NPCBase : MonoBehaviour, IInteract
{
    public int npcId;
    protected NPCData npcData;
    private Coroutine routine;
    private string targetTag = "Character";
    private bool isInit = false;

    protected void OnEnable()
    {
        if(npcData == null)
        {
            routine = this.RunRoutine(StartLoop(), routine);
        }
    }
    System.Collections.IEnumerator StartLoop()
    {
        while (!LoadStatus.IsReady)
            yield return null;

        npcData = DataLoader.GetData<NPCData>(DataType.NPC, npcId);

        isInit = true;
    }

    private void OnTriggerEnter(Collider col)
    {
        if(!isInit || !PassTrigger()) return;

        if(col.CompareTag(targetTag))
        {
            if (npcData == null) return;

            EventBus.Invoke<(int, bool)>("OnInteractNpc", (npcData.npcId, true));
        }    
    }

    private void OnTriggerExit(Collider col)
    {
        if(!isInit || !PassTrigger()) return;

        if(col.CompareTag(targetTag))
        {
            if (npcData == null) return;

            EventBus.Invoke<(int, bool)>("OnInteractNpc", (npcData.npcId, false));
        }
    }

    public abstract void Interact();
    public virtual bool PassTrigger() => true;
}
