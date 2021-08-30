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
    public static Block[,,] FromData(string dataString)
    {
        int parseStart = 0;
        int parseEnd = 0;

        Vector3Int dimensions = Vector3Int.zero;
        

        int[] lengths = new int[3];
        for (int i = 0; i < lengths.Length; i++)
        {
            parseEnd = dataString.IndexOf('x', parseStart);
            string lengthString = dataString.Substring(parseStart, parseEnd);
            lengths[i] = int.Parse(lengthString);
            parseStart = parseEnd;
        }
        dimensions = new Vector3Int(lengths[0], lengths[1], lengths[2]);

        Block[,,] chunk = new Block[dimensions.x, dimensions.y, dimensions.z];
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int z = 0; z < dimensions.z; z++)
                {
                    parseEnd = dataString.IndexOf(',', parseStart);
                    string typeString = dataString.Substring(parseStart, parseEnd);
                    int id;
                    if (int.TryParse(typeString, out id) == false)
                    {
                        // Unable to parse value, block is empty
                        chunk[x, y, z].Erase();
                        parseStart = parseEnd;
                        continue;
                    }
                    parseStart = parseEnd;

                    parseEnd = dataString.IndexOf(',', parseStart);
                    string healthString = dataString.Substring(parseStart, parseEnd);
                    int health = int.Parse(healthString);
                    parseStart = parseEnd;

                    chunk[x, y, z].Replace(BlockData.GetByID(id), health);
                }
            }
        }

        return chunk;
    }







    /// <summary>
    /// DOES NOT DO ANYTHING PRESENTLY
    /// </summary>
    /// <param name="originalBlockGrid"></param>
    /// <param name="newDirection"></param>
    /// <returns></returns>
    public static Block[,,] RotateBlocksInChunk(Block[,,] originalBlockGrid, Vector3 eulerAngles)
    {
        return originalBlockGrid;
    }
}
