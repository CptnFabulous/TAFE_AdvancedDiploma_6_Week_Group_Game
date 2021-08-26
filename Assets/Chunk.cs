using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Block
{
    public BlockData type;
    public int health;
    public bool invincible;

    //public static bool operator ==(Block lhs, Block rhs);
    //public static bool operator !=(Block lhs, Block rhs);
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    

    public int width = 16;
    public int height = 16;
    public int length = 16;


    Block[,,] blocks;
    public Block[,,] CurrentBlocks
    {
        get
        {
            if (blocks == null)
            {
                blocks = new Block[width, height, length];
            }
            return blocks;
        }
        set
        {
            blocks = value;
            Refresh();
        }
    }
    MeshFilter meshData;
    MeshRenderer renderer;
    MeshCollider collider;

    private void Awake()
    {
        meshData = GetComponent<MeshFilter>();
        renderer = GetComponent<MeshRenderer>();
        collider = GetComponent<MeshCollider>();
    }

    

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
        
        int xLength = blocks.GetLength(0);
        int yLength = blocks.GetLength(1);
        int zLength = blocks.GetLength(2);

        for (int x = 0; x < xLength; x++)
        {
            for (int y = 0; y < yLength; y++)
            {
                for (int z = 0; z < zLength; z++)
                {
                    Block currentBlock = blocks[x, y, z];
                    //Debug.Log(currentBlock.type);


                    // If block is empty, there's nothing to be rendered.
                    if (currentBlock.type == null)
                    {
                        //Debug.Log("Block at " + new Vector3(x, y, z) + " is empty");
                        continue;
                    }

                    #region Check corners to add faces appropriately
                    Vector3 coordinates = new Vector3(x, y, z);
                    Vector3[] directions = new Vector3[6]
                    {
                        Vector3.left,
                        Vector3.right,
                        Vector3.up,
                        Vector3.down,
                        Vector3.back,
                        Vector3.forward
                    };

                    // For each space adjacent to the block
                    for (int i = 0; i < directions.Length; i++)
                    {
                        Vector3 adjacentCoordinates = coordinates + directions[i];


                        adjacentCoordinates.x = Mathf.Clamp(adjacentCoordinates.x, 0, xLength - 1);
                        adjacentCoordinates.y = Mathf.Clamp(adjacentCoordinates.y, 0, yLength - 1);
                        adjacentCoordinates.z = Mathf.Clamp(adjacentCoordinates.z, 0, zLength - 1);
                        Block adjacent = blocks[(int)adjacentCoordinates.x, (int)adjacentCoordinates.y, (int)adjacentCoordinates.z];

                        // If adjacent block is present and opaque (or the same block, meaning the check was clamped to the edges), there's no need to render this face
                        if ((adjacent.type != null && adjacent.type.isTransparent == false) && !adjacent.Equals(currentBlock))
                        {
                            continue;
                        }

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
                    }

                    //int count = (yLength * x) + (zLength * y) + z;
                    //Debug.Log("#" + count + ", " + coordinates);
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


        return chunkMesh;
    }



    public void PlaceNewBlock(Block spaceToReplace, BlockData type)
    {
        spaceToReplace.type = type;
        spaceToReplace.health = type.maxHealth;
    }

    public Block GetBlock(Vector3 worldPosition)
    {
        Vector3 coordinates = worldPosition - transform.position;
        return blocks[(int)coordinates.x, (int)coordinates.z, (int)coordinates.z];
    }

    

    public void DamageBlocks(int damage, Vector3[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            Block blockDamaged = GetBlock(positions[i]);
            if (blockDamaged.type == null || blockDamaged.invincible)
            {
                return;
            }

            blockDamaged.health -= damage;

            if (blockDamaged.health <= 0)
            {
                blockDamaged = new Block();
            }
        }

        Refresh();
    }
    /*
    public string GetChunkSaveData()
    {
        
    }
    */
    public Block[,,] FillChunkCompletely(BlockData type)
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
                    PlaceNewBlock(current, type);
                }
            }
        }

        return chunk;
    }

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
