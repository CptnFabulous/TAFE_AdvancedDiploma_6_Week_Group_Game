using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region singleton
    public static GameManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DoOnAwake();
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }
    }
    #endregion

    private int currentRoom = 0;
    [SerializeField] private int maxRooms = 10;
    private Scene gameScene;
    [SerializeField] private List<GameObject> roomPrefabs;

    /// <summary>
    /// This method is called the first time the gamemanger is loaded
    /// </summary>
    private void DoOnAwake()
    {
        gameScene = SceneManager.GetActiveScene();
    }

    public void SelectRoom()
    {
        Instantiate(roomPrefabs[Random.Range(0, roomPrefabs.Count)]);
    }

    public void EnterNewRoom()
    {
        if(currentRoom >= maxRooms)
        {
            Victory();
        }
        else
        {
            currentRoom++;
            SceneManager.LoadScene(gameScene.buildIndex);
        }
    }

    public void StartAgain()
    {
        currentRoom = 0;
        SceneManager.LoadScene(gameScene.buildIndex);
    }

    private void Victory()
    {

    }
}
