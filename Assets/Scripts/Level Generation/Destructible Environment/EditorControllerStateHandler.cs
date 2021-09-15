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
            Debug.Log("Pause button pressed, current state is " + currentState);
            if (currentState == UserState.Active)
            {
                Debug.Log("Pausing game");
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
        SetPlayerActiveState(false);
        SwitchScreen(pauseMenu); 
    }

    public void GoIntoMenus()
    {
        currentState = UserState.InMenus;
        SetPlayerActiveState(false);
    }

    public void ResumeGame()
    {
        currentState = UserState.Active;
        SetPlayerActiveState(true);

        SwitchScreen(headsUpDisplay);
    }

    public void SetPlayerActiveState(bool active)
    {
        if (active)
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void SwitchScreen(Canvas screen)
    {
        headsUpDisplay.gameObject.SetActive(false);
        pauseMenu.gameObject.SetActive(false);
        screen.gameObject.SetActive(true);

    }
}
