using System;
using System.Collections;
using UnityEngine;
public class CharacterVFXSetter
{
    private readonly Transform normalParentTr, finalParentTr;
    private int vfxId, finalVfxId;
    public CharacterVFXSetter(Transform normalParentTr, Transform finalParentTr)
    {
        this.normalParentTr = normalParentTr;
        this.finalParentTr  = finalParentTr;
    }

    
    public void SetVFX(int normalAttack_VFXId, int finalAttack_VFXId)
    {
        if (vfxId == normalAttack_VFXId && finalVfxId == finalAttack_VFXId) return;

        vfxId       = normalAttack_VFXId;
        finalVfxId  = finalAttack_VFXId;

        DestroyChild();
        CreateVFX(normalAttack_VFXId, finalAttack_VFXId);
    }

    //매프레임, 매번 바뀌는거 X, 특정상활 일때만 바뀜 -> 굳이 풀링을해서 공간차지 하지않는것이 오히려 좋을것같음
    private void DestroyChild()
    {
        if(normalParentTr != null)
        {
            for (int i = normalParentTr.childCount - 1; i >= 0 ; i--)
            {
                GameObject child = normalParentTr.GetChild(i).gameObject;

                if (child == null) continue;
                UnityEngine.Object.DestroyImmediate(child);
            }
        }
        
        if (finalParentTr != null)
        {
            for (int i = finalParentTr.childCount - 1; i >= 0; i--)
            {
                GameObject child = finalParentTr.GetChild(i).gameObject;

                if (child == null) continue;
                UnityEngine.Object.Destroy(child);
            }
        }
    }

    private void CreateVFX(int normalAttack_VFXId, int finalAttack_VFXId)
    {
        if(normalAttack_VFXId > 0)
        {
            GameObject normalAttackVfxPrefab = DataLoader.GetData<GameObject>(DataType.Pool, normalAttack_VFXId);
            GameObject newVFX_Normal = UnityEngine.Object.Instantiate(normalAttackVfxPrefab);
            newVFX_Normal.transform.SetParent(normalParentTr, false);
            newVFX_Normal.transform.localPosition = Vector3.zero;
        }

        if(finalAttack_VFXId > 0)
        {
            GameObject finalAttackVfxPrefab = DataLoader.GetData<GameObject>(DataType.Pool, finalAttack_VFXId);
            GameObject newVFX_Final = UnityEngine.Object.Instantiate(finalAttackVfxPrefab);
            newVFX_Final.transform.SetParent(finalParentTr, false);
            newVFX_Final.transform.localPosition = Vector3.zero;
        }
    }
}
