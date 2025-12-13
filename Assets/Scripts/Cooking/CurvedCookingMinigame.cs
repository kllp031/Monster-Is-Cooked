using UnityEngine;
using UnityEngine.UI;

public class CurvedCookingMinigame : MonoBehaviour
{
    [Header("Game Configuration")]
    [Tooltip("Speed of the needle in Degrees per Second")]
    public float rotationSpeed = 150f;

    [Tooltip("How many degrees of error are allowed? (e.g. 5 degrees leeway)")]
    public float toleranceDegrees = 5f;

    [Header("Visual References")]
    public Image successZoneImage;
    public RectTransform needleTransform;

    [Header("Bar Constraints")]
    // The Left side of the gauge (Start point)
    public float leftLimit = 90f;
    // The Right side of the gauge (End point)
    public float rightLimit = -90f;

    [Header("Zone Settings")]
    [Range(0.05f, 0.5f)]
    public float desiredFillAmount = 0.15f; // Size of the green bar

    // ---------------------------------------------------------
    // Events & State
    // ---------------------------------------------------------

    public System.Action<bool> OnCookFinished; // True = Success, False = Fail
    private bool isCooking = false;
    public bool IsCooking => isCooking;
    private bool movingClockwise = true; // Equivalent to "isMovingRight"
    private float currentAngle;

    // ---------------------------------------------------------
    // Core Loop
    // ---------------------------------------------------------

    void Update()
    {
        if (!isCooking) return;

        HandleNeedleMovement();
    }

    private void HandleNeedleMovement()
    {
        float step = rotationSpeed * Time.deltaTime;

        if (movingClockwise)
        {
            // Moving towards Right Limit (Angles decrease: 90 -> -90)
            currentAngle -= step;
            if (currentAngle <= rightLimit)
            {
                currentAngle = rightLimit; // Clamp
                movingClockwise = false;   // Flip direction
            }
        }
        else
        {
            // Moving towards Left Limit (Angles increase: -90 -> 90)
            currentAngle += step;
            if (currentAngle >= leftLimit)
            {
                currentAngle = leftLimit; // Clamp
                movingClockwise = true;   // Flip direction
            }
        }

        // Apply rotation to the Z axis
        needleTransform.localEulerAngles = new Vector3(0, 0, currentAngle);
    }

    // ---------------------------------------------------------
    // Game Flow Control
    // ---------------------------------------------------------

    public void StartCooking()
    {
        // 1. Randomize the zone first
        RandomizeZone();

        // 2. Reset Needle to the Left Start point
        currentAngle = leftLimit;
        needleTransform.localEulerAngles = new Vector3(0, 0, currentAngle);
        movingClockwise = true;

        // 3. Enable Game Loop
        isCooking = true;
    }

    public void FinishCook() // Call this when Player presses Space/Interact
    {
        if (!isCooking) return;

        isCooking = false;

        // Check the result
        bool success = CheckWinCondition();

        if (success)
            SoundManager.Instance.PlaySFX(SoundManager.Instance.cookingSuccess);
        else
            SoundManager.Instance.PlaySFX(SoundManager.Instance.cookingFailure);

        // Trigger Event (You can subscribe to this in your Stove script)
        OnCookFinished?.Invoke(success);

        Debug.Log(success ? "COOK SUCCESS!" : "COOK FAILED!");
    }

    // ---------------------------------------------------------
    // Logic Helpers
    // ---------------------------------------------------------

    private void RandomizeZone()
    {
        successZoneImage.fillAmount = desiredFillAmount;

        // Calculate width in degrees
        float zoneWidthDegrees = desiredFillAmount * 360f;

        // Calculate valid range for the "Start" of the arc
        // Logic: The arc starts at Z and goes Clockwise.

        float maxStartAngle = leftLimit;
        float minStartAngle = rightLimit + zoneWidthDegrees;

        // Safety Check
        if (minStartAngle > maxStartAngle)
        {
            Debug.LogError("Zone is too big for the bar limits!");
            return;
        }

        // Pick random angle
        float randomStartAngle = Random.Range(minStartAngle, maxStartAngle);

        // Apply
        successZoneImage.rectTransform.localEulerAngles = new Vector3(0, 0, randomStartAngle);
    }

    private bool CheckWinCondition()
    {
        // 1. Get current needle angle
        float needleZ = needleTransform.localEulerAngles.z;
        // Normalize 0-360 to -180 to 180 range for easier math
        if (needleZ > 180) needleZ -= 360;

        // 2. Get Zone Start and End
        float zoneStart = successZoneImage.rectTransform.localEulerAngles.z;
        if (zoneStart > 180) zoneStart -= 360;

        float zoneWidth = successZoneImage.fillAmount * 360f;
        float zoneEnd = zoneStart - zoneWidth; // Because we fill Clockwise (decreasing angle)

        // 3. Debug visuals
        // Debug.Log($"Needle: {needleZ} | Zone: {zoneStart} to {zoneEnd}");

        // 4. Check intersection with Tolerance
        // Since angles decrease going Right, "Start" is the Higher Value, "End" is Lower.
        // We add tolerance to the top and subtract from the bottom.

        bool inside = (needleZ <= zoneStart + toleranceDegrees) &&
                      (needleZ >= zoneEnd - toleranceDegrees);

        return inside;
    }
}