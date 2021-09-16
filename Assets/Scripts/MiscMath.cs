using System;
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

    /*
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
    */

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
        // int amountInXSlice = (size.y * size.z);
        int x = Mathf.FloorToInt(index / (size.y * size.z)); // Divides index by the amount required to increment x by one (i.e. y and z dimensions multiplied)
        index %= (size.y * size.z); // Remainder is then used to calculate y and z values
        int y = Mathf.FloorToInt(index / size.z); // Divides index by the amount required to increment y by one (i.e. z dimension)
        index %= size.z; // Remainder is produced. Since we are on the last axis, no more division is needed, so this is the z value
        return new Vector3Int(x, y, index);
    }



    public static Color32 FloatToColour(float value)
    {
        // Splits the 32 bits of a float into 4 sets of eight bits
        // 00000000 00000000 00000000 00000000
        /*
        Color32 newColour = new Color32();
        unsafe
        {

        }
        */
        byte[] data = BitConverter.GetBytes(value);
        return new Color32(data[0], data[1], data[2], data[3]);
        /*
        byte r = value & 0xff;
        byte g = (value >> 8) & 0xff;
        byte b = (value >> 16) & 0xff;
        byte a = (value >> 24) & 0xff;
        return new Color32(r, g, b, a);
        */
    }

    public static float FloatFromColour(Color32 colour)
    {
        // I literally copied this code from the Internet, don't ask me how it works. Something about bit-shifting?

        byte[] data = new byte[]
        {
            colour.r,
            colour.g,
            colour.b,
            colour.a,
        };
        return BitConverter.ToSingle(data, 0);

        //float f = colour.r & colour.g & colour.b & colour.a;

        //return (colour.a << 24) | (colour.b << 16) | (colour.g << 8) | colour.r;
    }


    public static bool CoinFlip(float probability)
    {
        // Ensures probability is a value between zero and one. Zero is always false, one is always true.
        probability = Mathf.Clamp(probability, 0, 1);
        // Creates a random value and clamps it to just above the minimum probability and just below the maximum probability.
        return UnityEngine.Random.Range(0 + Mathf.Epsilon, 1 - Mathf.Epsilon) < probability;
    }
}
