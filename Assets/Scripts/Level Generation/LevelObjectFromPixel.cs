using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Object To Scan For", menuName = "Scriptable Objects/Level Object From Pixel", order = 0)]
public class LevelObjectFromPixel : ScriptableObject
{
    public Color colourReferenceInSaveFile = Color.white;
    public GameObject prefabToSpawn;
    public BlockData blockToPlaceOnFloor;

    static LevelObjectFromPixel[] internalList;
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
}
