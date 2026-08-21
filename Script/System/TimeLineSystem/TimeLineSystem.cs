using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;
using System.Collections.Generic;
using System;


public enum TrackType
{
    Single,
    Hierarchy
}

[System.Serializable]
public struct HierarchyTrackBinding
{
    public string parentBindName;
    public UnityEngine.Object parentPrefab;
    
    [HideInInspector]
    public UnityEngine.Object parentBindTarget;

    public ChildTrackBinding[] childTracks;

    public UnityEngine.Object GetObject(string name)
    {
        if(name.Equals(parentBindName))
            return parentBindTarget;

        for(int i = 0; i < childTracks.Length; i++)
        {
            ChildTrackBinding child = childTracks[i];

            if(name.Equals(child.childName))
                return child.childBindTarget;
        }

        Util.Log($"HierarchyTrackBinding.cs GetObject() UnityEngine.Object:{name} Not Found","red");
        return parentBindTarget;
    }
}

[System.Serializable]
public struct ChildTrackBinding
{
    public string childName;

    [HideInInspector]
    public UnityEngine.Object childBindTarget;
}

[System.Serializable]
public struct SingleTrackBinding
{
    public string bindName;

    public UnityEngine.Object prefab;

    [HideInInspector]
    public UnityEngine.Object bindTarget;
}


[System.Serializable]
public struct TrackBinding : IEquatable<TrackBinding>
{
    public TrackType trackType;

    public SingleTrackBinding singleInfo;
    public HierarchyTrackBinding hierarchyInfo;

    public bool Equals(TrackBinding bind)
    {
        return trackType.Equals(TrackType.Single) ? this.singleInfo.Equals(bind.singleInfo) : this.hierarchyInfo.Equals(bind.hierarchyInfo);
    }

    public override bool Equals(object bind)
    {
        return base.Equals(bind);
    }

    public static bool operator !=(TrackBinding bindingA, TrackBinding bindingB)
    {
        bool isNotMatched = bindingA.trackType != bindingB.trackType;

        if(isNotMatched) return true;

        bool isSingle = bindingA.Equals(TrackType.Single) && bindingB.Equals(TrackType.Single);
        
        return isSingle ? 
                !bindingA.singleInfo.Equals(bindingB.singleInfo) : 
                !bindingA.hierarchyInfo.Equals(bindingB.hierarchyInfo);
    }

    public static bool operator ==(TrackBinding bindingA, TrackBinding bindingB)
    {
        bool isNotMatched = bindingA.trackType != bindingB.trackType;

        if(isNotMatched) return false;

        bool isSingle = bindingA.Equals(TrackType.Single) && bindingB.Equals(TrackType.Single);

        return isSingle ? 
                bindingA.singleInfo.Equals(bindingB.singleInfo) : 
                bindingA.hierarchyInfo.Equals(bindingB.hierarchyInfo);
    }

    public override int GetHashCode()
    {
        return this.GetHashCode();
    }
}   

[System.Serializable]
public struct TimeLineInfo
{
    public List<TrackBinding> bindings;

    public TrackBinding GetTrack(string name)
    {
        for(int i = 0; i < bindings.Count; i++)
        {
            TrackBinding binding = bindings[i];

            if(binding.trackType == TrackType.Single)
            {
                if(name.Equals(binding.singleInfo.bindName))
                    return binding;
            }
            else
            {
                if(name.Equals(binding.hierarchyInfo.parentBindName))
                    return binding;

                for(int j = 0; j < binding.hierarchyInfo.childTracks.Length; j++)
                {
                    ChildTrackBinding childBinding = binding.hierarchyInfo.childTracks[j];
                    
                    if(name.Equals(childBinding.childName))
                        return binding;
                }
            }
        }

        Util.Log($"TimeLineInfo.cs GetTrack() {name} Not Found", "red");
        return bindings[0];
    }
}

public class TimeLineSystem : MonoBehaviour
{
    private PlayableDirector _director;
    private Animator _cameraAnimator, _fadePanelAnimator;

    private bool _isPlayerNeedTeleport = false, _isInactiveGameState = false;
    private Vector3 _lastPlayerTeleportPos;
    private Quaternion _lastPlayerTeleportRotate;
    private Transform _playerTr, _timeLInePlayerTr;
    private List<UnityEngine.Object> _timeLineCreatedObjects = new();

    private TimeLineAsset _currentTimelineData;

    private void OnEnable()
    {
        _director = GetComponent<PlayableDirector>();

        _director.stopped += TimelineEnd;
        _cameraAnimator      = GameObject.Find("CutSceneCam").GetComponent<Animator>();
        _fadePanelAnimator   = GameObject.Find("MainCanvas").transform.FindTarget("Timeline_Fade").GetComponent<Animator>();

        EventBus.Sub<Transform>("SetCharacterTransform", SetPlayer);
        EventBus.Sub<int>("PlayTimeLine", PlayTimeLine);

        EventBus.Sub("OnTimeLineStart", OnStartTimeLine);
        EventBus.Sub("OnTimeLineEnd", OnTimelineEnd);
    }

    private void OnDisable()
    {
        _director.stopped -= TimelineEnd;

        EventBus.UnSub<Transform>("SetCharacterTransform", SetPlayer);
        EventBus.UnSub<int>("PlayTimeLine", PlayTimeLine);

        EventBus.UnSub("OnTimeLineStart", OnStartTimeLine);
        EventBus.UnSub("OnTimeLineEnd", OnTimelineEnd);
    }

    private void SetPlayer(Transform player)
    {
        _playerTr = player;
    }

    private void PlayTimeLine(int timeLineId)
    {
        _director?.Stop();

        EventBus.Invoke("EnableCutSceneCam");
        EventBus.Invoke("StopCamera");

        _currentTimelineData = DataLoader.GetData<TimeLineAsset>(DataType.TimeLine, timeLineId);

        if(_currentTimelineData.needGameStateStop)
        {
            EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState",  GameStateType.Stop, GameEnableTimeSet.False);
            CursorManager.CursorActive(false);
            EventBus.Invoke<bool>("SetCameraRotateLock", true);
            EventBus.Invoke<bool>("Lock_All_UI", true);
            EventBus.Invoke<bool>("MainCanvasActive", false);
            _isInactiveGameState = true;
        }

        string eventName = _currentTimelineData.timeLineStartEvent.timelineEventName;
        if(!string.IsNullOrEmpty(eventName))
            _currentTimelineData.timeLineStartEvent.EventInvoke();

        SetMode();

        TimeLineInfo timeLineInfo   = _currentTimelineData.timeLineInfo;
        
        if(timeLineInfo.Equals(default)) return;
        TimelineAsset timeline      =  _currentTimelineData.timeline;

        TimeLineInfo settedTimeLineInfo = CreateObjects(timeLineInfo);
        Binding(settedTimeLineInfo, timeline);  

        _director.Play();
    }
    
    private void SetMode()
    {
        if(_currentTimelineData.needHold)
            _director.extrapolationMode = DirectorWrapMode.Hold;
        else
            _director.extrapolationMode = DirectorWrapMode.None;
    }

    private TimeLineInfo CreateObjects(TimeLineInfo info)
    {
        for (int i = 0; i < info.bindings.Count; i++)
        {
            TrackBinding bindingInfo  = info.bindings[i];

            switch(bindingInfo.trackType)
            {
                case TrackType.Single:

                    UnityEngine.Object prefab = bindingInfo.singleInfo.prefab;
                    UnityEngine.Object obj = Instantiate(prefab);

                    if(obj is GameObject gObj)
                    {
                        gObj.SetActive(true);
                        _timeLineCreatedObjects.Add(gObj);

                        obj = gObj.GetComponent<Animator>();
                    }  

                    TrackBinding singleBinding = bindingInfo;
                    singleBinding.singleInfo.bindTarget = obj;

                    info.bindings[i] = singleBinding;
                    break;  

                case TrackType.Hierarchy:

                    UnityEngine.Object parentPrefab = bindingInfo.hierarchyInfo.parentPrefab;
                    UnityEngine.Object parentObject = Instantiate(parentPrefab);

                    TrackBinding hierarchyBinding = bindingInfo;

                    GameObject parentGameObject = parentObject as GameObject;
                    _timeLineCreatedObjects.Add(parentObject);

                    if(parentGameObject != null)
                    {
                        parentGameObject.SetActive(true);
                        parentObject = parentGameObject.GetComponent<Animator>();

                        ChildTrackBinding[] childTracks = bindingInfo.hierarchyInfo.childTracks;

                        for(int j = 0; j < childTracks.Length; j++)
                        {
                            ChildTrackBinding child = childTracks[j];

                            child.childBindTarget = parentGameObject.transform.FindTarget(child.childName).GetComponent<Animator>();

                            childTracks[j] = child;
                        }

                        hierarchyBinding.hierarchyInfo.childTracks = childTracks;
                    }   

                    hierarchyBinding.hierarchyInfo.parentBindTarget = parentObject;
                    
                    info.bindings[i] = hierarchyBinding;
                    break;
            }
        }

        return info;
    }
    
    private void Binding(TimeLineInfo timeLineInfo, TimelineAsset timeline)
    {
        _director.playableAsset = timeline;

        foreach(var output in _director.playableAsset.outputs)
        {
            if (output.streamName == "Camera")
            {
                _director.SetGenericBinding(output.sourceObject, _cameraAnimator);
                continue;
            }
            
            if(output.streamName == "Fade")
            {
                _director.SetGenericBinding(output.sourceObject, _fadePanelAnimator);
                continue;
            }
            else if(output.streamName == "Fade_Active")
            {
                _director.SetGenericBinding(output.sourceObject, _fadePanelAnimator.gameObject);
                continue;
            }

            TrackBinding match = timeLineInfo.GetTrack(output.streamName);

            if(match != null)
            {
                switch(match.trackType)
                {
                    case TrackType.Single:

                        if (output.streamName.Equals("Player"))
                        {
                            UnityEngine.Object compareObject = match.singleInfo.bindTarget;

                            if(compareObject is Component comp)
                                _timeLInePlayerTr = comp.transform;
                            else if(compareObject is GameObject obj)
                                _timeLInePlayerTr = obj.transform;

                            _isPlayerNeedTeleport = true;
                        }

                        _director.SetGenericBinding(output.sourceObject, match.singleInfo.bindTarget);

                    continue;

                    case TrackType.Hierarchy:

                        UnityEngine.Object target = match.hierarchyInfo.GetObject(output.streamName);

                        if (output.streamName.Equals("Player"))
                        {
                            if(target is Component comp)
                                _timeLInePlayerTr = comp.transform;
                            else if(target is GameObject obj)
                                _timeLInePlayerTr = obj.transform;

                            _isPlayerNeedTeleport = true;
                        }

                        _director.SetGenericBinding(output.sourceObject, target);

                    continue;
                }   
            }    
        }        
    }
    
    //End
    private void TimelineEnd(PlayableDirector d)
    {
        if(_isPlayerNeedTeleport)
        {
            if (_timeLInePlayerTr != null)
            {
                _lastPlayerTeleportPos = _timeLInePlayerTr.position;
                _lastPlayerTeleportRotate = _timeLInePlayerTr.rotation;
            }
        }

        if(_isInactiveGameState)
        {
            EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState",  GameStateType.Run, GameEnableTimeSet.False);
            EventBus.Invoke<bool>("SetCameraRotateLock", false);
            EventBus.Invoke<bool>("Lock_All_UI", false);
            EventBus.Invoke<bool>("MainCanvasActive", true);
            _isInactiveGameState = false;
        }

        ClearTimeLineObjects();

        if(_currentTimelineData.needMovePlayer)
        {
            EventBus.Invoke("DisableCutSceneCam");
            EventBus.Invoke<bool, float>("BlackPanelFade", true, 0.45f);
            OnTimelineEnd();
        }

        string eventName = _currentTimelineData.timeLineEndEvent.timelineEventName;
        if(!string.IsNullOrEmpty(eventName))
            _currentTimelineData.timeLineEndEvent.EventInvoke();
    }

    private void ClearTimeLineObjects()
    {
        for(int i = 0; i < _timeLineCreatedObjects.Count; i++)
        {
            UnityEngine.Object obj = _timeLineCreatedObjects[i];
            Destroy(obj);
        }

        _timeLineCreatedObjects.Clear();
    }

    //Event
    private void OnStartTimeLine()
    {
        _playerTr.GetComponentInChildren<Animator>().applyRootMotion = false;

        this.RunRoutine(CaptureKinematic());
    }

    private IEnumerator CaptureKinematic()
    {
        Rigidbody rigid = _playerTr.GetComponent<Rigidbody>();
        if(!rigid.isKinematic) rigid.linearVelocity = Vector3.zero;

        yield return new WaitForFixedUpdate();

        if(!rigid.isKinematic) rigid.isKinematic = true;
    }
    
    public void OnTimelineEnd()
    {
        if(!_isPlayerNeedTeleport) return;

        ClearTimeLineObjects();
        EventBus.Invoke("ResumeCamera");

        this.RunRoutine(ReleaseKinematic());

        _isPlayerNeedTeleport = false;
    }

    private IEnumerator ReleaseKinematic()
    {
        Rigidbody rigid = _playerTr.GetComponent<Rigidbody>();

        rigid.MovePosition(_lastPlayerTeleportPos + Vector3.up * 0.5f);  
        rigid.MoveRotation(_lastPlayerTeleportRotate);

        yield return new WaitForFixedUpdate();

        EventBus.Invoke("CameraUpdatePosition");
        
        rigid.isKinematic = false;
        rigid.linearVelocity = Vector3.zero;
    }
}
