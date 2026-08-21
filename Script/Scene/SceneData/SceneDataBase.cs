using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SceneDataBase", menuName = "SceneData/SceneDataBase")]
public class SceneDataBase : ScriptableObject
{
    public List<SceneData> lists;
}
