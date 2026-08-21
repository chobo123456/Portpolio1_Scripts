using UnityEngine;
using System.Collections;

public partial class EshaSwordClone : CloneBase<CloneEnemyDataBox>
{
    private MaterialPropertyBlock _block;
    private Renderer[] _renderers;
    public Indicator _indicator;
    private GameObject _trail, _impact, _pulse, _indicatorObj;
    private bool _isInitialize = false;
    private LayerMask _targetLayer, _groundLayer;
    private Coroutine _routine, _fadeRoutine;

    public override void OnEnable()
    {
        base.OnEnable();

        if(!_isInitialize)
        {
            _block = new();
            _renderers = transform.GetComponentsInChildren<Renderer>();

            Transform visualParent = transform.Find("Visual");

            _trail = visualParent.Find("Trail").gameObject;
            _impact = visualParent.Find("Impact").gameObject;
            _pulse = transform.Find("Damager").gameObject;

            _targetLayer = LayerMask.GetMask("Character");
            _groundLayer = LayerMask.GetMask("Ground");
        }

        _trail.SetActive(false);
        _impact.SetActive(false);
        _pulse.SetActive(false);

        this.RunRoutine(FadingLoop(1f, 0f), _fadeRoutine);   
        _isInitialize = true;
    }

    private void OnDisable()
    {
        _trail?.SetActive(false);
        _indicatorObj?.SetActive(false);
        _impact?.SetActive(false);
        _pulse?.SetActive(false);
    }
}


public partial class EshaSwordClone : CloneBase<CloneEnemyDataBox>
{
    #region Execute
    public override void Execute() 
    {
        this.RunRoutine(ExecuteLoop(), _routine);
    }

    IEnumerator ExecuteLoop()
    {
        _trail.SetActive(true);

        Vector3 startPosition = transform.position;
        Vector3 endPosition = GetPosition();

        float lerpSpeed = 0.5f, per = 0f, currentDelta = 0f;

        while(per <= 1f)
        {
            currentDelta += Time.deltaTime;
            per = currentDelta / lerpSpeed;

            Vector3 lerpedVector3 = Vector3.Lerp(startPosition, endPosition, per);
            transform.position = lerpedVector3;

            yield return null;
        }

        _trail.SetActive(false);
        _impact.SetActive(true);

        Vector3 indicatorPos = transform.TransformPoint(livingEntityDataBox.col.center + ((livingEntityDataBox.col.height * 0.5f) * Vector3.down));
        indicatorPos.y += 0.2f;

        _indicatorObj = _indicator.GetIndicator(livingEntityDataBox.enemyData.hit_VfxId);

        yield return this.RunRoutine(_indicator.IndicatorLoop(_indicatorObj, indicatorPos, transform.rotation));

        #region pulse
        _pulse.SetActive(true);
        yield return YieldUtil.WaitForSeconds(0.5f);
        Attack();
        _pulse.SetActive(false);

        _pulse.SetActive(true);
        yield return YieldUtil.WaitForSeconds(0.5f);
        Attack();
        _pulse.SetActive(false);

        _pulse.SetActive(true);
        yield return YieldUtil.WaitForSeconds(0.5f);
        Attack();
        _pulse.SetActive(false);
        #endregion

        this.RunRoutine(DisableLoop());
        _impact.SetActive(false);
    }
    
    private Vector3 GetPosition()
    {
        Vector3 rayStartPos = transform.position;
        Vector3 direction = Vector3.down;
        RaycastHit hit;
        float distance = 10f;   

        Physics.Raycast(rayStartPos, direction, out hit, distance, _groundLayer);

        return hit.point + (Vector3.up * 0.2f);
    }
    
    private void Attack()
    {
        Vector3 startPos = transform.position;
        float radius = livingEntityDataBox.enemyData.attackRange;

        Collider[] cols = Physics.OverlapSphere(startPos, radius, _targetLayer);

        for(int i = 0; i < cols.Length; i++)
        {
            Collider col = cols[i];

            var comp = col.GetComponentInChildren<IDamageable>();

            if(comp != null)
            {
                Vector3 knockbackDir = col.transform.position - transform.position;
                knockbackDir.Normalize();
                knockbackDir *= 5f;
                
                comp.TakeDamage(
                    new DamageSource
                    {
                        damageAmount = livingEntityDataBox.enemyData.attackDamage,
                        knockbackDir = knockbackDir,
                        hit_vfxId = livingEntityDataBox.enemyData.hit_VfxId
                    }
                );   
            }
        }
    }

    #endregion

    IEnumerator DisableLoop()
    {
        yield return this.RunRoutine(FadingLoop(0f, 1f), _fadeRoutine);

        gameObject.SetActive(false);
    }

    #region Dissolve
    IEnumerator FadingLoop(float start, float end)
    {
        float curTime = 0f, per = 0f, fadeTime = 0.5f;

        while(per < 1f)
        {
            curTime += Time.deltaTime;
            per = curTime / fadeTime;

            float curFadeVal = Mathf.Lerp(start, end, per);
            SetDissolve(curFadeVal);
            yield return null;
        }

        SetDissolve(end);
    }

    private void SetDissolve(float amount)
    {
        for(int i = 0; i < _renderers.Length; i++)
        {
            Renderer renderer = _renderers[i];

            renderer.GetPropertyBlock(_block);
            _block.SetFloat("_DissolveAmount", amount);
            renderer.SetPropertyBlock(_block);
        }
    }
    #endregion
}