using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageTextPool : PoolManagerBase<DamageText>
{
    private int textId = 0;

    private GameObject prefab;

    public DamageTextPool(Transform targetTr)
    {
        Initialize_Prefab(targetTr);

        SetPool(
            containerTr : targetTr, 
            conditionMethod : Condition, 
            capacity : 5, 
            newObjName : "DamageText"); 

        EventBus.Sub<float, Vector3, Element>("SetDamageText", SetText);

        targetTr.parent.GetComponent<MonoBehaviour>().RunRoutine(Initialize_Text());
    }

    public void OnDisable()
    {
        EventBus.UnSub<float, Vector3, Element>("SetDamageText", SetText);
    }

    private async void Initialize_Prefab(Transform tr)
    {
        MonoBehaviour mono = tr.parent.GetComponent<MonoBehaviour>();
        prefab = await AddressableUtil.Load_Instant<GameObject>("DamageText", mono.GetCancelOnDestroy());
    }
    IEnumerator Initialize_Text()
    {
        yield return new WaitUntil(() => prefab != null);
        
        _ = base.GetFromPool(textId, prefab);
    }

    private bool Condition(DamageText text)
    {
        return !text.gameObject.activeSelf;
    }

    private void SetText(float amount, Vector3 position, Element element)
    {
        DamageText text = base.GetFromPool(textId, prefab);

        text.transform.position = position;
        text.gameObject.SetActive(true);
        text.SetText(amount);

        text.SetColor(GetElementColor(element));
    }

    private Color GetElementColor(Element element)
    {
        switch(element)
        {
            case Element.Light:
                return Color.white;
            case Element.Dark:
                return Color.black;
            case Element.Fire:
                return Color.red;  
            case Element.Water:
                return Color.cyan; 
            case Element.Wind:
                return Color.green;  
            case Element.Ground:
                return Color.brown;  
            default:
                return Color.red;
        }
    }
}