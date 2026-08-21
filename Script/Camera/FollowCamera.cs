using System.Collections;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    
    [Header("Target")]
    public Transform target;

    [Header("DampSpeed")]
    public float dampSpeed = 0.15f;

    [Header("Offset Y")]
    public float offSetY = 0.25f;

    [Header("LerpSpeed")]
    public float lerpSpeed = 10f;

    [Header("DebugMode")]
    public bool isDebugMode = false;
    
    private Vector3 currentVel = Vector3.zero;
    private bool lockMove = false;

    private AnimationCurve cameraShakeCurve;

    private void OnEnable()
    {
        EventBus.Sub("ResumeCamera", Resume);
        EventBus.Sub("StopCamera", Stop);
        EventBus.Sub("CameraUpdatePosition", UpdatePosition);
        EventBus.Sub<Transform>("SetCharacterTransform", SetTarget);
        EventBus.Sub<(float, float, float)>("CameraShake", CameraShake);
        EventBus.Sub<float>("FollowCamera_SetDamp", SetDamp);
    }

    private void OnDisable()
    {
        EventBus.UnSub("ResumeCamera", Resume);
        EventBus.UnSub("StopCamera", Stop);
        EventBus.UnSub("CameraUpdatePosition", UpdatePosition);
        EventBus.UnSub<Transform>("SetCharacterTransform", SetTarget);
        EventBus.UnSub<(float, float, float)>("CameraShake", CameraShake);
        EventBus.UnSub<float>("FollowCamera_SetDamp", SetDamp);
    }

    private void Resume()
    {
        lockMove = false;
    }
    private void Stop()
    {
        lockMove = true;
    }

    private void SetDamp(float value)
    {
        dampSpeed = value;
    }
    private void SetTarget(Transform newTarget)
    {
        target = newTarget;

        UpdatePosition();
    }

    private void LateUpdate()
    {
        if(target == null || lockMove) return;

        if(!isDebugMode && !GameState.IsActive()) return;
            
        Move();
    }

    private void Move()
    {
        Vector3 calculate = new Vector3(target.position.x, target.position.y + offSetY, target.position.z);

        transform.position = Vector3.SmoothDamp(
            transform.position, 
            calculate, 
            ref currentVel, 
            dampSpeed, 
            Mathf.Infinity, 
            Time.unscaledDeltaTime);
    }

    private void CameraShake((float frequency, float amplitude, float duringTime) tuple)
    {
        lockMove = true;

        this.RunRoutine(ShakeLoop(tuple.frequency, tuple.amplitude, tuple.duringTime), "FollowCamera_CameraShake");
    }

    IEnumerator ShakeLoop(float frequency, float amplitude, float duringTime = 0.05f)
    {
        if(cameraShakeCurve == null) 
            cameraShakeCurve = DataLoader.GetData<AnimationCurve>(DataType.AnimationCurve, 10000);

        float currentTime = 0f;

        while(currentTime < duringTime)
        {
            currentTime += Time.unscaledDeltaTime;
            float speedMul = cameraShakeCurve.Evaluate(currentTime / duringTime);

            float finalAmplitude = amplitude * speedMul;
            Vector3 randomPos = transform.position + new Vector3(Random.Range(-amplitude, amplitude), Random.Range(-amplitude, amplitude), Random.Range(-amplitude, amplitude));

            transform.position = randomPos;

            yield return YieldUtil.WaitForSecondsRealtime(frequency);
        }

        lockMove = false;
    }

    private void UpdatePosition()
    {
        if(target == null) return;

        Vector3 calculate = new Vector3(target.position.x, target.position.y + offSetY, target.position.z);
        transform.position = calculate;
    }
}
