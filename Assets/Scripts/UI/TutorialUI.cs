using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    private void OnEnable()
    {
        Time.timeScale = 0f; // Pause the game when tutorial is active
    }

    private void OnDisable()
    {
        Time.timeScale = 1f; // Resume the game when tutorial is closed
    }
}
