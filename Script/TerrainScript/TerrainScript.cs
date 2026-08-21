using UnityEngine;
using System.Collections.Generic;

public enum TerrainQuality
{
    High,
    Normal,
    Low
}
public class TerrainScript : MonoBehaviour
{
    private List<Terrain> terrains = new();

    private void OnEnable()
    {
        InitializeList();
        InitializeEvent();
    }

    private void InitializeList()
    {
        Terrain[] terrainInChildren = transform.GetComponentsInChildren<Terrain>();

        for(int i = 0; i < terrainInChildren.Length; i++)
            terrains.Add(terrainInChildren[i]);
    }

    private void InitializeEvent()
    {
        EventBus.Sub<TerrainQuality>("SetTerrainQuality", SetQuality);
    }

    private void OnDisable()
    {
        EventBus.UnSub<TerrainQuality>("SetTerrainQuality", SetQuality);
    }

    private void SetQuality(TerrainQuality quality)
    {
        switch(quality)
        {
            case TerrainQuality.High:
                Loop( 
                    20,
                    200f,
                    200f,
                    0.5f,
                    100f,
                    50f
                );
                break;
            case TerrainQuality.Normal:
                Loop( 
                    50,
                    100f,
                    100f,
                    0.3f,
                    50f,
                    30f 
                );
                break;
            case TerrainQuality.Low:
                Loop( 
                    100,
                    50f,
                    50f,
                    0.15f,
                    50f,
                    20f
                );
                break;
        }
    }

    private void SetTerrainPixelError(Terrain terrain, int value)
    {
        terrain.heightmapPixelError = value;
    }

    private void SetTerrainBaseMapDistance(Terrain terrain, float value)
    {
        terrain.basemapDistance = value;
    }

    private void SetTerrainDetailDistance(Terrain terrain, float value)
    {
        terrain.detailObjectDistance = value;
    }

    private void SetTerrainDetailDensity(Terrain terrain, float value)
    {
        terrain.detailObjectDensity = value;
    }

    private void SetTerrainTreeDistance(Terrain terrain, float value)
    {
        terrain.treeDistance = value;
    }

    private void SetTerrainTreeBillboardDistance(Terrain terrain, float value)
    {
        terrain.treeBillboardDistance = value;
    }

    private void Loop(  int pixelErrorValue, 
                        float baseMapDistanceValue,
                        float detailDistanceValue,
                        float detailDensityValue,
                        float treeDistanceValue,
                        float treeBillboardDistanceValue)
    {
        for(int i = 0; i < terrains.Count; i++)
        {
            Terrain terrain = terrains[i];
            TerrainData terrainData = terrain.terrainData;

            SetTerrainPixelError(terrain, pixelErrorValue);
            SetTerrainBaseMapDistance(terrain, baseMapDistanceValue);
            SetTerrainDetailDistance(terrain, detailDistanceValue);
            SetTerrainDetailDensity(terrain, detailDensityValue);
            SetTerrainTreeDistance(terrain, treeDistanceValue);
            SetTerrainTreeBillboardDistance(terrain, treeBillboardDistanceValue);
        }
    }
}
