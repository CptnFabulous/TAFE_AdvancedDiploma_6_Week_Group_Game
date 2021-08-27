using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (type.IsInvincible)
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
        if (lhs.type == rhs.type && )
    }
    public static bool operator !=(Block lhs, Block rhs)
    {

    }
    */
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public Vector3Int dimensions = new Vector3Int(16, 16, 16);


    public Block[,,] blocks { get; private set; }
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
    public void Refresh()
    {
        Debug.Log("refreshing");
        meshData.mesh = GenerateMesh();
        collider.sharedMesh = meshData.mesh;
    }
    public Mesh GenerateMesh()
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> triIndexes = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        Vector3Int lengths = new Vector3Int(blocks.GetLength(0), blocks.GetLength(1), blocks.GetLength(2));

        for (int x = 0; x < lengths.x; x++)
        {
            for (int y = 0; y < lengths.y; y++)
            {
                for (int z = 0; z < lengths.z; z++)
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
                        Vector3Int adjacentCoordinates = coordinates + directions[i];
                        
                        // Have some kind of check so that if it exceeds the array bounds, get the first block of the adjacent chunk
                        adjacentCoordinates.Clamp(Vector3Int.zero, new Vector3Int(lengths.x - 1, lengths.y - 1, lengths.z - 1));

                        Block adjacent = Block(adjacentCoordinates);
                        // If an adjacent block exists
                        // If it isn't the same block because of clamping
                        // If the object is opaque OR if transparent, the same type as the current block
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
                        float texWidth = (float)currentBlock.type.facePixelDimensions.x / (float)currentBlock.type.textureData.width;
                        float texHeight = (float)currentBlock.type.facePixelDimensions.y / (float)currentBlock.type.textureData.height;
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

        //Texture2D combinedTexture = new Texture2D();
        //combinedTexture.width = 
        chunkMesh.uv = uvs.ToArray();

        return chunkMesh;
    }
    #endregion

    public Block Block(Vector3Int coordinates)
    {
        return blocks[coordinates.x, coordinates.y, coordinates.z];
    }

    public Vector3Int GetCoordinatesFromWorldPosition(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        return Vector3Int.RoundToInt(position);
    }

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
        Refresh();
    }

    public string SaveData()
    {
        string saveString = dimensions.x + "x" + dimensions.y + "x" + dimensions.z + ",";

        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int z = 0; z < dimensions.z; z++)
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

    /*
    public string GetChunkSaveData()
    {
        
    }
    */


    /*
    public Block[,,] FillChunkFromFile(string saveData)
    {
        Block[,,] chunk = new Block[width, height, length];

        int xLength = blocks.GetLength(0);
        int yLength = blocks.GetLength(1);
        int zLength = blocks.GetLength(2);
        for (int x = 0; x < xLength; x++)
        {
            for (int y = 0; y < yLength; y++)
            {
                for (int z = 0; z < zLength; z++)
                {
                    Block current = chunk[x, y, z];

                    // Do stuff
                }
            }
        }
    }
    */
}
