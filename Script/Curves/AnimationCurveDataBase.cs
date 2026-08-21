using UnityEngine;

[CreateAssetMenu(fileName = "AnimationCurveDataBase", menuName = "AnimationData/DataBase")]
public class AnimationCurveDataBase : ScriptableObject
{
    public AnimationCurveData[] curveLists;
}
