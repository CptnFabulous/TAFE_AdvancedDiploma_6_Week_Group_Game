using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawnEnemy : MonoBehaviour
{
    public GameObject enemy;
    // Start is called before the first frame update
    void Awake()
    {
        Instantiate(enemy, transform);
    }
}
