using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class SceneMoveSystem : MonoBehaviour
{
    protected void SceneMove(int recentSceneId, int sceneId)
    {
        this.RunRoutine(MoveLoop(recentSceneId, sceneId));
    }
    
    IEnumerator MoveLoop(int recentSceneId, int sceneId)
    {
        yield return new WaitUntil(() => LoadStatus.IsReady);

        SceneData recentSceneData = DataLoader.GetData<SceneData>(DataType.Scene, recentSceneId);
        SceneData sceneData = DataLoader.GetData<SceneData>(DataType.Scene, sceneId);

        string recentSceneName = recentSceneData.sceneName;
        string sceneName = sceneData.sceneName;

        AsyncOperation loadOper = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        loadOper.allowSceneActivation = false;

        while(loadOper.progress < 0.9f) yield return null;

        yield return YieldUtil.WaitForSeconds(1f);

        loadOper.allowSceneActivation = true;

        yield return loadOper;

        Scene targetScene = SceneManager.GetSceneByName(sceneName);
        if(!targetScene.IsValid() || !SceneManager.SetActiveScene(targetScene)) yield break;

        SceneManager.UnloadSceneAsync(recentSceneName);

        PlayerMatch.SetSceneId(sceneId);
        EventBus.Invoke("PlayerETCDataSave");
    }
}
