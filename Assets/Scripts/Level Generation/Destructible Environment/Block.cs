using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public enum Direction
{
    right,
    left,
    up,
    down,
    forward,
    backward
}
*/
[System.Serializable]
public struct Block
{
    public BlockData type;
    public int health;

    public bool Exists
    {
        get
        {
            return type != null;
        }
    }

    public bool TryDamage(int amount)
    {
        if (Exists == false || (type.IsInvincible && amount > -1))
        {
            return false;
        }

        if (amount <= -1)
        {
            Erase();
            return true;
        }

        health -= amount;
        if (health <= 0)
        {
            Erase();
        }
        return true;
    }

    public void Erase()
    {
        type = null;
        health = 0;
    }

    public void Replace(BlockData newType)
    {
        Replace(newType, newType.maxHealth);
    }

    public void Replace(BlockData newType, int newHealth)
    {
        type = newType;
        health = Mathf.Clamp(newHealth, 1, newType.maxHealth);
    }
}