﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Auaora;

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
    private int attackRangeUps = 0;
    private int dashCooldownUps = 0;

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

    public void AddItem(int itemID)
    {
        HUDScript hudRef = FindObjectOfType<HUDScript>();
        switch (itemID)
        {
            default:
                attackSpeedUps++;
                hudRef.DisplayText("Attack Speed Up");
                break;
            case 1:
                healthUps++;
                FindObjectOfType<PlayerScript>().HealPlayer(5);
                hudRef.DisplayText("Health Up");
                break;
            case 2:
                attackForceUps++;
                hudRef.DisplayText("Attack Force Up");
                break;
            case 3:
                attackRangeUps++;
                hudRef.DisplayText("Attack Range Up");
                break;
            case 4:
                dashCooldownUps++;
                hudRef.DisplayText("Dash Cooldown Down");
                break;
        }
        hudRef.UpdateStatText();
    }

    public float GetAttackSpeed()
    {
        float speed = 1f;
        speed += (attackSpeedUps * 0.2f);
        return speed;
    }

    public float GetAttackForceBonus()
    {
        float force = 0f;
        force += (attackForceUps * 0.8f);
        return force;
    }

    public float GetDashCooldownBonus()
    {
        float cooldown = 0f;
        cooldown += (dashCooldownUps * 0.07f);
        return cooldown;
    }

    public float GetAttackRangeBonus()
    {
        float range = 1f;
        range += (attackRangeUps * 0.15f);
        return range;
    }

    public int GetHealthBonus()
    {
        int health = 0;
        health += (healthUps * 2);
        return health;
    }
}
