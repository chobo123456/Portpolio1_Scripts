using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


public enum EnemyState
{
    Running,
    Fail,
    Success
}

public abstract class Node
{
    public abstract EnemyState Execute();
    public virtual void Undo(bool isRootUndo = false) {}
}
