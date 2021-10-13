using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStunned : BaseState
{
    private float waitTime = 0.1f;
    public override bool GroundCheck()
    {
        return true;
    }

    public override void UpdateState()
    {
        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
        }
        else if (machine.IsAtGround())
        {
            machine.EndKnockback();
            if (!machine.GroundCheck())
            {
                machine.Die();
            }
            machine.ChangeState(machine.GetIdle().GetStateCopy());
        }
    }
}
