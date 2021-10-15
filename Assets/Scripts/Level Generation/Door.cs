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


    public DoorType type = DoorType.EntryAndExit;
    public UnityEvent onOpen;
    public UnityEvent onClose;


    Collider playerBlocker;
    NavMeshObstacle aiBlocker;

    private void Awake()
    {
        playerBlocker = GetComponent<Collider>();
        aiBlocker = GetComponent<NavMeshObstacle>();

        if (enabled)
        {
            Close();
        }
        else
        {
            Open();
        }
    }


    private void OnEnable()
    {
        Close();
    }

    private void OnDisable()
    {
        Open();
    }

    void Open()
    {
        //Debug.Log("Opening");
        playerBlocker.enabled = false;
        aiBlocker.enabled = false;
        onOpen.Invoke();
    }

    void Close()
    {
        //Debug.Log("Closing");
        playerBlocker.enabled = true;
        aiBlocker.enabled = true;
        onClose.Invoke();
    }
}
