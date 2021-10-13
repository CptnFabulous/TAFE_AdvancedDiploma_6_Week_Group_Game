using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceablePlatformScript : MonoBehaviour
{
    public void BreakPlatform()
    {
        AbilityManager.SoleManager.BreakPlatform();
        Destroy(gameObject);
    }
}
