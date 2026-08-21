using UnityEngine;

public class CutSceneCam : MonoBehaviour
{
    private Camera cam;

    private void OnEnable()
    {
        cam = GetComponent<Camera>();
        cam.enabled = false;
        
        EventBus.Sub("EnableCutSceneCam", Enable);
        EventBus.Sub("DisableCutSceneCam", Disable);
        
    }
    
    private void OnDisable()
    {
        EventBus.UnSub("EnableCutSceneCam", Enable);
        EventBus.UnSub("DisableCutSceneCam", Disable);
    }

    private void Enable()
    {
        cam.enabled = true;
    }

    private void Disable()
    {
        cam.enabled = false;
    }
}
