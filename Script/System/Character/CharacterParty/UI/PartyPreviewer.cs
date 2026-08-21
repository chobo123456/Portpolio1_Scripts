using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public struct PartyPreviewerPayload
{
    public int index;
    public bool isDisable;
    public GameObject previewModelPrefab;
}

public class PartyPreviewer : MonoBehaviour
{
    private Dictionary<int, Transform> slotBarTr = new();
    private GameObject currentCamera;
    private void OnEnable()
    {
        Transform findTr = transform.FindTarget("CharacterMeshs");
        for(int i = 0; i < findTr.childCount; i++)
        {
            Transform targetSlotTr = findTr.Find($"CharacterMesh_{i + 1}");
            slotBarTr.Add(i, targetSlotTr);
        }

        EventBus.Sub<PartyPreviewerPayload>("Party_UI_UpdatePreview", OnChangeUI);
        EventBus.Sub<bool>("SetPartyCam", ToggleCamera);

        if (currentCamera == null)
        {
            currentCamera = transform.Find("PartyCam").gameObject;
        }

        ToggleCamera(false);
    }

    private void OnDestroy()
    {
        EventBus.UnSub<PartyPreviewerPayload>("Party_UI_UpdatePreview", OnChangeUI);
        EventBus.UnSub<bool>("SetPartyCam", ToggleCamera);
    }

    private void ToggleCamera(bool active)
    {
        currentCamera.SetActive(active);
    }

    private void OnChangeUI(PartyPreviewerPayload payload)
    {
        if(slotBarTr.TryGetValue(payload.index, out Transform parentTr))
        {
            if (payload.isDisable)
            {
                ClearRecentObjects(parentTr, payload.index);
                return;
            }
            
            ClearRecentObjects(parentTr, payload.index);
            SettingObject(parentTr, payload.previewModelPrefab);
        }
    }

    private void SettingObject(Transform parentTr, GameObject previewPrefab)
    {
        GameObject newObj = UnityEngine.Object.Instantiate(previewPrefab);
        newObj.transform.SetParent(parentTr);

        newObj.SetActive(false);
        
        newObj.transform.position = parentTr.position;
        newObj.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        newObj.SetActive(true);
    }
    private void ClearRecentObjects(Transform parentTr, int index)
    {
        int childCount = parentTr.childCount;

        if(childCount > 0)
        {
            for(int i = 0; i < childCount; i++)
            {
                GameObject obj = parentTr.GetChild(i).gameObject;

                UnityEngine.Object.Destroy(obj);
            }
        }
    }
}
