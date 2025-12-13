using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

// This class is used by the player to manage holding, dropping and throwing food items.
// General Idea: When we interact with a food that is interactable, that food will tell this food holder to hold it by calling PickUpFood() function
public class FoodHolder : MonoBehaviour
{
    //private Food heldFood;
    [SerializeField] Transform foodPivot;
    [SerializeField] GameObject dropFoodButton;
    [SerializeField] GameObject throwButton;
    [SerializeField] GameObject targetCircle;
    [Header("Throwing Settings")]
    [SerializeField] float targetCircleMovingTime = 1.0f;
    [SerializeField] Transform playerGroundPosition; // Use this if the player pivot is not at the bottom of the player's sprite
    [SerializeField] Vector2 throwDirection = Vector2.right;
    [SerializeField] float throwStartDistance = 2.0f;
    [SerializeField] float throwEndDistance = 10.0f;    
    [SerializeField] float throwAngle = 45f;
    [SerializeField] float throwHeightOffset = 1.0f;
    [SerializeField] LayerMask obstacleLayer;
    [Header("Animator")]
    [SerializeField] string startHoldingFoodTrigger = "";
    [SerializeField] string stopHoldingFoodTrigger = "";

    private Coroutine throwingCoroutine = null;

    // Food holder will get the selected food directy from HotbarManager
    public Food HeldFood
    {
        get
        {
            if (HotbarManager.Instance != null) 
                return HotbarManager.Instance.CurrentFood;
            return null;
        }
    }

    private void OnEnable()
    {
        if (targetCircle == null) Debug.LogWarning("Throw Food Circle is not assigned!");
        else targetCircle.SetActive(false);
        if (playerGroundPosition == null) Debug.LogWarning("Player Ground Position is not assigned!");
    }
    private void OnDisable()
    {
        if (targetCircle != null) targetCircle.SetActive(false);
        if (throwingCoroutine != null) { StopCoroutine(throwingCoroutine); throwingCoroutine = null; }
    }

    private void Update()
    {
        if (HeldFood != null)
        {
            if (dropFoodButton != null) dropFoodButton.SetActive(true);
            if (throwButton != null) throwButton.SetActive(true);

            HeldFood.transform.position = foodPivot.position;
        }
        else
        {
            if (dropFoodButton != null) dropFoodButton?.SetActive(false);
            if (throwButton != null) throwButton?.SetActive(false);
        }
    }

    public void PickUpFood(Food food)
    {
        if (HotbarManager.Instance != null)
        {
            HotbarManager.Instance.AddFoodToHotBar(food);
        }
    }
    private void DropHeldFood()
    {
        if (HeldFood == null) return;
        // Drop the food
        HeldFood.transform.position = transform.position + transform.right; // Drop in front of player
        if (HotbarManager.Instance != null) HotbarManager.Instance.RemoveSelectedFood();
    }
    public void ServeFood()
    {
        if (HeldFood != null) Destroy(HeldFood.gameObject);
        if (HotbarManager.Instance != null) HotbarManager.Instance.RemoveSelectedFood();
    }
    public void OnThrowFoodStarted()
    {
        // Check conditions
        if (targetCircle == null) return;
        if (playerGroundPosition == null) return;
        if (HeldFood == null) return;

        // Start moving the target circle
        if (throwingCoroutine != null) StopCoroutine(throwingCoroutine);
        throwingCoroutine = StartCoroutine(ThrowingCoroutine());
    }
    //private void OnThrowFoodEnded()
    //{
    //    if (targetCircle == null || heldFood == null || targetCircleStartPosition == null) return;
    //    Vector2 target = targetCircle.transform.position;
    //    target.y = targetCircleStartPosition.position.y; // Project target to ground level

    //    // Calculate throw speed
    //    float speed = CalculateThrowSpeed((Vector2)targetCircleStartPosition.position + new Vector2(0, throwHeightOffset), target, throwAngle, heldFood.GravityScale);
    //    if (speed > 0)
    //    {
    //        float direction = Mathf.Sign(target.x - heldFood.transform.position.x);
    //        float velocityX = speed * Mathf.Cos(throwAngle * Mathf.Deg2Rad) * direction;
    //        float velocityY = speed * Mathf.Sin(throwAngle * Mathf.Deg2Rad);
    //        heldFood.ApplyVelocity(new Vector2(velocityX, 0), velocityY);

    //    }
    //    heldFood.transform.position = targetCircleStartPosition.position;
    //    heldFood.SetHeight(throwHeightOffset);
    //    heldFood.IsDropped();
    //    heldFood = null;
    //}
    //private float CalculateThrowSpeed(Vector2 startPos, Vector2 targetPos, float angle, float gravityScale)
    //{
    //    angle = angle * Mathf.Deg2Rad; // Convert to radians
    //    float g = -Physics2D.gravity.magnitude * gravityScale;
    //    float x = Mathf.Abs(targetPos.x - startPos.x);
    //    float y = targetPos.y - startPos.y;
    //    float numerator = g * x * x;
    //    float denominator = 2 * (y - Mathf.Tan(angle) * x) * Mathf.Pow(Mathf.Cos(angle), 2);
    //    if (denominator >= 0)
    //    {
    //        Debug.LogWarning("Invalid throw parameters. Cannot calculate throw speed.");
    //        Debug.Log($" startPos: {startPos}, endPos: {targetPos}, g: {g}, x: {x}, y: {y}, denominator: {denominator}");
    //        return -1;
    //    }
    //    return Mathf.Sqrt(numerator / denominator);
    //}
    public void StopThrowFood()
    {
        if (targetCircle != null) targetCircle.SetActive(false);
        if (throwingCoroutine != null) StopCoroutine(throwingCoroutine);
        throwingCoroutine = null;
    }
    public void OnThrowFoodEnded()
    {
        // Check conditions
        if (throwingCoroutine == null) return;
        StopThrowFood();

        if (targetCircle == null || HeldFood == null || playerGroundPosition == null) return;
        
        Vector2 groundStartPoint = (Vector2)playerGroundPosition.position + throwDirection.normalized * throwStartDistance;
        Vector2 groundEndPoint = targetCircle.transform.position;

        // Calculate throw speed
        float speed = CalculateThrowSpeed((groundEndPoint - groundStartPoint).magnitude, -throwHeightOffset, throwAngle, HeldFood.GravityScale);
        if (speed > 0)
        {
            float groundSpeed = speed * Mathf.Cos(throwAngle * Mathf.Deg2Rad);
            Vector2 groundVelocity = (groundEndPoint - groundStartPoint).normalized * groundSpeed;
            float verticalVelocity = speed * Mathf.Sin(throwAngle * Mathf.Deg2Rad);
            HeldFood.ApplyVelocity(groundVelocity, verticalVelocity);

        }

        // Adjust held food's position
        HeldFood.transform.position = groundStartPoint;
        HeldFood.SetHeight(throwHeightOffset);
        HotbarManager.Instance.RemoveSelectedFood(); //  Tell Hotbar manager to remove the dropped food
    }
    private float CalculateThrowSpeed(float horizontalDistance, float verticalDistance, float angle, float gravityScale)
    {
        angle = angle * Mathf.Deg2Rad; // Convert to radians
        float g = -Physics2D.gravity.magnitude * gravityScale;
        float x = Mathf.Abs(horizontalDistance);
        float y = verticalDistance;
        float numerator = g * x * x;
        float denominator = 2 * (y - Mathf.Tan(angle) * x) * Mathf.Pow(Mathf.Cos(angle), 2);
        if (denominator >= 0)
        {
            Debug.LogWarning("Invalid throw parameters. Cannot calculate throw speed.");
            Debug.Log($" horizontalDistance {horizontalDistance}, vverticalDistance: {verticalDistance}, g: {g}, x: {x}, y: {y}, denominator: {denominator}");
            return -1;
        }
        return Mathf.Sqrt(numerator / denominator);
    }

    // These functions are called by the Input System
    public void OnThrowFood(InputAction.CallbackContext context)
    {
        if (context.started) OnThrowFoodStarted(); // Should use animation trigger (or any kinds of state machine) instead, if the animation is set successfully, it will call the OnThrowFoodStarted function
        if (context.canceled) OnThrowFoodEnded(); // Should use animation trigger (or any kinds of state machine) instead, if the animation is set successfully, it will call the OnThrowFoodEnded function
    }
    public void OnDropFood(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            DropHeldFood();
        }
    }

    IEnumerator ThrowingCoroutine()
    {
        if (targetCircle == null) yield break;

        targetCircle.SetActive(true);
        targetCircle.transform.position = (Vector2)playerGroundPosition.position + throwDirection.normalized * throwStartDistance;

        Vector2 targetPos = targetCircle.transform.position;
        Vector2 startPosition;
        Vector2 endPosition;

        float timeElapsed = 0;
        bool movingForward = true;

        SoundManager.Instance.PlaySFX(SoundManager.Instance.playerThrow);
        while (HeldFood != null)
        {
            throwDirection = (transform.localScale.x <= 0) ? Vector2.right : Vector2.left;
            var checkCollider = Physics2D.Raycast(transform.position, throwDirection, throwStartDistance, obstacleLayer);
            if (checkCollider.collider != null) { StopThrowFood(); yield break; }
            // Recalculate the start and end positions each loop in case the throwing direction changed
            startPosition = (Vector2)playerGroundPosition.position + throwDirection.normalized * throwStartDistance;
            endPosition = (Vector2)playerGroundPosition.position + throwDirection.normalized * throwEndDistance;
            Debug.Log($"Direction: {throwDirection}, start position: {startPosition}, end position: {endPosition}");

            timeElapsed += Time.deltaTime;
            if (timeElapsed >= targetCircleMovingTime)
            {
                // If the target circle has reached the end value, switch direction
                movingForward = (movingForward)? false : true;
                timeElapsed = 0;
            }

            targetPos = (movingForward) ?
                Vector2.Lerp(startPosition, endPosition, timeElapsed / targetCircleMovingTime)
                :
                Vector2.Lerp(endPosition, startPosition, timeElapsed / targetCircleMovingTime);

            targetCircle.transform.position = targetPos;

            yield return null;
        }

        targetCircle.SetActive(false);
    }
}
