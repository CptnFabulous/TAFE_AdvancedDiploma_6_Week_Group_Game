using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public int numberOfRooms = 8;
    public Chunk roomPrefab;

    [Header("Rooms")]
    public Texture2D entryRoom;
    public Texture2D[] roomLayouts;
    public Texture2D[] hallwayLayouts;
    public Texture2D exitRoom;

    List<Chunk> allRooms = new List<Chunk>();
    int directionsToAngleObjects = 8;


    private void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        AddRoom(entryRoom);

        for (int i = 0; i < numberOfRooms; i++)
        {
            // Selects a random room

            //int randomIndex = Random.Range(0, roomLayouts.Length - 1);
            //AddRoom(roomLayouts[randomIndex]);

            AddRoom(MiscMath.GetRandomFromArray(roomLayouts));

            if (MiscMath.CoinFlip(1))
            {
                AddRoom(MiscMath.GetRandomFromArray(hallwayLayouts));
            }
            /*
            if (hallwayLayouts.Length > 0 && MiscMath.CoinFlip(0.5f))
            {
                randomIndex = Random.Range(0, hallwayLayouts.Length);
                AddRoom(hallwayLayouts[randomIndex]);
            }
            */
        }

        AddRoom(exitRoom);

        NavMeshUpdateHandler.Current.SetupMesh();
    }

    void AddRoom(Texture2D imageFile)
    {
        // Don't generate a room if there is nothing to generate.
        if (imageFile == null)
        {
            return;
        }
        
        Chunk newRoom = GenerateRoom(imageFile);

        #region Check doors and orient new room so it lines up with the existing one
        // If there is a previous room
        if (allRooms.Count >= 1)
        {
            // Gets the previous room
            Chunk oldRoom = allRooms[allRooms.Count - 1];

            // Find the entry door for the current room
            Door entryDoor = null;
            Door[] doorsInNewRoom = newRoom.GetComponentsInChildren<Door>();
            for (int i = 0; i < doorsInNewRoom.Length; i++)
            {
                if (doorsInNewRoom[i].type == Door.DoorType.Entry)
                {
                    entryDoor = doorsInNewRoom[i];
                    break;
                }
            }

            // Find the exit door for the previous room
            Door exitDoor = null;
            Door[] doorsInOldRoom = oldRoom.GetComponentsInChildren<Door>();
            for (int i = 0; i < doorsInOldRoom.Length; i++)
            {
                if (doorsInOldRoom[i].type == Door.DoorType.Exit)
                {
                    exitDoor = doorsInOldRoom[i];
                    break;
                }
            }

            // If an entry door and exit door could not be found, end the function prematurely.
            if (entryDoor == null || exitDoor == null)
            {
                return;
            }


            newRoom.transform.rotation = exitDoor.transform.rotation * entryDoor.transform.localRotation;

            // Position new room so its entry lines up with the old room's exit
            Vector3 positionForEntryDoor = exitDoor.transform.position + exitDoor.transform.forward;
            Vector3 relativePositionOfEntryDoorFromRoomTransform = entryDoor.transform.position - newRoom.transform.position;
            newRoom.transform.position = positionForEntryDoor - relativePositionOfEntryDoorFromRoomTransform;
        }
        else
        {
            newRoom.transform.position = transform.position;
            newRoom.transform.rotation = transform.rotation;
        }
        #endregion
        allRooms.Add(newRoom);

        // Check all meshes in the room
        MeshFilter[] meshesInRoomObject = newRoom.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < meshesInRoomObject.Length; i++)
        {
            // If the mesh is in the terrain layer, it is meant to be part of the level geometry. Add it to the nav mesh update handler.
            if(MiscMath.IsObjectInLayerMask(meshesInRoomObject[i].gameObject.layer, NavMeshUpdateHandler.Current.autoFindTerrainLayers))
            {
                NavMeshUpdateHandler.Current.AddSource(meshesInRoomObject[i]);
            }
        }
    }

    Chunk GenerateRoom(Texture2D imageFile)
    {
        // Generates a chunk object, and an array of blocks
        Chunk newRoom = Instantiate(roomPrefab, transform);
        newRoom.name = imageFile.name;
        Vector3Int size = new Vector3Int(imageFile.width, 1, imageFile.height);
        Block[,,] blocksForChunk = new Block[size.x, size.y, size.z];
        
        // Checks each pixel in the room data image file
        Color[] pixels = imageFile.GetPixels();
        for (int p = 0; p < pixels.Length; p++)
        {
            
            if (pixels[p] == Color.clear) // If pixel is clear, obviously nothing will be spawned here
            {
                continue;
            }
            
            // Check each pixel against the available pixels to scan for
            for (int r = 0; r < LevelObjectFromPixel.All.Length; r++)
            {
                // Check if the current pixel has the same colour as one of the tile's reference colour.
                // Alpha value is ignored because that is used to calculate rotation.
                LevelObjectFromPixel reference = LevelObjectFromPixel.All[r];
                Color pixelColourNoAlpha = pixels[p];
                pixelColourNoAlpha.a = 1;
                Color referenceColourNoAlpha = reference.colourReferenceInSaveFile;
                referenceColourNoAlpha.a = 1;
                if (pixelColourNoAlpha != referenceColourNoAlpha)
                {
                    //Debug.Log(pixelColourNoAlpha + ", " + referenceColourNoAlpha);
                    continue;
                }

                // If the current pixel matches a colour reference, this room needs to be populated with a specific thing.
                // Calculate coordinates based off position in array
                Vector3Int coordinates = MiscMath.IndexFor3DArrayFromSingle(p, size);

                // Place block on floor
                BlockData block = reference.GetBlock();
                if (block != null)
                {
                    blocksForChunk[coordinates.x, coordinates.y, coordinates.z].type = block;
                    blocksForChunk[coordinates.x, coordinates.y, coordinates.z].health = block.maxHealth;
                }
                // Spawn object in space
                GameObject prefab = reference.GetPrefab();
                if (prefab != null)
                {
                    GameObject prefabOnFloor = Instantiate(prefab, newRoom.transform);

                    // Get total height of object after rotation
                    float heightOfCenterFromFloor = 0.5f;
                    MeshRenderer renderer = prefabOnFloor.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        Vector3 upper = new Vector3(0, prefabOnFloor.transform.position.y, 0);
                        Vector3 lower = new Vector3(0, renderer.bounds.min.y, 0);
                        heightOfCenterFromFloor += Vector3.Distance(upper, lower);
                    }
                    else
                    {
                        Collider collider = prefabOnFloor.GetComponent<Collider>();
                        if (collider != null)
                        {
                            Vector3 upper = new Vector3(0, prefabOnFloor.transform.position.y, 0);
                            Vector3 lower = new Vector3(0, collider.bounds.min.y, 0);
                            heightOfCenterFromFloor += Vector3.Distance(upper, lower);
                        }
                    }
                    // Sets object to appropriate position and raises altitude so it clears the floor.
                    prefabOnFloor.transform.localPosition = coordinates + transform.up * heightOfCenterFromFloor;

                    // Determine which direction to rotate the object based on the alpha value in the save file pixel.
                    float alpha = pixels[p].a;
                    float segmentSize = 1f / directionsToAngleObjects;
                    for (int cd = 0; cd < directionsToAngleObjects + 1; cd++)
                    {
                        float amountOfSegments = segmentSize * cd;
                        bool greaterThanMin = alpha > amountOfSegments - (segmentSize / 2);
                        bool lessThanMax = alpha < amountOfSegments + (segmentSize / 2);
                        if (greaterThanMin && lessThanMax)
                        {
                            float angle = 360 / directionsToAngleObjects * cd;
                            prefabOnFloor.transform.localRotation = Quaternion.Euler(0, angle, 0);
                            break;
                        }
                    }
                }
            }

        }

        // Add block data to chunk
        newRoom.Rewrite(blocksForChunk);

        return newRoom;
    }
}
