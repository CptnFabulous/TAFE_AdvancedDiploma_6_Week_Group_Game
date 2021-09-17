using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(Rigidbody))]
public class EnemyBehaviour : MonoBehaviour
{
    private Transform playerTransform;

    private BaseState currentState;
    [SerializeField] private EnemyIdle idle;
    [SerializeField] private EnemyMove move;
    [SerializeField] private EnemyAttack attack;

    private NavMeshAgent agent;
    private Animator animator;
    private Rigidbody rigid;

    private const float GROUND_CHECK_RATE = 10;
    private float groundCheckTimer = 0;
    private LayerMask groundMask;

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

        animator = GetComponent<Animator>();

        rigid = GetComponent<Rigidbody>();
        rigid.isKinematic = true;

        groundMask = LayerMask.GetMask("Ground", "Level Geometry");
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

        groundCheckTimer += Time.deltaTime * GROUND_CHECK_RATE;
        if(groundCheckTimer > 1)
        {
            groundCheckTimer = 0;
            if(!currentState.GroundCheck())
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// returns true if there's ground beneath it
    /// </summary>
    /// <returns></returns>
    public bool GroundCheck()
    {
        return Physics.Raycast(transform.position, Vector3.down, 10f, groundMask) && rigid.isKinematic;
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

    /// <summary>
    /// Returns a Vector2Int where:
    /// each value is a positive 1 if this unit's value is higher than the player's
    /// otherwise, it's negative one
    /// </summary>
    public Vector2Int GetPlayerDirectionBasic()
    {
        return new Vector2Int(transform.position.x > playerTransform.position.x ? -1 : 1, transform.position.z > playerTransform.position.z ? -1 : 1);
    }

    public Vector3 GetPlayerDirectionSpecific()
    {
        Vector3 direction = new Vector3(playerTransform.position.x - transform.position.x, 0, playerTransform.position.z - transform.position.z).normalized;
        return direction;
    }

    public bool IsPlayerInLineOfSight()
    {
        return Physics.Raycast(transform.position, (playerTransform.position - transform.position).normalized, out RaycastHit hit, move.detectionRadius) && hit.transform.CompareTag("Player");
    }

    public void SetAgentDestination(Vector3 _target)
    {
        if(agent.enabled)
            agent.SetDestination(_target);
    }

    public bool GetAgentEnabled()
    {
        return agent.enabled;
    }

    public void SetAgentEnabled(bool _enabled)
    {
        agent.enabled = _enabled;
    }

    public bool AgentHasPath()
    {
        return agent.hasPath;
    }

    public void SetAnimTrigger(string _triggerName)
    {
        animator.SetTrigger(_triggerName);
    }

    public void SetAnimFloat(string _floatName, float _value)
    {
        animator.SetFloat(_floatName, _value);
    }

    public void MoveRigidbody(Vector3 _movement)
    {
        rigid.MovePosition(transform.position + _movement);
    }

    public void Knockback(Vector3 _force)
    {
        ChangeState(ScriptableObject.CreateInstance<EnemyStunned>());
        agent.enabled = false;
        rigid.isKinematic = false;
        rigid.AddForce(_force, ForceMode.Impulse);
        StartCoroutine(AddKnockback(1));
    }

    public void Knockback(Vector3 _force, float _knockbackTime)
    {
        ChangeState(ScriptableObject.CreateInstance<EnemyStunned>());
        agent.enabled = false;
        rigid.isKinematic = false;
        rigid.AddForce(_force, ForceMode.Impulse);
        StartCoroutine(AddKnockback(_knockbackTime));
    }

    private IEnumerator AddKnockback(float _knockbackTime)
    {
        yield return new WaitForSeconds(_knockbackTime);
        ChangeState(idle.GetStateCopy());
        agent.enabled = move.usingAgent;
        rigid.isKinematic = true;
    }
}
