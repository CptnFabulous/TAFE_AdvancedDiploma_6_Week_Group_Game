using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.IO;

public class LevelEditorHUD : MonoBehaviour
{
    public LevelEditor controller;
    
    public Text currentBlock;
    public SearchWindow fileWindow;
    public Button save;
    public Button overwrite;
    public Button load;
    public Button delete;
    public Button cancel;

    private void Awake()
    {
        fileWindow.onItemSearched.AddListener(UpdateSaveAndLoadOptions);
        save.onClick.AddListener(()=> controller.SaveLevel(fileWindow.input.text));
        overwrite.onClick.AddListener(() => controller.OverwriteLevel(fileWindow.SelectionIndex));
        load.onClick.AddListener(() => controller.LoadLevel(fileWindow.SelectionIndex));
        delete.onClick.AddListener(() => controller.DeleteLevel(fileWindow.SelectionIndex));
        // Add listeners so pressing the save or delete buttons will properly update the search window
        cancel.onClick.AddListener(CloseSearchWindow);
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
        fileWindow.gameObject.SetActive(true);

        // Disables other functions
        controller.movementController.enabled = false;
        controller.enabled = false;
    }

    public void CloseSearchWindow()
    {
        // Hides window and restores regular functions
        fileWindow.gameObject.SetActive(false);
        controller.movementController.enabled = true;
        controller.enabled = true;
    }

    void UpdateSaveAndLoadOptions(int fileSelectionIndex)
    {
        // If text is input but no file is found, enable saving to file
        save.interactable = fileSelectionIndex < 0 && fileWindow.input.text != "";
        // If index is greater than zero, a file is found matching the searched value
        overwrite.interactable = fileSelectionIndex >= 0;
        load.interactable = fileSelectionIndex >= 0;
        delete.interactable = fileSelectionIndex >= 0;
    }


}
