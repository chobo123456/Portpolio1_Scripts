using UnityEngine;
using System.Collections;

public struct QuestMinimapPointPayload
{
    public bool isDrawable;
    public Vector3 questPoint;
}

public class MinimapQuestPoint : MonoBehaviour
{
    public Transform minimapQuestPin;
    private void OnEnable()
    {
        EventBus.Sub<QuestMinimapPointPayload>("Quest_Minimap_Pointing", Pointing);
    }

    private void OnDisable()
    {
        EventBus.UnSub<QuestMinimapPointPayload>("Quest_Minimap_Pointing", Pointing);
    }

    private void Pointing(QuestMinimapPointPayload payload)
    {   
        if(!payload.isDrawable)
        {
            minimapQuestPin.gameObject.SetActive(false);
            return;
        } 

        minimapQuestPin.gameObject.SetActive(true);
        minimapQuestPin.position = payload.questPoint + new Vector3(0f, 1f, 0f);
    }
}
