using UnityEngine;
using UnityEngine.UI;

public class CookingMinigame : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("How fast the slider handle moves back and forth.")]
    public float cursorSpeed = 1f;

    [Tooltip("Width of the winning area as a percentage of the slider (0.2 = 20%).")]
    public float perfectZoneWidth = 0.2f;

    [Tooltip("How much leeway to give the player (0.05 = 5% extra space on both sides).")]
    public float hitTolerance = 0.05f;

    [Header("UI References")]
    public Slider slider;
    public RectTransform perfectZone;
    public RectTransform handle;

    // ---------------------------------------------------------
    // Events & State
    // ---------------------------------------------------------

    public System.Action<CookResult> OnCookFinished;
    public enum CookResult { Normal, Bad }

    private bool isMovingRight = true;
    private bool isCooking = false;

    // Public property to check state safely from other scripts
    public bool IsCooking => isCooking;

    // ---------------------------------------------------------
    // Core Loop
    // ---------------------------------------------------------

    void Update()
    {
        if (!isCooking) return;

        HandleCursorMovement();
    }

    private void HandleCursorMovement()
    {
        float step = cursorSpeed * Time.deltaTime;

        if (isMovingRight)
        {
            slider.value += step;
            if (slider.value >= 1f) isMovingRight = false;
        }
        else
        {
            slider.value -= step;
            if (slider.value <= 0f) isMovingRight = true;
        }
    }

    // ---------------------------------------------------------
    // Game Flow Control
    // ---------------------------------------------------------

    public void StartCooking()
    {
        gameObject.SetActive(true);

        // Reset State
        slider.value = 0;
        isCooking = true;
        isMovingRight = true;

        RandomizePerfectZone();
    }

    public void FinishCook()
    {
        if (!isCooking) return;

        isCooking = false;
        EvaluateResult();

        gameObject.SetActive(false);
    }

    // ---------------------------------------------------------
    // Logic Helpers
    // ---------------------------------------------------------

    private void RandomizePerfectZone()
    {
        // Pick a random center point within valid bounds
        float halfWidth = perfectZoneWidth / 2f;
        float center = Random.Range(halfWidth, 1f - halfWidth);

        // Convert center + width to Anchor Min/Max
        float min = center - halfWidth;
        float max = center + halfWidth;

        // Apply to UI RectTransform
        perfectZone.anchorMin = new Vector2(min, perfectZone.anchorMin.y);
        perfectZone.anchorMax = new Vector2(max, perfectZone.anchorMax.y);
    }

    private void EvaluateResult()
    {
        float pointerPos = slider.value;
        float zoneMin = perfectZone.anchorMin.x;
        float zoneMax = perfectZone.anchorMax.x;

        CookResult result;

        // Check if pointer is inside the zone (plus tolerance buffer)
        bool isHit = pointerPos > (zoneMin - hitTolerance) &&
                     pointerPos < (zoneMax + hitTolerance);

        if (isHit)
        {
            result = CookResult.Normal;
        }
        else
        {
            result = CookResult.Bad;
        }

        OnCookFinished?.Invoke(result);
    }
}