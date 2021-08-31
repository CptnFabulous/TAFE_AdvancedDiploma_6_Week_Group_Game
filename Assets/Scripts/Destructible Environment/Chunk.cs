using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public enum Direction
{
    right,
    left,
    up,
    down,
    forward,
    backward
}
*/
[System.Serializable]
public struct Block
{
    public BlockData type;
    public int health;

    public bool Exists
    {
        get
        {
            return type != null;
        }
    }

    public bool TryDamage(int amount)
    {
        if (Exists == false || (type.IsInvincible && amount > -1))
        {
            return false;
        }

        if (amount <= -1)
        {
            Erase();
            return true;
        }

        health -= amount;
        if (health <= 0)
        {
            Erase();
        }
        return true;
    }

    public void Erase()
    {
        type = null;
        health = 0;
    }

    public void Replace(BlockData newType)
    {
        Replace(newType, newType.maxHealth);
    }

    public void Replace(BlockData newType, int newHealth)
    {
        type = newType;
        health = Mathf.Clamp(newHealth, 1, newType.maxHealth);
    }
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public Vector2Int facePixelDimensions = new Vector2Int(256, 256);
    public Block[,,] blocks { get; private set; }
    public Vector3Int Dimensions
    {
        get
        {
            if (blocks == null)
            {
                return Vector3Int.zero;
            }
            return new Vector3Int(blocks.GetLength(0), blocks.GetLength(1), blocks.GetLength(2));
        }
    }
    public Vector3Int PositionInLevelGrid { get; set; }
    MeshFilter meshData;
    MeshRenderer renderer;
    MeshCollider collider;
    

    private void Awake()
    {
        meshData = GetComponent<MeshFilter>();
        renderer = GetComponent<MeshRenderer>();
        collider = GetComponent<MeshCollider>();
    }

    #region Update chunk when changes are made
    void Refresh(bool hasChanged)
    {
        if (hasChanged == false)
        {
            return;
        }
        
        meshData.mesh = GenerateMesh();
        collider.sharedMesh = meshData.mesh;
        //Debug.Log(SaveData());
    }
    Mesh GenerateMesh()
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> triIndexes = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int x = 0; x < Dimensions.x; x++)
        {
            for (int y = 0; y < Dimensions.y; y++)
            {
                for (int z = 0; z < Dimensions.z; z++)
                {
                    #region Checks to render block
                    // Obtains block data
                    Block currentBlock = blocks[x, y, z];
                    if (currentBlock.type == null) // If block is empty
                    {
                        continue; // Nothing to render
                    }

                    #region Check corners to add faces appropriately
                    Vector3Int coordinates = new Vector3Int(x, y, z);
                    Vector3Int[] directions = new Vector3Int[6]
                    {
                        Vector3Int.left,
                        Vector3Int.right,
                        Vector3Int.up,
                        Vector3Int.down,
                        new Vector3Int(0, 0, -1), // Backward
                        new Vector3Int(0, 0, 1), // Forward
                    };

                    // For each grid direction from the block
                    for (int i = 0; i < directions.Length; i++)
                    {
                        #region Check that the face is worth rendering
                        Vector3Int adjacentCoordinates = coordinates + directions[i];
                        Block adjacent;
                        /*
                        // Check if adjacent coordinates are still inside the mesh
                        if (AreCoordinatesValid(adjacentCoordinates, out Vector3Int clampedCoordinates))
                        {
                            adjacent = Block(adjacentCoordinates);
                        }
                        else
                        {
                            // If not, convert the coordinates to a world position and check for a block inside an adjacent chunk
                            Vector3 adjacentBlockWorldPosition = WorldPositionFromCoordinates(adjacentCoordinates);
                            if (LevelGrid.Current.TryGetChunkCoordinates(adjacentBlockWorldPosition, out adjacentCoordinates, out Chunk adjacentChunk))
                            {
                                adjacent = adjacentChunk.Block(adjacentCoordinates);
                            }
                            else
                            {
                                // If not, clamp original coordinates to inside the chunk
                                adjacentCoordinates = clampedCoordinates;
                                adjacent = Block(adjacentCoordinates);
                            }
                        }
                        */

                        adjacentCoordinates = ClampCoordinatesToChunk(adjacentCoordinates);
                        adjacent = blocks[adjacentCoordinates.x, adjacentCoordinates.y, adjacentCoordinates.z];
                        // If an adjacent block exists
                        // If the adjacent block is not the current one due to clamping
                        // If the object is opaque OR transparent but the same type as the current block
                        if (adjacent.Exists && (adjacent.type.isTransparent == false || adjacent.type == currentBlock.type) && adjacentCoordinates != coordinates)
                        {
                            continue;
                        }
                        #endregion

                        #region Generate mesh verts and tris, and add them to the arrays so they reference each other appropriately
                        Vector3[] faceCorners = new Vector3[4]
                        {
                            new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f),
                            new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
                        };
                        int[] cornerIndexes = new int[6]
                        {
                            0,1,2,
                            1,3,2
                        };

                        // Process corner values by rotating them to match the face, and shifting them to match the coordinates in the chunk
                        Quaternion directionAsQuaternion = Quaternion.LookRotation(directions[i], Vector3.up);
                        for (int c = 0; c < faceCorners.Length; c++)
                        {
                            faceCorners[c] = directionAsQuaternion * faceCorners[c];
                            faceCorners[c] += coordinates;
                        }
                        // Process corner indexes so when the corner values are moved into the new array, they still get the right corners
                        for (int ci = 0; ci < cornerIndexes.Length; ci++)
                        {
                            cornerIndexes[ci] += verts.Count;
                        }

                        verts.AddRange(faceCorners);
                        triIndexes.AddRange(cornerIndexes);
                        #endregion

                        #region Add UV data for texturing
                        Vector2 uvOrigin = currentBlock.type.GetUVFromDirection(i);
                        float texWidth = (float)facePixelDimensions.x / renderer.material.mainTexture.width;
                        float texHeight = (float)facePixelDimensions.y / renderer.material.mainTexture.height;
                        Vector2 scaling = new Vector2(texWidth, texHeight);
                        Vector2[] uvsForFace = new Vector2[]
                        {
                            Vector2.up,
                            Vector2.one,
                            Vector2.zero,
                            Vector2.right,
                        };
                        for (int uvIndex = 0; uvIndex < uvsForFace.Length; uvIndex++)
                        {
                            uvsForFace[uvIndex] += uvOrigin;
                            uvsForFace[uvIndex].Scale(scaling);
                            uvs.Add(uvsForFace[uvIndex]);
                        }
                        #endregion
                    }
                    #endregion
                    #endregion
                }
            }
        }

        Mesh chunkMesh = new Mesh();
        chunkMesh.name = "Mesh of chunk at " + transform.position;
        chunkMesh.vertices = verts.ToArray();
        chunkMesh.triangles = triIndexes.ToArray();
        chunkMesh.Optimize();
        chunkMesh.RecalculateNormals();
        chunkMesh.uv = uvs.ToArray();

        return chunkMesh;
    }
    #endregion

    #region Information about chunk
    public Block Block(Vector3Int coordinates)
    {
        coordinates = ClampCoordinatesToChunk(coordinates);
        return blocks[coordinates.x, coordinates.y, coordinates.z];
    }
    public Chunk AdjacentChunk(Vector3Int direction)
    {
        // Get chunk's coordinates in level manager
        return LevelGrid.Current.GetChunk(PositionInLevelGrid + direction);
    }
    public bool TryGetCoordinates(Vector3 worldPosition, out Vector3Int validCoordinates)
    {
        Vector3Int localPosition = Vector3Int.RoundToInt(transform.InverseTransformPoint(worldPosition));
        validCoordinates = localPosition;
        return AreCoordinatesValid(validCoordinates, out validCoordinates);
    }
    /// <summary>
    /// Sanity check for if coordinates are valid in array, checked by clamping them and seeing if there was any change.
    /// </summary>
    /// <param name="coordinates"></param>
    /// <param name="saneCoordinates"></param>
    /// <returns></returns>
    public bool AreCoordinatesValid(Vector3Int coordinates, out Vector3Int validCoordinates)
    {
        validCoordinates = ClampCoordinatesToChunk(coordinates);
        return coordinates == validCoordinates;
    }

    public Vector3Int ClampCoordinatesToChunk(Vector3Int coordinates)
    {
        coordinates.Clamp(Vector3Int.zero, Dimensions - Vector3Int.one);
        return coordinates;
    }

    public Vector3 WorldPositionFromCoordinates(Vector3Int coordinates)
    {
        return transform.TransformPoint(coordinates);
    }

    public string SaveData()
    {
        string saveString = Dimensions.x + "x" + Dimensions.y + "x" + Dimensions.z + ",";

        for (int x = 0; x < Dimensions.x; x++)
        {
            for (int y = 0; y < Dimensions.y; y++)
            {
                for (int z = 0; z < Dimensions.z; z++)
                {
                    // Get variables of block struct in this position
                    Block current = blocks[x, y, z];
                    if (current.Exists)
                    {
                        saveString += current.type.id + "," + current.health + ",";
                    }
                    else
                    {
                        saveString += "n,";
                    }

                }
            }
        }

        return saveString;
    }
    #endregion

    #region Make changes to chunk
    public void DamageBlock(Vector3Int position, int damage)
    {
        bool hasChanged = blocks[position.x, position.y, position.z].TryDamage(damage);
        Refresh(hasChanged);
        
    }
    public void DamageMultipleBlocks(Vector3Int[] positions, int damage)
    {
        bool hasChanged = false;
        for (int i = 0; i < positions.Length; i++)
        {
            bool blockDamaged = blocks[positions[i].x, positions[i].y, positions[i].z].TryDamage(damage);
            // Checks if the current block has changed, or if the change check was already tripped by a previous block
            hasChanged = blockDamaged == true || hasChanged == true;
        }
        Refresh(hasChanged);
    }


    
    public bool TryReplaceBlock(Vector3 worldPosition, BlockData type)
    {
        if (TryGetCoordinates(worldPosition, out Vector3Int saneCoordinates))
        {
            ReplaceBlock(saneCoordinates, type);
            return true;
        }
        return false;
    }

    public bool TryReplaceBlock(Vector3Int coordinates, BlockData type)
    {
        if (AreCoordinatesValid(coordinates, out Vector3Int saneCoordinates))
        {
            ReplaceBlock(saneCoordinates, type);
            return true;
        }
        return false;
    }

    public void ReplaceBlock(Vector3Int position, BlockData type)
    {
        blocks[position.x, position.y, position.z].Replace(type);
        Refresh(true);
    }
    public void Rewrite(Block[,,] blockData)
    {
        blocks = blockData;
        Debug.Log("Rewriting chunk");
        Refresh(true);
    }
    #endregion
}
