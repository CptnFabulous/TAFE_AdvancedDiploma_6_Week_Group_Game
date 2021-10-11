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
    private bool spawnActive = true;
    private List<float> spawnTimers;
    private List<EnemyBehaviour> enemiesSpawned;

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

        enemiesSpawned = new List<EnemyBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSpawning)
            return;

        if(spawnActive == true)
        {
            spawnActive = false;
            //string log = "";
            for (int i = 0; i < spawnTimers.Count; ++i)
            {
                //log += i + ": " + spawnTimers[i];
                if(spawnTimers[i] > 0)
                {
                    spawnActive = true;
                    spawnTimers[i] -= Time.deltaTime;
                    if(spawnTimers[i] < 0)
                    {
                        SpawnEnemy();
                    }
                }
            }
            //Debug.Log(log);
        }

        if(enemiesSpawned.Count == 0 && !spawnActive)
        {
            SpawnItem();
            enabled = false;
        }
    }

    private void SpawnItem()
    {
        Debug.Log("Item get!");
        //temporary
        StartCoroutine(nameof(NextRoom));
    }

    private IEnumerator NextRoom()
    {
        yield return new WaitForSeconds(3);
        GameManager.Instance.EnterNewRoom();
    }

    private void SpawnEnemy()
    {
        Transform spawnPosition = spawnPositions[Random.Range(0, spawnPositions.Count)];
        GameObject enemy = enemies[Random.Range(0, enemies.Count)];
        GameObject newEnemy = Instantiate(enemy, spawnPosition);
        newEnemy.GetComponent<EnemyBehaviour>().SetSpawner(enemiesSpawned);
        spawnPositions.Remove(spawnPosition);
    }
}
