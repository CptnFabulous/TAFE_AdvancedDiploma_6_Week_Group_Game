using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Events;

public class SearchWindow : MonoBehaviour
{
    [Header("Interactables")]
    public InputField input;
    public Dropdown.DropdownEvent onItemSearched;

    string[] availableNames;
    List<string> filesMatchingSearchText = new List<string>();
    int selectedFileIndex = 0;
    public int SelectionIndex { get; private set; }

    void Awake()
    {
        input.onValueChanged.AddListener((_)=> RefreshOptions());
        //search.onValueChanged.AddListener((_) => enabled = false);
    }
    
    void RefreshOptions()
    {
        // Turn the index into an invalid value, because ints can't be null
        SelectionIndex = -1;

        filesMatchingSearchText.Clear();
        for (int i = 0; i < availableNames.Length; i++)
        {
            // Check if files exist that contain the text in the search bar
            if (availableNames[i].Contains(input.text))
            {
                filesMatchingSearchText.Add(availableNames[i]);
            }
            // Check if a file exists that exactly matches the text in the search bar
            if (availableNames[i] == input.text)
            {
                SelectionIndex = i;
            }
        }

        onItemSearched.Invoke(SelectionIndex);
    }

    public void LoadWithFiles(string[] namesToLookFor)
    {
        availableNames = namesToLookFor;
        //filesMatchingSearchText.Clear();
        SelectionIndex = -1;
        input.text = "";
        // The input field should automatically refresh the data because its text file was changed
    }
}
