using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastToBlock : MonoBehaviour
{
    public float raycastLength = 5;
    public LayerMask hitDetection = ~0;
    public int blockDamage = 1;

    public BlockData blockToPlace;

    public Explosion explosionStats;

    RaycastHit rh;
    Chunk targetedChunk;
    Vector3Int targetedBlockCoords;
    Vector3 faceDirection;

    private void Update()
    {
        //Debug.DrawRay(transform.position, transform.forward * raycastLength, Color.green);
        
        if (TryCheckBlock(transform.position, transform.forward))
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Destroy block
                targetedChunk.DamageBlock(targetedBlockCoords, blockDamage);
            }

            if (Input.GetMouseButtonDown(1))
            {
                // Place block
                Vector3Int placePosition = Vector3Int.RoundToInt(faceDirection) + targetedBlockCoords;
                targetedChunk.ReplaceBlock(placePosition, blockToPlace);
            }

            if (Input.GetMouseButtonDown(2))
            {
                explosionStats.Detonate(rh.point);
            }
        }
        else
        {
            targetedChunk = null;
            targetedBlockCoords = Vector3Int.zero;
            faceDirection = Vector3.zero;
        }
    }

    bool TryCheckBlock(Vector3 origin, Vector3 direction)
    {
        targetedBlockCoords = Vector3Int.zero;
        targetedChunk = null;
        faceDirection = -direction;
        
        // Launch a raycast and get appropriate info - if raycast is false, nothing is found, return null
        if (!Physics.Raycast(origin, direction, out rh, raycastLength, hitDetection))
        {
            return false;
        }

        faceDirection = rh.normal;
        Debug.DrawRay(rh.point, rh.normal, Color.blue);

        // If no chunk is found, return null
        targetedChunk = rh.collider.GetComponent<Chunk>();
        if (targetedChunk == null)
        {
            return false;
        }

        Vector3 pointInsideBlock = rh.point + -faceDirection * 0.1f;

        // Now that a point inside the block is obtained, convert it to coordinates inside the chunk
        if (!targetedChunk.TryGetCoordinates(pointInsideBlock, out targetedBlockCoords))
        {
            return false;
        }
        Vector3 debugBlockPosition = targetedChunk.transform.TransformPoint(targetedBlockCoords);
        Debug.DrawLine(rh.point + rh.normal, debugBlockPosition, Color.red);
        Debug.DrawLine(rh.point + rh.normal, debugBlockPosition + rh.normal, Color.green);

        //Debug.Log("Check for player interaction");
        if (targetedChunk.Block(targetedBlockCoords).Exists == false)
        {
            return false;
        }

        return true;
    }

    
}
