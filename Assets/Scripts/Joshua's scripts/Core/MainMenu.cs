using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void HowToPlay()
    {
        SceneManager.LoadSceneAsync("HowToPlay", LoadSceneMode.Additive);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
