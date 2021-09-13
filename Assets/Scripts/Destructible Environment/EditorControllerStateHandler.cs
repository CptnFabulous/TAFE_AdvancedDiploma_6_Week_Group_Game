using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorControllerStateHandler : MonoBehaviour
{
    public enum UserState
    {
        Active,
        InMenus,
        Paused
    }

    UserState currentState;



    public Canvas headsUpDisplay; 
    public Canvas pauseMenu;
    
    // Start is called before the first frame update
    void Start()
    {
        ResumeGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == UserState.Active)
            {
                PauseGame();
            }
            else if (currentState == UserState.Paused)
            {
                ResumeGame();
            }
        }
    }


    public void PauseGame()
    {
        currentState = UserState.Paused;
        SetPlayerActiveStateForMenus(false);
        SwitchScreen(pauseMenu); 
    }

    public void ResumeGame()
    {
        currentState = UserState.Active;
        SetPlayerActiveStateForMenus(true);

        SwitchScreen(headsUpDisplay);
    }

    public void SetPlayerActiveStateForMenus(bool active)
    {
        if (active)
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            currentState = UserState.InMenus;
        }
        else
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            currentState = UserState.Active;
        }
    }

    void SwitchScreen(Canvas screen)
    {
        headsUpDisplay.gameObject.SetActive(false);
        pauseMenu.gameObject.SetActive(false);
        screen.gameObject.SetActive(true);

    }
}
