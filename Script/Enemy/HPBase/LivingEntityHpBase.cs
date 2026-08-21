using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class LivingEntityHpBase : MonoBehaviour, IHpBase
{
    public bool IsDie { get; protected set; } = false;
    public bool IsHit { get; protected set; } = false;
    public ReactiveProperty<float> curHp { get; protected set; }
    public ReactiveProperty<float> maxHp { get; protected set; }
}