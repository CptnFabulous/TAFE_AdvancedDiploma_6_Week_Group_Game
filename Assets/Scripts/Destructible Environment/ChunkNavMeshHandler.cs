﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using UnityEditor.AI;

public class ChunkNavMeshHandler : MonoBehaviour
{
    public Chunk[] chunksToManageMeshesOf;
    public Bounds outerLimits;


    NavMeshData currentMesh;
    NavMeshBuildSettings settings;
    List<NavMeshBuildSource> sources;

    public void BakeMeshForFirstTime()
    {
        settings = NavMesh.GetSettingsByID(0);
        settings.minRegionArea = 0.2f;

        sources = new List<NavMeshBuildSource>();
        for (int i = 0; i < chunksToManageMeshesOf.Length; i++)
        {
            //Debug.Log(chunksToManageMeshesOf[i].terrainMesh.vertices.Length);
            NavMeshBuildSource newSource = new NavMeshBuildSource();
            newSource.sourceObject = chunksToManageMeshesOf[i].terrainMesh;
            newSource.shape = NavMeshBuildSourceShape.Mesh;
            newSource.area = 0;
            newSource.size = Vector3.one;
            newSource.transform = chunksToManageMeshesOf[i].transform.localToWorldMatrix;
            sources.Add(newSource);
        }

        currentMesh = NavMeshBuilder.BuildNavMeshData(settings, sources, outerLimits, transform.position, transform.rotation);
        NavMesh.AddNavMeshData(currentMesh);
    }

    public void RebakeMesh()
    {
        if (chunksToManageMeshesOf.Length <= 0)
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


        //NavMeshAgent na = GetComponent<NavMeshAgent>();
        //na.

        //NavMeshData updatedMesh = NavMeshBuilder.BuildNavMeshData(settings, sources, 

        //if (NavMeshBuilder.UpdateNavMeshData())
    }
}