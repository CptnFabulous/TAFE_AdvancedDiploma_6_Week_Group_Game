using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockInteraction : MonoBehaviour
{
    public float raycastLength = 5;
    public LayerMask hitDetection = ~0;

    public RaycastHit ColliderDetected { get; private set; }
    public Chunk TargetedChunk { get; private set; }
    public Vector3Int TargetedBlockCoords { get; private set; }

    public bool TryCheckBlock(Vector3 origin, Vector3 direction)
    {
        ColliderDetected = new RaycastHit();
        TargetedBlockCoords = Vector3Int.zero;
        TargetedChunk = null;

        // Launch a raycast and get appropriate info - if raycast is false, nothing is found, return null
        if (!Physics.Raycast(origin, direction, out RaycastHit rh, raycastLength, hitDetection))
        {
            // Raycast detected nothing
            return false;
        }
        ColliderDetected = rh;

        Debug.DrawRay(rh.point, rh.normal, Color.blue);

        // If no chunk is found, return null
        TargetedChunk = rh.collider.GetComponent<Chunk>();
        if (TargetedChunk == null)
        {
            // Collider is not a chunk
            return false;
        }

        // Get a world position that is actually inside the block
        Vector3 pointInsideBlock = rh.point + -rh.normal * 0.1f;
        // Then try to convert it to coordinates inside the chunk
        if (!TargetedChunk.TryGetCoordinates(pointInsideBlock, out Vector3Int coords))
        {
            // Position obtained somehow doesn't match up with a point on the grid
            return false;
        }
        TargetedBlockCoords = coords;
        Vector3 debugBlockPosition = TargetedChunk.transform.TransformPoint(TargetedBlockCoords);


        Debug.DrawLine(rh.point + rh.normal, debugBlockPosition, Color.red);
        Debug.DrawLine(rh.point + rh.normal, debugBlockPosition + rh.normal, Color.green);

        // Checks that the position has produced a block (this should pretty much always return true)
        if (TargetedChunk.Block(TargetedBlockCoords).Exists == false)
        {
            // The block data is somehow empty
            return false;
        }

        return true;
    }

    
}
