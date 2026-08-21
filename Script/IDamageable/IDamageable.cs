using UnityEngine;

public static class ElementCase
{
    public static float OnElementCase(this MonoBehaviour mono, Element currentElementType, Element damageElement, float damage)
    {   
        if(currentElementType == damageElement) return damage;

        switch(currentElementType)
        {
            case Element.Light:
                return LightCase(damageElement, damage);
            case Element.Dark: 
                return DarkCase(damageElement, damage);
            case Element.Fire:
                return FireCase(damageElement, damage);
            case Element.Water:
                return WaterCase(damageElement, damage);
            case Element.Wind:
                return WindCase(damageElement, damage);
            case Element.Ground:
                return GroundCase(damageElement, damage);
            default:
                return damage;
        }
    }

    #region LightCase
    private static float LightCase(Element damageElement, float damage)
    {
        switch(damageElement)
        {
            case Element.Dark:
                return damage * 1.6f;
            case Element.Light:
            case Element.Wind:
            case Element.Fire:
            case Element.Water:
            case Element.Ground:
                return damage;
            default:
                return damage;
        }
    }
    #endregion

    #region DarkCase
    private static float DarkCase(Element damageElement, float damage)
    {
        switch(damageElement)
        {
            case Element.Light:
                return damage * 1.6f;  
            case Element.Dark:
            case Element.Wind:
            case Element.Fire:
            case Element.Water:
            case Element.Ground:
                return damage;
            default:
                return damage;
        }
    }
    #endregion

    #region FireCase
    private static float FireCase(Element damageElement, float damage)
    {
        switch(damageElement)
        {
            case Element.Ground:
                return damage * 0.9f; 
            case Element.Water:
                return damage * 1.4f;  
            case Element.Light:
            case Element.Dark:
            case Element.Wind:
            case Element.Fire:
                return damage;
            default:
                return damage;
        }
    }
    #endregion

    #region WaterCase
    private static float WaterCase(Element damageElement, float damage)
    {
        switch(damageElement)
        {
            case Element.Fire:
                return damage * 0.9f; 
            case Element.Wind:
                return damage * 1.4f;  
            case Element.Ground:
            case Element.Water:
            case Element.Light:
            case Element.Dark:
                return damage;
            default:
                return damage;
        }
    }
    #endregion

    #region WindCase
    private static float WindCase(Element damageElement, float damage)
    {
        switch(damageElement)
        {
            case Element.Ground:
                return damage * 1.4f;  
            case Element.Water:
                return damage * 0.9f; 
            case Element.Fire:
            case Element.Wind:
            case Element.Light:
            case Element.Dark:
                return damage;
            default:
                return damage;
        }
    }
    #endregion

    #region GroundCase
    private static float GroundCase(Element damageElement, float damage)
    {
        switch(damageElement)
        {
            case Element.Fire:
                return damage * 1.4f;  
            case Element.Wind:
                return damage * 0.9f; 
            case Element.Ground: 
            case Element.Water:
            case Element.Light:
            case Element.Dark:
                return damage;
            default:
                return damage;
        }
    }
    #endregion
}
public struct DamageSource
{
    public int hit_vfxId;
    public int hit_sfxId;
    public float impactForce;
    public float damageAmount;    
    public float hitTime;
    public float poiseMinusAmount;
    public Vector3 knockbackDir;
    public CameraShakeSource cameraShakeSource;
    public Element elementType;
}

public struct CameraShakeSource
{
    public float amplitude;
    public float frequency;
    public float duringTime;
}

public interface IDamageable
{
    void TakeDamage(DamageSource source);
}

public interface IHealable
{
    bool Heal(float amount);
}
