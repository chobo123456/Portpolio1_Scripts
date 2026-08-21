using System.Collections;
using System.Collections.Generic;
public abstract class CompositeNode : Node
{
    protected List<Node> nodes;

    public CompositeNode(params Node[] nodes_Args)
    {
        nodes = new();

        for (int i = 0; i < nodes_Args.Length; i++) 
            nodes.Add(nodes_Args[i]);
    }

    public CompositeNode(List<Node> nodes)
    {
        this.nodes = new();
        this.nodes = nodes;
    }

    public CompositeNode() {
        nodes = new();
    }

    public virtual void AddNode(Node node)
    {
        nodes.Add(node);
    }
}