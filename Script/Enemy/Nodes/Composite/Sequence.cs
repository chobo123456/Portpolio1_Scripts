using UnityEngine;
using System.Collections.Generic;

public class Sequence : CompositeNode
{
    private int _currentIndex = 0;

    public Sequence(params Node[] nodes_Args) : base(nodes_Args){}
    public Sequence(List<Node> newNodes) : base(newNodes) {}

    public override EnemyState Execute()
    {
        while(_currentIndex < nodes.Count)
        {
            EnemyState state = nodes[_currentIndex].Execute();
            
            if(state == EnemyState.Running)
            {
                return EnemyState.Running;
            }
            else if(state == EnemyState.Fail)
            {   
                _currentIndex = 0;
                return EnemyState.Fail;
            }
            else
            {
                _currentIndex++;
            }
        }

        _currentIndex = 0;
        return EnemyState.Success;
    }

    public override void Undo(bool isRootUndo = false)
    {
        _currentIndex = 0;

        for(int i = 0; i < nodes.Count;i++)
            nodes[i].Undo(isRootUndo);
    }
}

