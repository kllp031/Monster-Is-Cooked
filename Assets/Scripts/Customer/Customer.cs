using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour, IInteractable
{
    public enum Mood {
        Happy,
        Neutral,
        Angry,
        Embarrassed
    }

    [SerializeField] Recipe requestFood; // Use for demo only
    [SerializeField] float waitingTime = 120f; // Use for demo only
    [SerializeField] Mood mood = Mood.Happy;
    [SerializeField] CustomerTimer customerTimer;
    [Header("UI")]
    [SerializeField] Sprite hiddenFoodSprite; // Use to hide the request food until player asks (Demo only)
    [SerializeField] Animator foodRequestBox; // Use for fade in / fade out effect (Demo only)
    [SerializeField] Animator foodRequestBoxBackground; // Use for bouncing effect (Demo only)
    [SerializeField] Image foodIcon;
    [SerializeField] Animator moodIcon; //  Use to display current mood

    private CustomerDetail customerDetail = null;
    private bool isAsked = false;
    private bool isLeft = false;
    private int seatNumer = 0;

    public int SeatNumber { get => seatNumer; set => seatNumer = value; }

    private void OnValidate()
    {
        if (foodRequestBox == null) Debug.LogWarning("This customer has no food request box assigned!");
        if (foodIcon == null) Debug.LogWarning("This customer has no food icon assigned!");
    }
    private void OnEnable()
    {
        isAsked = false;
        isLeft = false;
        // Use for demo only
        SetMood(Mood.Happy);
        if (customerTimer != null)
        {
            customerTimer.gameObject.SetActive(true);
            customerTimer.SetTimer(waitingTime);
            customerTimer.ResetCounter();
            customerTimer.StartTimer();
        }
        if (foodIcon != null && hiddenFoodSprite != null)
            foodIcon.sprite = hiddenFoodSprite;
    }
    public void SetDetail(CustomerDetail detail)
    {
        customerDetail = detail;
    }
    public void SetMood(Mood mood)
    {
        this.mood = mood;
        if (moodIcon != null)
        {
            moodIcon.SetTrigger(mood.ToString());
        }
    }
    public void OnInteract(GameObject obj)
    {
        if (isLeft) return; // Ignore further interactions if customer has already left

        if (obj.GetComponent<FoodHolder>() != null) //  Interact with player
        {
            if (!isAsked) // Show the requested food if customer hasnt been asked yet
            {
                if (requestFood != null)
                {
                    // Use for demo only
                    if (foodIcon != null) foodIcon.sprite = requestFood.icon;
                    if (foodRequestBoxBackground != null) foodRequestBoxBackground.SetTrigger("Bounce");
                    //if (foodIcon != null) foodIcon.sprite = customerDetail.FoodRequest.Recipe.icon;
                }
                isAsked = true;
            }
            else
            {
                Debug.Log("Customer received food directly from player!");
                OnLeave();
            }
        }
        else if (obj.GetComponent<Food>() != null)
        {
            // Use for demo only
            {
                Debug.Log("Customer received food indirectly!");
                SetMood(Mood.Embarrassed);
                OnLeave();
            }

        }
    }
    public void ProcessFood(Food food)
    {
        Debug.Log("Process food");
        if (customerDetail == null) { Debug.LogWarning("This customer has no detail assigned!"); return; }
        if (customerDetail.FoodRequest == null) { Debug.LogWarning("This customer's detail has invalid food request!"); return; }
    }
    public void OnLeave()
    {
        // This function will be called by the timer when the timer runs out or when the customer receives the food
        Debug.Log("Customer is leaving: " + mood.ToString());
        isLeft = true;
        if (customerTimer != null)
        {
            customerTimer.StopTimer();
            customerTimer.gameObject.SetActive(false);
        }
    }

    // Use for UI
    public void OnPlayerEntered(Collider2D collider)
    {
        if (foodRequestBox != null) foodRequestBox.SetTrigger("Open");
    }
    public void OnPlayerLeft(Collider2D collider)
    {
        if (foodRequestBox != null) foodRequestBox.SetTrigger("Close");
    }

}

[Serializable]
public class CustomerDetail
{
    [SerializeField] float appearTime;
    [SerializeField] Food foodRequest;

    public Food FoodRequest { get => foodRequest; }
}
