using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public Vector3 force;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            EnemyBehaviour[] enemies = FindObjectsOfType<EnemyBehaviour>();
            foreach(EnemyBehaviour enemy in enemies)
            {
                enemy.Knockback(force);
            }
        }
    }
}
