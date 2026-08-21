using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public interface IGroggyAble
{
    bool isGroggy { get; set; }
}

public interface ITargetFocusAble
{
    ITarget currentTarget { get; }
}

public interface ICloneAble
{
    List<IClone> clones {get;}
}

public interface IRecordAble
{
    List<Vector3> recordPos {get;}
}

public interface IPhaseAble
{
    int phase { get; set; }
}