using UnityEngine;

public class FollowCameraRotate : MonoBehaviour
{
    [Header("Sensitivity")]
    public float mouseSensitivity = 0.5f;

    [Header("Limit")]
    public float upLimit = 30f;
    public float downLimit = -180;

    [Header("LerpSpeed")]
    public float rotateLerpSpeed = 7.5f;
    public float focusRotateLerpSpeed = 3.5f;

    [Header("LerpSpeed")]
    public bool isDebugMode = false;

    private string inputX = "Mouse X", inputY = "Mouse Y";
    private float rotationX, rotationY;

    private bool isReady = false, rotateLock = false, isFindingEnemy = false;
    private Transform enemyTr;
    
    private void Awake()
    {
        EventBus.Sub("ResumeCamera", Resume);
        EventBus.Sub("StopCamera", Stop);
        EventBus.Sub<Transform>("SetCharacterTransform", InitializeRotate);
        EventBus.Sub<Transform>("SetCameraForceRotate", StartFocus);
        EventBus.Sub<bool>("SetCameraRotateLock", LockCameraRotate);
        EventBus.Sub<float>("SetCameraSensitivity", SetSensitivity);
    }

    private void OnDestroy()
    {
        EventBus.UnSub("ResumeCamera", Resume);
        EventBus.UnSub("StopCamera", Stop);
        EventBus.UnSub<Transform>("SetCharacterTransform", InitializeRotate);
        EventBus.UnSub<Transform>("SetCameraForceRotate", StartFocus);
        EventBus.UnSub<bool>("SetCameraRotateLock", LockCameraRotate);
        EventBus.UnSub<float>("SetCameraSensitivity", SetSensitivity);
    }

    private void Resume()
    {
        rotateLock = false;
    }

    private void Stop()
    {
        rotateLock = true;
    }


    private void InitializeRotate(Transform targetTr)
    {
        if(isReady) return;

        rotationX = targetTr.transform.eulerAngles.x;
        rotationY = targetTr.transform.eulerAngles.y;

        rotationX = Mathf.Clamp(rotationX, upLimit, downLimit);

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);

        isReady = true;
    }

    private void LockCameraRotate(bool active)
    {
        rotateLock = active;
    }
    private void SetSensitivity(float value)
    {
        mouseSensitivity = value;
    }

    private void LateUpdate()
    {
        if(!isReady || Time.timeScale == 0 || rotateLock) return;

        if(!isDebugMode && !GameState.IsActive()) return;

        if(isFindingEnemy)
        {
            OnFocus();
            return;
        }

        Rotate();
    }

    private void StartFocus(Transform targetTr)
    {
        if(isFindingEnemy) return;
    
        enemyTr = targetTr;
        isFindingEnemy = true;
    }

    private void OnFocus()
    {
        if(enemyTr == null)
        {
            isFindingEnemy = false;
            return;
        }

        Vector3 direction = (enemyTr.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Vector3 angles = targetRotation.eulerAngles;

        Vector3 viewPos = Camera.main.WorldToViewportPoint(enemyTr.position);

        bool isVisible = viewPos.z > 0 && 
            viewPos.x < 0.9f && 
            viewPos.x > 0.1f;

        if (isVisible)
        {
            isFindingEnemy = false; 
            return;
        }

        Quaternion lerped = Quaternion.Slerp(transform.rotation, Quaternion.Euler(rotationX, angles.y, 0f), Time.deltaTime * focusRotateLerpSpeed);
        rotationY = lerped.eulerAngles.y;
        transform.rotation = lerped;
    }

    private void Rotate()
    {
        float mouseX = Input.GetAxis(inputX) * mouseSensitivity;
        float mouseY = Input.GetAxis(inputY) * mouseSensitivity;

        rotationX -= mouseY;
        rotationY += mouseX;

        rotationX = Mathf.Clamp(rotationX, upLimit, downLimit);
        
        Quaternion lerped = Quaternion.Slerp(transform.rotation, Quaternion.Euler(rotationX, rotationY, 0f), Time.deltaTime * rotateLerpSpeed);

        transform.rotation = lerped;
    }
}
