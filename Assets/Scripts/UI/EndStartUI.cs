using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndStartUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] RectTransform startUI;
    [SerializeField] RectTransform endUI;
    [SerializeField] Image BGImage;

    [Header("Settings")]
    [SerializeField] float animationDuration = 0.5f;
    [Tooltip("Where the UI moves to when hidden (relative to its center)")]
    [SerializeField] Vector2 offScreenOffset = new Vector2(0, -1000);
    [Range(0, 1)][SerializeField] float bgMaxAlpha = 0.8f; // Max opacity for background

    private Vector2 _startUiOrigin;
    private Vector2 _endUiOrigin;
    private Coroutine _startRoutine;
    private Coroutine _endRoutine;

    private void Start()
    {
        // 1. Store the positions where you placed them in the Editor
        if (startUI) _startUiOrigin = startUI.anchoredPosition;
        if (endUI) _endUiOrigin = endUI.anchoredPosition;


        //startUI.anchoredPosition = _startUiOrigin + offScreenOffset;
        endUI.anchoredPosition = _endUiOrigin + offScreenOffset;

        // Optional: Hide them immediately on start if you want
        //SetUIState(startUI, false, true);
        //SetUIState(endUI, false, true);
    }

    // Call this via button or other scripts
    public void ToggleStartScreen(bool show)
    {
        if (_startRoutine != null) StopCoroutine(_startRoutine);
        _startRoutine = StartCoroutine(AnimateUI(startUI, _startUiOrigin, show));
    }

    // Call this via button or other scripts
    public void ToggleEndScreen(bool show)
    {
        if (_endRoutine != null) StopCoroutine(_endRoutine);
        _endRoutine = StartCoroutine(AnimateUI(endUI, _endUiOrigin, show));
    }

    private IEnumerator AnimateUI(RectTransform targetUI, Vector2 originalPos, bool show)
    {
        float time = 0;

        // Define Start and End positions for the UI
        Vector2 posStart = targetUI.anchoredPosition;
        Vector2 posEnd = show ? originalPos : originalPos + offScreenOffset;

        // Define Start and End alpha for the Background
        Color bgStartColor = BGImage.color;
        Color bgEndColor = BGImage.color;
        bgEndColor.a = show ? bgMaxAlpha : 0f; // Fade to 0 if hiding, fade to Max if showing

        while (time < animationDuration)
        {
            // Smooth step for nicer ease-in/ease-out effect
            float t = time / animationDuration;
            t = t * t * (3f - 2f * t);

            // 1. Move the UI
            targetUI.anchoredPosition = Vector2.Lerp(posStart, posEnd, t);

            // 2. Fade the Background
            // We only fade the background if we have a reference to it
            if (BGImage != null)
            {
                BGImage.color = Color.Lerp(bgStartColor, bgEndColor, t);
            }

            time += Time.deltaTime;
            yield return null;
        }

        // Ensure exact final values
        targetUI.anchoredPosition = posEnd;
        if (BGImage != null) BGImage.color = bgEndColor;
    }

    [ContextMenu("Test Toggle Start UI")]
    private void TestToggleStartUI()
    {
        if (startUI == null) return;
        bool isVisible = Vector2.Distance(startUI.anchoredPosition, _startUiOrigin) < 0.1f;
        ToggleStartScreen(!isVisible);
    }

    [ContextMenu("Test Toggle End UI")]
    private void TestToggleEndUI()
    {
        if (endUI == null) return;
        bool isVisible = Vector2.Distance(endUI.anchoredPosition, _endUiOrigin) < 0.1f;
        ToggleEndScreen(!isVisible);
    }
}