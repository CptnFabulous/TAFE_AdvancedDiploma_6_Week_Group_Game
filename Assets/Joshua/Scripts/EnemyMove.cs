using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "EnemyStates/Move/Default", order = 2)]
public class EnemyMove : BaseState
{
    [SerializeField] protected float moveSpeed;
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

    private void Move()
    {
        machine.SetAgentDestination(machine.GetPlayerPosition());
    }
}
