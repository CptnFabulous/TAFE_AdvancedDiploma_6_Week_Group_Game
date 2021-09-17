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

    public List<MeshFilter> terrainMeshes;
    public LayerMask terrainDetectionLayers;
    public Bounds outerLimits;
    public bool autoGenerateBounds;

    [Header("Additional NavMesh settings")]
    public int settingsIndex = 0;
    public int areaIndex = 0;
    public float minRegionArea = 0.2f;

    NavMeshData currentMesh;
    NavMeshBuildSettings settings;
    List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
    /*
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
    }
    */



    public void SetupMesh()
    {
        settings = NavMesh.GetSettingsByID(settingsIndex);
        settings.minRegionArea = minRegionArea;



        /*
        List<NavMeshBuildSource> autoGatheredSources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(outerLimits, terrainDetectionLayers, NavMeshCollectGeometry.RenderMeshes, areaIndex, null, autoGatheredSources);
        sources.AddRange(autoGatheredSources);
        */

        for (int i = 0; i < terrainMeshes.Count; i++)
        {
            NavMeshBuildSource newSource = new NavMeshBuildSource
            {
                sourceObject = terrainMeshes[i].sharedMesh,
                shape = NavMeshBuildSourceShape.Mesh,
                transform = terrainMeshes[i].transform.localToWorldMatrix,
                size = Vector3.one, // Not sure if this is necessary when adding meshes from the world
                area = areaIndex, // In a complex game, change this depending on the properties of the environment object.
            };
            sources.Add(newSource);
        }

        currentMesh = NavMeshBuilder.BuildNavMeshData(settings, sources, outerLimits, transform.position, transform.rotation);
        NavMesh.AddNavMeshData(currentMesh);
    }

    public void RebakeMesh()
    {
        if (terrainMeshes.Count <= 0)
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
