using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private void Awake()
    {
        Time.timeScale = 0;
        GameManager.Instance.paused = true;
    }

    public void Resume()
    {
        Time.timeScale = 1;
        GameManager.Instance.paused = false;
        SceneManager.UnloadSceneAsync("PauseMenu");
    }

    public void HowToPlay()
    {
        SceneManager.LoadSceneAsync("HowToPlay", LoadSceneMode.Additive);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1;
        GameManager.Instance.paused = false;
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
