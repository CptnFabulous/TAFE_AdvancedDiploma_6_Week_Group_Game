using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public Vector2Int facePixelDimensions = new Vector2Int(256, 256);
    public string saveString = "";
    public Block[,,] blocks;
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
    public MeshFilter meshData { get; private set; }
    public MeshRenderer renderer { get; private set; }
    public MeshCollider collider { get; private set; }
    public Mesh terrainMesh { get; private set; }



    private void OnValidate()
    {
        if (saveString != "")
        {
            LoadFromData(saveString);
            saveString = "";
        }
    }


    public void Awake()
    {
        meshData = GetComponent<MeshFilter>();
        renderer = GetComponent<MeshRenderer>();
        collider = GetComponent<MeshCollider>();
    }

    #region Update chunk when changes are made
    public void Refresh(bool hasChanged)
    {
        if (hasChanged == false)
        {
            return;
        }

        //Debug.Log(SaveData());

        UpdateMesh();

        LevelGrid.Current.navMeshHandler.RebakeMesh();
    }
    void UpdateMesh()
    {
        if (terrainMesh == null)
        {
            terrainMesh = new Mesh();
            terrainMesh.name = "Mesh for chunk " + name;
        }

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
                        float texWidth = (float)facePixelDimensions.x / renderer.sharedMaterial.mainTexture.width;
                        float texHeight = (float)facePixelDimensions.y / renderer.sharedMaterial.mainTexture.height;
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

        terrainMesh.vertices = verts.ToArray();
        terrainMesh.triangles = triIndexes.ToArray();
        terrainMesh.Optimize();
        terrainMesh.RecalculateNormals();
        terrainMesh.uv = uvs.ToArray();

        meshData.mesh = terrainMesh;
        collider.sharedMesh = terrainMesh;
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
        Debug.Log("Failed on frame " + Time.frameCount + ", " + coordinates + ", " + saneCoordinates);
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

    /*
    public string SaveData()
    {
        string dimensions = Dimensions.x + "x" + Dimensions.y + "x" + Dimensions.z;
        string position = transform.position.x + "," + transform.position.y + "," + transform.position.z;
        string rotation = transform.eulerAngles.x + "," + transform.eulerAngles.y + "," + transform.eulerAngles.z;
        string scale = transform.lossyScale.x + "," + transform.lossyScale.y + "," + transform.lossyScale.z;
        
        string blockValues = "";
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
                        blockValues += current.type.id + "t";
                        if (current.health != current.type.maxHealth)
                        {
                            blockValues += current.health + "h";
                        }
                    }
                    // If not, have no space between the commas. I was going to have an 'n' to specify null but that would take up space
                    blockValues += ",";
                }
            }
        }

        string saveString = dimensions + ";" + position + ";" + rotation + ";" + scale + ";" + blockValues;

        return saveString;
    }
    */
    public Texture2D SaveData()
    {
        // Create array of colours
        Color32[] blockData = new Color32[(Dimensions.x * Dimensions.y * Dimensions.z) + 12];
        // The first twelve colour values are put aside to store essential bit and float values
        blockData[0] = MiscMath.FloatToColour(Dimensions.x); // Dimensions X
        blockData[1] = MiscMath.FloatToColour(Dimensions.y); // Dimensions Y
        blockData[2] = MiscMath.FloatToColour(Dimensions.z); // Dimensions Z
        blockData[3] = MiscMath.FloatToColour(transform.position.x); // Position X
        blockData[4] = MiscMath.FloatToColour(transform.position.y); // Position Y
        blockData[5] = MiscMath.FloatToColour(transform.position.z); // Position Z
        blockData[6] = MiscMath.FloatToColour(transform.rotation.x); // Rotation X
        blockData[7] = MiscMath.FloatToColour(transform.rotation.y); // Rotation Y
        blockData[8] = MiscMath.FloatToColour(transform.rotation.z); // Rotation Z
        blockData[9] = MiscMath.FloatToColour(transform.lossyScale.x); // Scale X
        blockData[10] = MiscMath.FloatToColour(transform.lossyScale.y); // Scale Y
        blockData[11] = MiscMath.FloatToColour(transform.lossyScale.z); // Scale Z
        for (int i = 12; i < blockData.Length; i++)
        {
            // Calculates index based on dimensions to produce coordinates for block
            Block current = Block(MiscMath.IndexFor3DArrayFromSingle(i, Dimensions));
            if (current.Exists == false)
            {
                blockData[i] = new Color32();
                continue;
            }
            blockData[i].r = (byte)current.type.id;
            if (current.health != current.type.maxHealth)
            {
                blockData[i].g = (byte)current.health;
            }
        }

        int dimensions = Mathf.CeilToInt(Mathf.Sqrt(blockData.Length));
        Texture2D t = new Texture2D(dimensions, dimensions);
        t.SetPixels32(blockData);
        return t;
    }

    public void LoadData(Texture2D saveData)
    {
        // Turns pixels in image into a list of Color32 values
        Color32[] values = saveData.GetPixels32();

        Vector3Int dimensions = new Vector3Int();
        dimensions.x = Mathf.RoundToInt(MiscMath.FloatFromColour(values[0]));
        dimensions.y = Mathf.RoundToInt(MiscMath.FloatFromColour(values[1]));
        dimensions.z = Mathf.RoundToInt(MiscMath.FloatFromColour(values[2]));
        Vector3 position = new Vector3();
        position.x = MiscMath.FloatFromColour(values[3]);
        position.y = MiscMath.FloatFromColour(values[4]);
        position.z = MiscMath.FloatFromColour(values[5]);
        Vector3 eulerAngles = new Vector3();
        eulerAngles.x = MiscMath.FloatFromColour(values[6]);
        eulerAngles.y = MiscMath.FloatFromColour(values[7]);
        eulerAngles.z = MiscMath.FloatFromColour(values[8]);
        Vector3 lossyScale = new Vector3();
        lossyScale.x = MiscMath.FloatFromColour(values[9]);
        lossyScale.y = MiscMath.FloatFromColour(values[10]);
        lossyScale.z = MiscMath.FloatFromColour(values[11]);

        Block[,,] newGrid = new Block[dimensions.x, dimensions.y, dimensions.z];
        for (int i = 12; i < values.Length; i++)
        {
            Vector3Int c = MiscMath.IndexFor3DArrayFromSingle(i, dimensions);
            if (values[i].g == 0)
            {
                // If health is zero, there either isn't or shouldn't be a block here. Do not fill.
                continue;
            }
            newGrid[c.x, c.y, c.z].type = BlockData.AllBlocks[values[i].r];
            newGrid[c.x, c.y, c.z].health = values[i].g;
        }
    }











    public void LoadFromData(string saveString)
    {
        int startIndex = 0;
        int endIndex = 0;

        // For the preliminary variables, split each one by checking for the semicolon delimiter. Then, split each value 
        #region Chunk dimensions
        endIndex = saveString.IndexOf(';', startIndex);
        string dimensionData = saveString.Substring(startIndex, endIndex - startIndex);
        string[] lengths = dimensionData.Split(new char[] { ',' });
        Vector3Int dimensions = new Vector3Int(int.Parse(lengths[0]), int.Parse(lengths[1]), int.Parse(lengths[2]));
        Block[,,] newBlockArray = new Block[dimensions.x, dimensions.y, dimensions.z];
        saveString.Remove(startIndex, endIndex - startIndex);
        #endregion

        #region Position
        endIndex = saveString.IndexOf(';', startIndex);
        string positionData = saveString.Substring(startIndex, endIndex - startIndex);
        string[] worldCoords = positionData.Split(new char[] { ',' });
        Vector3 position = new Vector3(float.Parse(worldCoords[0]), float.Parse(worldCoords[1]), float.Parse(worldCoords[2]));
        saveString.Remove(startIndex, endIndex - startIndex);
        #endregion

        #region Rotation
        endIndex = saveString.IndexOf(';', startIndex);
        string rotationData = saveString.Substring(startIndex, endIndex - startIndex);
        string[] angles = rotationData.Split(new char[] { ',' });
        Vector3 eulerAngles = new Vector3(float.Parse(angles[0]), float.Parse(angles[1]), float.Parse(angles[2]));
        saveString.Remove(startIndex, endIndex - startIndex);
        #endregion

        #region Scale
        endIndex = saveString.IndexOf(';', startIndex);
        string scaleData = saveString.Substring(startIndex, endIndex - startIndex);
        string[] proportions = scaleData.Split(new char[] { ',' });
        Vector3 lossyScale = new Vector3(float.Parse(proportions[0]), float.Parse(proportions[1]), float.Parse(proportions[2]));
        saveString.Remove(startIndex, endIndex - startIndex);
        #endregion

        // Split single string into a separate string for each block, without removing empty entries. Any empty strings in this array represent there being no block
        string[] blockValues = saveString.Split(new char[] { ',' }, System.StringSplitOptions.None);
        int blockIndex = 0;
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int z = 0; z < dimensions.z; z++)
                {
                    /*
                    I'm trying to think of the best way to extract a series of integer values.
                    Data is saved by showing a letter to denote what value the number is representing.
                    Type (t), health (h). Later on I want to have values for directions - forward (f) and up (u) which act as integers to get a direction from a static array.
                    Some data types are always present, but if a value is default or irrelevant it will be omitted to shorten the string.
                    E.g.
                    A block with full health (max health is contained in type) will be t0,
                    A block with ID 4 and 2 health points remaining will be t4h2,
                    (NOT IMPLEMENTED) A block with ID 7, 1 health point, facing +y and with a world up of -z would be t7h1f3u5
                    I'm trying to think of a good clean way to extract this data as appropriate integers
                    */

                    // Gets the current block string value and increments the index by one so the next block can be obtained
                    string current = blockValues[blockIndex];
                    blockIndex++;

                    // If string is empty, there is no block.
                    if (current.Length <= 0)
                    {
                        continue;
                    }

                    
                    // Extract and parse type integer, then apply to block
                    int typeEndIndex = current.IndexOf('t');
                    string t = current.Substring(0, typeEndIndex);
                    newBlockArray[x, y, z].Replace(BlockData.AllBlocks[int.Parse(t)]);
                    current.Remove(0, typeEndIndex);

                    // Check for 'h' delimiter. If so, extract and parse health value, then apply
                    int healthEndIndex = current.IndexOf('h');
                    if (healthEndIndex >= 0)
                    {
                        // Record health value
                        string h = current.Substring(0, healthEndIndex);
                        newBlockArray[x, y, z].health = int.Parse(h);
                        current.Remove(0, healthEndIndex);
                    }
                    /*
                    int forwardEndIndex = current.IndexOf('f');
                    if (forwardEndIndex >= 0)
                    {
                        // Record forward index
                    }

                    int upEndIndex = current.IndexOf('u');
                    if (upEndIndex >= 0)
                    {
                        // Record forward index
                    }
                    */
                }
            }
        }


        transform.position = position;
        transform.rotation = Quaternion.Euler(eulerAngles);
        MiscMath.SetLossyScale(transform, lossyScale);

        Rewrite(newBlockArray);
    }
    



















}
