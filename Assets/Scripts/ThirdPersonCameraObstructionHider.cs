using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraObstructionHider : MonoBehaviour
{
    public GameObject player;
    public float viewingAngle = 45;
    //public float padding = 5;

    

    List<Renderer> currentRenderers = new List<Renderer>();
    Renderer[] playerRenderers;
    Camera playerCamera;


    private void Awake()
    {
        playerRenderers = player.GetComponentsInChildren<Renderer>();
        playerCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        //Gizmos.DrawSphere(transform.position, Vector3.Distance(transform.position, player.transform.position));
        Collider[] collidersBetween = Physics.OverlapSphere(transform.position, Vector3.Distance(transform.position, player.transform.position), playerCamera.cullingMask);
        for (int i = 0; i < collidersBetween.Length; i++)
        {
            currentRenderers.AddRange(collidersBetween[i].GetComponentsInChildren<Renderer>());
        }

        // Neatly check through every renderer in the list
        currentRenderers.RemoveAll(r => IsRendererSafeFromCulling(r));


        // Now that we have the list of current renderers in the radius




    }

    bool IsRendererSafeFromCulling(Renderer r)
    {
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] == r)
            {
                return true;
            }
        }

        //if (r.dis)
        
        Vector3 direction = player.transform.position - transform.position;
        float distance = Vector3.Distance(transform.position, r.bounds.center);
        Vector3 closestPointDirection = r.bounds.ClosestPoint(transform.position) - transform.position;

        bool outside = Vector3.Angle(direction, closestPointDirection) > viewingAngle;
        r.enabled = outside;
        return outside;
    }
}
