using UnityEngine;

public abstract class UseItemData : ItemData 
{
    public virtual void UseItem(int characterId) {}
    public virtual void UseItem() {}
}

