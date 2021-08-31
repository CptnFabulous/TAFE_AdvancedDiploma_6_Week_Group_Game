using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditingController : MonoBehaviour
{
    public BlockInteraction interactionData;
    public int blockDamage = 1;
    public Explosion explosionStats;
    public LevelEditorHUD hud;

    BlockData blockToPlace;
    int blockIndex;

    
    // Start is called before the first frame update
    void Start()
    {
        blockToPlace = BlockData.AllBlocks[blockIndex];
        hud.currentBlock.text = blockToPlace.name;
    }
    

    // Update is called once per frame
    void Update()
    {
        if (Input.mouseScrollDelta.magnitude != 0)
        {
            float f = Input.mouseScrollDelta.y * (1 / Input.mouseScrollDelta.y);
            int indexChange = Mathf.RoundToInt(f);
            blockIndex += indexChange;
            blockIndex = MiscMath.InverseClamp((int)blockIndex, 0, BlockData.AllBlocks.Length - 1);
            blockToPlace = BlockData.AllBlocks[blockIndex];
            hud.currentBlock.text = blockToPlace.name;
        }
        
        if (interactionData.TryCheckBlock(transform.position, transform.forward))
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Destroy block
                interactionData.TargetedChunk.DamageBlock(interactionData.TargetedBlockCoords, blockDamage);
            }

            if (Input.GetMouseButtonDown(1))
            {
                // Place block
                Vector3Int placePosition = Vector3Int.RoundToInt(interactionData.ColliderDetected.normal) + interactionData.TargetedBlockCoords;
                interactionData.TargetedChunk.TryReplaceBlock(placePosition, blockToPlace);
            }

            if (Input.GetMouseButtonDown(2))
            {
                explosionStats.Detonate(interactionData.ColliderDetected.point);
            }
        }
    }
}
