using UnityEngine;

public class RotationManager
{
    private readonly PlayerDataBox _box;
    public bool RotateLock {get; set;}
    
    private bool _isAttackCall = false, _isDebugMode = false;
    private Vector3 _curDirection;
    private float _currentDampSpeed = 0f, _desiredDampSpeed = 0.114f;

    public RotationManager(PlayerDataBox box, bool isDebugMode = false)
    {
        _isDebugMode = isDebugMode;
        _box = box;
    }

    public void OnAttackStartRotate()
    {
        Transform target = _box.sensor.LookTarget;
        
        if(target != null)
            _curDirection = (target.position - _box.rigid.transform.position).normalized;

        RotateLock = true;
        _isAttackCall = true;
    }

    public void OnAttackEndRotate()
    {
        _isAttackCall = false;
        RotateLock = false;
    }

    public void UpdateRotate()
    {
        if (!LoadStatus.IsReady) return;

        if(!_isDebugMode && !GameState.IsActive()) return;

        if(_isAttackCall)
        {
            if(_box.sensor.LookTarget == null) return;
            RotateForceCase();
        }

        if(RotateLock) return;
        
        Vector3 dir = _box.input.GetMoveInput();

        if (dir.sqrMagnitude < 0.01f) return;

        Vector3 moveDirection = CalculateDir(dir);

        float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.SmoothDampAngle(
            _box.rigid.transform.rotation.eulerAngles.y,
            targetAngle,
            ref _currentDampSpeed,
            _desiredDampSpeed
        );
        
        Quaternion rotate = Quaternion.Euler(0f, smoothAngle, 0f);

        _box.rigid.angularVelocity = Vector3.zero;
        _box.rigid.MoveRotation(rotate);
    }

    private void RotateForceCase()
    {
        Vector3 ownerToTarget = _box.sensor.LookTarget.position - _box.rigid.position;
        float distance = ownerToTarget.magnitude;
        
        if(distance <= _box.stat.StatData.autoTargeting.priximityRange)
        {
            Vector3 direction = _curDirection;
            direction.y = 0f;
            Quaternion attackTargetRotate = Quaternion.LookRotation(direction);

            _box.rigid.MoveRotation(attackTargetRotate);
        }
    }

    private Vector3 CalculateDir(Vector3 moveInput)
    {
        moveInput.Normalize();

        Vector3 cameraRight = Camera.main.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 moveDir = (cameraRight * moveInput.x) + (cameraForward * moveInput.z);
        moveDir.Normalize();

        return moveDir;
    }
}
