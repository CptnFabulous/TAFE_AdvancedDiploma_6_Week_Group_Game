using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkGenerator
{

    public static Block[,,] Flood(Vector3Int lengths, BlockData type)
    {
        //float time = Time.realtimeSinceStartup;
        Block[,,] chunk = new Block[lengths.x, lengths.y, lengths.z];
        for (int x = 0; x < lengths.x; x++)
        {
            //Debug.Log("Generating blocks on new X layer at " + time);
            for (int y = 0; y < lengths.y; y++)
            {
                //Debug.Log("Generating blocks on new Y layer at " + time);
                for (int z = 0; z < lengths.z; z++)
                {
                    //Debug.Log("Generating block on new Z layer at " + time);
                    chunk[x, y, z].Replace(type);
                }
            }
        }

        return chunk;
    }
    public static Block[,,] Hollow(Vector3Int lengths, BlockData type)
    {
        Block[,,] chunk = new Block[lengths.x, lengths.y, lengths.z];
        for (int x = 0; x < lengths.x; x++)
        {
            for (int y = 0; y < lengths.y; y++)
            {
                for (int z = 0; z < lengths.z; z++)
                {
                    if (x > 0 && x < lengths.x - 1 && y > 0 && y < lengths.y - 1 && z > 0 && z < lengths.z - 1)
                    {
                        continue;
                    }

                    chunk[x, y, z].Replace(type);
                }
            }
        }

        return chunk;
    }
    public static Block[,,] OnlyFloor(Vector3Int lengths, BlockData type)
    {
        Block[,,] chunk = new Block[lengths.x, lengths.y, lengths.z];
        for (int x = 0; x < lengths.x; x++)
        {
            for (int y = 0; y < lengths.y; y++)
            {
                for (int z = 0; z < lengths.z; z++)
                {
                    if (y != 0)
                    {
                        continue;
                    }

                    chunk[x, y, z].Replace(type);
                }
            }
        }

        return chunk;
    }
    public static Block[,,] Empty(Vector3Int lengths)
    {
        return new Block[lengths.x, lengths.y, lengths.z];
    }

    public static readonly Vector3Int[] cardinalDirections = new Vector3Int[6]
    {
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.up,
        Vector3Int.down,
        new Vector3Int(0, 0, -1), // Backward
        new Vector3Int(0, 0, 1), // Forward
    };

    public static readonly Vector3[] faceCorners = new Vector3[4]
    {
        new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
    };

    public static readonly int[] cornerIndexes = new int[6]
    {
        0,
        1,
        2,
        1,
        3,
        2
        // Example: current amount of verts is 40
        // Max value is 39

        // 41st entry has an index of 40
        // First index is 0
        // Index (0) + original length (40) = 40 (correct index for 41st value)

        // So the 41st entry has an index of 39 + 1
    };

    public static readonly Vector2[] uvsForFace = new Vector2[4]
    {
        Vector2.up,
        Vector2.one,
        Vector2.zero,
        Vector2.right,
    };
}