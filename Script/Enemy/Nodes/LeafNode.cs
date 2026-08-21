using System.Collections;
using System.Collections.Generic;
public abstract class LeafNode<T> : Node where T : LivingEntityDataBox
{
    protected T box;
    public LeafNode(LivingEntityDataBox dataBox)
    {
        box = (T)dataBox;
    }
}