using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle", menuName = "EnemyStates/Idle/Default", order = 1)]
public class EnemyIdle : BaseState
{
    protected float timer = 0;
    [SerializeField] protected float waitTime = 1;

    public override BaseState GetStateCopy()
    {
        return CreateInstance<EnemyIdle>();
    }

    public override void UpdateState()
    {
        timer += Time.deltaTime;
        if(timer > waitTime)
        {
            Debug.Log("wait's over");
            machine.ChangeState(machine.GetMove().GetStateCopy());
        }
    }
}
