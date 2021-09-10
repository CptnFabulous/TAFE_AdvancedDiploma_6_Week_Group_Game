using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStunned : BaseState
{
    public override BaseState GetStateCopy()
    {
        return CreateInstance<EnemyStunned>();
    }
}
