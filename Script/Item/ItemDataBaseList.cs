using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDataBaseList", menuName = "Item/ItemDataBaseList")]
public class ItemDataBaseList : ScriptableObject
{
    public List<ItemDataBase> itemDataBase;
}
