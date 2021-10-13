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

    private void Awake()
    {
        playerBlocker = GetComponent<Collider>();
        aiBlocker = GetComponent<NavMeshObstacle>();
    }

    void SetOpenState(bool open)
    {
        playerBlocker.enabled = !open;
        aiBlocker.enabled = !open;
    }

    private void OnEnable()
    {
        SetOpenState(false);
    }

    private void OnDisable()
    {
        SetOpenState(true);
    }
}
