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
}
