using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Remember to turn off usingAgent;
/// </summary>
[CreateAssetMenu(fileName = "Move", menuName = "EnemyStates/Move/Jump", order = 2)]
public class EnemyJump : EnemyMove
{
    /// <summary>
    /// Goes between 0 and 1, used for jump anim
    /// </summary>
    private float jumpProgress = 0;
    private Vector2Int direction;
    private Vector3 origin;
    private Vector3 destination;
    [SerializeField] private float jumpHeight = 1;
    [SerializeField] private int jumpRange = 5;
    [SerializeField] private float jumpDuration = 1;
    private float jumpSpeed;
    private LayerMask groundMask;

    public override BaseState GetStateCopy()
    {
        EnemyJump newState = CreateInstance<EnemyJump>();
        newState.SetValues(jumpHeight, jumpRange, jumpDuration, detectionRadius, needsLineOfSight, usingAgent);
        return newState;
    }

    private void SetValues(float _jumpHeight, int _jumpRange, float _jumpDuration, float _detectionRadius, bool _needsLineOfSight, bool _usingAgent)
    {
        jumpHeight = _jumpHeight;
        jumpRange = _jumpRange;
        jumpDuration = _jumpDuration;
        detectionRadius = _detectionRadius;
        needsLineOfSight = _needsLineOfSight;
        usingAgent = _usingAgent;
    }

    public override void EnterState()
    {
        groundMask = machine.GroundMask;

        direction = machine.GetPlayerDirectionBasic();
        jumpSpeed = 1 / jumpDuration;
        origin = machine.transform.position;
        destination = SelectJumpDestination();
        machine.ActivateIndicator(destination);
        Instantiate(Resources.Load("Effects/Slime"), machine.transform.position, Quaternion.identity);
        machine.Splat();
    }

    public override void UpdateState()
    {
        Move();
        jumpProgress += Time.deltaTime * jumpSpeed;

        if (jumpProgress >= 1)
        {
            machine.transform.position = destination;
            machine.ChangeState(machine.GetAttack().GetStateCopy());
        }
    }

    private Vector3 SelectJumpDestination()
    {
        Vector3 target = origin;
        for (int i = 0; i < jumpRange; i++)
        {
            switch (Random.Range(0, 2))
            {
                case 0:
                    target += Vector3.right * direction.x;
                    break;
                case 1:
                    target += Vector3.forward * direction.y;
                    break;
            }
        }
        if (CheckIfInRange() && CheckIfWalkable(target))
        {
            machine.transform.forward = (target - origin).normalized;
            return target;
        }

        return machine.transform.position;
    }

    private bool CheckIfWalkable(Vector3 _pos)
    {
        return Physics.RaycastAll(_pos + (10 * Vector3.up), Vector3.down, 20f, groundMask).Length == 1;
    }

    protected override void Move()
    {
        machine.transform.position = Vector3.Lerp(origin, destination, jumpProgress);
        float currentHeight = -jumpHeight * (Mathf.Pow(2 * jumpProgress - 1, 2) - 1);
        machine.transform.Translate(currentHeight * Vector3.up);

        machine.SetAnimFloat("Move", 2 * jumpProgress - 1);
    }

    public override void DestroyState()
    {
        Instantiate(Resources.Load("Effects/Slime"), machine.transform.position, Quaternion.identity);
        machine.Splat();
        /*
        if(machine.transform.position != destination)
        {
            machine.transform.position = new Vector3(machine.transform.position.x, destination.y, machine.transform.position.z);
        }
        */
        machine.SetAnimFloat("Move", 0);
        machine.DeactivateIndicator();
        base.DestroyState();
    }

    public override bool GroundCheck()
    {
        return true;
    }
}
