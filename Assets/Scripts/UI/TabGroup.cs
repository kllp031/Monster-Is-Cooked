using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    [Header("Setup")]
    public GameObject[] tabPages;
    public Button[] tabButtons;

    [Header("Visuals")]
    public Color idleColor = Color.white;
    public Color activeColor = Color.gray;

    // Track which page is currently open
    private int currentPageIndex = 0;

    void Start()
    {
        // Initialize: Set the first tab active
        SwitchTab(0);
    }

    public void SwitchTab(int index)
    {
        // Sanity check: ensure the index is valid
        if (index < 0 || index >= tabPages.Length) return;

        currentPageIndex = index;

        // 1. Swap Pages
        for (int i = 0; i < tabPages.Length; i++)
        {
            tabPages[i].SetActive(i == index);
        }

        // 2. Highlight Buttons
        for (int i = 0; i < tabButtons.Length; i++)
        {
            Image btnImage = tabButtons[i].GetComponent<Image>();

            if (i == index)
            {
                btnImage.color = activeColor;
                // Optional: Make button unclickable while active
                tabButtons[i].interactable = false;
            }
            else
            {
                btnImage.color = idleColor;
                tabButtons[i].interactable = true;
            }
        }
    }

    public void NextTab()
    {
        // Calculate next index using "Modulo" to wrap around to 0
        int newIndex = (currentPageIndex + 1) % tabPages.Length;
        SwitchTab(newIndex);
    }

    public void PreviousTab()
    {
        int newIndex = currentPageIndex - 1;

        // If we go below 0, wrap around to the last tab
        if (newIndex < 0)
        {
            newIndex = tabPages.Length - 1;
        }

        SwitchTab(newIndex);
    }
}