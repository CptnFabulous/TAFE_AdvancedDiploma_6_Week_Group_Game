using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Object To Scan For", menuName = "Scriptable Objects/Level Object From Pixel", order = 0)]
public class LevelObjectFromPixel : ScriptableObject
{
    public Color colourReferenceInSaveFile = Color.white;
    public BlockData[] possibleBlocks;
    public GameObject[] possiblePrefabs;

    public virtual BlockData GetBlock()
    {
        return MiscMath.GetRandomFromArray(possibleBlocks);
    }

    public virtual GameObject GetPrefab()
    {
        return MiscMath.GetRandomFromArray(possiblePrefabs);
    }

    

    public static LevelObjectFromPixel[] All
    {
        get
        {
            if (internalList == null)
            {
                internalList = Resources.LoadAll<LevelObjectFromPixel>("");
            }

            return internalList;
        }
    }
    static LevelObjectFromPixel[] internalList;
}
