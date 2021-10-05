using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    private static AbilityManager soleManager;
    public static AbilityManager SoleManager
    {
        get
        {
            if (soleManager)
            {
                return soleManager;
            }
            else
            {
                return FindObjectOfType<AbilityManager>();
            }
        }
    }

    private List<int> specialAbilities = new List<int>();
    private int attackSpeedUps = 0;
    private int healthUps = 0;
    private int attackForceUps = 0;

    private void Start()
    {
        if (!soleManager)
        {
            soleManager = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddSpecialAbility(int abilityID)
    {
        if (!specialAbilities.Contains(abilityID))
        {
            specialAbilities.Add(abilityID);
        }
    }

    public bool DoesPlayerHaveSpecialAbility(int abilityID)
    {
        return specialAbilities.Contains(abilityID);
    }

    public float GetAttackSpeed()
    {
        float speed = 1f;
        speed += (attackSpeedUps * 0.1f);
        return speed;
    }

    public float GetAttackForceBonus()
    {
        float force = 0f;
        force += (attackForceUps * 0.5f);
        return force;
    }

    public int GetHealthBonus()
    {
        int health = 0;
        health += (healthUps * 5);
        return health;
    }
}
