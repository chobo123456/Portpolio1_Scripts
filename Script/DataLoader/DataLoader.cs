using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

#region DataStruct

//데이터 저장용도

[System.Serializable]
public struct Addresses
{
    public List<Address> addresses;
}

[System.Serializable]
public struct Address
{
    public string address;
}

#endregion

public enum DataType
{
    None,
    AIPath,
    AnimationCurve,
    BGM,
    Character,
    CharacterETC,
    DecoIcon,
    Enemy,
    EnemyPrefab,
    ElementSprite,
    Item,
    NPC,
    Pool,
    Quest,
    Recipe,
    Skill,
    Scene,    
    SFX,
    Weapon,
    WalkieTalkie,
    Voice,
    Talk,
    TimeLine,
    Tutorial
}

// ** 새로운 타임 추가시 팩토리 클래스에 추가할 타입의 생성자를 반드시 넣을것 ** 
public static class DataLoaderFactory
{
    public static ILoader GetLoader(DataType dataType)
    {
        switch(dataType)
        {
            case DataType.AIPath:
                return new Load_AIPathData();

            case DataType.AnimationCurve:
                return new Load_AnimationCurveData();

            case DataType.BGM :
                return new Load_BGMData();

            case DataType.Character :
                return new Load_CharacterData();

            case DataType.CharacterETC :
                return new Load_CharacterETCData();

            case DataType.DecoIcon :
                return new Load_DecoIconData();

            case DataType.Enemy :
                return new Load_EnemyData();
            
            case DataType.EnemyPrefab:
                return new Load_EnemyPrefabData();

            case DataType.ElementSprite :
                return new Load_ElementSpriteData();

            case DataType.Item :
                return new Load_ItemData();
        
            case DataType.NPC :
                return new Load_NPCData();

            case DataType.Pool :
                return new Load_PoolData();

            case DataType.Quest :
                return new Load_QuestData();
            
            case DataType.Recipe :
                return new Load_RecipeData();

            case DataType.Skill :
                return new Load_SkillData();
            
            case DataType.Scene :
                return new Load_SceneData();

            case DataType.SFX :
                return new Load_SFXData();

            case DataType.Weapon :
                return new Load_WeaponData();

            case DataType.WalkieTalkie:
                return new Load_WalkieTalkieData();

            case DataType.Voice :
                return new Load_VoiceData();
            
            case DataType.Talk :
                return new Load_TalkData();
            
            case DataType.TimeLine :
                return new Load_TimeLineData();
            
            case DataType.Tutorial:
                return new Load_TutorialData();
            
            default:
                return null;
        }
    }
}

public partial class DataLoader
{
    private static bool isLoad = false;
    public static bool isUpdated = false;
    private static Dictionary<DataType, ILoader> lists = new();
    private static Dictionary<DataType, bool> loadedStatus = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Initialize()
    {
        if(isLoad) return;
        
        //로드
        Loads();

        isLoad = true;
    }
    public static void Ready()
    {
        List<bool> bools = new();
        foreach(var loaded in loadedStatus)
        {
            if(loaded.Value)
            {
                bools.Add(true);
            }
        }
        
        if(bools.Count == loadedStatus.Count)
        {
            LoadStatus.LoadAllData();
        }
    }
    public static void SetReady(DataType type)
    {
        if(loadedStatus.ContainsKey(type))
            loadedStatus[type] = true;
    }
    public static bool IsExist(DataType type)
    {
        if(!lists.ContainsKey(type)) return false;

        if(lists.TryGetValue(type, out var loader))
        {
            return loader.IsExist();
        }

        return false;
    }
    public static T GetData<T>(DataType type, int id)
    {
        if(lists.TryGetValue(type, out var loader))
        {
            if(!loader.IsExist()) return (T)default;

            return (T)loader.GetData(id);
        }

        return (T)default;
    }
    public static int GetLoaderDataCount(DataType type)
    {
        if(lists.TryGetValue(type, out var loader))
        {
            if(!loader.IsExist()) return 0;
            
            return loader.GetCount();
        }

        return 0;
    }
}
public partial class DataLoader
{
    private static void Loads()
    {
        foreach(DataType type in Enum.GetValues(typeof(DataType)))
        {
            if(type == DataType.None) continue;

            AddDataLoader(type);
        }
            
    }
    private static void AddDataLoader(DataType type)
    {
        loadedStatus.Add(type, false);
        lists.Add(type, DataLoaderFactory.GetLoader(type));
    }
}

#region DataLoad
public interface ILoader
{
    object GetData(int id);
    bool IsExist();
    int GetCount();
}

public abstract class Data<T>
{
    protected Dictionary<int, T> list;

    public Data()
    {
        list = new();
    }

    public T Get(int id)
    {
        list.TryGetValue(id, out var value);

        return value;
    }
}

public abstract class DataLoad<T> : Data<T>, ILoader
{
    protected bool isLoad = false;
    private string address;

    public DataLoad(string address)
    {
        this.address = address;
        Initialize();
    }

    protected async Task<Type> TryLoad<Type>()
    {  
        TextAsset asset = await AddressableUtil.Load<TextAsset>(address);

        return JsonUtil.ParseFromJson<Type>(asset);
    }
    protected abstract Task Initialize();

    public object GetData(int id)
    {
        return base.Get(id);
    }

    public bool IsExist()
    {
        return isLoad;
    }

    public int GetCount()
    {
        return list.Count;
    }
}

#region A~
public class Load_AnimationCurveData : DataLoad<AnimationCurve>
{
    public Load_AnimationCurveData() : base("LoadAnimationCurveDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            AnimationCurveDataBase soData = await AddressableUtil.Load<AnimationCurveDataBase>(address);

            for(int j = 0; j < soData.curveLists.Length; j++)
            {
                AnimationCurveData dataSo = soData.curveLists[j];

                list.Add(dataSo.curveId, dataSo.curveData);
            }    
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.AnimationCurve);
        DataLoader.Ready();
    }
}

public class Load_AIPathData : DataLoad<Vector3[]>
{
    public Load_AIPathData() : base("LoadAIPathDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            AIPathDataBase soData = await AddressableUtil.Load<AIPathDataBase>(address);

            for(int j = 0; j < soData.paths.Length; j++)
            {
                AIPathData dataSo = soData.paths[j];
                list.Add(dataSo.pathId, dataSo.GetPath());
            }    
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.AIPath);
        DataLoader.Ready();
    }
}
#endregion

#region B~
public class Load_BGMData : DataLoad<AudioClip>
{

    public Load_BGMData() : base("LoadBGMDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            SoundSoBase soBase = await AddressableUtil.Load<SoundSoBase>(address);
            
            for(int j = 0; j < soBase.sounds.Count; j++)
            {
                SoundDataSO so = soBase.sounds[j];
                list.Add(so.Id, so.clip);
            }
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.BGM);
        DataLoader.Ready();
    }
}
#endregion

#region C~
public class Load_CharacterData : DataLoad<CharacterData>
{
    
    public Load_CharacterData() : base("LoadCharacterDatas") {}

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for(int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            var soData = await AddressableUtil.Load<CharacterDataBase>(address);

            for(int j = 0; j < soData.list.Count; j++)
            {
                CharacterData statData = soData.list[j];
                list.Add(statData.characterDataId, statData);
            }
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.Character);
        DataLoader.Ready();
    }
}

public class Load_CharacterETCData : DataLoad<Character_Prefab_Data>
{
    
    public Load_CharacterETCData() : base("LoadCharacter_Prefab_Datas") {}

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for(int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            var soData = await AddressableUtil.Load<CharacterPrefabData_Database>(address);

            for(int j = 0; j < soData.list.Count; j++)
            {
                Character_Prefab_Data etcData = soData.list[j];
                list.Add(etcData.id, etcData);
            }
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.CharacterETC);
        DataLoader.Ready();
    }
}

#endregion

#region D~

public class Load_DecoIconData : DataLoad<Sprite>
{

    public Load_DecoIconData() : base("LoadDecoIconDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            DecoIconInfo soData = await AddressableUtil.Load<DecoIconInfo>(address);
            
            list.Add(soData.id, soData.icon);
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.DecoIcon);
        DataLoader.Ready();
    }
}

#endregion

#region E~
public class Load_EnemyData : DataLoad<EnemyData>
{

    public Load_EnemyData() : base("LoadEnemyDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            EnemyAreaDataBase soData = await AddressableUtil.Load<EnemyAreaDataBase>(address);

            for(int j = 0; j < soData.database.Count; j++)
            {
                EnemyData enemyDataSo = soData.database[j];

                list.Add(enemyDataSo.enemyId, enemyDataSo);
            }
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.Enemy);
        DataLoader.Ready();
    }
}

public class Load_EnemyPrefabData : DataLoad<GameObject>
{
    public Load_EnemyPrefabData() : base("LoadEnemyPrefabDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            EnemyPrefabDataBase soData = await AddressableUtil.Load<EnemyPrefabDataBase>(address);

            for(int j = 0; j < soData.enemyPrefabDatas.Length; j++)
            {
                EnemyPrefabData dataSo = soData.enemyPrefabDatas[j];

                list.Add(dataSo.prefabId, dataSo.prefab);
            }    
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.EnemyPrefab);
        DataLoader.Ready();
    }
}

public class Load_ElementSpriteData : DataLoad<Sprite>
{
    public Load_ElementSpriteData() : base("LoadElementSpriteDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            ElementSpriteDataBase soData = await AddressableUtil.Load<ElementSpriteDataBase>(address);

            for(int j = 0; j < soData.list.Length; j++)
            {
                ElementSprite dataSo = soData.list[j];

                list.Add(dataSo.elementId, dataSo.elementSprite);
            }    
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.ElementSprite);
        DataLoader.Ready();
    }
}
#endregion

#region F~
#endregion

#region G~
#endregion

#region H~
#endregion

#region I~
public class Load_ItemData : DataLoad<ItemData>
{

    public Load_ItemData() : base("LoadItemDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            ItemDataBaseList soDataBaseList = await AddressableUtil.Load<ItemDataBaseList>(address);
            
            for(int j = 0; j < soDataBaseList.itemDataBase.Count; j++)
            {
                ItemDataBase soDataBase = soDataBaseList.itemDataBase[j];

                for(int t = 0; t < soDataBase.items.Count; t++)
                {
                    ItemData soData = soDataBase.items[t];
                    list.Add(soData.itemInfo.itemId, soData);
                }
            }
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.Item);
        DataLoader.Ready();
    }
}

#endregion

#region J~
#endregion

#region K~
#endregion

#region L~
#endregion

#region N~
public class Load_NPCData : DataLoad<NPCData>
{
    public Load_NPCData() : base("LoadNPCDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            NPCDataBase soData = await AddressableUtil.Load<NPCDataBase>(address);

            for(int j = 0; j < soData.database.Count; j++)
            {
                NPCData npcDataSo = soData.database[j];

                list.Add(npcDataSo.npcId, npcDataSo);
            }    
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.NPC);
        DataLoader.Ready();
    }
}

#endregion

#region M~
#endregion

#region O~
#endregion

#region P~
public class Load_PoolData : DataLoad<GameObject>
{

    public Load_PoolData() : base("LoadPoolDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            PoolDataBaseList soDataList = await AddressableUtil.Load<PoolDataBaseList>(address);

            for(int j = 0; j < soDataList.poolDataBaseList.Count; j++)
            {
                TypePoolDataBase soDataBase = soDataList.poolDataBaseList[j];

                for(int t = 0; t < soDataBase.poolDatas.Count; t++)
                {
                    PoolInfo soData = soDataBase.poolDatas[t];
                    list.Add(soData.pool_id, soData.pool_targetObject);
                }
            }
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.Pool);
        DataLoader.Ready();
    }
}

#endregion

#region Q~
public class Load_QuestData : DataLoad<QuestData>
{

    public Load_QuestData() : base("LoadQuestDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            QuestDataBase asset = await AddressableUtil.Load<QuestDataBase>(address);

            for(int j = 0; j < asset.questDatas.Length; j++)
            {
                QuestData questData = asset.questDatas[j];
                list.Add(questData.questId, questData);
            }
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.Quest);
        DataLoader.Ready();
    }
}

#endregion

#region R~
public class Load_RecipeData : DataLoad<CraftRecipe>
{
    public Load_RecipeData() : base("LoadRecipeDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            CraftRecipeDataBase soData = await AddressableUtil.Load<CraftRecipeDataBase>(address);

            for(int j = 0; j < soData.recipes.Length; j++)
            {
                CraftRecipe dataSo = soData.recipes[j];

                list.Add(dataSo.recipeId, dataSo);
            }    
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.Recipe);
        DataLoader.Ready();
    }
}

#endregion

#region S~
public class Load_SkillData : DataLoad<SkillData>
{

    public Load_SkillData() : base("LoadSkillDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            SkillDataBase soDataBase = await AddressableUtil.Load<SkillDataBase>(address);

            for(int j = 0; j < soDataBase.lists.Count; j++)
            {
                SkillData soData = soDataBase.lists[j];
                list.Add(soData.skillId, soData);
            }
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.Skill);
        DataLoader.Ready();
    }
}
public class Load_SFXData : DataLoad<AudioClip>
{

    public Load_SFXData() : base("LoadSFXDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            SoundSoBase soBase = await AddressableUtil.Load<SoundSoBase>(address);
            
            for(int j = 0; j < soBase.sounds.Count; j++)
            {
                SoundDataSO so = soBase.sounds[j];
                list.Add(so.Id, so.clip);
            }
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.SFX);
        DataLoader.Ready();
    }
}
public class Load_SceneData : DataLoad<SceneData>
{
    public Load_SceneData() : base("LoadSceneDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            SceneDataBase soData = await AddressableUtil.Load<SceneDataBase>(address);

            for(int j = 0; j < soData.lists.Count; j++)
            {
                SceneData dataSo = soData.lists[j];

                list.Add(dataSo.id, dataSo);
            }    
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.Scene);
        DataLoader.Ready();
    }
}

#endregion

#region T~
public class Load_TalkData : DataLoad<TalkData>
{
    public Load_TalkData() : base("LoadTalkDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            TalkDataBase soData = await AddressableUtil.Load<TalkDataBase>(address);

            for(int j = 0; j < soData.lists.Length; j++)
            {
                TalkData talkData = soData.lists[j];
                list.Add(talkData.talkId, talkData);
            }
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.Talk);
        DataLoader.Ready();
    }
}

public class Load_TimeLineData : DataLoad<TimeLineAsset>
{
    public Load_TimeLineData() : base("LoadTimeLineDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            TimeLineAssetBase timeLineBase = await AddressableUtil.Load<TimeLineAssetBase>(address);

            for(int j = 0; j < timeLineBase.timeLineDirectorObject.Length; j++)
            {
                TimeLineAsset asset = timeLineBase.timeLineDirectorObject[j];
  
                list.Add(asset.timeLineId, asset);
            }
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.TimeLine);
        DataLoader.Ready();
    }
}

public class Load_TutorialData : DataLoad<TutorialData>
{
    public Load_TutorialData() : base("LoadTutorialDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            TutorialDataBase dataBase = await AddressableUtil.Load<TutorialDataBase>(address);

            for(int j = 0; j < dataBase.list.Length; j++)
            {
                TutorialData asset = dataBase.list[j];
  
                list.Add(asset.tutorialId, asset);
            }
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.Tutorial);
        DataLoader.Ready();
    }
}

#endregion

#region U~
#endregion

#region V~
public class Load_VoiceData : DataLoad<VoiceDataSO>
{

    public Load_VoiceData() : base("LoadVoiceDatas") { }

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for (int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            VoiceDataSO soData = await AddressableUtil.Load<VoiceDataSO>(address);
            
            list.Add(soData.characterId, soData);
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.Voice);
        DataLoader.Ready();
    }
}

#endregion

#region W~
public class Load_WeaponData : DataLoad<WeaponStatData>
{
    
    public Load_WeaponData() : base("LoadWeaponDatas") {}

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for(int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            WeaponStatDataBase soData = await AddressableUtil.Load<WeaponStatDataBase>(address);

            for(int j = 0; j < soData.list.Count; j++)
            {
                WeaponStatData weaponData = soData.list[j];
                list.Add(weaponData.weaponId, weaponData);
            }
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.Weapon);
        DataLoader.Ready();
    }
}

public class Load_WalkieTalkieData : DataLoad<WalkieTalkieData>
{
    
    public Load_WalkieTalkieData() : base("LoadWalkieTalkieDatas") {}

    protected async override Task Initialize()
    {
        var data = await TryLoad<Addresses>();

        for(int i = 0; i < data.addresses.Count; i++)
        {
            string address = data.addresses[i].address;

            WalkieTalkieDataBase soData = await AddressableUtil.Load<WalkieTalkieDataBase>(address);

            for(int j = 0; j < soData.list.Length; j++)
            {
                WalkieTalkieData walkieTalkieData = soData.list[j];
                list.Add(walkieTalkieData.walkieTalkieDataId, walkieTalkieData);
            }
        }

        isLoad = true;

        SetReady();
    }

    protected virtual void SetReady()
    {
        DataLoader.SetReady(DataType.WalkieTalkie);
        DataLoader.Ready();
    }
}

#endregion

#region X~
#endregion

#region Y~
#endregion

#region Z~
#endregion

#endregion