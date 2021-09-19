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
            int randomIndex = Random.Range(0, possiblePrefabs.Length);
            if (randomIndex == possiblePrefabs.Length)
            {
                return prefabToSpawn;
            }
            else
            {
                return possiblePrefabs[randomIndex];
            }
        }
    }

    public override BlockData GetBlock
    {
        get
        {
            int randomIndex = Random.Range(0, possibleBlocks.Length);
            if (randomIndex == possibleBlocks.Length)
            {
                return blockToPlaceOnFloor;
            }
            else
            {
                return possibleBlocks[randomIndex];
            }
        }
    }
}
