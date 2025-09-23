using UnityEngine;

public class Food : MonoBehaviour
{
    public GameObject promptUI;  // assign the "PressE" Text in Inspector

    void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false); // Hide on start
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Make sure your Player has tag "Player"
        {
            if (promptUI != null)
                promptUI.SetActive(true); // Show "E"
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (promptUI != null)
                promptUI.SetActive(false); // Hide "E"
        }
    }
}
