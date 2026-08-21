using System.Collections.Generic;
using UnityEngine;

public class StateCheckNode : TaskNode<NormalEnemyDataBox>
{
    private EnemyAct targetAct;
    public StateCheckNode(NormalEnemyDataBox box, EnemyAct act) : base(box)
    {
        targetAct = act;
    }
    
    protected override EnemyState OnUpdate()
    {
        if(box.act == targetAct)
            return EnemyState.Success;

        return EnemyState.Fail;
    }
}

public class StateSetNode : TaskNode<NormalEnemyDataBox>
{
    private EnemyAct targetAct;
    public StateSetNode(NormalEnemyDataBox box, EnemyAct act) : base(box)
    {
        targetAct = act;
    }
    
    protected override void OnEnter()
    {
        box.act = targetAct;
    }

    protected override EnemyState OnUpdate()
    {
        return EnemyState.Success;
    }
}


public class StandNode : TaskNode<EnemyEntityDataBox>
{
    private readonly LayerMask layer;
    private readonly float standTime;
    private float standTimer;
    public StandNode(EnemyEntityDataBox box, float standTime = 2f) : base(box)
    {
        if(!box.enemyAnimationStorage.IsExist(EnemyAnimationType.Move))
            box.enemyAnimationStorage.Add(EnemyAnimationType.Move, new EnemyMoveAnimation(box));

        this.standTime = standTime;
        layer     = LayerMask.GetMask("Ground");
    }

    protected override void OnEnter()
    {
        standTimer = 0f;
    }

    protected override EnemyState OnUpdate()
    {  
        standTimer += Time.deltaTime;

        var moveAnim = box.enemyAnimationStorage.Get<EnemyMoveAnimation>(EnemyAnimationType.Move);
            
        moveAnim.SetMoveVelocity(0, 0);
        Stand();
        
        if(standTimer <= standTime)
            return EnemyState.Running;

        return EnemyState.Success;
    }

    private void Stand()
    {
        Vector3 startPos = box.rigid.transform.TransformPoint(box.col.center + Vector3.down * (box.col.height * 0.5f));
        
        Vector3 curVel = box.rigid.linearVelocity;
        curVel *= 0f;

        if(Physics.Raycast(startPos, Vector3.down, out var hit, 5f, layer))
        {
            float angle = Vector3.Angle(Vector3.down, hit.normal);

            if(angle > 0.1f && angle <= 45f)
            {
                curVel.y = -Physics.gravity.y * Time.deltaTime;
                box.rigid.linearVelocity = curVel;
                return;
            }
            else
            {
                curVel.y = Physics.gravity.y * Time.deltaTime;
                box.rigid.linearVelocity = curVel;
                return;
            }
        }
        else
        {
            curVel.y = box.rigid.linearVelocity.y;
            box.rigid.linearVelocity = curVel;

            return;
        }
    }

    public override void Undo(bool isRootUndo = false)
    {
        standTimer = 0f;
    }
}


public class MoveNode : TaskNode<EnemyEntityDataBox>
{
    private readonly LayerMask layer;
    private float moveSpeed = 3f;
    private float animMin, animMax;
    public MoveNode(EnemyEntityDataBox box, float moveSpeed, float animationMin, float animationMax) : base(box)
    {
        this.moveSpeed = moveSpeed;

        animMin = animationMin;
        animMax = animationMax;

        if(!box.enemyAnimationStorage.IsExist(EnemyAnimationType.Move))
            box.enemyAnimationStorage.Add(EnemyAnimationType.Move, new EnemyMoveAnimation(box));

        layer     = LayerMask.GetMask("Ground");
    }

    protected override EnemyState OnUpdate()
    {  
        Move();
        box.enemyAnimationStorage.Get<EnemyMoveAnimation>(EnemyAnimationType.Move).SetMoveVelocity(animMin, animMax);
        
        return EnemyState.Success;
    }

    private void Move()
    {
        Vector3 targetVector = box.nav.desiredVelocity;

        Vector3 startPos = box.rigid.transform.TransformPoint(box.col.center + (Vector3.down * (box.col.height * 0.5f)));
        Vector3 moveTo = Vector3.zero;

        if(Physics.Raycast(startPos, Vector3.down, out var hit, 2f, layer))
        {
            Vector3 projectVector = Vector3.ProjectOnPlane(targetVector, hit.normal);
            moveTo = projectVector.normalized * moveSpeed;
        }
        else
            moveTo = targetVector.normalized * moveSpeed;

        box.rigid.linearVelocity = Vector3.Lerp(box.rigid.linearVelocity, moveTo, Time.fixedDeltaTime * 8f);
    }

    public override void Undo(bool isRootUndo = false)
    {
        base.Undo(isRootUndo);

        if (isRootUndo)
        {
            if(!box.rigid.isKinematic) box.rigid.linearVelocity = Vector3.zero;
            box.enemyAnimationStorage.Get<EnemyMoveAnimation>(EnemyAnimationType.Move).SetMoveVelocity(0f, 0f);
        }
    }
}


public class MoveBackNode : TaskNode<EnemyEntityDataBox>
{
    private float moveSpeed, backDistance = 5f, animClampMin, animClampMax;
    private LayerMask groundLayer;
    public MoveBackNode(EnemyEntityDataBox box, float speed, float backDistance = 5f, float animClampMin = -1f, float animClampMax = 1f) : base(box)
    {
        this.moveSpeed = speed;
        this.backDistance = backDistance;
        this.animClampMin = animClampMin;
        this.animClampMax = animClampMax;

        groundLayer = LayerMask.GetMask("Ground");
    }

    protected override EnemyState OnUpdate()
    {  
        Vector3 target = box.nav.destination;

        float distance = (target - box.rigid.transform.position).magnitude;
        
        if(distance > backDistance)
            return EnemyState.Success;

        Move();
        box.enemyAnimationStorage.Get<EnemyMoveAnimation>(EnemyAnimationType.Move).SetMoveVelocity(animClampMin, animClampMax);
        
        return EnemyState.Running;
    }

    private void Move()
    {
        Vector3 targetTo = box.rigid.transform.position - box.nav.destination;

        Vector3 startPos = box.rigid.transform.TransformPoint(box.col.center + (Vector3.down * (box.col.height * 0.5f)));
        Vector3 moveTo = Vector3.zero;

        if(Physics.Raycast(startPos, Vector3.down, out var hit, 2f, groundLayer))
        {
            Vector3 projectVector = Vector3.ProjectOnPlane(targetTo, hit.normal);
            moveTo = projectVector.normalized * moveSpeed;
        }
        else
            moveTo = targetTo.normalized * moveSpeed;

        box.rigid.linearVelocity = Vector3.Lerp(box.rigid.linearVelocity, moveTo, Time.fixedDeltaTime * 8f);
    }
}


public class PatrolPointSettingNode : TaskNode<EnemyEntityDataBox>
{
    private Vector3[] patrolPoints;
    private Vector3 currentGoal;
    private int index;
    private float stopDistance = 4f;
    public PatrolPointSettingNode(EnemyEntityDataBox box, Vector3[] patrolPoints, float stopDistance) : base(box)
    {
        this.patrolPoints = patrolPoints;

        this.stopDistance = stopDistance;
         
        if(box.nav.updatePosition) box.nav.updatePosition = false;
        if(box.nav.updateRotation) box.nav.updateRotation = false;

        currentGoal = patrolPoints[1];

        box.nav.SetDestination(currentGoal);
    }

    protected override void OnEnter()
    {
        if(box.nav.destination != currentGoal) 
        {
            EventBus.Invoke<MonoBehaviour, bool>("EnemyUnDetect", box.mono, true);
            box.nav.SetDestination(currentGoal);
        }

        if(IsNearGoal())
        {
            index = (index + 1) % patrolPoints.Length;
            currentGoal = patrolPoints[index];

            box.nav.SetDestination(currentGoal);
        }   
    }

    protected override EnemyState OnUpdate()
    {
        return EnemyState.Success;
    }

    private bool IsNearGoal()
    {
        float distance = (currentGoal - box.rigid.transform.position).sqrMagnitude;

        if(distance <= stopDistance) return true;

        return false;
    }
}


public class SearchNode : TaskNode<EnemyEntityDataBox>
{
    private readonly LayerMask layer;
    private bool invert = false;
    private float fovAngle = 120f, checkFovAngle;
    public SearchNode(EnemyEntityDataBox box, bool isInvert = false) : base(box)
    {
        layer = LayerMask.GetMask("Character");
        this.invert = isInvert;

        if(box.nav.updatePosition) box.nav.updatePosition = false;
        if(box.nav.updateRotation) box.nav.updateRotation = false;

        checkFovAngle = Mathf.Cos((fovAngle / 2) * Mathf.Deg2Rad);
    }

    protected override EnemyState OnUpdate()
    {
        Vector3 result = SearchEnemy();
        bool isDetect = result != Vector3.zero;

        if(isDetect)
        {
            EventBus.Invoke<MonoBehaviour, int, bool>("EnemyDetect", box.mono, box.livingEntityId, true);
            box.nav.SetDestination(result);
            return invert ? EnemyState.Fail : EnemyState.Success; 
        }
        else
        {
            EventBus.Invoke<MonoBehaviour, bool>("EnemyUnDetect", box.mono, true);
            return invert ? EnemyState.Success : EnemyState.Fail; 
        }
    }

    private Vector3 SearchEnemy()
    {
        Vector3 target = Vector3.zero;

        Vector3 startPos = box.rigid.transform.TransformPoint(box.col.center);
        Collider[] cols = Physics.OverlapSphere(startPos, box.enemyData.chaseDetectRadius, layer);
        
        if(cols != null && cols.Length > 0)
            target = cols[0].transform.position;

        if(target != Vector3.zero)
        {
            Vector3 targetTo = target - box.rigid.position;
            
            float angle = Vector3.Dot(box.rigid.transform.forward, targetTo.normalized);
            
            if(angle >= checkFovAngle)
                return target;
            else if(targetTo.magnitude <= 3f)
                return target;
        }

        return target;
    }
}


public class StartleNode : TaskNode<EnemyEntityDataBox>
{
    private bool isDetect = false;
    private readonly float triggerDistance;
    private float detectTime = 0f, resetAbleTime = 10f;

    public StartleNode(EnemyEntityDataBox box, float triggerDistance = 10f) : base(box)
    {   
        this.triggerDistance = triggerDistance;
    }

    protected override EnemyState OnUpdate()
    {
        float distance = (box.nav.destination - box.col.transform.position).magnitude;
        
        if(isDetect && distance > triggerDistance + 5f)
        {
            if(Time.time - detectTime >= resetAbleTime)
                isDetect = false;
        }

        if(!isDetect && distance <= triggerDistance)
        {
            isDetect = true;
            detectTime = Time.time;
            return EnemyState.Success;
        }

        return EnemyState.Fail;
    }
}


public class AttackNode : TaskNode<EnemyEntityDataBox>
{
    public AttackNode(EnemyEntityDataBox box) : base(box)
    {
        box.enemyAnimationStorage.Add(EnemyAnimationType.Attack, new EnemyAttackAnimation(box));
    }

    protected override void OnEnter()
    {
        var attackAnim = box.enemyAnimationStorage.Get(EnemyAnimationType.Attack);
        attackAnim.OnActAnimEnter();
    }

    protected override EnemyState OnUpdate()
    {
        var attackAnim = box.enemyAnimationStorage.Get(EnemyAnimationType.Attack);
        return attackAnim.IsEnd() ? EnemyState.Success : EnemyState.Running;
    }

    protected override void OnExit()
    {
        var attackAnim = box.enemyAnimationStorage.Get(EnemyAnimationType.Attack);
        attackAnim.OnActAnimExit();
    }
}


public class IndicatorNode : TaskNode<EnemyEntityDataBox>
{
    private float indicatorDuringTime = 0.2f, indicatorActiveTime = 0f;
    private int indicatorId;
    private Transform indicatorTr;
    private GameObject currentIndicator;
    public IndicatorNode(
        EnemyEntityDataBox box, 
        float duration = 0f, 
        float indicatorDuringTime = 0.2f, 
        int indicatorId = 1000000) : base(box, duration)
    {
        this.indicatorDuringTime    = indicatorDuringTime;
        this.indicatorId            = indicatorId;

        indicatorTr = box.col.transform.Find("IndicatorTransform");

        _ = EventBus.Invoke_Func<int, GameObject>("Pool_GetGameObject", indicatorId);
    }

    protected override void OnEnter()
    {
        currentIndicator = EventBus.Invoke_Func<int, GameObject>("Pool_GetGameObject", indicatorId);
        currentIndicator.transform.rotation = indicatorTr.rotation;
        currentIndicator.transform.position = indicatorTr.position;
        currentIndicator.SetActive(true);

        indicatorActiveTime = Time.time;
    }

    protected override EnemyState OnUpdate()
    {  
        return Time.time - indicatorActiveTime >= indicatorDuringTime ? EnemyState.Success : EnemyState.Running;
    }

    protected override void OnExit()
    {
        currentIndicator.SetActive(false);
    }
}


public class DashNode : TaskNode<EnemyEntityDataBox>
{
    public DashNode(EnemyEntityDataBox box, float duration = 0f) : base(box, duration)
    {
        if(!box.enemyAnimationStorage.IsExist(EnemyAnimationType.Dash))
            box.enemyAnimationStorage.Add(EnemyAnimationType.Dash, new EnemyDashAnimation(box));
    }

    protected override void OnEnter()
    {
        var dashAnim = box.enemyAnimationStorage.Get(EnemyAnimationType.Dash);
        dashAnim.OnActAnimEnter();
    }

    protected override EnemyState OnUpdate()
    {  
        var dashAnim = box.enemyAnimationStorage.Get(EnemyAnimationType.Dash);
        return dashAnim.IsEnd() ? EnemyState.Success : EnemyState.Running;
    }
}


public class TeleportNode : TaskNode<EnemyEntityDataBox>
{
    private Vector3 teleportPos;
    private bool hasMoved = false;
    private PoissonDiskSamplingVector poissonDiskSampling;
    public TeleportNode(EnemyEntityDataBox box, float duration = 0f) : base(box, duration)
    {
        poissonDiskSampling = new(5f);
    }

    protected override void OnEnter()
    {
        Vector3 centerPos  = box.nav.destination;
        teleportPos = poissonDiskSampling.GetRandomRange(centerPos) + (Vector3.up * 0.2f);

        hasMoved = false;
    }

    protected override EnemyState OnUpdate()
    {
        if(!hasMoved)
        {
            box.rigid.MovePosition(teleportPos);
            box.nav.Warp(box.rigid.position);
            hasMoved = true;

            return EnemyState.Running;
        }

        return EnemyState.Success;
    }
}


public class ExecuteInPlaceClone : TaskNode<EnemyEntityDataBox>
{
    private readonly ICloneAble cloneClass;
    public ExecuteInPlaceClone(EnemyEntityDataBox box, float duration = 0f) : base(box, duration)
    {
        cloneClass = box as ICloneAble;
    }
    protected override void OnEnter()
    {
        for(int i = 0; i < cloneClass.clones.Count; i++)
        {
            var clone = cloneClass.clones[i];

            clone.SetCommand(CloneCommand.InPlace);
            clone.Execute();
        }
    }

    protected override EnemyState OnUpdate()
    {
        return EnemyState.Success;
    }

    protected override void OnExit()
    {
        cloneClass.clones.Clear();
    }
}


public class ExecuteDashClone : TaskNode<EnemyEntityDataBox>
{
    private readonly ICloneAble cloneClass;
    public ExecuteDashClone(EnemyEntityDataBox box, float duration = 0f) : base(box, duration)
    {
        cloneClass = box as ICloneAble;
    }
    protected override void OnEnter()
    {
        for(int i = 0; i < cloneClass.clones.Count; i++)
        {
            var clone = cloneClass.clones[i];

            clone.SetCommand(CloneCommand.Dash);
            clone.Execute();
        }
    }

    protected override EnemyState OnUpdate()
    {
        return EnemyState.Success;
    }

    protected override void OnExit()
    {
        cloneClass.clones.Clear();
    }
}


public class VanishNode : TaskNode<EnemyEntityDataBox>
{
    private MaterialPropertyBlock block;
    private Renderer[] renderers;
    private float start, end;
    private bool success = false, isVanish = false;
    private int originLayer, originDamageCompLayer, invisibleLayer;
    private IIgnoreDamageAble damageComponent;
    private Coroutine routine;
    public VanishNode(
        EnemyEntityDataBox box, 
        float duration = 0f,
        bool isVanish = false) : base(box, duration)
    {
        if(isVanish)
        {
            start = 0f;
            end = 1f;
        }
        else
        {
            start = 1f;
            end = 0f;
        }

        this.isVanish = isVanish;

        block = new();
        renderers = box.rigid.GetComponentsInChildren<Renderer>();

        invisibleLayer = LayerMask.NameToLayer("InvisibleLayer");
        originLayer = box.mono.gameObject.layer;
        damageComponent = box.mono.GetComponentInChildren<IIgnoreDamageAble>();
    }

    protected override void OnEnter()
    {
        success = false;   

        SetLayer();
        box.mono.RunRoutine(Fading(start, end), routine);
    }

    private void SetLayer()
    {
        if(isVanish)
        {
            if(damageComponent != null) damageComponent.IgnoreDamage = true;
            box.mono.gameObject.layer = invisibleLayer;
        }
        else
        {
            if(damageComponent != null) damageComponent.IgnoreDamage = false;
            box.mono.gameObject.layer = originLayer;
        }
    }

    System.Collections.IEnumerator Fading(float start, float end)
    {
        float curTime = 0f, per = 0f, fadeTime = 0.25f;

        while(per < 1f)
        {
            curTime += Time.deltaTime;
            per = curTime / fadeTime;

            float curFadeVal = Mathf.Lerp(start, end, per);
            SetDissolve(curFadeVal);
            yield return null;
        }

        SetDissolve(end);

        success = true;
    }

    private void SetDissolve(float amount)
    {
        for(int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            renderer.GetPropertyBlock(block);
            block.SetFloat("_DissolveAmount", amount);
            renderer.SetPropertyBlock(block);
        }
    }

    protected override EnemyState OnUpdate()
    {
        return success ? EnemyState.Success : EnemyState.Running;
    }
}


public class TurnNode : TaskNode<EnemyEntityDataBox>
{
    private bool needRotateFinish = false;
    public TurnNode(EnemyEntityDataBox box, bool needRotateFinish = false) : base(box)
    {
        this.needRotateFinish = needRotateFinish;
    }

    protected override EnemyState OnUpdate()
    { 
        if(needRotateFinish)
        {
            if(!IsFinishRotate())
            {
                Rotate();
                return EnemyState.Running;
            }
        }
        else
        {
            Rotate();
        }

        return EnemyState.Success;
    }

    private void Rotate()
    {
        Vector3 targetPosition = box.nav.destination - box.rigid.transform.position;
        targetPosition.y    = 0f;
        Vector3 direction   = targetPosition.normalized;

        Quaternion look = Quaternion.LookRotation(direction);
        Quaternion slerped = Quaternion.Slerp(box.rigid.rotation, look, Time.deltaTime * 5.5f);
        box.rigid.MoveRotation(slerped);
    }

    private bool IsFinishRotate()
    {
        Vector3 targetPosition = box.nav.destination - box.rigid.transform.position;
        targetPosition.y    = 0f;
        Vector3 direction   = targetPosition.normalized;

        float angle = Vector3.Angle(box.rigid.transform.forward, direction);
        return angle <= 5f;
    }
}


public class AnimTurnNode : TaskNode<EnemyEntityDataBox>
{
    private bool isTurned = false;
    public AnimTurnNode(EnemyEntityDataBox box, int layer = 3) : base(box) 
    { 
        if(!box.enemyAnimationStorage.IsExist(EnemyAnimationType.Turn))
            box.enemyAnimationStorage.Add(EnemyAnimationType.Turn, new TurnAnimation(box));
    }

    protected override void OnEnter()
    {
        Vector3 targetPosition = box.nav.destination - box.rigid.transform.position;
        targetPosition.y    = 0f;
        Vector3 direction   = targetPosition.normalized;

        float angle = Vector3.SignedAngle(box.rigid.transform.forward, direction, Vector3.up);

        if(Mathf.Abs(angle) >= 45f)
        {
            isTurned = true;

            var turnAnim = box.enemyAnimationStorage.Get<TurnAnimation>(EnemyAnimationType.Turn);
        
            turnAnim.OnActAnimEnter();
            turnAnim.SetRotationDirection(angle);
        }
    }

    protected override EnemyState OnUpdate()
    {
        if(!isTurned)
        {
            Rotate();
            return IsFinishRotate() ? EnemyState.Success : EnemyState.Fail;
        } 
        
        var turnAnim = box.enemyAnimationStorage.Get(EnemyAnimationType.Turn);
        return turnAnim.IsEnd() ? EnemyState.Success : EnemyState.Running;
    }

    protected override void OnExit()
    {
        if(isTurned)
        {
            var turnAnim = box.enemyAnimationStorage.Get(EnemyAnimationType.Turn);
            turnAnim.OnActAnimExit();

            isTurned = false;
        }
    }

    private void Rotate()
    {
        Vector3 targetPosition = box.nav.destination - box.rigid.transform.position;
        targetPosition.y    = 0f;
        Vector3 direction   = targetPosition.normalized;

        Quaternion look = Quaternion.LookRotation(direction);
        Quaternion slerped = Quaternion.Slerp(box.rigid.rotation, look, Time.deltaTime * 5.5f);
        box.rigid.MoveRotation(slerped);
    }

    private bool IsFinishRotate()
    {
        Vector3 targetPosition = box.nav.destination - box.rigid.transform.position;
        targetPosition.y    = 0f;
        Vector3 direction   = targetPosition.normalized;

        float angle = Vector3.Angle(box.rigid.transform.forward, direction);
        return angle <= 5f;
    }
}


public class RecordNode : TaskNode<EnemyEntityDataBox>
{
    private readonly IRecordAble recordableClass;
    public RecordNode(EnemyEntityDataBox box, float duration = 0f) : base(box, duration)
    {
        recordableClass = box as IRecordAble;
    }

    protected override void OnEnter()
    {
        if(recordableClass.recordPos.Count >= 10)
            recordableClass.recordPos.RemoveAt(0);

        Vector3 currentDestination = box.nav.destination + Vector3.up;
        recordableClass.recordPos.Add(currentDestination);
    }

    protected override EnemyState OnUpdate()
    {
        return EnemyState.Success;
    }
}
 

public class ClearRecordNode : TaskNode<EnemyEntityDataBox>
{
    private readonly IRecordAble recordableClass;
    public ClearRecordNode(EnemyEntityDataBox box, float duration = 0f) : base(box, duration)
    {
        recordableClass = box as IRecordAble;
    }

    protected override void OnEnter()
    {
        recordableClass.recordPos.Clear();
    }
    
    protected override EnemyState OnUpdate()
    {
        return EnemyState.Success;
    }
}


public class SummonCloneOnRecordPoint : TaskNode<EnemyEntityDataBox>
{
    
    private readonly int summonCloneId;
    private readonly ICloneAble cloneAbleClass;
    private readonly IRecordAble recordableClass;
    private bool isReadyToSuccess = false;
    private PoissonDiskSamplingVector poissonDiskSampling;

    public SummonCloneOnRecordPoint(EnemyEntityDataBox box, float duration = 0f, int cloneId = 101) : base(box, duration)
    {
        summonCloneId = cloneId;

        cloneAbleClass = box as ICloneAble;
        recordableClass = box as IRecordAble;

        poissonDiskSampling = new(5f);
    }

    protected override void OnEnter()
    {
        isReadyToSuccess = false;

        for(int i = 0; i < recordableClass.recordPos.Count; i++)
        {
            IClone clone = EventBus.Invoke_Func<int, IClone>("Get_EnemyClone", summonCloneId);

            //position
            Vector3 spawnPoint = recordableClass.recordPos[i];
            spawnPoint = poissonDiskSampling.GetRandomRange(spawnPoint);
            clone.SetPosition(spawnPoint);

            //rotation
            Vector3 targetPoint = box.nav.destination;
            Vector3 targetDirection = targetPoint - spawnPoint;
            targetDirection.y = 0f;
            targetDirection.Normalize();

            if(targetDirection != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(targetDirection);
                clone.SetRotation(rotation);
            }

            clone.SetActive(true);

            cloneAbleClass.clones.Add(clone);
        }

        isReadyToSuccess = true;
    }

    protected override EnemyState OnUpdate()
    {
        if(!isReadyToSuccess) return EnemyState.Running;

        return EnemyState.Success;
    }

    protected override void OnExit()
    {
        isReadyToSuccess = false;
    }
}


public class EshaSummonSword : TaskNode<EnemyEntityDataBox>
{
    private readonly int targetSummonCloneId;
    public EshaSummonSword(EnemyEntityDataBox box, float duration = 0f, int cloneId = 101) : base(box, duration)
    {
        targetSummonCloneId = cloneId;

        _ = EventBus.Invoke_Func<int, IClone>("Get_EnemyClone", targetSummonCloneId);
    }   

    private Quaternion GetQuaternion(Vector3 startPosition, Vector3 targetPosition)
    {
        Vector3 desiredDirection = targetPosition - startPosition;
        desiredDirection.y = 0f;

        desiredDirection.Normalize();

        return Quaternion.LookRotation(desiredDirection);
    }

    protected override void OnEnter()
    {
        IClone clone = EventBus.Invoke_Func<int, IClone>("Get_EnemyClone", targetSummonCloneId);

        Vector3 targetPosition = box.nav.destination;
        Vector3 desiredSpawnPoint = targetPosition + (Vector3.up * 5f);

        clone.SetPosition(desiredSpawnPoint);
        clone.SetRotation(GetQuaternion(box.rigid.position, targetPosition));
        clone.SetActive(true);

        clone.Execute();
    }

    protected override EnemyState OnUpdate()
    {
        return EnemyState.Success;
    }
}


public class SummonProjectile : TaskNode<EnemyEntityDataBox>
{
    private readonly PoissonDiskSamplingVector poissonDiskSampling;
    private readonly int projectileId;
    private readonly LayerMask targetLayer;
    public SummonProjectile(EnemyEntityDataBox box, float duration = 0f, int projectileId = 10000) : base(box, duration)
    {
        poissonDiskSampling = new(10f);

        this.projectileId   = projectileId;

        targetLayer = LayerMask.GetMask("CharacterDamage");

        _ = EventBus.Invoke_Func<int, ProjectileBase>("Pool_GetProjectile", projectileId);
    }

    protected override void OnEnter()
    {
        Vector3 centor   = box.rigid.position;
        
        float RandomY = Random.Range(7f, 10f);
        Vector3 startPos = poissonDiskSampling.GetRandomRange(centor) + (Vector3.up * RandomY);
        Vector3 endPos   = box.nav.destination;

        Vector3 knockbackDir = endPos - startPos;

        ProjectileBase projectile = EventBus.Invoke_Func<int, ProjectileBase>("Pool_GetProjectile", projectileId);
        projectile.transform.position = startPos;
        projectile.Initialize(
                targetLayer,
                new DamageSource
                {
                    damageAmount = box.enemyData.attackDamage,
                    knockbackDir = knockbackDir,
                    hit_vfxId = box.enemyData.hit_VfxId
                },
                startPos,
                endPos,
                ProjectileType.Curve);

        projectile.gameObject.SetActive(true);
        projectile.Execute();
    }

    protected override EnemyState OnUpdate()
    {
        return EnemyState.Success;
    }
}


public class RemoveClone : TaskNode<EnemyEntityDataBox>
{
    private readonly ICloneAble cloneClass;
    public RemoveClone(EnemyEntityDataBox box, float duration = 0f) : base(box, duration)
    {
        cloneClass = box as ICloneAble;
    }
    protected override void OnEnter()
    {
        for(int i = 0; i < cloneClass.clones.Count; i++)
        {
            var clone = cloneClass.clones[i];
            clone?.Exception();
        }

        cloneClass.clones.Clear();
    }

    protected override EnemyState OnUpdate()
    {
        return EnemyState.Success;
    }
}


public class BackOriginalPosition : TaskNode<EnemyEntityDataBox>
{
    private bool waitting = false;
    private Vector3 originalPosition;
    public BackOriginalPosition(EnemyEntityDataBox box, float duration = 0f) : base(box, duration)
    {
        originalPosition = box.col.transform.position;
    }

    protected override void OnEnter()
    {
        waitting = true;
    }

    protected override EnemyState OnUpdate()
    {
        if(waitting)
        {
            box.rigid.MovePosition(originalPosition + (Vector3.up * 0.2f));
            waitting = false;
            return EnemyState.Running;
        }

        return EnemyState.Success;
    }

    protected override void OnExit()
    {
        waitting = false;
    }
}


public class AnimationTriggerNode : TaskNode<EnemyEntityDataBox>
{
    private readonly EnemyAnimationType animationType;

    public AnimationTriggerNode(
        EnemyEntityDataBox box, 
        EnemyAnimationType animationType,
        string parameterName, 
        string animationClipName,
        bool useRootMotion = false,
        float duration = 0f) : base(box, duration) 
    { 
        this.animationType = animationType;

        if(!box.enemyAnimationStorage.IsExist(animationType))
            box.enemyAnimationStorage.Add(animationType, new TriggerAnimation(box, parameterName, animationClipName, useRootMotion));
    }

    protected override void OnEnter()
    {
        var animation = box.enemyAnimationStorage.Get(animationType);
        animation.OnActAnimEnter();
    }

    protected override EnemyState OnUpdate()
    {
        var animation = box.enemyAnimationStorage.Get(animationType);
        return animation.IsEnd() ? EnemyState.Success : EnemyState.Running;
    }

    protected override void OnExit()
    {
        var animation = box.enemyAnimationStorage.Get(animationType);
        animation.OnActAnimExit();
    }

    public override void Undo(bool isRootUndo = false)
    {
        OnExit();
    }
}


public class WaitingNode : TaskNode<EnemyEntityDataBox>
{   
    private float currentDuration = 0f, waitDuration = 1f;
    public WaitingNode(EnemyEntityDataBox box, float waitDuration = 1f, float duration = 0f) : base(box, duration) 
    { 
        this.waitDuration = waitDuration;
    }

    protected override void OnEnter()
    {
        currentDuration = 0f;
    }

    protected override EnemyState OnUpdate()
    {
        currentDuration += Time.deltaTime;

        bool isOverDuration = currentDuration >= waitDuration;
        
        return isOverDuration ? EnemyState.Success : EnemyState.Running;
    }

    protected override void OnExit()
    {
        currentDuration = 0f;
    }

}


public class DebugNode : Node
{
    private string _logContent;

    public DebugNode(string logContent)
    {
        _logContent = logContent;
    }


    public override EnemyState Execute()
    {
        Util.Log($"{_logContent}");

        return EnemyState.Success;
    }
}