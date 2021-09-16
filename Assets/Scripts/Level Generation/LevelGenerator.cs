using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public int numberOfRooms = 8;
    public Texture2D entryRoom;
    public Texture2D[] roomLayouts;
    public Texture2D[] hallwayLayouts;
    public Texture2D exitRoom;
    public Chunk roomPrefab;

    List<Chunk> allRooms = new List<Chunk>();


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
            int randomIndex = Random.Range(0, roomLayouts.Length);
            AddRoom(roomLayouts[randomIndex]);

            if (hallwayLayouts.Length > 0 && MiscMath.CoinFlip(0.5f))
            {
                randomIndex = Random.Range(0, hallwayLayouts.Length);
                AddRoom(hallwayLayouts[randomIndex]);
            }
        }

        AddRoom(exitRoom);

        // Add terrain mesh data to nav mesh handler
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        Bounds levelBounds = new Bounds();

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

        // If there is a previous room
        if (allRooms.Count > 1)
        {
            #region Orient new room so its entry lines up with the previous room's exit
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



            // Use mesh bounds centre (instead of renderer or collider bounds) to get local space values for entry and exit rooms
            //Bounds newRoomBounds = newRoom.terrainMesh.bounds;
            //Bounds oldRoomBounds = oldRoom.terrainMesh.bounds;

            // Get direction of the entry door
            // Get direction of the exit door
            // Rotate new room so entry door is facing in the same direction as the exit door


            /*
            Vector3 globalExitDoorRotation = exitDoor.transform.localPosition - oldRoom.transform.InverseTransformPoint(oldRoom.terrainMesh.bounds.center);
            globalExitDoorRotation = MiscMath.ConvertDirectionToCardinalDirection(globalExitDoorRotation.normalized);
            globalExitDoorRotation = oldRoom.transform.TransformDirection(globalExitDoorRotation);
            Vector3 localEntryDoorRotation = entryDoor.transform.localPosition - newRoom.transform.InverseTransformPoint(newRoom.terrainMesh.bounds.center);
            localEntryDoorRotation = MiscMath.ConvertDirectionToCardinalDirection(localEntryDoorRotation.normalized);
            Quaternion entryDoorLocalQuaternion = Quaternion.LookRotation(localEntryDoorRotation, transform.up);
            // Rotates the new room to the same direction as the exit door, plus the relative rotation of its entry door
            newRoom.transform.LookAt(newRoom.transform.position + (entryDoorLocalQuaternion * globalExitDoorRotation), transform.up);

            // Shifts the new room so the entry door is one tile away from the exit door in the same direction
            Vector3 positionForEntryDoor = exitDoor.transform.position + globalExitDoorRotation;
            Vector3 entryDoorRelativePosition = entryDoor.transform.position - newRoom.transform.position;
            newRoom.transform.position = positionForEntryDoor - entryDoorRelativePosition;
            */
            #endregion
        }
        else
        {
            newRoom.transform.position = transform.position;
            newRoom.transform.rotation = transform.rotation;
        }

        allRooms.Add(newRoom);
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
            // Check each pixel against the available pixels to scan for
            for (int r = 0; r < LevelObjectFromPixel.All.Length; r++)
            {
                // Check if the current pixel matches a 
                LevelObjectFromPixel reference = LevelObjectFromPixel.All[r];
                if (pixels[p] != reference.colourReferenceInSaveFile)
                {
                    continue;
                }

                // If the current pixel matches a colour reference, this room needs to be populated with a specific thing.
                Vector3Int coordinates = MiscMath.IndexFor3DArrayFromSingle(p, size);

                // Place block on floor
                BlockData block = reference.blockToPlaceOnFloor;
                if (block != null)
                {
                    blocksForChunk[coordinates.x, coordinates.y, coordinates.z].type = block;
                    blocksForChunk[coordinates.x, coordinates.y, coordinates.z].health = block.maxHealth;
                }
                // Spawn object in space
                if (reference.prefabToSpawn != null)
                {
                    GameObject prefabOnFloor = Instantiate(reference.prefabToSpawn, newRoom.transform);
                    //prefabOnFloor.transform.localRotation = Quaternion.Euler(reference.defaultRotationEulerAngles);
                    // Get total height of object after rotation
                    float heightOfCenterFromFloor = prefabOnFloor.GetComponent<MeshRenderer>().bounds.extents.y;
                    // Sets object to appropriate position and raises altitude so it clears the floor.
                    prefabOnFloor.transform.localPosition = coordinates + Vector3.up * (0.5f + heightOfCenterFromFloor);


                }
            }

        }

        newRoom.Rewrite(blocksForChunk);

        return newRoom;
    }
}
