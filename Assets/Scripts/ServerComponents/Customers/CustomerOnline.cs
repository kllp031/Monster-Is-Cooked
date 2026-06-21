using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class CustomerOnline : NetworkBehaviour, IInteractable
{
    [Header("Customer Settings")]
    [SerializeField] SkinGallery skinGallery;
    [SerializeField] SpriteLibrary spriteLibrary;
    [SerializeField]
    List<CoinEarnPercentagePerMood> coinEarnPercentages = new List<CoinEarnPercentagePerMood>() // Khá là hard code, sau có time thì để vào LevelDesign
    {
        new CoinEarnPercentagePerMood(Customer.Mood.Happy, 1.0f),
        new CoinEarnPercentagePerMood(Customer.Mood.Neutral, 0.5f),
        new CoinEarnPercentagePerMood(Customer.Mood.Angry, 0.1f),
        new CoinEarnPercentagePerMood(Customer.Mood.Hate, 0.0f),
        new CoinEarnPercentagePerMood(Customer.Mood.Embarrassed, 0.0f)
    };

    [SerializeField] float throwFoodBonus = 0.5f;
    [SerializeField] Animator customerAnimator;
    [SerializeField] CustomerTimerOnline customerTimer;

    [Header("Detect player settings")] // Used for request food box displaying
    [SerializeField] LayerMask playerLayer;
    [SerializeField] float detectRange = 2.0f;
    [SerializeField] Vector2 detectPointOffset;
    [SerializeField] Color detectRangeColor = Color.yellow;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 2.0f;
    [SerializeField] float distanceToTarget = 0.1f; // If the distance between customer and the target is less than this value, customer will be considered has reached target

    [Header("UI")]
    [SerializeField] Sprite hiddenFoodSprite; // Use to hide the request food until player asks
    [SerializeField] Animator foodRequestBox; // Use for fade in / fade out effect 
    [SerializeField] string foodRequestBoxAnimatorBool = "IsOpen";
    [SerializeField] Animator foodRequestBoxBackground; // Use for bouncing effect
    [SerializeField] string foodRequestBoxShowFoodTrigger = "Bounce";
    [SerializeField] Image foodIcon;
    [SerializeField] Animator moodIcon; //  Use to display current mood
    [SerializeField] ParticleSystem coinParticle; // Play when customer pays for the food

    // Networked properties
    [Networked] string SkinId { get; set; } // Use for displaying different skins
    [Networked, OnChangedRender(nameof(OnMoodChange))] Customer.Mood Mood { get; set; }
    [Networked] CustomerDetailOnline CustomerDetail { get; set; }
    [Networked] public bool IsActivated { get; set; }
    [Networked] public bool IsReadyToEat { get; set; }
    [Networked] public PathDetails TablePathDetails { get; private set; }

    private bool isAsked = false;

    public float MoveSpeed { get => moveSpeed; }
    public Animator CustomerAnimator { get => customerAnimator; }

    private void OnValidate()
    {
        //if (foodRequestBox == null) Debug.LogWarning("This customer has no food request box assigned!");
        //if (foodIcon == null) Debug.LogWarning("This customer has no food icon assigned!");
        //if (spriteLibrary == null) Debug.LogWarning("Please assign a sprite library to display different skins!");
        //if (skinGallery == null) Debug.LogWarning("Please assign a skin gallery to display skins");
    }
    private void OnEnable()
    {
        isAsked = false;
        if (customerTimer != null) customerTimer.gameObject.SetActive(false);
        if (foodIcon != null && hiddenFoodSprite != null)
            foodIcon.sprite = hiddenFoodSprite;
    }

    public override void Spawned()
    {
        base.Spawned();

        if (Runner != null && Runner.IsSharedModeMasterClient)
        {
            Mood = Customer.Mood.Happy;
            //IsActivated = false;
            IsReadyToEat = false;

            //if (customerTimer != null) customerTimer.SetTimer(CustomerDetail.WaitingTime);
        }

        // Set skin
        var skin = skinGallery.GetSkin(SkinId);
        if (spriteLibrary != null && skin != null) spriteLibrary.spriteLibraryAsset = skin;

        if (CustomersSpawnerOnline.Instance != null)
        {
            CustomersSpawnerOnline.Instance.OnCustomerSpawned(this);
        }
    }

    public override void Render()
    {
        CheckForPlayer();
    }
    //private void FixedUpdate()
    //{
    //    CheckForPlayer();
    //}

    public void SetActivated(bool activated)
    {
        if (Runner != null && Runner.IsSharedModeMasterClient) IsActivated = activated;
    }
    public void SetDetail(CustomerDetailOnline detail)
    {

        if (Runner != null && Runner.IsSharedModeMasterClient) CustomerDetail = detail;
    }
    public void SetTablePath(PathDetails tablePath)
    {
        if (Runner != null && Runner.IsSharedModeMasterClient) TablePathDetails = tablePath;
    }
    public void SetSkin(string skinId)
    {
        if (Runner != null && Runner.IsSharedModeMasterClient) SkinId = skinId;
    }
    public void SetMood(Customer.Mood mood)
    {
        if (Runner != null && Runner.IsSharedModeMasterClient) Mood = mood;
    }

    private void OnMoodChange()
    {
        if (moodIcon != null)
        {
            moodIcon.SetTrigger(Mood.ToString());
        }
    }

    // Can run locally
    public void OnClickInteract()
    {
        if (!IsReadyToEat) return; // Ignore any interactions if customer is not ready to eat

        if (!isAsked) // Show the requested food if this customer hasn't been asked yet
        {
            ShowRequestedFood();
            isAsked = true;
        }
    }

    // Can be use by any client
    public void OnInteract(GameObject obj)
    {
        //Debug.Log("On interacted with: " + obj.name);
        if (!IsReadyToEat) return; // Ignore any interactions if customer is not ready to eat

        if (obj.GetComponent<FoodHolderOnline>() != null) //  Interact with player, PROBABLY USE FOOTHOLDER ONLINE 
        {
            if (!isAsked) // Show the requested food if this customer hasn't been asked yet
            {
                ShowRequestedFood();
                isAsked = true;
            }
            else if (obj.GetComponent<FoodHolderOnline>().HeldRecipe != null)
            {
                //Food receivedFood = obj.GetComponent<FoodHolderOnline>().HeldFood;
                //if (receivedFood != null)
                //{
                    RPC_AnnouncePlayerServeFood(/*receivedFood.Recipe.RecipeName*/obj.GetComponent<FoodHolderOnline>().HeldRecipe.RecipeName); // Master client is responsible for processing the food and update coins
                    obj.GetComponent<FoodHolderOnline>().ServeFood();
                //}
            }
        }
        else if (obj.GetComponent<Food>() != null && Runner != null && Runner.IsSharedModeMasterClient) // The collided object is a food, only check for collision on master client's side
        {
            {
                if (!ProcessFood(obj.GetComponent<Food>().Recipe.RecipeName)) SetMood(Customer.Mood.Embarrassed);
                if (obj.TryGetComponent(out NetworkObject foodNetworkObj))
                {
                    Runner.Despawn(foodNetworkObj);
                }
                else
                {
                    obj.SetActive(false);
                }
                float percentage = GetCoinEarnPercentage(Mood);
                if (Mood != Customer.Mood.Embarrassed) percentage += throwFoodBonus;

                RPC_AnnounceServed(CustomerDetail.FoodRequestId, percentage);
            }

        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_AnnouncePlayerServeFood(string recipeName)
    {
        if (!ProcessFood(recipeName)) SetMood(Customer.Mood.Hate);
        float percentage = GetCoinEarnPercentage(Mood);
        RPC_AnnounceServed(CustomerDetail.FoodRequestId, percentage);
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_AnnounceServed(string foodId, float percentage)
    {
        //Debug.Log("I receive served announcement, I'm master client: " + Runner.IsSharedModeMasterClient);

        if (!IsReadyToEat) return; // Already served/left — block double-serve race (two players serving same customer)
        IsReadyToEat = false;      // Claim immediately; subsequent queued RPCs early-out above

        if (GameManagerOnline.Instance != null && CustomersSpawnerOnline.Instance != null)
        {
            var recipe = CustomersSpawnerOnline.Instance.RecipeGallery.GetRecipe(foodId);
            if (recipe != null)
            {
                GameManagerOnline.Instance.CollectedMoney += (int)(percentage * (float)recipe.Price);
                if (percentage != 0) RPC_PlayCoinParticle();
            }
        }
        OnLeave();
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_PlayCoinParticle()
    {
        if (coinParticle != null) coinParticle.Play();
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_NeutralizeCustomer()
    {
        if (foodRequestBox != null) foodRequestBox.SetBool(foodRequestBoxAnimatorBool, false);
    }

    private void ShowRequestedFood()
    {
        if (CustomersSpawnerOnline.Instance != null && CustomersSpawnerOnline.Instance.RecipeGallery != null)
        {
            var requestedFood = CustomersSpawnerOnline.Instance.RecipeGallery.GetRecipe(CustomerDetail.FoodRequestId);
            if (requestedFood != null)
            {
                if (foodIcon != null) foodIcon.sprite = requestedFood.Icon;
                if (foodRequestBoxBackground != null) foodRequestBoxBackground.SetTrigger(foodRequestBoxShowFoodTrigger);
            }
        }
    }
    public void ReadyToEat()
    {
        if (Runner == null || !Runner.IsSharedModeMasterClient) return;
        IsReadyToEat = true;
        SetMood(Customer.Mood.Happy);

        if (customerTimer != null)
        {
            //Debug.Log("Timer before: " + customerTimer.gameObject.activeInHierarchy);
            customerTimer.gameObject.SetActive(true);
            //Debug.Log("Timer after: " + customerTimer.gameObject.activeInHierarchy);
            //Debug.Log("Customer details waiting time: " + CustomerDetail.WaitingTime);
            customerTimer.SetTimer(CustomerDetail.WaitingTime);
            customerTimer.ResetTimer();
            customerTimer.StartTimer();
        }
    }
    public bool ProcessFood(string recipeName)
    {
        if (Runner == null || !Runner.IsSharedModeMasterClient) return false;
        if (CustomersSpawnerOnline.Instance == null) { /*Debug.LogWarning("No customers spawner found in this scene!");*/ return false; }
        if (CustomersSpawnerOnline.Instance.RecipeGallery == null) { /*Debug.LogWarning("No recipe gallery found!");*/ return false; }
        Recipe foodRequest = CustomersSpawnerOnline.Instance.RecipeGallery.GetRecipe(CustomerDetail.FoodRequestId);
        if (recipeName == foodRequest.RecipeName) return true;
        return false;

    }
    public bool MoveToTarget(Vector2 target)
    {
        if (Runner == null || !Runner.IsSharedModeMasterClient) return true;

        // //Debug.Log("Distance: " + Vector2.Distance(transform.position, target));
        if (Vector2.Distance(transform.position, target) <= distanceToTarget) return true;
        else
        {
            Vector2 direction = target - (Vector2)transform.position;
            direction = direction.normalized;
            transform.position += (Vector3)(direction * Runner.DeltaTime * moveSpeed);
            return false;
        }

    }
    public void OnLeave()
    {
        // This function will be called by the timer when the timer runs out or when the customer receives the food
        //Debug.Log("Customer leaves");
        IsReadyToEat = false;
        if (customerTimer != null)
        {
            customerTimer.StopTimer();
            customerTimer.gameObject.SetActive(false);
        }
        if (TablesManagerOnline.Instance != null) TablesManagerOnline.Instance.ReturnTable(Object.Id);

        RPC_NeutralizeCustomer();
    }
    public float GetCoinEarnPercentage(Customer.Mood mood)
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
        if (Object.IsValid && !IsReadyToEat) return; //  Only check for player if this customer is ready to eat
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
public struct CustomerDetailOnline: INetworkStruct
{
    [SerializeField] float waitingTime;
    [SerializeField] NetworkString<_32> recipeId;

    public CustomerDetailOnline(float appearTime, float waitingTime, string foodRequestId)
    {
        this.waitingTime = waitingTime;

        this.recipeId = foodRequestId;
    }

    public float WaitingTime { get => waitingTime; }
    public string FoodRequestId { get => recipeId.ToString(); }
}
