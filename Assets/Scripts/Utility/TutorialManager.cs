using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Required for Button references

[System.Serializable]
public class TutorialStep
{
    [TextArea(3, 10)]
    public string[] sentences;
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI Components")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI dialogueText;

    // NEW: We need reference to buttons to hide/show them
    public GameObject backButton;
    public GameObject nextButton; // Optional: Rename text to "Finish" on last step

    [System.Serializable]
    public struct NamedTutorial
    {
        public string tutorialID;
        public TutorialStep step;
    }
    public List<NamedTutorial> allTutorials;

    // CHANGED: Queue replaced by List + Index
    private List<string> currentSentences = new List<string>();
    private int currentIndex = 0;

    private HashSet<string> completedTutorials = new HashSet<string>();
    private bool isTutorialActive = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        LoadCompletedTutorials();
    }

    private void Start()
    {
        tutorialPanel.SetActive(false);
    }

    public void TriggerEventTutorial(string id)
    {
        if (isTutorialActive || completedTutorials.Contains(id)) return;

        NamedTutorial target = allTutorials.Find(t => t.tutorialID == id);

        if (!string.IsNullOrEmpty(target.tutorialID))
        {
            StartTutorial(target.step);
            completedTutorials.Add(id);
            SaveCompletedTutorial(id);
        }
    }

    // =================================================================================
    // CORE LOGIC CHANGES
    // =================================================================================

    private void StartTutorial(TutorialStep tutorial)
    {
        isTutorialActive = true;
        tutorialPanel.SetActive(true);

        // 1. Convert array to List so we can navigate it
        currentSentences.Clear();
        currentSentences.AddRange(tutorial.sentences);

        // 2. Reset Index
        currentIndex = 0;

        UpdateUI();
    }

    public void DisplayNextSentence()
    {
        // If we are at the end, close the tutorial
        if (currentIndex >= currentSentences.Count - 1)
        {
            EndTutorial();
            return;
        }

        // Otherwise, go forward
        currentIndex++;
        UpdateUI();
    }

    public void DisplayPreviousSentence()
    {
        // Safety check: Don't go below 0
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateUI();
        }
    }

    // Shared logic for updating text and buttons
    private void UpdateUI()
    {
        // Stop any old typing and start new
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentences[currentIndex]));

        // Handle Button Visibility
        // Hide Back button if we are on the first page
        if (backButton != null)
            backButton.SetActive(currentIndex > 0);

        // Optional: Change "Next" button text to "Finish" on the last page?
        // You can add that logic here if you have a reference to the Button's Text component.
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = sentence;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int totalVisibleCharacters = dialogueText.textInfo.characterCount;
        int counter = 0;

        while (counter < totalVisibleCharacters)
        {
            int visibleCount = counter % (totalVisibleCharacters + 1);
            dialogueText.maxVisibleCharacters = visibleCount;
            yield return new WaitForSeconds(0.01f);
            counter++;
        }
        dialogueText.maxVisibleCharacters = totalVisibleCharacters;
    }

    private void EndTutorial()
    {
        isTutorialActive = false;
        tutorialPanel.SetActive(false);
    }

    // =================================================================================
    // SAVING (Unchanged)
    // =================================================================================

    private void SaveCompletedTutorial(string id)
    {
        PlayerPrefs.SetInt("Tut_" + id, 1);
        PlayerPrefs.Save();
    }

    private void LoadCompletedTutorials()
    {
        foreach (var tut in allTutorials)
        {
            if (PlayerPrefs.HasKey("Tut_" + tut.tutorialID))
                completedTutorials.Add(tut.tutorialID);
        }
    }

    [ContextMenu("Clear Saved Tutorials")]
    public void ClearSavedTutorials()
    {
        foreach (var tut in allTutorials)
        {
            PlayerPrefs.DeleteKey("Tut_" + tut.tutorialID);
        }
        completedTutorials.Clear();
    }
}