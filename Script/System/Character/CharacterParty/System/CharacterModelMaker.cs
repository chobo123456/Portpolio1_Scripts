using UnityEngine;

public class CharacterModelMaker
{
    public GameObject GetModel(Transform parent, GameObject modelPrefab)
    {
        GameObject newObj = GameObject.Instantiate(modelPrefab);
        newObj.transform.SetParent(parent);
        newObj.transform.localPosition = Vector3.zero;
        newObj.transform.localRotation = Quaternion.identity;
        
        return newObj;
    }
}

