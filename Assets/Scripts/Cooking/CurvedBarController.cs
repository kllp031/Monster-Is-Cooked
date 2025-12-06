using UnityEngine;
using UnityEngine.UI;

public class CurvedBarController : MonoBehaviour
{
    [Header("References")]
    // Just the image itself, no pivot needed!
    public Image successZoneImage;

    [Header("Bar Limits (Degrees)")]
    public float leftLimit = 90f;
    public float rightLimit = -90f;

    [Header("Zone Settings")]
    // Fill amount is 0 to 1 of a FULL CIRCLE (360 degrees)
    [Range(0.05f, 0.5f)] // Limited range since background is only 180 degrees
    public float desiredFillAmount = 0.1f;

    [Header("Neddle Settings")]
    public RectTransform needleTransform;

    void Start()
    {
        RandomizeZone();
    }

    public void RandomizeZone()
    {
        // 1. Apply the desired size
        successZoneImage.fillAmount = desiredFillAmount;

        // 2. Calculate width in degrees (Radial 360)
        float zoneWidthDegrees = desiredFillAmount * 360f;

        // 3. Calculate safe bounds for the STARTing rotation.
        // The zone starts at rotation Z and fills clockwise.

        // Constraint A: Start angle can't be further left than the left limit.
        float maxStartAngle = leftLimit;

        // Constraint B: End angle (Start - Width) can't be further right than right limit.
        // So, Start must be at least (RightLimit + Width).
        float minStartAngle = rightLimit + zoneWidthDegrees;

        // Safety check
        if (minStartAngle > maxStartAngle)
        {
            Debug.LogError("Zone is too big for the bar limits!");
            successZoneImage.rectTransform.localEulerAngles = Vector3.zero;
            return;
        }

        // 4. Pick a random start angle within bounds
        float randomStartAngle = Random.Range(minStartAngle, maxStartAngle);

        // 5. Apply rotation directly to the image
        successZoneImage.rectTransform.localEulerAngles = new Vector3(0, 0, randomStartAngle);
    }

    // To help you check Win/Loss later
    public bool IsNeedleInZone(float needleAngle)
    {
        // Normalize angle if needed (Unity likes -180 to 180)
        if (needleAngle > 180) needleAngle -= 360;

        float startAngle = successZoneImage.rectTransform.localEulerAngles.z;
        // Normalize start angle too
        if (startAngle > 180) startAngle -= 360;

        float zoneWidth = successZoneImage.fillAmount * 360f;
        float endAngle = startAngle - zoneWidth;

        Debug.Log($"Needle Angle: {needleAngle}, Zone Start: {startAngle}, Zone End: {endAngle}");

        // Check if needle is between start and end
        return needleAngle <= startAngle && needleAngle >= endAngle;
    }

    [ContextMenu("Test Randomize")]
    public void TestRandomizeFromEditor()
    {
        RandomizeZone();
    }

    [ContextMenu("Test Needle")]
    public void TestNeedle()
    {
        if (IsNeedleInZone(needleTransform.localEulerAngles.z))
        {
            Debug.Log("Needle is IN the zone!");
        }
        else
        {
            Debug.Log("Needle is OUTSIDE the zone!");
        }
    }
}