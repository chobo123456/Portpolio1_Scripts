using System.Collections;
using UnityEngine;

public enum ManagerType
{
    Inventory,
    Party,
    Quest,
    GrowthTap,
    Craft,
    Pool
}
public static class LoadStatus
{
    //Non_Instant
    public static bool IsReady { get; private set; } = false;
    public static bool IsReady_ETCSave              { get; private set; } = false;

    //Instant
    public static bool IsReady_Inventory            { get; private set; } = false;
    public static bool IsReady_CharacterParty       { get; private set; } = false;
    public static bool IsReady_Quest                { get; private set; } = false;
    public static bool IsReady_GrowthTap            { get; private set; } = false;
    public static bool IsReady_Craft                { get; private set; } = false;
    
    public static bool IsReady_Pool                 { get; private set; } = false;

    public static void LoadAllData()
    {
        IsReady = true;
    }

    public static void SetStatus(ManagerType type, bool isLoaded)
    {
        switch(type)
        {
            case ManagerType.Inventory:
                IsReady_Inventory = isLoaded;
                break;
            case ManagerType.Party:
                IsReady_CharacterParty = isLoaded;
                break;
            case ManagerType.Quest:
                IsReady_Quest = isLoaded;
                break; 
            case ManagerType.GrowthTap:
                IsReady_GrowthTap = isLoaded;
                break;  
            case ManagerType.Craft:
                IsReady_Craft = isLoaded;
                break;
            case ManagerType.Pool:
                IsReady_Pool = isLoaded;
                break;
        }
    }
}
