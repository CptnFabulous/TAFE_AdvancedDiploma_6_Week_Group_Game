using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraDepthTextureMode : MonoBehaviour
{
    [SerializeField] private Color color;
    [SerializeField, Range(0, 1)] private float strength;
    private Material material;

    private void Awake()
    {
        material = new Material(Shader.Find("Hidden/DepthShader"));
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(material != null)
        {
            material.SetColor("_Color", color);
            material.SetFloat("_Strength", strength);
            Graphics.Blit(source, destination, material);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
