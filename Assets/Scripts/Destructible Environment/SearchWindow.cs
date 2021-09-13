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
    public Dropdown results;
    public Dropdown.DropdownEvent onResultSelected;

    string[] availableOptions;
    List<int> resultsIncludingSearchText = new List<int>();
    int selectedFileIndex = 0;
    public int SelectionIndex { get; private set; }

    void Awake()
    {
        // Add listener so when the user updates what is in the input field, it updates the options accordingly
        input.onValueChanged.AddListener((_)=> RefreshOptions());
        // Add listener so when a value from the dropdown is selected, it provides an actual result from the search window
        results.onValueChanged.AddListener(SelectResult);
        //search.onValueChanged.AddListener((_) => enabled = false);
    }
    void RefreshOptions()
    {
        // Turn the index into an invalid value, because ints can't be null
        SelectionIndex = -1;

        #region Obtain valid options
        resultsIncludingSearchText.Clear();
        for (int i = 0; i < availableOptions.Length; i++)
        {
            // Check if files exist that contain the text in the search bar
            if (availableOptions[i].Contains(input.text))
            {
                resultsIncludingSearchText.Add(i);
            }
            // Check if a file exists that exactly matches the text in the search bar
            if (availableOptions[i] == input.text)
            {
                SelectionIndex = i;
            }
        }

        //onResultSelected.Invoke(SelectionIndex);
        #endregion



        if (resultsIncludingSearchText.Count > 0)
        {
            results.ClearOptions();
            // Populate results dropdown
            for (int i = 0; i < resultsIncludingSearchText.Count; i++)
            {
                // Use the int in resultsIncludingSearchText.Count as an index to find the appropriate text
                results.options.Add(new Dropdown.OptionData(availableOptions[resultsIncludingSearchText[i]]));
            }
            results.RefreshShownValue();
            results.template.gameObject.SetActive(true);
        }
        else
        {
            results.template.gameObject.SetActive(false);
        }




    }
    void SelectResult(int indexForValidResults)
    {
        // Consult the results list for the appropriate index
        onResultSelected.Invoke(resultsIncludingSearchText[indexForValidResults]);
    }

    public void LoadWithFiles(string[] namesToLookFor)
    {
        availableOptions = namesToLookFor;
        //filesMatchingSearchText.Clear();
        SelectionIndex = -1;
        input.text = "";
        // The input field should automatically refresh the data because its text file was changed
    }
}
