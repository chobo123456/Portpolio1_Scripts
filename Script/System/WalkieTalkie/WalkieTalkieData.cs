using UnityEngine;

[System.Serializable]
public struct WalkieTalkieTalkData
{
    public Sprite speekNpc;
    public string speekContext;
}

[CreateAssetMenu(fileName = "WalkieTalkieData", menuName = "WalkieTalkie/WalkieTalkieData")]
public class WalkieTalkieData : ScriptableObject
{
    public int walkieTalkieDataId;

    public WalkieTalkieTalkData[] walkieTalkieDatas;
}
