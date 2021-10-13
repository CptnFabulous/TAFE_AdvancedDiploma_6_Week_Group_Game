using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.IO;

public class LevelEditorHUD : MonoBehaviour
{
    public LevelEditor controller;
    
    public Text currentBlock;

    [Header("Saving and loading")]
    public GameObject saveAndLoadWindow;
    public SearchWindow fileWindow;
    public Button save;
    public Button overwrite;
    public Button load;
    public Button delete;
    public Button cancel;

    private void Awake()
    {
        saveAndLoadWindow.SetActive(false);
        fileWindow.onResultSelected.AddListener(UpdateSaveAndLoadOptions);


        save.onClick.AddListener(()=> controller.SaveLevel(fileWindow.input.text));
        save.onClick.AddListener(OpenSearchWindow);

        overwrite.onClick.AddListener(() => controller.OverwriteLevel(fileWindow.SelectionIndex));

        load.onClick.AddListener(() => controller.LoadLevel(fileWindow.SelectionIndex));

        delete.onClick.AddListener(() => controller.DeleteLevel(fileWindow.SelectionIndex));
        delete.onClick.AddListener(OpenSearchWindow);
        // Add listeners so pressing the save or delete buttons will properly update the search window
        cancel.onClick.AddListener(CloseSearchWindow);

        CloseSearchWindow();
    }


    public void OpenSearchWindow()
    {
        // Generate a list of appropriate names
        string[] saveFileNames = new string[controller.SaveFiles.Length];
        for (int i = 0; i < saveFileNames.Length; i++)
        {
            saveFileNames[i] = controller.SaveFiles[i].Name;
        }
        // Adds file names to search window, and enables it
        fileWindow.LoadWithFiles(saveFileNames);
        UpdateSaveAndLoadOptions(fileWindow.SelectionIndex);
        saveAndLoadWindow.SetActive(true);

        // Disables other functions
        controller.movementController.enabled = false;
        controller.enabled = false;
        controller.stateHandler.SetPlayerActiveState(false);
        controller.stateHandler.GoIntoMenus();
    }

    public void CloseSearchWindow()
    {
        // Hides window and restores regular functions
        saveAndLoadWindow.SetActive(false);
        controller.movementController.enabled = true;
        controller.enabled = true;
        controller.stateHandler.SetPlayerActiveState(true);
        controller.stateHandler.ResumeGame();
    }

    void UpdateSaveAndLoadOptions(int fileSelectionIndex)
    {
        Debug.Log("Updating options for selected file on frame " + Time.frameCount);
        // If no file with a matching name is found, enable saving a new file with this name
        save.interactable = fileWindow.ExactResultNotFound && fileWindow.input.text != "";
        // If a file is found and selected, enable loading, overwriting or deleting
        overwrite.interactable = fileWindow.ResultsFound && fileWindow.SelectionIndex >= 0;
        load.interactable = fileWindow.ResultsFound && fileWindow.SelectionIndex >= 0;
        delete.interactable = fileWindow.ResultsFound && fileWindow.SelectionIndex >= 0;
    }


}
