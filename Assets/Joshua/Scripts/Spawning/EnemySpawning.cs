using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyWeightPair
{
    public GameObject enemy;
    [Min(1)] public int weight;

    public void AddEnemies(ref List<GameObject> _list)
    {
        for(int i = 0; i < weight; ++i)
        {
            _list.Add(enemy);
        }
    }
}

public class EnemySpawning : MonoBehaviour
{
    private List<GameObject> enemies;
    [SerializeField] private List<EnemyWeightPair> spawnTable;

    [SerializeField, Min(1)] private int minEnemySpawns = 1;
    private List<Transform> spawnPositions;

    [SerializeField] private float minSpawnTime = 1;
    [SerializeField] private float maxSpawnTime = 2;
    private List<float> spawnTimers;
    private int enemiesSpawned = 0;

    public bool isSpawning = true;

    private void Awake()
    {
        enemies = new List<GameObject>();
        foreach (EnemyWeightPair pair in spawnTable)
        {
            pair.AddEnemies(ref enemies);
        }
        spawnPositions = new List<Transform>();
        foreach(Transform t in transform)
        {
            //This gets the first level of child transforms without getting the parent
            spawnPositions.Add(t);
        }
        if(spawnPositions.Count < minEnemySpawns)
        {
            Debug.LogError("Not enough spawn positions. Add empty gameobjects as children to this one to use as spawn positions");
            enabled = false;
        }


        spawnTimers = new List<float>();
        int numOfSpawns = Random.Range(minEnemySpawns, spawnPositions.Count);
        for(int i = 0; i < numOfSpawns; ++i)
        {
            spawnTimers.Add(Random.Range(minSpawnTime, maxSpawnTime));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSpawning)
            return;

        for (int i = 0; i < spawnTimers.Count; ++i)
        {
            if(spawnTimers[i] > 0)
            {
                spawnTimers[i] -= Time.deltaTime;
            }
            else
            {
                SpawnEnemy();
                enemiesSpawned++;
            }
        }

        if (spawnTimers.Count <= enemiesSpawned)
            enabled = false;
    }

    private void SpawnEnemy()
    {
        Transform spawnPosition = spawnPositions[Random.Range(0, spawnPositions.Count)];
        GameObject enemy = enemies[Random.Range(0, enemies.Count)];
        Instantiate(enemy, spawnPosition);
        spawnPositions.Remove(spawnPosition);
    }
}
