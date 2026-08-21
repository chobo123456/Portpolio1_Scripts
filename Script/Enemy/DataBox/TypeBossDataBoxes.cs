using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EshaDataBox : BossDataBox, IGroggyAble, ITargetFocusAble, ICloneAble, IRecordAble, IPhaseAble
{
    public bool isGroggy { get; set; } = false;
    public ITarget currentTarget {get; set;}
    public List<IClone> clones      {get; set;} = new();
    public List<Vector3> recordPos  {get; set;} = new();
    public int phase { get; set; } = 1;
    public EshaDataBox(Transform owner, int enemyId) : base(owner, enemyId) {}
}
