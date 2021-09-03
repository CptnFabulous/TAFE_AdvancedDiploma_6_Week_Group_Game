using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FillChunk
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


}