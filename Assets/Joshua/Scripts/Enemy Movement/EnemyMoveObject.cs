using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "EnemyMove", menuName = "EnemyObjects/MoveObjects/Default", order = 1)]
public class EnemyMoveObject : ScriptableObject
{
    public float moveSpeed = 3.0f;
    public bool usingAgent = true;
    public float targetInterval = 1.0f;

    public virtual void Move(ref NavMeshAgent _agent, Vector3 _target)
    {
        _agent.SetDestination(_target);
    }
}
