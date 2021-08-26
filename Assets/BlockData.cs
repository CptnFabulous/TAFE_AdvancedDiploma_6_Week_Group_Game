using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Block", menuName = "Scriptable Objects/Block", order = 0)]
public class BlockData : ScriptableObject
{
    static BlockData[] internalList;
    public static BlockData[] AllBlocks
    {
        get
        {
            if (internalList == null)
            {
                internalList = Resources.LoadAll<BlockData>("Blocks");
            }

            return internalList;
        }
    }




    public int id;
    public Texture left;
    public Texture right;
    public Texture top;
    public Texture bottom;
    public Texture front;
    public Texture back;

    public int maxHealth = 5;
    public bool isTransparent;
}
