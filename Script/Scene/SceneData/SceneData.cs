using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SceneData", menuName = "SceneData/SceneData")]
public class SceneData : ScriptableObject
{
    public int id;
    public string sceneName;
}
