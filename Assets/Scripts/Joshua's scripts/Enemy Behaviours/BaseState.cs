using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState : ScriptableObject
{
    [HideInInspector]
    public EnemyBehaviour machine;

    public virtual BaseState GetStateCopy() { return (BaseState)CreateInstance(GetType()); }
    public virtual void EnterState() { }
    public virtual void UpdateState() { }
    public virtual bool GroundCheck() { return machine.GroundCheck(); }
    public virtual void DestroyState() { Destroy(this); }
}
