using UnityEngine;

[CreateAssetMenu(fileName = "AnimationCurveData", menuName = "AnimationData/CurveData")]
public class AnimationCurveData : ScriptableObject
{
    public int curveId;
    public AnimationCurve curveData;
}
