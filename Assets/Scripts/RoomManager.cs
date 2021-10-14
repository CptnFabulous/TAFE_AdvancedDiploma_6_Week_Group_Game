using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{


    public EnemyBehaviour[] enemiesInRoom;
    public Door entryDoor;
    public Door exitDoor;

    private void Start()
    {
        enemiesInRoom = GetComponentsInChildren<EnemyBehaviour>();
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
}
