using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

[CreateAssetMenu(fileName = "AIPathData", menuName = "AIPath/AIPathData")]
public class AIPathData : ScriptableObject
{
    public int pathId;
    public GameObject pathPrefab;
    public Vector3[] GetPath()
    {
        if(pathPrefab != null)
        {
            SplineContainer container = pathPrefab.GetComponent<SplineContainer>();
            Spline spline = container.Spline;

            Vector3[] newPathList = new Vector3[spline.Count];

            int index = 0;
            foreach(var knot in spline.Knots)
            {
                newPathList[index] = container.transform.TransformPoint(knot.Position);
                index++;
            }

            return newPathList;
        }

        return null;
    }
}
