using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCompletionCheck : MonoBehaviour
{

    EnemyBehaviour[] enemiesInLevel;

    ItemScript rewardItem;

    public void Initialise()
    {
        enemiesInLevel = FindObjectsOfType<EnemyBehaviour>();
        rewardItem = FindObjectOfType<ItemScript>();

        rewardItem.gameObject.SetActive(false);
    }



    private void Update()
    {
        if (enemiesInLevel == null || AllEnemiesDefeated(enemiesInLevel))
        {
            rewardItem.gameObject.SetActive(true);
            enabled = false;
        }
    }



    public bool AllEnemiesDefeated(EnemyBehaviour[] enemies)
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            // If one of the fields still references an enemy, that enemy has not been destroyed and is still alive
            if (enemies[i] != null)
            {
                return false;
            }
        }

        return true;
    }
}
