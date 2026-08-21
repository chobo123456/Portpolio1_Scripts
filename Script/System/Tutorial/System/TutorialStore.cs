using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TutorialSaveData
{
    public List<int> _finishedTutorialList;
    public int _recentTutorialId;
}


public class TutorialStore
{
    private List<int> _finishedTutorialList = new();
    private int _recentTutorialId;
    private Save<TutorialSaveData> _save;

    public TutorialStore()
    {
        _save    = new("Player/Tutorial", "TutorialCase");

        if(_save.IsExist())
            Load();
    } 

    private void Load()
    {
        _recentTutorialId     = _save.savedData._recentTutorialId;
        _finishedTutorialList    = _save.savedData._finishedTutorialList;
    }

    public void Save(int tutorialId)
    {
        _save.Saving(new TutorialSaveData {
            _recentTutorialId = tutorialId,
            _finishedTutorialList = _finishedTutorialList
        });
    }

    public void FinishTutorial(int tutorialId)
    {
        if(!_finishedTutorialList.Contains(tutorialId))
            _finishedTutorialList.Add(tutorialId);
    }

    public int GetRecentTutorialId()
    {
        return _recentTutorialId;
    }

    public List<int> GetFinishedTutorialIds()
    {
        return _finishedTutorialList;
    }
}