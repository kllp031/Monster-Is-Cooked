using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D.Animation;
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
    [SerializeField] SpriteLibrary spriteLibrary; // Use for displaying different skins
    [SerializeField]
    List<CoinEarnPercentagePerMood> coinEarnPercentages = new List<CoinEarnPercentagePerMood>()
    {
        new CoinEarnPercentagePerMood(Mood.Happy, 1.0f),
        new CoinEarnPercentagePerMood(Mood.Neutral, 0.5f),
        new CoinEarnPercentagePerMood(Mood.Angry, 0.1f),
        new CoinEarnPercentagePerMood(Mood.Embarrassed, 0.0f)
    };
    [SerializeField] Mood mood = Mood.Happy;
    [SerializeField] float throwFoodBonus = 0.5f;
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
    [SerializeField] ParticleSystem coinParticle; // Play when customer pays for the food

    [SerializeField] private CustomerDetail customerDetail = null;
    private bool isActivated = false;
    private bool isAsked = false;
    private bool isReadyToEat = false;
    private Path tablePath;

    public Path TablePath { get => tablePath; }
    public float MoveSpeed { get => moveSpeed; }
    public Animator CustomerAnimator { get => customerAnimator; }
    public bool IsReadyToEat { get => isReadyToEat; set => isReadyToEat = value; }
    public bool IsActivated { get => isActivated; }

    private void OnValidate()
    {
        if (foodRequestBox == null) Debug.LogWarning("This customer has no food request box assigned!");
        if (foodIcon == null) Debug.LogWarning("This customer has no food icon assigned!");
        if (spriteLibrary == null) Debug.LogWarning("Please assign a sprite library to display different skins!");
    }
    private void OnEnable()
    {
        isActivated = false;
        isAsked = false;
        isReadyToEat = false;
        if (customerTimer != null) customerTimer.gameObject.SetActive(false);

    }
    private void FixedUpdate()
    {
        CheckForPlayer();
    }
    
    public void SetDetail(CustomerDetail detail)
    {
        customerDetail = detail;
        customerTimer.SetTimer(customerDetail.WaitingTime);
    }
    public void SetTablePath(Path tablePath) { this.tablePath = tablePath; }
    public void SetSkin(SpriteLibraryAsset skin)
    {
        if (spriteLibrary == null || skin == null) return;
        spriteLibrary.spriteLibraryAsset = skin;
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
                    if (foodIcon != null) foodIcon.sprite = customerDetail.FoodRequest.Icon;
                    if (foodRequestBoxBackground != null) foodRequestBoxBackground.SetTrigger(foodRequestBoxShowFoodTrigger);
                }
                isAsked = true;
            }
            else
            {
                Food receivedFood = obj.GetComponent<FoodHolder>().HeldFood;
                if (receivedFood != null)
                {
                    if (!ProcessFood(receivedFood.Recipe)) SetMood(Mood.Angry);
                    obj.GetComponent<FoodHolder>().ServeFood();
                    float percentage = GetCoinEarnPercentage(mood);
                    if (GameManager.Instance != null) GameManager.Instance.CollectedMoney += (int)(percentage * (float)customerDetail.FoodRequest.Price);
                    if (percentage != 0 && coinParticle != null) coinParticle.Play();
                    OnLeave();
                }
            }
        }
        else if (obj.GetComponent<Food>() != null)
        {
            {
                if (!ProcessFood(obj.GetComponent<Food>().Recipe)) SetMood(Mood.Embarrassed);
                obj.SetActive(false);
                float percentage = GetCoinEarnPercentage(mood);
                if (mood != Mood.Embarrassed) percentage += throwFoodBonus;
                if (GameManager.Instance != null) GameManager.Instance.CollectedMoney += (int)(percentage * (float)customerDetail.FoodRequest.Price);
                if (percentage != 0 && coinParticle != null) coinParticle.Play();
                OnLeave();
            }

        }
    }
    public void Activate()
    {
        if (customerDetail == null) { Debug.LogWarning("This customer doesn't have any customer detail to be activated!"); return; }
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
    public bool ProcessFood(Recipe food)
    {
        if (customerDetail == null) { Debug.LogWarning("This customer has no detail assigned!"); return false; }
        if (customerDetail.FoodRequest == null) { Debug.LogWarning("This customer's detail has invalid food request!"); return false; }

        Recipe foodRequest = customerDetail.FoodRequest;
        if (food == foodRequest) return true;
        return false;
        
    }
    public bool MoveToTarget(Vector2 target)
    {
        if (Vector2.Distance(transform.position, target) <= distanceToTarget) return true;
        else
        {
            Vector2 direction = target - (Vector2)transform.position;
            direction = direction.normalized;
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
        if (TablesManager.Instance != null) TablesManager.Instance.ReturnTable(this);
    }
    public float GetCoinEarnPercentage(Mood mood)
    {
        float res = 0.0f;
        foreach (var item in coinEarnPercentages)
        {
            if (item.Mood == mood) { res = item.EarnPercentage; break; }
        }
        return res;
    }

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
public class CoinEarnPercentagePerMood
{
    [SerializeField] Customer.Mood mood;
    [SerializeField] float earnPercentage = 1.0f;

    public Customer.Mood Mood { get => mood; }
    public float EarnPercentage { get => earnPercentage; }

    public CoinEarnPercentagePerMood(Customer.Mood mood, float earnPercentage)
    {
        this.mood = mood;
        this.earnPercentage = earnPercentage;
    }
}

[Serializable]
public class CustomerDetail
{
    [SerializeField] float waitingTime = 5;
    [SerializeField] Recipe foodRequest;

    public CustomerDetail(float appearTime, float waitingTime, Recipe foodRequest)
    {
        this.waitingTime = waitingTime;
        this.foodRequest = foodRequest;
    }

    public float WaitingTime { get => waitingTime; }
    public Recipe FoodRequest { get => foodRequest; }
}
