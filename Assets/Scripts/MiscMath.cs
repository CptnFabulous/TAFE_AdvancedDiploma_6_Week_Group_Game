using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MiscMath
{
    public static float InverseClamp(float f, float min, float max)
    {
        if (f > max)
        {
            f = min;
        }
        if (f < min)
        {
            f = max;
        }
        return f;
    }

    public static int InverseClamp(float f, int min, int max)
    {
        if (f > max)
        {
            f = min;
        }
        if (f < min)
        {
            f = max;
        }
        return (int)f;
    }

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


    /// <summary>
    /// Sets an object's scale to a global value regardless of its parent's scaling. I found this code off the internet and it may not work properly.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="globalScale"></param>
    public static void SetLossyScale(Transform t, Vector3 globalScale)
    {
        t.localScale = Vector3.one;
        t.localScale = new Vector3(globalScale.x / t.lossyScale.x, globalScale.y / t.lossyScale.y, globalScale.z / t.lossyScale.z);
    }



    public static int SingleIndexFor3DArray(Vector3Int coordinates, Vector3Int size)
    {
        int x = size.z * size.y * coordinates.x;
        int y = size.z * coordinates.y;
        int z = coordinates.z;
        return x + y + z;
    }
    public static Vector3Int IndexFor3DArrayFromSingle(int index, Vector3Int size)
    {
        Vector3Int finalValue = Vector3Int.zero;
        while (index > size.y * size.z)
        {
            index -= (size.y * size.z);
            finalValue.x++;
        }
        while (index > size.z)
        {
            index -= size.z;
            finalValue.y++;
        }
        /*
        while (index > 1)
        {
            index -= 1;
            finalValue.z++;
        }
        */
        finalValue.z = index;
        return finalValue;
    }
}
