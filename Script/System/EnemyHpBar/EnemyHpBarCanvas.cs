using UnityEngine;
using System.Collections.Generic;

public class EnemyHpBarCanvas : MonoBehaviour
{
    public GameObject hpPrefab;
    private Dictionary<MonoBehaviour, EnemyHpBarUI> models = new();
    private List<EnemyHpBarUI> pool = new();
    private RectTransform mainCanvas;

    private void OnEnable()
    {
        mainCanvas = GetComponent<RectTransform>();

        InitPool();

        EventBus.Sub<MonoBehaviour, HPBar_ViewModel>("EnemyRegisterViewModel", Register);
        EventBus.Sub<MonoBehaviour>("EnemyUnRegisterViewModel", UnRegister);
    }

    private void OnDisable()
    {
        EventBus.UnSub<MonoBehaviour, HPBar_ViewModel>("EnemyRegisterViewModel", Register);
        EventBus.UnSub<MonoBehaviour>("EnemyUnRegisterViewModel", UnRegister);
    }

    private void Register(MonoBehaviour mono, HPBar_ViewModel viewModel)
    {
        if(!models.ContainsKey(mono))
        {
            EnemyHpBarUI hpUI = GetHpBarUI();
            hpUI.SetViewModel(viewModel);
            hpUI.gameObject.SetActive(true);
            models.Add(mono, hpUI);  
        }
    }

    private void UnRegister(MonoBehaviour mono)
    {
        if(models.TryGetValue(mono, out EnemyHpBarUI hpUI))
        {
            hpUI.OnDisable();
            hpUI.gameObject.SetActive(false);
            models.Remove(mono);   
        }
    }

    private void LateUpdate()
    {
        foreach(var map in models)
        {
            EnemyHpBarUI hpbar = map.Value;
            MonoBehaviour mono = map.Key;

            if(hpbar.IsUnUsable)
            {
                Vector3 world = mono.transform.position + Vector3.up;
                Vector2 screen = Camera.main.WorldToScreenPoint(world);

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    mainCanvas,
                    screen,
                    null,
                    out Vector2 local
                );

                hpbar.SetPosition(local);
            }
        }
    }

    private void InitPool()
    {
        for(int i = 0; i < 20; i++)
        {
            GameObject newObj = Object.Instantiate(hpPrefab);
            newObj.SetActive(false);

            newObj.transform.SetParent(this.transform, false);

            var comp = newObj.GetComponent<EnemyHpBarUI>();

            if(comp != null)
            {
                comp.Initialize();
                pool.Add(comp);
            }
        }
    }

    private EnemyHpBarUI GetHpBarUI()
    {
        for(int i = 0; i < pool.Count; i++)
        {
            var hpBar = pool[i];

            if(!hpBar.IsUnUsable)
                return hpBar;
        }

        InitPool();

        return GetHpBarUI();
    }
}
