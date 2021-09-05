using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class EnemyBehaviour : MonoBehaviour
{
    private Transform playerTransform;

    private BaseState currentState;
    [SerializeField] private EnemyIdle idle;
    [SerializeField] private EnemyMove move;
    [SerializeField] private EnemyAttack attack;

    private NavMeshAgent agent;

    private void Awake()
    {
        if(idle == null)
        {
            idle = ScriptableObject.CreateInstance<EnemyIdle>();
        }
        if (move == null)
        {
            move = ScriptableObject.CreateInstance<EnemyMove>();
        }
        if (attack == null)
        {
            attack = ScriptableObject.CreateInstance<EnemyAttack>();
        }

        agent = GetComponent<NavMeshAgent>();
        agent.speed = move.moveSpeed;
        agent.enabled = move.usingAgent;
    }

    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;

        ChangeState(idle.GetStateCopy());
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState();
        }
    }

    #region StateMachine
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
    #endregion

    public Vector3 GetPlayerPosition()
    {
        return playerTransform.position;
    }

    public float GetPlayerDistance()
    {
        return Vector3.Distance(transform.position, playerTransform.position);
    }

    public void SetAgentDestination(Vector3 _target)
    {
        agent.SetDestination(_target);
    }

    public bool AgentHasPath()
    {
        return agent.hasPath;
    }
}
