using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehaviour : MonoBehaviour
{
    private BaseState currentState;
    [SerializeField] private EnemyIdle idle;
    [SerializeField] private EnemyMove move;
    [SerializeField] private EnemyAttack attack;

    protected NavMeshAgent agent;


    private void Start()
    {
        ChangeState(idle.GetStateCopy());
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState();
        }
    }

    public EnemyIdle GetIdle()
    {
        return idle;
    }

    public EnemyMove GetMove()
    {
        return move;
    }

    public EnemyAttack GetAttack()
    {
        return attack;
    }

    public void ChangeState(BaseState _state)
    {
        if(currentState != null)
        {
            currentState.DestroyState();
        }
        currentState = _state;
        if(currentState != null)
        {
            currentState.machine = this;
            currentState.EnterState();
        }
    }
}
