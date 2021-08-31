using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LevelGrid : MonoBehaviour
{
    static LevelGrid internalReference;
    public static LevelGrid Current
    {
        get
        {
            // If reference does not exist or is inactive, find one that is
            if (internalReference == null || internalReference.gameObject.activeSelf == false)
            {
                // Find a LevelGrid in the scene that is enabled
                LevelGrid[] gridsInScene = FindObjectsOfType<LevelGrid>();
                for (int i = 0; i < gridsInScene.Length; i++)
                {
                    if (gridsInScene[i].gameObject.activeSelf == true)
                    {
                        internalReference = gridsInScene[i];
                        break;
                    }
                }
            }

            return internalReference;
        }
    }
    
    public Chunk chunkPrefab;
    public Vector3Int chunkSize = new Vector3Int(16, 16, 16);
    public int levelLengthInChunks = 8;
    public Vector3Int chunkDimensions = new Vector3Int(4, 4, 4);
    public Chunk[,,] chunksInLevel;
    public Vector3Int Dimensions
    {
        get
        {
            if (chunksInLevel == null)
            {
                return Vector3Int.zero;
            }
            return new Vector3Int(chunksInLevel.GetLength(0), chunksInLevel.GetLength(1), chunksInLevel.GetLength(2));
        }
    }

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
            //Debug.Log("Generating blocks on new X layer");
            for (int y = 0; y < chunkDimensions.y; y++)
            {
                //Debug.Log("Generating blocks on new Y layer");
                for (int z = 0; z < chunkDimensions.z; z++)
                {
                    //Debug.Log("Generating block on new Z layer");
                    Vector3 position = new Vector3(chunkSize.x * x, chunkSize.y * y, chunkSize.z * z);
                    Chunk currentChunk = Instantiate(chunkPrefab, position, Quaternion.identity, transform);
                    // Figure out what to fill the chunk with
                    currentChunk.PositionInLevelGrid = new Vector3Int(x, y, z);
                    chunksInLevel[x, y, z] = currentChunk;
                    currentChunk.Rewrite(FillChunk.Hollow(chunkSize, blockToFill));
                }
            }
        }
    }

    public Chunk GetChunk(Vector3Int gridCoordinates)
    {
        return chunksInLevel[gridCoordinates.x, gridCoordinates.y, gridCoordinates.z];
    }
    public bool TryGetChunkCoordinates(Vector3 worldPosition, out Vector3Int coordinates, out Chunk chunkContaining)
    {
        for (int x = 0; x < Dimensions.x; x++)
        {
            for (int y = 0; y < Dimensions.y; y++)
            {
                for (int z = 0; z < Dimensions.z; z++)
                {
                    Debug.Log("Checking coordinates in adjacent chunk at " + new Vector3Int(x, y, z));
                    chunkContaining = chunksInLevel[x, y, z];

                    // Checks if the chunk is present, and if a valid set of coordinates can be obtained.
                    if (chunkContaining != null && chunkContaining.TryGetCoordinates(worldPosition, out coordinates))
                    {
                        return true;
                    }
                }
            }
        }
        coordinates = Vector3Int.RoundToInt(worldPosition);
        chunkContaining = null;
        return false;
    }
}
