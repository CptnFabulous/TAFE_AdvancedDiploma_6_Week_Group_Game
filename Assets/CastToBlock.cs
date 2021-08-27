using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastToBlock : MonoBehaviour
{
    public float raycastLength = 5;
    public LayerMask hitDetection = ~0;
    public int blockDamage = 1;

    public BlockData blockToPlace;


    Chunk chunk;
    Vector3Int targetedBlock;
    Vector3 faceDirection;

    private void Update()
    {
        //Debug.DrawRay(transform.position, transform.forward * raycastLength, Color.green);
        
        if (TryCheckBlock(out targetedBlock, out chunk, out faceDirection))
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Destroy block
                chunk.DamageBlock(targetedBlock, blockDamage);
            }

            if (Input.GetMouseButtonDown(1))
            {
                // Place block
                Vector3Int placePosition = Vector3Int.RoundToInt(faceDirection) + targetedBlock;
                chunk.ReplaceBlock(placePosition, blockToPlace);
            }
        }
        else
        {
            chunk = null;
            targetedBlock = Vector3Int.zero;
            faceDirection = Vector3.zero;
        }
    }

    bool TryCheckBlock(out Vector3Int blockCoords, out Chunk chunkBlockIsIn, out Vector3 faceDirection)
    {
        blockCoords = Vector3Int.zero;
        chunkBlockIsIn = null;
        faceDirection = -transform.forward;

        Vector3 castDirection = transform.forward;
        
        RaycastHit rh;
        // Launch a raycast and get appropriate info - if raycast is false, nothing is found, return null
        if (!Physics.Raycast(transform.position, castDirection, out rh, raycastLength, hitDetection))
        {
            return false;
        }

        faceDirection = rh.normal;
        Debug.DrawRay(rh.point, rh.normal, Color.blue);

        // If no chunk is found, return null
        chunkBlockIsIn = rh.collider.GetComponent<Chunk>();
        if (chunkBlockIsIn == null)
        {
            return false;
        }

        Vector3 pointInsideBlock = rh.point + -faceDirection * 0.1f;


        // Now that a point inside the block is obtained, convert it to coordinates inside the chunk
        blockCoords = chunkBlockIsIn.GetCoordinatesFromWorldPosition(pointInsideBlock);

        Debug.DrawLine(rh.point + rh.normal, blockCoords, Color.red);
        Debug.DrawLine(rh.point + rh.normal, blockCoords + rh.normal, Color.green);
        

        if (chunkBlockIsIn.Block(blockCoords).Exists == false)
        {
            return false;
        }

        return true;
    }

    
}
