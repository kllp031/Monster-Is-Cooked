using UnityEngine;

// General Idea: We can create different kinds of food and each food will have different implementation of the OnInteract() function
// This is a simple example of a food that will call the PickUpFood() function on the player's FoodHolder script
public class Food : MonoBehaviour
{
    //public GameObject promptUI;  // assign the "PressE" Text in Inspector

    //void Start()
    //{
    //    if (promptUI != null)
    //        promptUI.SetActive(false); // Hide on start
    //}

    //void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player")) // Make sure your Player has tag "Player"
    //    {
    //        if (promptUI != null)
    //            promptUI.SetActive(true); // Show "E"
    //    }
    //}

    //void OnTriggerExit2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        if (promptUI != null)
    //            promptUI.SetActive(false); // Hide "E"
    //    }
    //}


    public void OnInteract(GameObject player)
    {
        if(player && player.GetComponent<FoodHolder>())
        {
            player.GetComponent<FoodHolder>().PickUpFood(this.gameObject);
        }
    }
}
