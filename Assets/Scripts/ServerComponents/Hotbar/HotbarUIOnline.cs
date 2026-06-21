using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Online version of <see cref="HotbarUI"/>. Subscribe vào
/// <see cref="HotbarManagerOnline"/> + dùng <see cref="HotbarSlotUIOnline"/>.
/// Logic slide panel + visual giữ nguyên.
/// </summary>
public class HotbarUIOnline : MonoBehaviour
{
    [SerializeField] private RectTransform hotbarPanel;
    [SerializeField] private RectTransform hotbarAnchor;

    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.5f;

    public List<HotbarSlotUIOnline> uiSlots;

    private Vector2 onscreenPosition;
    private Vector2 offscreenPosition;
    private Coroutine currentAnimation;
    private int currentSelectionIndex = -1;

    private void Awake()
    {
        if (hotbarAnchor != null)
        {
            onscreenPosition = hotbarAnchor.anchoredPosition;
            offscreenPosition = new Vector2(onscreenPosition.x, -onscreenPosition.y);
        }
    }

    private void Start()
    {
        if (hotbarAnchor == null) { /*Debug.LogError($"{nameof(HotbarUIOnline)}: hotbarAnchor null.");*/ return; }
        if (HotbarManagerOnline.Instance == null) { /*Debug.LogError($"{nameof(HotbarUIOnline)}: HotbarManagerOnline.Instance null.");*/ return; }

        uiSlots = hotbarPanel.GetComponentsInChildren<HotbarSlotUIOnline>().ToList();

        HotbarManagerOnline.Instance.OnHotbarUpdated += RefreshVisuals;
        HotbarManagerOnline.Instance.OnSelectionChanged += HandleSelectionChange;

        currentSelectionIndex = HotbarManagerOnline.Instance.SelectedSlotIndex;
        RefreshVisuals();
    }

    private void OnEnable()
    {
        if (HotbarManagerOnline.Instance != null)
        {
            HotbarManagerOnline.Instance.OnHotbarUpdated += RefreshVisuals;
            HotbarManagerOnline.Instance.OnSelectionChanged += HandleSelectionChange;
        }
    }

    private void OnDisable()
    {
        if (HotbarManagerOnline.Instance != null)
        {
            HotbarManagerOnline.Instance.OnHotbarUpdated -= RefreshVisuals;
            HotbarManagerOnline.Instance.OnSelectionChanged -= HandleSelectionChange;
        }
    }

    public void HidePanel()
    {
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(SlidePanel(offscreenPosition));
    }

    public void ShowPanel()
    {
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(SlidePanel(onscreenPosition));
    }

    private IEnumerator SlidePanel(Vector2 targetPosition)
    {
        Vector2 startPosition = hotbarAnchor.anchoredPosition;
        float elapsedTime = 0f;
        while (elapsedTime < slideDuration)
        {
            float t = elapsedTime / slideDuration;
            t = Mathf.SmoothStep(0f, 1f, t);
            hotbarAnchor.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        hotbarAnchor.anchoredPosition = targetPosition;
    }

    private void HandleSelectionChange(int newIndex)
    {
        currentSelectionIndex = newIndex;
        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        if (HotbarManagerOnline.Instance == null) return;
        for (int i = 0; i < uiSlots.Count; i++)
        {
            Recipe r = HotbarManagerOnline.Instance.GetRecipeAt(i);
            bool isSelected = (i == currentSelectionIndex);
            uiSlots[i].Setup(i, r, isSelected);
        }
    }
}
