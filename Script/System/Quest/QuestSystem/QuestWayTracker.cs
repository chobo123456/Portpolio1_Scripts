using System.Collections;
using UnityEngine;

public struct QuestWayTrackerPayload
{
    public bool isTrackable;
    public Vector3 trackPoint;
}

public class QuestWayTracker : MonoBehaviour
{
    private bool isMarkable = false;
    private Transform character;
    private Vector3 questPoint;

    private float interval = 0.2f, intervalTimer = 0f;

    void OnEnable()
    {
        SubscribeEvent(true);
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<QuestWayTrackerPayload>("Quest_WayTracker_StartTrack", OnTrackStart);
            EventBus.Sub<Transform>("SetCharacterTransform", SetCharacterTr);
        }
        else
        {
            EventBus.UnSub<QuestWayTrackerPayload>("Quest_WayTracker_StartTrack", OnTrackStart);
            EventBus.UnSub<Transform>("SetCharacterTransform", SetCharacterTr);
        }
    }

    private void OnDisable()
    {
        SubscribeEvent(false);
    }

    void Update()
    {
        if (isMarkable)
        {
            if (intervalTimer > interval)
            {
                intervalTimer = 0f;

                if (IsNearQuestPoint())
                {
                    isMarkable = false;
                    EventBus.Invoke<QuestType, int>("QuestManager_OnAskQuestFinish", QuestType.WayPoint, 0);
                }
            }

            intervalTimer += Time.deltaTime;
        }
    }

    private bool IsNearQuestPoint()
    {
        float distanceSq = (questPoint - character.position).sqrMagnitude;

        if (distanceSq <= 12f)
            return true;

        return false;
    }

    private void SetCharacterTr(Transform character)
    {
        this.character = character;
    }

    private void OnTrackStart(QuestWayTrackerPayload payload)
    {
        if (!payload.isTrackable)
        {
            isMarkable = false;
            return;
        } 

        isMarkable = true;
        questPoint = payload.trackPoint;
    }
}
