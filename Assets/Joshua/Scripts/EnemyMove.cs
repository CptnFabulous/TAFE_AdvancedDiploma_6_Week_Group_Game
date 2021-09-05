using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "EnemyStates/Move/Default", order = 1)]
public class EnemyMove : BaseState
{
    public float moveSpeed = 3;
    [SerializeField] protected float detectionRadius = 5;
    public bool usingAgent = true;

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
        if(machine.GetPlayerDistance() < detectionRadius)
        {
            machine.ChangeState(machine.GetAttack().GetStateCopy());
        }

        if (!machine.AgentHasPath())
        {
            Move();
        }
    }

    protected virtual void Move()
    {
        machine.SetAgentDestination(machine.GetPlayerPosition());
    }
}
