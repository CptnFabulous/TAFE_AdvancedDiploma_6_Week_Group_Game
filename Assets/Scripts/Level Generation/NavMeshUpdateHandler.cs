using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshUpdateHandler : MonoBehaviour
{
    static NavMeshUpdateHandler currentReference;
    public static NavMeshUpdateHandler Current
    {
        get
        {
            if (currentReference == null)
            {
                currentReference = FindObjectOfType<NavMeshUpdateHandler>();
            }
            return currentReference;
        }
    }

    //[Head]
    public bool autoFindTerrain;
    public LayerMask autoFindTerrainLayers;
    public Bounds outerLimits;
    public bool autoGenerateBounds;

    [Header("Additional NavMesh settings")]
    public int settingsIndex = 0;
    public int areaIndex = 0;
    public float minRegionArea = 0.2f;

    NavMeshData currentMesh;
    NavMeshBuildSettings settings;
    List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
    
    public void AddSource(MeshFilter terrainMesh)
    {
        NavMeshBuildSource newSource = new NavMeshBuildSource
        {
            sourceObject = terrainMesh.sharedMesh,
            shape = NavMeshBuildSourceShape.Mesh,
            transform = terrainMesh.transform.localToWorldMatrix,
            size = Vector3.one, // Not sure if this is necessary
            area = areaIndex, // In a complex game, change this depending on the properties of the environment object.
        };
        sources.Add(newSource);

        if (autoGenerateBounds == true)
        {
            MeshRenderer renderer = terrainMesh.GetComponent<MeshRenderer>();
            // If array length is only one, replace bounds
            if (sources.Count == 1)
            {
                outerLimits = renderer.bounds;
            }
            else
            {
                // If array is longer, expand existing bounds to accomodate new bounds
                Vector3 min = Vector3.Min(outerLimits.min, renderer.bounds.min);
                Vector3 max = Vector3.Max(outerLimits.max, renderer.bounds.max);
                outerLimits.min = min;
                outerLimits.max = max;
            }
        }
        
    }

    public void SetupMesh()
    {
        settings = NavMesh.GetSettingsByID(settingsIndex);
        settings.minRegionArea = minRegionArea;

        if (autoFindTerrain)
        {
            List<NavMeshBuildSource> autoGatheredSources = new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(outerLimits, autoFindTerrainLayers, NavMeshCollectGeometry.RenderMeshes, areaIndex, null, autoGatheredSources);
            sources.AddRange(autoGatheredSources);
        }

        currentMesh = NavMeshBuilder.BuildNavMeshData(settings, sources, outerLimits, transform.position, transform.rotation);
        NavMesh.AddNavMeshData(currentMesh);
    }

    public void RebakeMesh()
    {
        if (currentMesh == null || sources.Count <= 0)
        {
            return;
        }

        if (NavMeshBuilder.UpdateNavMeshData(currentMesh, settings, sources, outerLimits))
        {
            Debug.Log("Nav mesh successfully updated");
        }
        else
        {
            Debug.Log("Nav mesh update failed");
        }
    }
}
