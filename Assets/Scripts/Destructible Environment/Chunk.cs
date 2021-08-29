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

    public void Damage(int amount)
    {
        if (Exists == false && type.IsInvincible)
        {
            return;
        }
        
        health -= amount;
        if (health <= 0)
        {
            Erase();
        }
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
    /*
    public static bool operator ==(Block lhs, Block rhs)
    {
        return (lhs.type == rhs.type && lhs.health == rhs.health);
    }
    public static bool operator !=(Block lhs, Block rhs)
    {
        return (lhs.type != rhs.type || lhs.health != rhs.health);
    }
    */
}

/*
public struct SpecialBlockData
{
    public Vector3Int coordinatesInChunk;
    public Direction forward;
    public Direction up;
    public int health;
}
*/

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
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

    void Refresh()
    {
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

                    // For each space adjacent to the block
                    for (int i = 0; i < directions.Length; i++)
                    {
                        #region Check that the face is worth rendering
                        Block adjacent;
                        Vector3Int adjacentCoordinates = coordinates + directions[i];

                        // Check if adjacent coordinates are still inside the mesh
                        // If not, convert the coordinates to a world position and check for a block inside an adjacent chunk
                        // If not, clamp original coordinates to inside the chunk


                        if (AreCoordinatesValid(adjacentCoordinates, out Vector3Int saneCoordinates))
                        {
                            Debug.Log("Obtaining regular coordinates");
                            adjacent = Block(adjacentCoordinates);
                        }
                        else if (LevelGrid.Current.TryGetChunkCoordinates(transform.TransformPoint(adjacentCoordinates), out Vector3Int coordinatesInAdjacentChunk, out Chunk adjacentChunk))
                        {
                            Debug.Log("Valid adjacent chunk found");
                            adjacent = adjacentChunk.Block(coordinatesInAdjacentChunk);

                            // If coordinates are invalid, check the next chunk using the current directional value
                            //Vector3Int coordinatesInAdjacentChunk = adjacentCoordinates - Dimensions;
                            //adjacent = AdjacentChunk(directions[i]).Block(coordinatesInAdjacentChunk);
                        }
                        else
                        {
                            Debug.Log("Sane clamped coordinates substituted");
                            adjacent = Block(saneCoordinates);
                        }

                        // If an adjacent block exists
                        // If the adjacent block is not the current one due to clamping
                        // If the object is opaque OR transparent but the same type as the current block
                        if (adjacent.Exists && (adjacent.type.isTransparent == false || adjacent.type == currentBlock.type) && !adjacent.Equals(currentBlock))
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
                        float texWidth = 64f / renderer.material.mainTexture.width;
                        float texHeight = 64f / renderer.material.mainTexture.height;
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
        return blocks[coordinates.x, coordinates.y, coordinates.z];
    }
    public Chunk AdjacentChunk(Vector3Int direction)
    {
        // Get chunk's coordinates in level manager
        return LevelGrid.Current.GetChunk(PositionInLevelGrid + direction);
    }
    public bool TryGetCoordinates(Vector3 worldPosition, out Vector3Int coordinates)
    {
        Vector3Int localPosition = Vector3Int.RoundToInt(transform.InverseTransformPoint(worldPosition));
        coordinates = localPosition;
        return AreCoordinatesValid(coordinates, out coordinates);
    }
    /// <summary>
    /// Sanity check for if coordinates are valid in array, checked by clamping them and seeing if there was any change.
    /// </summary>
    /// <param name="coordinates"></param>
    /// <param name="saneCoordinates"></param>
    /// <returns></returns>
    public bool AreCoordinatesValid(Vector3Int coordinates, out Vector3Int validCoordinates)
    {
        validCoordinates = coordinates;
        validCoordinates.Clamp(Vector3Int.zero, Dimensions - Vector3Int.one);
        Debug.Log(coordinates + ", " + validCoordinates);
        return coordinates == validCoordinates;
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
        blocks[position.x, position.y, position.z].Damage(damage);
        Refresh();
        
    }
    public void DamageMultipleBlocks(Vector3Int[] positions, int damage)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            blocks[positions[i].x, positions[i].y, positions[i].z].Damage(damage);
        }
        Refresh();
    }
    public void ReplaceBlock(Vector3Int position, BlockData type)
    {
        blocks[position.x, position.y, position.z].Replace(type);
        Refresh();
    }
    public void Rewrite(Block[,,] blockData)
    {
        blocks = blockData;
        Debug.Log("Rewriting chunk");
        Refresh();
    }
    #endregion
}
