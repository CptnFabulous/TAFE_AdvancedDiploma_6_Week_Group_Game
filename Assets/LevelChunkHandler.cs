using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LevelChunkHandler : MonoBehaviour
{
    static LevelChunkHandler internalReference;
    public static LevelChunkHandler Current
    {
        get
        {
            if (internalReference == null)
            {
                internalReference = FindObjectOfType<LevelChunkHandler>();
            }

            return internalReference;
        }
    }
    
    public Chunk chunkPrefab;
    public int levelLengthInChunks = 8;
    public Vector3Int chunkDimensions = new Vector3Int(4, 4, 4);
    public Chunk[,,] chunksInLevel;


    public BlockData blockToFill;

    private void Start()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        chunksInLevel = new Chunk[chunkDimensions.x, chunkDimensions.y, chunkDimensions.z];
        
        for (int x = 0; x < chunkDimensions.x; x++)
        {
            for (int y = 0; y < chunkDimensions.y; y++)
            {
                for (int z = 0; z < chunkDimensions.z; z++)
                {
                    Vector3 position = new Vector3(chunkPrefab.dimensions.x * x, chunkPrefab.dimensions.y * y, chunkPrefab.dimensions.z * z);
                    Chunk currentChunk = Instantiate(chunkPrefab, position, Quaternion.identity, transform);
                    // Figure out what to fill the chunk with



                    currentChunk.Rewrite(FillChunk.Flood(currentChunk, blockToFill));

                    Debug.Log("Filling chunk");

                    chunksInLevel[x, y, z] = currentChunk;
                }
            }
        }
    }

    

    public Vector3Int WorldCoordinatesToChunkCoordinates(Vector3Int c, out Chunk chunkContaining)
    {
        c.Clamp(Vector3Int.zero, chunkDimensions * chunkPrefab.dimensions);
        chunkContaining = null;

        int x = 0;
        int y = 0;
        int z = 0;
        while (c.x > chunkPrefab.dimensions.x)
        {
            c.x -= chunkPrefab.dimensions.x;
            x++;
        }
        while (c.y > chunkPrefab.dimensions.y)
        {
            c.y -= chunkPrefab.dimensions.y;
            y++;
        }
        while (c.z > chunkPrefab.dimensions.z)
        {
            c.z -= chunkPrefab.dimensions.z;
            z++;
        }

        chunkContaining = chunksInLevel[x, y, z];

        return c;
    }
    
}
