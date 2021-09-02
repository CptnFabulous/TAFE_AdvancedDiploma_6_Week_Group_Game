using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "EnemyStates/Move/Default", order = 2)]
public class EnemyMove : BaseState
{
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float detectionRadius = 5;

    public override BaseState GetStateCopy()
    {
        return CreateInstance<EnemyMove>();
    }

    public override void UpdateState()
    {
        base.UpdateState();
    }

    public override void Move()
    {
        base.Move();
    }
}
