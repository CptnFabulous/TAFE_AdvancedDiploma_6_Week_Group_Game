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

    public string[] AvailableOptions { get; private set; }
    public int SelectionIndex { get; private set; }
    public int ExactResultIndex { get; private set; }
    List<int> resultsIncludingSearchText = new List<int>();


    public bool ResultsFound
    {
        get
        {
            return resultsIncludingSearchText.Count > 0;
        }
    }
    public bool ExactResultNotFound
    {
        get
        {
            return ExactResultIndex < 0;
        }
    }


    void Awake()
    {
        // Add listener so when the user updates what is in the input field, it updates the options accordingly
        //input.onValueChanged.AddListener((_)=> RefreshOptions());
        input.onEndEdit.AddListener((_) => RefreshOptions());
        // Add listener so when a value from the dropdown is selected, it provides an actual result from the search window
        results.onValueChanged.AddListener(SelectResult);
        //search.onValueChanged.AddListener((_) => enabled = false);
    }
    void RefreshOptions()
    {
        // Turn the index into an invalid value, because ints can't be null
        SelectionIndex = -1;
        ExactResultIndex = -1;

        #region Obtain valid options
        resultsIncludingSearchText.Clear();
        for (int i = 0; i < AvailableOptions.Length; i++)
        {
            // Check if files exist that contain the text in the search bar
            Debug.Log(AvailableOptions[i]);
            if (AvailableOptions[i].Contains(input.text))
            {
                Debug.Log("Adding option " + AvailableOptions[i] + " to results on frame " + Time.frameCount);
                resultsIncludingSearchText.Add(i);

                // If the file exactly matches the text in the search bar, automatically select it
                if (AvailableOptions[i] == input.text)
                {
                    ExactResultIndex = i;
                }
            }
        }
        #endregion

        #region Populate results dropdown
        if (resultsIncludingSearchText.Count > 0)
        {
            results.ClearOptions();
            // Populate results dropdown
            for (int i = 0; i < resultsIncludingSearchText.Count; i++)
            {
                // Use the int in resultsIncludingSearchText.Count as an index to find the appropriate text
                results.options.Add(new Dropdown.OptionData(AvailableOptions[resultsIncludingSearchText[i]]));
            }
            results.RefreshShownValue();
            results.Show();
            //results.template.gameObject.SetActive(true);
            SelectResult(0);
        }
        else
        {
            // No results to display, update table accordingly
            results.Hide();
            //results.template.gameObject.SetActive(false);
            onResultSelected.Invoke(-1);
        }
        #endregion
    }
    void SelectResult(int indexForValidResults)
    {
        // Consult the results list for the appropriate index
        SelectionIndex = resultsIncludingSearchText[indexForValidResults];
        // Updates search bar
        input.text = AvailableOptions[SelectionIndex];
        onResultSelected.Invoke(SelectionIndex);
    }

    public void LoadWithFiles(string[] namesToLookFor)
    {
        AvailableOptions = namesToLookFor;
        input.text = "";
        // The input field should automatically refresh the data because its text file was changed
    }
}
