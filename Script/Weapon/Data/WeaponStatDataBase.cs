using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponStatDataBase", menuName = "Weapon/DataBase")]
public class WeaponStatDataBase : ScriptableObject
{
    public List<WeaponStatData> list;
}
