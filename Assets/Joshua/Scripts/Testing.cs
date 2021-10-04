using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            EnemyBehaviour[] enemies = FindObjectsOfType<EnemyBehaviour>();
            foreach(EnemyBehaviour enemy in enemies)
            {
                enemy.Knockback(Vector3.up * 10);
            }
        }
    }
}
