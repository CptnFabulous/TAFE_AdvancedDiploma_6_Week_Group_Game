using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "EnemyStates/Attack/Charge", order = 2)]
public class EnemyCharge : EnemyAttack
{
    [SerializeField] private float chargeSpeed = 5;
    private bool usingAgent;
    private Vector3 attackDirection;

    public override BaseState GetStateCopy()
    {
        return CreateInstance<EnemyCharge>();
    }

    public override void EnterState()
    {
        usingAgent = machine.GetAgentEnabled();
        machine.SetAgentEnabled(false);
        attackDirection = machine.GetPlayerDirectionSpecific();
        machine.transform.forward = attackDirection;
        base.EnterState();
    }

    public override void UpdateState()
    {
        machine.MoveRigidbody(chargeSpeed * Time.deltaTime * attackDirection);
        base.UpdateState();
    }

    public override void DestroyState()
    {
        machine.SetAgentEnabled(usingAgent);
        base.DestroyState();
    }
}
