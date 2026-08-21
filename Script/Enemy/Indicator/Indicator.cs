using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "InDicator", menuName = "Enemy/InDicator")]
public class Indicator : ScriptableObject
{   
    public float endActive_Time;

    public GameObject GetIndicator(int indicatorId)
    {
        GameObject indicator = EventBus.Invoke_Func<int, GameObject>("Pool_GetGameObject", indicatorId);
        return indicator;
    }

    public IEnumerator IndicatorLoop(GameObject indicator, Vector3 position, Quaternion rotation)
    {
        indicator.transform.position = position;
        indicator.transform.rotation = rotation;
        indicator.SetActive(true);

        yield return YieldUtil.WaitForSeconds(endActive_Time);
        indicator.SetActive(false);
    }
}