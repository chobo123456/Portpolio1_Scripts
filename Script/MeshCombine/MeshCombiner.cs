using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    private LODGroup group;
    private MeshFilter[] meshFilter;
    private MeshRenderer[] meshRenderer;
    private MeshCollider meshCollider;

    void Start()
    {
        meshFilter = new MeshFilter[2];
        meshRenderer = new MeshRenderer[2];

        Process(transform.FindTarget("LOD0"), 0);
        Process(transform.FindTarget("LOD1"), 1);

        SetLOD();
    }

    private void Process(Transform parentTr, int index)
    {
        Material originalMaterial = GetMaterial(parentTr);

        if(!HasSameMaterialInChildren(parentTr, originalMaterial))
        {
            Util.Log("Process() 같은 머티리얼이 아님!");
            return;
        } 

        CombineInstance[] newInstance = GetCombineMeshes(parentTr);

        if(newInstance == null) return;

        SettingMesh(newInstance, originalMaterial, parentTr, index);
    }

    private Material GetMaterial(Transform parentTr)
    {
        return parentTr.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;   
    }

    private bool HasSameMaterialInChildren(Transform targetTr, Material originalMaterial)
    {
        MeshRenderer[] childRenderer = targetTr.GetComponentsInChildren<MeshRenderer>();

        for(int i = 1; i < childRenderer.Length; i++)
        {
            if(childRenderer[i].sharedMaterial != originalMaterial)
            {
                return false;
            }
        }

        return true;
    }

    private CombineInstance[] GetCombineMeshes(Transform targetTr)
    {
        MeshFilter[] childFilter = targetTr.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] meshes = new CombineInstance[childFilter.Length];

        Matrix4x4 parentWorldMatrix = transform.parent.worldToLocalMatrix;
        for(int i = 0; i < childFilter.Length; i++)
        {
            MeshFilter filter = childFilter[i];
            meshes[i] = new CombineInstance
            {
                mesh        = filter.sharedMesh,
                transform   = parentWorldMatrix * filter.transform.localToWorldMatrix
            };
            filter.gameObject.SetActive(false);
        }

        return meshes;
    }
    

    private void SettingMesh(CombineInstance[] meshes, Material originalMaterial, Transform targetTr, int index)
    {
        meshFilter[index]      = targetTr.gameObject.AddComponent<MeshFilter>();
        meshRenderer[index]    = targetTr.gameObject.AddComponent<MeshRenderer>();
        
        Mesh newMesh = new Mesh();
        newMesh.CombineMeshes(meshes);

        meshFilter[index].mesh = newMesh;
        meshRenderer[index].material = originalMaterial;

        if(index == 0) 
        {
            meshCollider = targetTr.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = newMesh;
        }

        for(int i = 0; i < targetTr.childCount; i++)
        {
            GameObject childObj = targetTr.GetChild(i).gameObject;
            childObj.isStatic = true;
        }
    }

    private void SetLOD()
    {
        if(meshRenderer.Length <= 0) return;
        
        group = this.gameObject.AddComponent<LODGroup>();

        LOD[] lods = new LOD[2];

        Renderer[] renderers = new Renderer[1];
        renderers[0] = meshRenderer[0];
        lods[0] = new LOD(0.15f, renderers);

        Renderer[] renderers1 = new Renderer[1];
        renderers1[0] = meshRenderer[1];
        lods[1] = new LOD(0.05f, renderers1);

        group.SetLODs(lods);
        group.RecalculateBounds();
    }
}
