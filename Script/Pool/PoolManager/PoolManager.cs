using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class PoolManager  : MonoBehaviour 
{
    private SoundPool soundPool;
    private DamageTextPool damageTextPool;
    private GameObjectPool gameObjectPool;
    private EnemyPool enemyPool;
    private ProjectilePool projectilePool;
    private ItemObjectPool itemObjectPool;

    private void OnEnable()
    {
        soundPool      = new(this.transform.Find("SoundPoolContainer")); 
        gameObjectPool = new(this.transform.Find("GameObjectPoolContainer")); 
        enemyPool      = new(this.transform.Find("EnemyContainer"));
        damageTextPool = new(this.transform.Find("TextPoolContainer"));
        projectilePool = new(this.transform.Find("ProjectileContainer"));
        itemObjectPool = new(this.transform.Find("ItemObjectContainer"));

        LoadStatus.SetStatus(ManagerType.Pool, false);
    }

    private void OnDisable()
    {
        LoadStatus.SetStatus(ManagerType.Pool, false);

        soundPool.OnDisable();
        gameObjectPool.OnDisable();
        damageTextPool.OnDisable();
        projectilePool.OnDisable();
        itemObjectPool.OnDisable();
    }
}

public struct PoolInitIds
{
    public List<int> ids;
}

public class Pool<T>
{
    private Transform container;
    private Dictionary<int, List<T>> lists;
    private int poolCount = 5;
    private readonly DataType dataType;
    private readonly System.Func<T ,bool> conditionMethod;
    private readonly System.Func<int, List<T>> initializeListMethod;
    private readonly string instantiateObjectNameDefine;
    public Pool(Transform containerTr = null, 
                System.Func<T, bool> conditionMethod = null, 
                System.Func<int, List<T>> initializeListMethod = null, 
                int capacity = 1, 
                DataType type = DataType.None, 
                string newObjName = "",
                PoolInitIds initInfo = default)
    {
        lists = new();
        if(containerTr != null) container = containerTr;

        poolCount = capacity;
        this.dataType = type;

        if(conditionMethod != null) this.conditionMethod = conditionMethod;
        if(initializeListMethod != null) this.initializeListMethod = initializeListMethod;

        if(!string.IsNullOrEmpty(newObjName))
            instantiateObjectNameDefine = newObjName;

        if(initInfo.ids != null && initInfo.ids.Count > 0)
        {
            for(int i = 0; i < initInfo.ids.Count; i++)
            {
                int id = initInfo.ids[i];
                _ = GetFromPool(id);
            }
        }
    }

    public T GetFromPool(int id, GameObject prefab = default, string name = "")
    {
        if(lists.TryGetValue(id, out var list))
        {
            for(int i = 0; i < list.Count; i++)
            {
                var t = list[i];

                if(conditionMethod.Invoke((T)t))
                    return t;
            }
        }
        
        bool isExeception = false;

        if(!lists.ContainsKey(id)) lists[id] = new List<T>();

        if(initializeListMethod != null)
        {
            List<T> newList = initializeListMethod.Invoke(id);
            for(int i = 0; i < newList.Count; i++)
                lists[id].Add(newList[i]);

            return GetFromPool(id);
        }
        else if(prefab == default)
        {
            isExeception = LoadPrefabUseDataLoader(id, name);

            if(isExeception) return (T)default;

            return GetFromPool(id, prefab, name);
        }
        else if(prefab != default)
        {
            isExeception = LoadUsePrefab(id, prefab, name);

            if(isExeception) return (T)default;

            return GetFromPool(id, prefab, name);
        }

        return (T)default;
    }

    #region Load_Case
    private bool LoadPrefabUseDataLoader(int id, string name = "")
    {
        if(dataType != DataType.None)
        {
            GameObject prefab = DataLoader.GetData<GameObject>(dataType, id);

            if(typeof(T) == typeof(GameObject))
            {
                if(prefab == null) return true;

                GameObject obj = GameObject.Instantiate(prefab);
                obj.hideFlags = HideFlags.DontSaveInEditor;

                obj.transform.SetParent(container);

                if(string.IsNullOrEmpty(name))
                    SetName($"{id}", obj);
                else
                    SetName(name, obj);

                obj.SetActive(false);

                if(obj is T t)
                    lists[id].Add(t);

                return false;
            }
            else if(typeof(Component).IsAssignableFrom(typeof(T)))
            {
                if(prefab == null) return true;

                GameObject obj = GameObject.Instantiate(prefab);
                obj.hideFlags = HideFlags.DontSaveInEditor;

                obj.transform.SetParent(container);

                if(string.IsNullOrEmpty(name))
                        SetName($"{id}", obj);
                else
                    SetName(name, obj);

                obj.SetActive(false);

                T comp = obj.GetComponent<T>();

                if(comp != null)
                    lists[id].Add(comp);

                return false;
            }
        }

        return true;
    }

    private bool LoadUsePrefab(int id, GameObject prefab, string name = "")
    {
        if(typeof(T) == typeof(GameObject))
        {
            GameObject objPrefab = prefab;

            if(objPrefab == null) return true;

            GameObject obj = GameObject.Instantiate(objPrefab);
            obj.hideFlags = HideFlags.DontSaveInEditor;

            obj.transform.SetParent(container);

            if(string.IsNullOrEmpty(name))
                SetName($"{id}", obj);
            else
                SetName(name, obj);

            obj.SetActive(false);

            if(obj is T t)
                lists[id].Add(t);

            return false;
        }
        else if(typeof(Component).IsAssignableFrom(typeof(T)))
        {
            GameObject objPrefab = prefab;

            if(objPrefab == null) return true;

            GameObject obj = GameObject.Instantiate(objPrefab);
            obj.transform.SetParent(container);

            if(string.IsNullOrEmpty(name))
                SetName($"{id}", obj);
            else
                SetName(name, obj);

            obj.SetActive(false);

            var comp = obj.GetComponent<T>();

            if(comp is T TComp && TComp != null) lists[id].Add(TComp);

            return false;
        }
        else if(typeof(T).IsInterface)
        {
            GameObject objPrefab = prefab;

            if(objPrefab == null) return true;
            
            GameObject obj = GameObject.Instantiate(objPrefab);
            obj.transform.SetParent(container);

            if(string.IsNullOrEmpty(name))
                SetName($"{id}", obj);
            else
                SetName(name, obj);

            obj.SetActive(false);

            var comp = obj.GetComponent<T>();

            if(comp is T TComp && TComp != null) lists[id].Add(TComp);
            
            return false;
        }

        return true;
    }
    #endregion

    private void SetName(string name, GameObject obj)
    {
        obj.name = name;
    }
}