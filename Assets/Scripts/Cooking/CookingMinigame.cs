using UnityEngine;
using UnityEngine.UI;

public class CookingMinigame : MonoBehaviour
{
    public Slider slider;
    public RectTransform perfectZone;  // UI area for "orange zone"

    public float speed = 1f;
    private bool movingRight = true;
    private bool cooking = false;

    public float perfectZoneWidth = 0.2f;   // 20% of slider width (adjust in inspector)


    public System.Action<CookResult> OnCookFinished;

    public enum CookResult { Suspicious, Normal, Delicious }

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            FinishCook();
        }
    }

    public void StartCooking()
    {
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

        if (pointerPos > zoneMin && pointerPos < zoneMax)
        {
            result = CookResult.Delicious;
        }
        else if (Mathf.Abs(pointerPos - zoneMin) < 0.1f ||
                 Mathf.Abs(pointerPos - zoneMax) < 0.1f)
        {
            result = CookResult.Normal;
        }
        else
        {
            result = CookResult.Suspicious;
        }

        OnCookFinished?.Invoke(result);
    }

}
