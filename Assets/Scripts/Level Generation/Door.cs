using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NavMeshObstacle))]
public class Door : MonoBehaviour
{
    //[System.Serializable]
    public enum DoorType
    {
        Entry,
        Exit,
        EntryAndExit,
    }


    public DoorType type;
    public UnityEvent onOpen;
    public UnityEvent onClose;


    Collider playerBlocker;
    NavMeshObstacle aiBlocker;
    //MeshRenderer debugRenderer;
    //public Material debugOpen;
    //public Material debugClose;

    private void Awake()
    {
        playerBlocker = GetComponent<Collider>();
        aiBlocker = GetComponent<NavMeshObstacle>();
    }

    public void SetOpenState(bool open)
    {
        playerBlocker.enabled = !open;
        aiBlocker.enabled = !open;
        aiBlocker.carving = true;
    }
}
