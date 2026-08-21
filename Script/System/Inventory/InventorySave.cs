using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public struct InventoryData
{
    public int itemId;
    public int itemAmount;
    public int instanceId;
}

[System.Serializable]
public struct InventoryDatas
{
    public List<InventoryData> data_List;
}
public class InventorySaver
{
    private readonly string inventoryPath;
    private static string directoryPath;
    public InventorySaver(string _filePath, string _directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath))
        {
            directoryPath = _directoryPath;
            Initialize_Directory();
        }

        inventoryPath = JsonUtil.Combine_Path(directoryPath, _filePath);
    }

    private void Initialize_Directory()
    {
        if(!JsonUtil.IsExistDirectory(directoryPath))
        {
            JsonUtil.MakeDirectory(directoryPath);
        }
    }

    public void InventorySave(InventoryDatas data)
    {
        string json = JsonUtil.ParseToJson(data);

        JsonUtil.FileWrite(inventoryPath, json);
    }

    public bool IsFileExist()
    {
        return JsonUtil.IsExistFile(inventoryPath);
    }
    public List<InventoryData> InventoryLoad()
    {
        string json = JsonUtil.FileRead(inventoryPath);
        
        return JsonUtil.ParseFromJson<InventoryDatas>(json).data_List;
    }
}