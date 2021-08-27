using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MiscMath
{
    public static Vector3 CentreOfMultiplePositions(Vector3[] positions)
    {
        Vector3 finalValue = Vector3.zero;
        for(int i = 0; i < positions.Length; i++)
        {
            finalValue += positions[i];
        }
        return finalValue / positions.Length;
    }

    public static Vector3 ConvertDirectionToCardinalDirection(Vector3 originalDirection)
    {
        // Change direction value so it points in the approximate cardinal direction
        float[] axes = new float[3] { originalDirection.x, originalDirection.y, originalDirection.z };
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
        return new Vector3(axes[0], axes[1], axes[2]).normalized;
    }
}
