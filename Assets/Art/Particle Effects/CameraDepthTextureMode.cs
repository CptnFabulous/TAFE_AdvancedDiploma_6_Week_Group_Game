using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraDepthTextureMode : MonoBehaviour
{
    private void Awake()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;

    }
}
