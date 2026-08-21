using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class TitleSceneMoveSystem : SceneMoveSystem
{
    public int currentSceneId = 0;
    public Button sceneMoveButton;
    public GameObject eventSystem, spinner;

    private void OnEnable()
    {
        if(sceneMoveButton != null) sceneMoveButton.onClick.AddListener(MoveScene);
        if(spinner != null) spinner.SetActive(false);
    }

    private void MoveScene()
    {
        sceneMoveButton.gameObject.SetActive(false);
        eventSystem.SetActive(false);
        spinner.SetActive(true);

        int targetMoveSceneId = PlayerMatch.GetSceneId();

        if (targetMoveSceneId <= 0)
        {
            Util.Log($"target Scene Id is '0' try To Move SceneId '1'", "red");
            targetMoveSceneId = 1;
        }

        base.SceneMove(currentSceneId, targetMoveSceneId);
    }
}
