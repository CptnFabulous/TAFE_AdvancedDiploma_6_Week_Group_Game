using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "EnemyStates/Move/Default", order = 1)]
public class EnemyMove : BaseState
{
    public float moveSpeed = 3;
    public float detectionRadius = 5;
    public bool usingAgent = true;
    public bool needsLineOfSight = true;
    public override BaseState GetStateCopy()
    {
        return CreateInstance<EnemyMove>();
    }

    public override void EnterState()
    {
        Move();
    }

    public override void UpdateState()
    {
        if(CheckIfInRange())
        {
            machine.ChangeState(machine.GetAttack().GetStateCopy());
        }

        if (!machine.AgentHasPath())
        {
            Move();
        }

        machine.SetAnimFloat("Move", machine.GetAgentSpeed() / moveSpeed);
    }

    protected virtual bool CheckIfInRange()
    {
        if (needsLineOfSight)
        {
            return machine.IsPlayerInLineOfSight();
        }
        return machine.GetPlayerDistance() < detectionRadius;
    }

    protected virtual void Move()
    {
        machine.SetAgentDestination(machine.GetPlayerPosition());
    }

}
