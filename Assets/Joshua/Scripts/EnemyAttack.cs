using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "EnemyStates/Attack/Default", order = 3)]
public class EnemyAttack : BaseState
{
    public override BaseState GetStateCopy()
    {
        return CreateInstance<EnemyAttack>();
    }
}
