using UnityEngine;
using UnityEngine.Rendering;

public class MinimapCamera : MonoBehaviour
{
    private Transform cameraTarget;
    private Camera minimapCam;
    private void OnEnable()
    {
        minimapCam = GetComponent<Camera>();

        EventBus.Sub<bool>("UseMinimap", UseMinimap);
        EventBus.Sub<Transform>("SetCharacterTransform", SetPinTarget);
        
        RenderPipelineManager.beginCameraRendering += SetFogDisable;
    }

    private void OnDisable()
    {
        EventBus.UnSub<bool>("UseMinimap", UseMinimap);
        EventBus.UnSub<Transform>("SetCharacterTransform", SetPinTarget);

        RenderPipelineManager.beginCameraRendering -= SetFogDisable;
    }

    private void UseMinimap(bool isActive)
    {
        minimapCam.enabled = isActive;
    }

    private void SetPinTarget(Transform cameraTarget)
    {
        this.cameraTarget = cameraTarget;
    }
    
    private void LateUpdate()
    {
        if(cameraTarget == null) return;
        transform.position = new Vector3(cameraTarget.position.x, transform.position.y, cameraTarget.position.z);
    }

    private void SetFogDisable(ScriptableRenderContext context, Camera camera)
    {
        if(camera == minimapCam)
            RenderSettings.fog = false;
        else
            RenderSettings.fog = true;
    }   
}
