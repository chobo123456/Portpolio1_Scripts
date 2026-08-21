using UnityEngine;
using System.Collections.Generic;

public class EnemyCullingGroupManager : MonoBehaviour
{
    public Camera mainCamera;
    public float[] cullDistance;
    private Dictionary<ICullable, int> map = new(100);
    private List<ICullable> enemies = new(100);
    private int count = 0;
    public int cullCount = 100;
    private CullingGroup cullingGroup;
    private BoundingSphere[] bounds;

    private float timeCounter = 0f, updateInterval = 0.2f;

    private void OnEnable()
    {
        EventBus.Sub<ICullable>("SubCull", Register);
        EventBus.Sub<ICullable>("UnSubCull", UnRegister);

        if(bounds == null)
        {
            bounds = new BoundingSphere[cullCount];
            for(int i = 0; i < cullCount; i++)
                bounds[i] = new BoundingSphere(Vector3.zero, 1f);
        }

        if(cullingGroup == null)
        {
            cullingGroup = new CullingGroup();
            cullingGroup.targetCamera = mainCamera;
            cullingGroup.SetBoundingSpheres(bounds);
            cullingGroup.SetBoundingSphereCount(0);
            cullingGroup.SetDistanceReferencePoint(mainCamera.transform);
            cullingGroup.SetBoundingDistances(cullDistance);
            cullingGroup.onStateChanged = OnStateChange;
        }
    }

    private void Register(ICullable cullTarget)
    {
        if(count == bounds.Length)
        {
            System.Array.Resize(ref bounds, count * 2);
            cullingGroup.SetBoundingSpheres(bounds);
        }

        bounds[count] = new BoundingSphere(cullTarget.GetTRS().position, 1f);
        enemies.Add(cullTarget);
        map[cullTarget] = count;
        count++;
        cullingGroup.SetBoundingSphereCount(count);
    }

    private void UnRegister(ICullable cullTarget)
    {
        if(cullingGroup == null || cullTarget == null || !map.TryGetValue(cullTarget, out int value)) return;
        
        cullingGroup.EraseSwapBack(value);
        CullingGroup.EraseSwapBack(value, bounds, ref count);

        int lastIndex = enemies.Count - 1;
        var lastIndexEnemy = enemies[lastIndex];
        enemies[value] = lastIndexEnemy;
        enemies.RemoveAt(lastIndex);

        if(lastIndexEnemy.GetTRS()) map[lastIndexEnemy] = value;
        map.Remove(cullTarget);
    }

    private void Update()
    {
        timeCounter += Time.deltaTime;
        
        if(timeCounter >= updateInterval)
        {
            for(int i = 0; i < cullCount; i++)
            {
                if(i < enemies.Count)
                {
                    if(enemies[i] != null)
                    {
                        bounds[i].position = enemies[i].GetTRS().position;
                    }
                }
            }

            timeCounter = 0f;
        }
    }

    private void OnStateChange(CullingGroupEvent evt)
    {
        int index               = evt.index;
        float currentDistance   = evt.currentDistance;

        ICullable enemyObj = enemies[index]; 

        if((currentDistance == 0 || currentDistance == 1) && (evt.isVisible || evt.wasVisible))
        {
            enemyObj.ToggleOn();
        }
        else if(currentDistance == 1 && !evt.isVisible)
        {
            enemyObj.DisableUpdate();
        }
        else if(currentDistance == 2)
        {
            enemyObj.ToggleOff();
        }
    }

    private void OnDestroy()
    {
        if(cullingGroup != null)
        {
            cullingGroup.Dispose();
            cullingGroup = null;
        }
    }
}