using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "EnemyMove", menuName = "EnemyObjects/MoveObjects/Jump", order = 2)]
public class EnemyJump : EnemyMoveObject
{
    public int maxJumpDistance = 3;


    public override void Move(ref NavMeshAgent _agent, Vector3 _target)
    {
        Vector3 agentPos = _agent.transform.position;
        Vector3 direction = new Vector3(_target.x - agentPos.x, 0, _target.z - agentPos.z);
        direction.Normalize();
    }
}
