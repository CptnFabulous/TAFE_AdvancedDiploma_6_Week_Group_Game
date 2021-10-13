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


            newRoom.transform.rotation = exitDoor.transform.rotation * entryDoor.transform.rotation;

            // Rotate room so the entry is in the same direction as the exit
            /*
            Vector3 globalExitDoorRotation = exitDoor.transform.localPosition - oldRoom.transform.InverseTransformPoint(oldRoom.terrainMesh.bounds.center);
            globalExitDoorRotation = MiscMath.ConvertDirectionToCardinalDirection(globalExitDoorRotation.normalized);
            globalExitDoorRotation = oldRoom.transform.TransformDirection(globalExitDoorRotation);
            Vector3 localEntryDoorRotation = entryDoor.transform.localPosition - newRoom.transform.InverseTransformPoint(newRoom.terrainMesh.bounds.center);
            localEntryDoorRotation = MiscMath.ConvertDirectionToCardinalDirection(localEntryDoorRotation.normalized);
            Quaternion entryDoorLocalQuaternion = Quaternion.LookRotation(localEntryDoorRotation, transform.up);
            // Rotates the new room to the same direction as the exit door, plus the relative rotation of its entry door
            newRoom.transform.LookAt(newRoom.transform.position + (entryDoorLocalQuaternion * globalExitDoorRotation), transform.up);
            */

            // Position new room so its entry lines up with the old room's exit
            Vector3 positionForEntryDoor = exitDoor.transform.position + exitDoor.transform.forward;
            Vector3 relativePositionOfEntryDoorFromRoomTransform = entryDoor.transform.position - newRoom.transform.position;
            Vector3 positionForNewRoom = positionForEntryDoor - relativePositionOfEntryDoorFromRoomTransform;
            newRoom.transform.position = positionForNewRoom;
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
                // Check if the current pixel matches a 
                LevelObjectFromPixel reference = LevelObjectFromPixel.All[r];
                Color pixelColourNoAlpha = pixels[p];
                pixelColourNoAlpha.a = 1;
                Color referenceColourNoAlpha = reference.colourReferenceInSaveFile;
                referenceColourNoAlpha.a = 1;

                if (pixelColourNoAlpha != referenceColourNoAlpha)
                {
                    Debug.Log(pixelColourNoAlpha + ", " + referenceColourNoAlpha);
                    continue;
                }

                

                // If the current pixel matches a colour reference, this room needs to be populated with a specific thing.
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
                    //prefabOnFloor.transform.localRotation = Quaternion.Euler(reference.defaultRotationEulerAngles);
                    // Get total height of object after rotation
                    float heightOfCenterFromFloor = 0.5f;
                    MeshRenderer renderer = prefabOnFloor.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        heightOfCenterFromFloor += renderer.bounds.extents.y;
                    }
                    // Sets object to appropriate position and raises altitude so it clears the floor.
                    prefabOnFloor.transform.localPosition = coordinates + transform.up * heightOfCenterFromFloor;


                    float alpha = pixels[p].a;
                    float segmentSize = 1f / directionsToAngleObjects;
                    for (int cd = 0; cd < directionsToAngleObjects + 1; cd++)
                    {
                        float amountOfSegments = segmentSize * cd;
                        bool greaterThanMin = alpha > amountOfSegments - (segmentSize / 2);
                        bool lessThanMax = alpha < amountOfSegments + (segmentSize / 2);
                        //Debug.Log(prefab + ", " + alpha + ", " + segmentSize + ", " + amountOfSegments + ", " + greaterThanMin + ", " + lessThanMax);
                        if (greaterThanMin && lessThanMax)
                        {
                            float angle = 360 / directionsToAngleObjects * cd;
                            //Debug.Log("Placement angle for " + prefabOnFloor + " is " + angle);
                            prefabOnFloor.transform.localRotation = Quaternion.Euler(0, angle, 0);
                            break;
                        }
                    }
                }
            }

        }

        newRoom.Rewrite(blocksForChunk);

        return newRoom;
    }
}
