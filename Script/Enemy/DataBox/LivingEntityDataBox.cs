using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class LivingEntityDataBox
{
    public readonly MonoBehaviour mono;
    public readonly Rigidbody rigid;
    public readonly CapsuleCollider col;
    public readonly Animator animator;
    public readonly int livingEntityId;

    public LivingEntityDataBox(Transform livingEntity, int livingEntityId)
    {
        this.livingEntityId = livingEntityId;
        
        mono        =   livingEntity.GetComponent<MonoBehaviour>();
        rigid       =   livingEntity.GetComponent<Rigidbody>();
        col         =   livingEntity.GetComponent<CapsuleCollider>();
        animator    =   livingEntity.GetComponentInChildren<Animator>();
    }
}