using UnityEngine;


[CreateAssetMenu(fileName = "UseItemData", menuName = "Item/Data/Weapon")]
public class WeaponItemData : ItemData
{
    public int weaponId;

    public int GetEquipmentId() => weaponId;
}