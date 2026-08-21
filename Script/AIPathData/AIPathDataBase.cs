using UnityEngine;

[CreateAssetMenu(fileName = "AIPathDataBase", menuName = "AIPath/AIPathDataBase")]
public class AIPathDataBase : ScriptableObject
{
    public AIPathData[] paths;
}
