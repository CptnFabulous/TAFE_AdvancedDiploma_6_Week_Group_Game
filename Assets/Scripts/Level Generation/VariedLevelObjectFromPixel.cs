using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Varied Level Objects To Scan For", menuName = "Scriptable Objects/Varied Level Objects From Pixel", order = 0)]
public class VariedLevelObjectFromPixel : LevelObjectFromPixel
{
    public GameObject[] possiblePrefabs;
    public BlockData[] possibleBlocks;

    public override GameObject GetPrefab
    {
        get
        {
            if (possiblePrefabs.Length <= 0)
            {
                return null;
            }
            
            // Adds a plus one because the max value on the random.range function is exclusive and therefore will never return that value
            int randomIndex = Random.Range(0, possiblePrefabs.Length);
            Debug.Log(randomIndex + ", " + possiblePrefabs[randomIndex]);
            return possiblePrefabs[randomIndex];
            /*
            if (randomIndex == possiblePrefabs.Length)
            {
                Debug.Log("Item index: " + randomIndex);

                return prefabToSpawn;
            }
            else
            {
                Debug.Log("Item index: " + randomIndex);

                return possiblePrefabs[randomIndex];
            }
            */
        }
    }

    public override BlockData GetBlock
    {
        get
        {
            if (possibleBlocks.Length <= 0)
            {
                return null;
            }
            
            int randomIndex = Random.Range(0, possibleBlocks.Length);
            return possibleBlocks[randomIndex];
            /*
            int randomIndex = Random.Range(0, possibleBlocks.Length + 1);
            if (randomIndex == possibleBlocks.Length)
            {
                return blockToPlaceOnFloor;
            }
            else
            {
                return possibleBlocks[randomIndex];
            }
            */
        }
    }
}
