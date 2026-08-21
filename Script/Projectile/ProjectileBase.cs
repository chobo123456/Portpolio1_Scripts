using UnityEngine;

public enum ProjectileType
{
    Straight,
    Curve    
}

public abstract class ProjectileBase : MonoBehaviour
{
    protected LayerMask targetLayer;
    protected DamageSource source;
    protected Vector3 startPos, endPos;
    protected bool readyToFire = false;
    protected ProjectileType projectileType;

    public void Initialize(
        LayerMask targetLayer, 
        DamageSource source, 
        Vector3 startPos,
        Vector3 endPos,
        ProjectileType projectileType)
    {
        this.targetLayer    = targetLayer;
        this.source         = source;

        this.startPos       = startPos;
        this.endPos         = endPos;
        this.projectileType = projectileType;
    }
    public virtual void Execute() {}
}
