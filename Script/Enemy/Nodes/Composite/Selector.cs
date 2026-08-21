using UnityEngine;
using System.Collections.Generic;

public class Selector : CompositeNode
{
    private int _currentIndex = -1;

    public Selector(params Node[] nodes_Args) : base(nodes_Args) {}
    public Selector(List<Node> newNodes) : base(newNodes) {}
    public Selector() : base() {}

    public override EnemyState Execute()
    {
        for(int i = 0; i < nodes.Count; i++)
        {
            EnemyState state = nodes[i].Execute();

            if(state != EnemyState.Fail)
            {
                if(_currentIndex > -1 && _currentIndex != i)
                    nodes[_currentIndex].Undo();

                if(state == EnemyState.Running)
                {
                    _currentIndex = i;
                }
                else
                {
                    _currentIndex = -1;
                }
                //Success의경우 다음 노드를 호출하기에 확정적으로 두지않음

                return state;
            }
        }

        if(_currentIndex > -1)
            nodes[_currentIndex].Undo();

        _currentIndex = -1;

        return EnemyState.Fail;
    }

    public override void Undo(bool isRootUndo = false)
    {
        if(_currentIndex > -1)
            nodes[_currentIndex].Undo();

        _currentIndex = -1;
    }
}