using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkTest : MonoBehaviour
{
    public Chunk chunkToTest;
    public BlockData blockToFill;
    
    // Start is called before the first frame update
    void Start()
    {
        Block[,,] blocksToReplace = new Block[chunkToTest.width, chunkToTest.height, chunkToTest.length];
        for (int x = 0; x < chunkToTest.width; x++)
        {
            for (int y = 0; y < chunkToTest.height; y++)
            {
                for (int z = 0; z < chunkToTest.length; z++)
                {
                    //if (Random.Range(0f, 1f) > 0.5f)
                    if ((x % 2) == 0 && (y % 2) == 0 && (z % 2) == 0)
                    //if (x == 1 && z == 1)
                    {
                        //Debug.Log("replacing block at " + new Vector3(x, y, z));
                        blocksToReplace[x, y, z].type = blockToFill;
                        blocksToReplace[x, y, z].health = blockToFill.maxHealth;
                        //Debug.Log(blocksToReplace[x, y, z].type);
                    }
                    
                }
            }
        }

        chunkToTest.CurrentBlocks = blocksToReplace;
    }

    
}
