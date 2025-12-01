using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour, IInteractable
{
    [SerializeField] Recipe requestFood; // Use for demo only
    [Header("UI")]
    [SerializeField] Sprite hiddenFoodSprite; // Use to hide the request food until player asks (Demo only)
    [SerializeField] Animator foodRequestBox; // Use for fade in / fade out effect (Demo only)
    [SerializeField] Animator foodRequestBoxBackground; // Use for bouncing effect (Demo only)
    [SerializeField] Image foodIcon;

    private CustomerDetail customerDetail = null;
    private bool isAsked = false;
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
        // Use for demo only
        if (foodIcon != null && hiddenFoodSprite != null)
            foodIcon.sprite = hiddenFoodSprite;
    }
    public void SetDetail(CustomerDetail detail)
    {
        customerDetail = detail;
    }
    public void OnInteract(GameObject obj)
    {
        Debug.Log("Customer is interacted");
        if (!isAsked) // Show the requested food if customer hasnt been asked yet
        {
            if (requestFood != null)
            {
                // Use for demo only
                if (foodIcon != null) foodIcon.sprite = requestFood.icon;
                if (foodRequestBoxBackground != null) foodRequestBoxBackground.SetTrigger("Bounce");
            }
            isAsked = true;
        }
        else
        {
            if (obj.GetComponent<Food>() != null) Debug.Log("Customer received food indirectly!");
            if (obj.GetComponent<FoodHolder>() != null) Debug.Log("Customer received food directly from player!");

            if (customerDetail == null) { Debug.LogWarning("This customer has no detail assigned!"); return; }
            if (customerDetail.FoodRequest == null) { Debug.LogWarning("This customer's detail has invalid food request!"); return; }
            if (foodIcon != null) foodIcon.sprite = customerDetail.FoodRequest.Recipe.icon;
        }

    }

    public void OnPlayerEntered(Collider2D collider)
    {
        if (foodRequestBox != null) foodRequestBox.SetTrigger("Open");
    }
    public void OnPlayerLeft(Collider2D collider)
    {
        if (foodRequestBox != null) foodRequestBox.SetTrigger("Close");
    }
    public void OnLeave()
    {
        // This function will be called by the timer when the timer runs out
        Debug.Log("Customer is leaving");
    }
}

[Serializable]
public class CustomerDetail
{
    [SerializeField] float appearTime;
    [SerializeField] Food foodRequest;

    public Food FoodRequest { get => foodRequest; }
}
