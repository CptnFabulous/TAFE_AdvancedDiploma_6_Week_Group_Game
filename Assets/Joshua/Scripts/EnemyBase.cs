using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : MonoBehaviour
{
    private Transform playerTransform;

    [SerializeField] private EnemyMoveObject movement;
    private float targetTimer;
    private NavMeshAgent agent;

    [SerializeField] private EnemyAttackObject attack;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (movement.usingAgent)
        {
            agent.enabled = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        targetTimer += Time.deltaTime;
        if (targetTimer > movement.targetInterval)
        {
            targetTimer = 0;
            movement.Move(ref agent, playerTransform.position);
        }
    }
}
