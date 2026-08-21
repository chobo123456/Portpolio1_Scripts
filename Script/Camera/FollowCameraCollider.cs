using UnityEngine;

public enum CameraCase
{
    None,
    Boss,
}

public class FollowCameraCollider : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Layer")]
    public LayerMask targetLayer;

    [Header("Away Distance")]
    public float awayDistance = 4f;
    public float minDistance = 0.2f;

    [Header("Zoom Distance")]

    public float zoom_minDistance = 2.5f;
    public float zoom_maxDistance = 4f;

    [Header("LerpSpeed")]
    public float unDetectSpeed = 7.5f;
    public float detectSpeed = 100f;

    [Header("Radius")]
    public float radius = 0.05f;

    private float currentDistance, originZoom_Min, originZoom_Max, originAwayDis;

    private bool isDetectWall = false, isLock = false;

    private string wheel = "Wheel";
    private void OnEnable()
    {
        currentDistance = awayDistance;

        originZoom_Min = zoom_minDistance;
        originZoom_Max = zoom_maxDistance;
        originAwayDis = unDetectSpeed;

        EventBus.Sub("ResumeCamera", Resume);
        EventBus.Sub("StopCamera", Stop);
        EventBus.Sub<CameraCase>("SetCameraCase", ChangeCase);
    }

    private void OnDisable()
    {
        EventBus.UnSub("ResumeCamera", Resume);
        EventBus.UnSub("StopCamera", Stop);
        EventBus.UnSub<CameraCase>("SetCameraCase", ChangeCase);
    }

    private void Resume()
    {
        isLock = false;
    }

    private void Stop()
    {
        isLock = true;
    }

    private void LateUpdate()
    {
        if(isLock) return;

        Wheel();
        CheckWall();   
    }

    private void ChangeCase(CameraCase cameraCase)
    {
        switch(cameraCase)
        {
            case CameraCase.None:
                zoom_minDistance = originZoom_Min;
                zoom_maxDistance = originZoom_Max;
                awayDistance = originAwayDis;
                break;

            case CameraCase.Boss:
                zoom_minDistance = 3.8f;
                zoom_maxDistance = 8f;
                awayDistance = 8f;
                currentDistance = 8f;
                break;
        }
    }

    private void Wheel()
    {
        if(Mathf.Abs(Input.GetAxis(wheel)) <= 0f || isDetectWall) return;

        float input = Input.GetAxis(wheel);

        float calculateZoomAmount = awayDistance - input;

        awayDistance = Mathf.Clamp(calculateZoomAmount, zoom_minDistance, zoom_maxDistance);
    }

    private void CheckWall()
    {
        Vector3 desiredPos = target.position - (transform.rotation * Vector3.forward * awayDistance);
        //원하는 좌표 = 타겟 좌표(플레이어) - 현재 회전값 * 앞방향 * 거리 

        DetectWall(target.position, desiredPos);

        Vector3 finalCalculatePos = target.position - (transform.rotation * Vector3.forward * currentDistance);

        //최종 좌표 = 타겟 좌표(플레이어) - 현재 회전값 * 앞방향 * 부딫힌 위치
        //"타겟 좌표 - 현재 내가 보는곳에서 뒤로 부딫힌위치로 이동"

        //감지된거 안된거에따라 보간 속도를 결정
        float lerpSpeed = isDetectWall ? detectSpeed : unDetectSpeed;
        transform.position = Vector3.Lerp(transform.position, finalCalculatePos, Time.unscaledDeltaTime * lerpSpeed);
    }

    private void DetectWall(Vector3 targetPos, Vector3 desiredPos)
    {
        //방향 = (원하는 위치 - 타겟 좌표).정규화 (플레이어로 부터 뒤방향으로)
        Vector3 direction = (desiredPos - targetPos).normalized;

        //타겟 좌표로부터 원하는방향의 10퍼센트 간위치 (플레이어 위치에서 시작 X, 10퍼센트 떨어진 거리에서 시작 O)
        Vector3 rayStartPoint = targetPos - direction * 0.1f;

        //rayStartPoint에서 시작함, 플레이어쪽에서 뒤로 이동 최대거리 + 0.1만큼 움직임
        if(Physics.SphereCast(rayStartPoint, radius, direction, out RaycastHit hit, awayDistance, targetLayer))
        {
            //감지가 되었으면 찍힌거리에서 0.5 떨어진거리를 구하고 거기서 최소 최대 거리를 구함
            currentDistance = Mathf.Clamp(hit.distance - 0.5f, minDistance, awayDistance);

            //감지가 안되었으면 빠르게 또는 부드럽게 처리를 해야하기에 놓은 변수
            isDetectWall = true;
        }
        else
        {
            //감지가 안되었으면 차츰차츰 값이 커지도록 유도해서 확 변하는일없게 설정
            currentDistance = Mathf.Lerp(currentDistance, awayDistance, Time.unscaledDeltaTime * 4.5f);
            isDetectWall = false;
        }
    }
}
