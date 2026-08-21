using UnityEngine;
using System.Collections.Generic;
using System;


[System.Serializable]
public struct SaveStacks
{
    public List<string> list;
}

public class Save<T>
{
    public readonly string directoryPath, filePath, stackDirectorPath, stackPath;
    public readonly System.Func<bool> backupFileSaveCondition;
    public readonly System.Func<T, bool> validCondition;
    public T savedData;
    private SaveStacks stacks;

    public Save(
        string _directoryPath, 
        string _filePath, 
        System.Func<bool> backupFileSaveCondition = null,
        System.Func<T, bool> validCondition = null)
    {
        directoryPath           = JsonUtil.Combine_Path(Application.persistentDataPath, _directoryPath);
        filePath                = JsonUtil.Combine_Path(directoryPath, _filePath);
        stackDirectorPath       = JsonUtil.Combine_Path(directoryPath, $"{_filePath}_Stack");
        stackPath               = JsonUtil.Combine_Path(stackDirectorPath, "stacks");

        this.backupFileSaveCondition    = backupFileSaveCondition;
        this.validCondition             = validCondition;

        if(!JsonUtil.IsExistDirectory(directoryPath)) JsonUtil.MakeDirectory(directoryPath);
        if(!JsonUtil.IsExistDirectory(stackDirectorPath)) JsonUtil.MakeDirectory(stackDirectorPath);

        stacks.list = new();

        Loading();

        Application.quitting += SaveOnApplicationExit;
    }

    public void Loading()
    {
        bool isStackExist = IsStackExist();

        if(isStackExist)
        {
            string savedStackJson = JsonUtil.FileRead(stackPath);
            SaveStacks savedStacks = JsonUtil.ParseFromJson<SaveStacks>(savedStackJson);
            stacks = savedStacks;
        }

        if(IsExist())
        {
            string json = JsonUtil.FileRead(filePath);
            var temporaryData =  JsonUtil.ParseFromJson<T>(json);

            bool isNotValid = validCondition != null && !validCondition.Invoke(temporaryData);

            if(string.IsNullOrEmpty(json) || isNotValid)
            {
                if(isStackExist)
                {
                    int lastIndex = stacks.list.Count - 1;

                    if(lastIndex < 0) return;

                    //마지막으로 저장된 스택 가져옴
                    string recentSavePath = stacks.list[lastIndex];
                    string recentJson = JsonUtil.FileRead(recentSavePath);

                    savedData = JsonUtil.ParseFromJson<T>(recentJson);
                    JsonUtil.FileWrite(filePath, recentJson);
                }
            }
            else
                savedData = JsonUtil.ParseFromJson<T>(json);
        }
    }

    public void Saving(T data)
    {
        savedData = data;
        
        string json = JsonUtil.ParseToJson(data);
        JsonUtil.FileWrite(filePath, json);

        if(backupFileSaveCondition != null && backupFileSaveCondition.Invoke() && IsExist())
        {
            string randomPath = $"{stackDirectorPath}/{Guid.NewGuid()}";
            JsonUtil.FileCopy(filePath, randomPath);

            stacks.list.Add(randomPath);
            string stackJson = JsonUtil.ParseToJson(stacks);
            JsonUtil.FileWrite(stackPath, stackJson);
        }
    }

    public bool IsExist()
    {
        return JsonUtil.IsExistFile(filePath);   
    }

    public bool IsStackExist()
    {
        return JsonUtil.IsExistFile(stackPath);
    }
    public void SaveOnApplicationExit()
    {
        Saving(savedData);

        Application.quitting -= SaveOnApplicationExit;
    }
}
