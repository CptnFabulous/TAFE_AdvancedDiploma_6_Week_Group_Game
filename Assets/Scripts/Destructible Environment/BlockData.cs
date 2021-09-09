using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Block", menuName = "Scriptable Objects/Block", order = 0)]
public class BlockData : ScriptableObject
{
    public int id;
    public int maxHealth = 1;
    public bool isTransparent;
    //public Vector2Int facePixelDimensions = new Vector2Int(256, 256);
    public Vector2 uvLeft;
    public Vector2 uvRight;
    public Vector2 uvTop;
    public Vector2 uvBottom;
    public Vector2 uvBack;
    public Vector2 uvFront;

    public Vector2 GetUVFromDirection(int index)
    {
        Vector2 uv = Vector2.zero;
        switch(index)
        {
            case 0:
                uv = uvLeft;
                break;
            case 1:
                uv = uvRight;
                break;
            case 2:
                uv = uvTop;
                break;
            case 3:
                uv = uvBottom;
                break;
            case 4:
                uv = uvLeft;
                break;
            case 5:
                uv = uvRight;
                break;
        }
        return uv;
    }
    /*
    public Sprite Icon
    {
        get
        {

        }
    }
    */
    public bool IsInvincible
    {
        get
        {
            return maxHealth <= 0;
        }
    }

    #region Obtaining block data
    private void OnValidate()
    {
        int numberOfTimesChecked = 0;
        for (int i = 0; i < AllBlocks.Length; i++)
        {
            for (int c = 0; c < AllBlocks.Length; c++)
            {
                // If an object is later in the list than the current one, but has the same ID, change it to a new one
                if (AllBlocks[c].id == AllBlocks[i].id && AllBlocks[c] != AllBlocks[i])
                {
                    numberOfTimesChecked += 1;
                    AllBlocks[c].id = AllBlocks.Length - 1 + numberOfTimesChecked;
                }
            }
        }
    }

    static BlockData[] internalList;
    public static BlockData[] AllBlocks
    {
        get
        {
            if (internalList == null)
            {
                internalList = Resources.LoadAll<BlockData>("Blocks");
                //internalList = Resources.LoadAll<BlockData>("");
            }

            return internalList;
        }
    }
    public static BlockData GetByAlphabeticalOrder(int index)
    {
        return AllBlocks[index];
    }
    public static BlockData GetByID(int id)
    {
        BlockData type = null;
        for (int i = 0; i < AllBlocks.Length; i++)
        {
            if (AllBlocks[i].id == id)
            {
                type = AllBlocks[i];
                break;
            }
        }
        return type;
    }
    #endregion
}
