using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStunned : BaseState
{
    public override bool GroundCheck()
    {
        return true;
    }
}
