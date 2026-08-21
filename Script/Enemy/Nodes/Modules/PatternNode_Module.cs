using UnityEngine;

public class PatternNode_Module : Enemy_NodeModule
{
    public override Node SetNode(EnemyEntityDataBox box)
    {
        EshaPattern eshaPattern = new EshaPattern(box);
        return eshaPattern.GetNode();
    }
}