using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{

    //public Chunk destructibleMesh;
    public EnemyBehaviour[] enemiesInRoom;
    public Door entryDoor;
    public Door exitDoor;
    public BoxCollider entryAndExitTrigger;


    
    public void Initialise()
    {
        enemiesInRoom = GetComponentsInChildren<EnemyBehaviour>();


        // Update collider dimensions, then reduce the X and Z extents by one each
    }


    public bool AllEnemiesDefeated()
    {
        for (int i = 0; i < enemiesInRoom.Length; i++)
        {
            // If one of the fields still references an enemy, that enemy has not been destroyed and is still alive
            if (enemiesInRoom[i] != null)
            {
                return false;
            }
        }

        return true;
    }



    public void CheckAndUpdate()
    {
        bool allDead = AllEnemiesDefeated();
        entryDoor.enabled = allDead;
        exitDoor.enabled = allDead;

    }
}
