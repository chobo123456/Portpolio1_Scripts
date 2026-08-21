using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private TutorialMainUILockSystem _uiLockSystem;
    private TutorialStore _store;
    private TutorialSequencer _sequencer;
    private WaitUntil _waitEnableStep, _waitActiveState, _waitEventExist, _waitInitialized;
    private bool _enabledNextStep = true, _isInitialized = false;
    private void OnEnable()
    {
        this.RunRoutine(Booting());
    }

    private void ReferenceInstance()
    {
        _store              = new();
        _sequencer          = new(this.GetComponent<MonoBehaviour>());
        _uiLockSystem       = new();
    }

    private void SettingWaitUntil()
    {
        _waitEventExist = new WaitUntil(() => EventBus.HasEvent("UILock") && EventBus.HasEvent("UIShow"));
        _waitActiveState = new WaitUntil(() => GameState.IsActive());
        _waitEnableStep = new WaitUntil(() => _enabledNextStep);
        _waitInitialized = new WaitUntil(() => _isInitialized);
    }

    private void SubscribeEvents(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<bool>("EnableTutorialShowStep", TutorialEnableStep);
            EventBus.Sub<int>("StartTutorial", OnTutorialStart);
            EventBus.Sub<int>("EndTutorial", OnTutorialEnd);
        }
        else
        {
            EventBus.UnSub<bool>("EnableTutorialShowStep", TutorialEnableStep);
            EventBus.UnSub<int>("StartTutorial", OnTutorialStart);
            EventBus.UnSub<int>("EndTutorial", OnTutorialEnd);
        }
    }
    
    private void OnDisable()
    {
        SubscribeEvents(false);
    }

    IEnumerator Booting()
    {
        ReferenceInstance();
        SettingWaitUntil();
        SubscribeEvents(true);  

        yield return _waitEventExist;
        yield return new WaitUntil(() => LoadStatus.IsReady);

        _uiLockSystem.LoadState(_store.GetFinishedTutorialIds());

        yield return _waitActiveState;
        _isInitialized = true;
    }

    private void TutorialEnableStep(bool enabled)
    {
        _enabledNextStep = enabled;
    }

    private void OnTutorialStart(int tutorialId)
    {
        if(tutorialId <= 0) return;

        this.RunRoutine(Loop(tutorialId));
    }

    IEnumerator Loop(int tutorialId)
    {
        yield return _waitInitialized;
        yield return _waitEventExist;
        yield return _waitActiveState;
        
        _store.Save(tutorialId);
        _uiLockSystem.StartTutorial(tutorialId);

        if(!_enabledNextStep)
            yield return _waitEnableStep;

        _sequencer.StartSequence(tutorialId);
    }

    private void OnTutorialEnd(int tutorialId)
    {
        _uiLockSystem.EndTutorial();

        _store.FinishTutorial(tutorialId);
        _store.Save(tutorialId);
    }
}
