using UnityEngine;

public interface IDeco
{
    float GetHP();
    float GetAttackDamage();
    float GetMoveSpeed();

    //쿨타임용(ui에서 언제 사라질지를 알려주기 위한값임 다른데서 참조 금지)
}

public interface IDecoSetAble
{
    IDeco SetDeco(IDeco deco);
    DecoInfo GetDecoInfo();
}

public class Decorator : IDeco, IDecoSetAble
{
    protected IDeco target;
    private DecoInfo info;
    protected float startTime, duringTime, amount;
    protected bool isUsed = false;

    public Decorator(IDeco target, float amount = 1f, float duringTime = 0f, int decoIconId = 1) 
    {
        this.target = target;

        this.duringTime = duringTime;
        this.amount     = amount;

        startTime = Time.unscaledTime;

        info = new DecoInfo
        {
            icon = DataLoader.GetData<Sprite>(DataType.DecoIcon, decoIconId),
            activeTime = duringTime,
            startTime = startTime,
            instanceId = RandomNumber.GetNumber()
        };
    } 
    public virtual IDeco SetDeco(IDeco deco)
    {
        target = deco;
        return this;
    }

    public virtual float GetHP() => target.GetHP();
    public virtual float GetAttackDamage() => target.GetAttackDamage();
    public virtual float GetMoveSpeed() => target.GetMoveSpeed();
    public virtual DecoInfo GetDecoInfo() => info;
    public bool IsFinishUse() => isUsed;
}

public class AttackDamage_Mul_Effect : Decorator
{
    public AttackDamage_Mul_Effect(float amount, float duringTime, IDeco target) : base(target, amount, duringTime, 1) {}

    public override float GetAttackDamage()
    {
        if (Time.unscaledTime - startTime >= duringTime)
        {
            isUsed = true;
            return base.GetAttackDamage();
        }
        else
        {
            isUsed = false;
            return amount * base.GetAttackDamage();
        }
    }
}

public class AttackDamage_Add_Effect : Decorator
{
    public AttackDamage_Add_Effect(float amount, float duringTime, IDeco target) : base(target, amount, duringTime, 2)  {}

    public override float GetAttackDamage() 
    {
        if (Time.unscaledTime - startTime >= duringTime)
        {
            isUsed = true;
            return base.GetAttackDamage();
        }
        else
        {
            isUsed = false;
            return amount + base.GetAttackDamage();
        }
    }
}
public class MoveSpeedEffect : Decorator
{
    public MoveSpeedEffect(float amount, float duringTime, IDeco target) : base(target, amount, duringTime) {}

    public override float GetMoveSpeed()
    {
        if (Time.unscaledTime - startTime >= duringTime)
        {
            isUsed = true;
            return base.GetMoveSpeed();
        }
        else
        {
            isUsed = false;
            return amount * base.GetMoveSpeed();
        }
        
    }
}



