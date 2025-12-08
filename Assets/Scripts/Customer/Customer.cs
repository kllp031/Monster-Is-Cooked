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
    [Header("Customer Settings")]
    [SerializeField] Recipe foodRequest; // Use for demo only
    //[SerializeField] float waitingTime = 120f; // Use for demo only
    [SerializeField] Mood mood = Mood.Happy;
    [SerializeField] Path tablePath;
    [SerializeField] CustomerTimer customerTimer;
    [SerializeField] Animator customerAnimator;

    [Header("Detect player settings")] // Used for request food box displaying
    [SerializeField] LayerMask playerLayer;
    [SerializeField] float detectRange = 2.0f;
    [SerializeField] Vector2 detectPointOffset;
    [SerializeField] Color detectRangeColor = Color.yellow;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 2.0f;
    [SerializeField] float distanceToTarget = 0.1f; // If the distance between customer and the target is less than this value, customer will be considered has reached target
    [SerializeField] string animatorHorizontalValue = "Horizontal";
    [SerializeField] string animatorVerticalValue = "Vertical";

    [Header("UI")]
    [SerializeField] Sprite hiddenFoodSprite; // Use to hide the request food until player asks
    [SerializeField] Animator foodRequestBox; // Use for fade in / fade out effect 
    [SerializeField] string foodRequestBoxAnimatorBool = "IsOpen";
    [SerializeField] Animator foodRequestBoxBackground; // Use for bouncing effect
    [SerializeField] string foodRequestBoxShowFoodTrigger = "Bounce";
    [SerializeField] Image foodIcon;
    [SerializeField] Animator moodIcon; //  Use to display current mood

    [SerializeField] private CustomerDetail customerDetail = null;
    private bool isActivated = false;
    private bool isAsked = false;
    private bool isReadyToEat = false;
    private int seatNumer = 0;

    public int SeatNumber { get => seatNumer; set => seatNumer = value; }
    public Path TablePath { get => tablePath; set => tablePath = value; }
    public float MoveSpeed { get => moveSpeed; }
    public Animator CustomerAnimator { get => customerAnimator; }
    public bool IsReadyToEat { get => isReadyToEat; set => isReadyToEat = value; }
    public bool IsActivated { get => isActivated; }

    private void OnValidate()
    {
        if (foodRequestBox == null) Debug.LogWarning("This customer has no food request box assigned!");
        if (foodIcon == null) Debug.LogWarning("This customer has no food icon assigned!");
    }
    private void OnEnable()
    {
        isActivated = false;
        isAsked = false;
        isReadyToEat = false;
        if (customerTimer != null) customerTimer.gameObject.SetActive(false);

        // Demo
        customerDetail = new(0, 15, foodRequest);
    }
    private void FixedUpdate()
    {
        CheckForPlayer();
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
        if (!isReadyToEat) return; // Ignore any interactions if customer is not ready to eat

        if (customerDetail == null) return; // Can't interact with customers who doesn't have any customer detail

        if (obj.GetComponent<FoodHolder>() != null) //  Interact with player
        {
            if (!isAsked) // Show the requested food if this customer hasn't been asked yet
            {
                if (customerDetail.FoodRequest != null)
                {
                    // Uncomment this after applying the new Recipe
                    //if (foodIcon != null) foodIcon.sprite = customerDetail.FoodRequest.Recipe.Icon;
                    if (foodRequestBoxBackground != null) foodRequestBoxBackground.SetTrigger(foodRequestBoxShowFoodTrigger);
                }
                isAsked = true;
            }
            else
            {
                // Take the food from FoodHolder
                // Process the food
                OnLeave();
            }
        }
        else if (obj.GetComponent<Food>() != null)
        {
            {
                // Process the food
                OnLeave();
            }

        }
    }
    [ContextMenu("Activate")]
    public void Activate()
    {
        if (customerDetail == null) { Debug.LogWarning("This customer doesn't havve any customer detail to bbe activated!"); return; }
        isActivated = true;
    }
    public void ReadyToEat()
    {
        if (customerDetail == null) { Debug.LogWarning("This customer doesn't havve any customer detail to get ready!"); return; }
        isReadyToEat = true;
        SetMood(Mood.Happy);

        if (customerTimer != null)
        {
            customerTimer.gameObject.SetActive(true);
            customerTimer.SetTimer(customerDetail.WaitingTime);
            customerTimer.ResetCounter();
            customerTimer.StartTimer();
        }
        if (foodIcon != null && hiddenFoodSprite != null)
            foodIcon.sprite = hiddenFoodSprite;
    }
    public void ProcessFood(Recipe food)
    {
        if (customerDetail == null) { Debug.LogWarning("This customer has no detail assigned!"); return; }
        if (customerDetail.FoodRequest == null) { Debug.LogWarning("This customer's detail has invalid food request!"); return; }

        Recipe foodRequest = customerDetail.FoodRequest;
        // Compare food and foodRequest
        // Change mood based on the result
        // Update earned money according to the result
    }
    public bool MoveToTarget(Vector2 target)
    {
        if (Vector2.Distance(transform.position, target) <= distanceToTarget) return true;
        else
        {
            Vector2 direction = target - (Vector2)transform.position;
            direction = direction.normalized;
            Debug.Log(direction);
            transform.Translate(direction * Time.fixedDeltaTime * moveSpeed);
            if (customerAnimator != null)
            {
                customerAnimator.SetFloat(animatorHorizontalValue, direction.x);
                customerAnimator.SetFloat(animatorVerticalValue, direction.y);
            }
            return false;
        }

    }
    public void OnLeave()
    {
        // This function will be called by the timer when the timer runs out or when the customer receives the food
        isReadyToEat = false;
        if (foodRequestBox != null) foodRequestBox.SetBool(foodRequestBoxAnimatorBool, false);
        if (customerTimer != null)
        {
            customerTimer.StopTimer();
            customerTimer.gameObject.SetActive(false);
        }
    }

    // Use for UI
    //public void OnPlayerEntered(Collider2D collider)
    //{
    //    if (foodRequestBox != null && isReadyToEat) foodRequestBox.SetBool(foodRequestBoxAnimatorBool, true);
    //}
    //public void OnPlayerLeft(Collider2D collider)
    //{
    //    if (foodRequestBox != null && isReadyToEat) foodRequestBox.SetBool(foodRequestBoxAnimatorBool, false);
    //}

    private void CheckForPlayer()
    {
        if (!isReadyToEat) return; //  Only check for player if this customer is ready to eat
        var collider = Physics2D.OverlapCircle((Vector2)transform.position + detectPointOffset, detectRange, playerLayer);
        if (collider != null && collider.gameObject != null)
        {
            if (foodRequestBox != null) foodRequestBox.SetBool(foodRequestBoxAnimatorBool, true);
        }
        else
        {
            if (foodRequestBox != null) foodRequestBox.SetBool(foodRequestBoxAnimatorBool, false);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = detectRangeColor;
        Gizmos.DrawWireSphere((Vector2)transform.position + detectPointOffset, detectRange);
    }
}

[Serializable]
public class CustomerDetail
{
    [SerializeField] float appearTime;
    [SerializeField] float waitingTime;
    [SerializeField] Recipe foodRequest;

    public CustomerDetail(float appearTime, float waitingTime, Recipe foodRequest)
    {
        this.appearTime = appearTime;
        this.waitingTime = waitingTime;
        this.foodRequest = foodRequest;
    }

    public float AppearTime { get => appearTime; }
    public float WaitingTime { get => waitingTime; }
    public Recipe FoodRequest { get => foodRequest; }
}
