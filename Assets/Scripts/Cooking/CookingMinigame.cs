using UnityEngine;
using UnityEngine.UI;

public class CookingMinigame : MonoBehaviour
{
    public Slider slider;
    public RectTransform perfectZone;  // UI area for "orange zone"
    public RectTransform handle;  // Assign the slider handle rect here

    public float speed = 1f;
    private bool movingRight = true;
    private bool cooking = false;

    public float perfectZoneWidth = 0.2f;   // 20% of slider width (adjust in inspector)
    // Add this variable at the top with your other public variables
    [Tooltip("How much leeway to give the player (0.05 = 5% extra space on both sides)")]
    public float hitTolerance = 0.05f;


    public System.Action<CookResult> OnCookFinished;

    public enum CookResult { Normal, Bad}

    void Update()
    {
        if (!cooking) return;

        float range = speed * Time.deltaTime;

        if (movingRight)
        {
            slider.value += range;
            if (slider.value >= 1f) movingRight = false;
        }
        else
        {
            slider.value -= range;
            if (slider.value <= 0f) movingRight = true;
        }
    }

    public void StartCooking()
    {
        gameObject.SetActive(true);
        slider.value = 0;
        cooking = true;
        movingRight = true;

        RandomizePerfectZone();
    }

    void RandomizePerfectZone()
    {
        // pick a random center point
        float halfWidth = perfectZoneWidth / 2f;
        float center = Random.Range(halfWidth, 1f - halfWidth);

        // convert center + width to min/max anchors
        float min = center - halfWidth;
        float max = center + halfWidth;

        // apply to perfect zone rect
        perfectZone.anchorMin = new Vector2(min, perfectZone.anchorMin.y);
        perfectZone.anchorMax = new Vector2(max, perfectZone.anchorMax.y);
    }


    public bool IsCooking => cooking;

    public void FinishCook()
    {
        if (!cooking) return;

        cooking = false;

        float pointerPos = slider.value;
        float zoneMin = perfectZone.anchorMin.x;
        float zoneMax = perfectZone.anchorMax.x;

        CookResult result;

        // LOGIC CHANGE: We subtract tolerance from min and add it to max
        // This effectively widens the "win" area without changing the UI visuals
        if (pointerPos > (zoneMin - hitTolerance) && pointerPos < (zoneMax + hitTolerance))
        {
            result = CookResult.Normal;
        }
        else
        {
            result = CookResult.Bad;
        }

        OnCookFinished?.Invoke(result);
        gameObject.SetActive(false);
    }
}
