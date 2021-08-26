using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastToBlock : MonoBehaviour
{
    public float raycastLength = 5;
    public LayerMask hitDetection = ~0;

    Block CheckBlock(Vector3 direction)
    {
        RaycastHit rh;
        // Launch a raycast and get appropriate info - if raycast is false, nothing is found, return null
        if (!Physics.Raycast(transform.position, direction, out rh, raycastLength, hitDetection))
        {
            return new Block();
        }

        // If no chunk is found, return null
        Chunk chunk = rh.collider.GetComponent<Chunk>();
        if (chunk == null)
        {
            return new Block();
        }

        // Change direction value so it points in the approximate cardinal direction
        float[] axes = new float[3] { direction.x, direction.y, direction.z };
        int index = 0;
        for (int i = 0; i < axes.Length; i++)
        {
            // If current axis is larger than the previous largest one
            if (Mathf.Abs(axes[i]) > Mathf.Abs(axes[index]))
            {
                axes[index] = 0;
                index = i;
            }
            else
            {
                axes[i] = 0;
            }
        }
        Vector3 cardinalDirection = new Vector3(axes[0], axes[1], axes[2]).normalized;
        Vector3 pointInsideBlock = rh.point + cardinalDirection * 0.1f;
        Mathf.Round(pointInsideBlock.x);
        Mathf.Round(pointInsideBlock.y);
        Mathf.Round(pointInsideBlock.z);

        return chunk.GetBlock(pointInsideBlock);
    }
}
