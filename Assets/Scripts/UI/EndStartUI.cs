using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class EndStartUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] RectTransform startUI;
    [SerializeField] RectTransform endUI;
    //[SerializeField] RectTransform endFailUI;
    //[SerializeField] RectTransform endWinUI;

    [SerializeField] RectTransform retryBtn;
    [SerializeField] RectTransform nextBtn;

    [SerializeField] TMP_Text startUICustomersText;
    [SerializeField] TMP_Text startUIGoalText;

    [SerializeField] TMP_Text endUICustomerText;
    [SerializeField] TMP_Text endUICoinTxt;

    [SerializeField] Image BGImage;

    [Header("Settings")]
    [SerializeField] float animationDuration = 0.5f;
    [Tooltip("Where the UI moves to when hidden (relative to its center)")]
    [SerializeField] Vector2 offScreenOffset = new Vector2(0, -1000);
    [Range(0, 1)][SerializeField] float bgMaxAlpha = 0.8f;

    private Vector2 _startUiOrigin;
    private Vector2 _endUiOrigin;
    private Coroutine _startRoutine;
    private Coroutine _endRoutine;

    // We add a specific coroutine to handle the sequence
    private Coroutine _sequenceRoutine;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("EndStartUI: GameManager instance not found!");
            return;
        }

        GameManager.Instance.OnLevelEnd.AddListener(OnLevelEnd);
        GameManager.Instance.OnCollectedMoneyChanged.AddListener(OnUpdateCoinChange);

        if (startUI) _startUiOrigin = startUI.anchoredPosition;
        if (endUI) _endUiOrigin = endUI.anchoredPosition;

        // Initialize positions
        endUI.anchoredPosition = _endUiOrigin + offScreenOffset;
        startUI.anchoredPosition = _startUiOrigin + offScreenOffset;

        // Start with Start UI visible
        SetUpStartUI();
        ToggleStartScreen(true);
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnLevelEnd.RemoveListener(OnLevelEnd);
        GameManager.Instance.OnCollectedMoneyChanged.RemoveListener(OnUpdateCoinChange);
    }

    private void SetUpStartUI()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("EndStartUI: GameManager instance not found!");
            return;
        }

        if (startUI == null)
        {
            Debug.LogError("EndStartUI: Start UI reference is missing!");
            return;
        }

        LevelDetail currentLevel = GameManager.Instance.GetCurrentLevelDetail();

        if (currentLevel == null)
        {
            Debug.LogError("EndStartUI: Current level detail is missing!");
            return;
        }

        int targetMoney = currentLevel.TargetMoney;
        int customerCount = currentLevel != null && currentLevel.CustomerDetails != null
            ? currentLevel.CustomerDetails.Count
            : 0;

        startUICustomersText.text = customerCount.ToString();
        startUIGoalText.text = "0/" + targetMoney.ToString();
    }

    private void SetUpEndUI()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("EndStartUI: GameManager instance not found!");
            return;
        }

        if (endUI == null)
        {
            Debug.LogError("EndStartUI: Start UI reference is missing!");
            return;
        }

        LevelDetail currentLevel = GameManager.Instance.GetCurrentLevelDetail();

        if (currentLevel == null)
        {
            Debug.LogError("EndStartUI: Current level detail is missing!");
            return;
        }

        int targetMoney = currentLevel.TargetMoney;
        int collectedMoney = GameManager.Instance.CollectedMoney;
        int customerCount = currentLevel != null && currentLevel.CustomerDetails != null
            ? currentLevel.CustomerDetails.Count
            : 0;

        //endUICoinTxt.text = collectedMoney.ToString();
        endUICustomerText.text = customerCount.ToString() + "/" + customerCount.ToString();
    }

    private void OnUpdateCoinChange()
    {
        UpdateEndUICoins(GameManager.Instance.CollectedMoney);
    }

    private void OnLevelEnd(bool isWin)
    {
        // 1. Show appropriate end UI (Logic moved from OnEnable)
        if (isWin)
        {
            ToggleWinUI();
        }
        else
        {
            ToggleFailUI();
        }

        // 2. Show the end screen
        ToggleEndScreen(true);

        // 3. Update the coin text (Logic preserved from your old method)
        OnUpdateCoinChange();
    }
    // --- BUTTON EVENTS ---

    public void OnStartClicked()
    {
        // Immediate toggle for starting the game
        ToggleStartScreen(false);
        GameManager.Instance.StartCurrentLevel();
    }

    public void OnRetryClicked()
    {
        // Start the sequence instead of immediate function calls
        if (_sequenceRoutine != null) StopCoroutine(_sequenceRoutine);
        _sequenceRoutine = StartCoroutine(ReturnToStartSequence());
        //if (GameManager.Instance != null)
        //{
        //    GameManager.Instance.RetryLevel();
        //}
        //else
        //{
        //    Debug.LogError("EndStartUI: GameManager instance not found!");
        //}
    }

    public void OnNextClicked()
    {
        // Start the sequence instead of immediate function calls
        if (_sequenceRoutine != null) StopCoroutine(_sequenceRoutine);
        _sequenceRoutine = StartCoroutine(ReturnToStartSequence());
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NextLevel();
        }
        else
        {
            Debug.LogError("EndStartUI: GameManager instance not found!");
        }
    }

    // --- THE NEW SEQUENCE ---

    private IEnumerator ReturnToStartSequence()
    {
        // 1. Tell End UI to go off-screen
        ToggleEndScreen(false);

        // 2. Wait exactly as long as the animation takes
        yield return new WaitForSeconds(animationDuration);

        // 3. Optional: Clean up Win/Fail objects so they are fresh for next time
        //if (endWinUI) endWinUI.gameObject.SetActive(false);
        //if (endFailUI) endFailUI.gameObject.SetActive(false);
        if (nextBtn) nextBtn.gameObject.SetActive(false);
        if (retryBtn) retryBtn.gameObject.SetActive(false);

        // 4. Now that End UI is gone, bring in the Start UI
        ToggleStartScreen(true);
    }

    // --- ANIMATION LOGIC (UNCHANGED) ---

    public void ToggleStartScreen(bool show)
    {
        if (_startRoutine != null) StopCoroutine(_startRoutine);
        // If we are showing the Start Screen, we definitely want the BG (true).
        // If we are hiding it, we let the animation handle the fade out.
        _startRoutine = StartCoroutine(AnimateUI(startUI, _startUiOrigin, show));
    }

    public void ToggleEndScreen(bool show)
    {
        if (_endRoutine != null) StopCoroutine(_endRoutine);
        // Only animate BG if showing End Screen. 
        // If hiding End Screen (going back to start), the Start Screen logic will pick up the BG later.
        // However, if we simply want the BG to fade out with the End Screen, pass true.
        _endRoutine = StartCoroutine(AnimateUI(endUI, _endUiOrigin, show));
        SetUpEndUI();
    }

    private IEnumerator AnimateUI(RectTransform targetUI, Vector2 originalPos, bool show, bool animateBG = true)
    {
        float time = 0;
        Vector2 posStart = targetUI.anchoredPosition;
        Vector2 posEnd = show ? originalPos : originalPos + offScreenOffset;

        // Determine BG target alpha
        Color bgStartColor = BGImage.color;
        Color bgEndColor = BGImage.color;

        // If showing UI -> Max Alpha. If hiding UI -> 0 Alpha.
        bgEndColor.a = show ? bgMaxAlpha : 0f;

        while (time < animationDuration)
        {
            float t = time / animationDuration;
            t = t * t * (3f - 2f * t); // Smooth step

            targetUI.anchoredPosition = Vector2.Lerp(posStart, posEnd, t);

            if (BGImage != null && animateBG)
            {
                BGImage.color = Color.Lerp(bgStartColor, bgEndColor, t);
            }

            time += Time.deltaTime;
            yield return null;
        }

        targetUI.anchoredPosition = posEnd;
        if (BGImage != null && animateBG) BGImage.color = bgEndColor;
    }

    public void ToggleWinUI()
    {
        retryBtn.gameObject.SetActive(false);
        nextBtn.gameObject.SetActive(true);
    }

    public void ToggleFailUI()
    {
        nextBtn.gameObject.SetActive(false);
        retryBtn.gameObject.SetActive(true);
    }

    public void UpdateEndUICoins(int coinCount)
    {
        if (endUICoinTxt != null)
        {
            endUICoinTxt.text = coinCount.ToString();
        }
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



    [ContextMenu("Test Toggle Win UI")]
    private void TestToggleCancelUI()

    {
        if (nextBtn == null || retryBtn == null) return;

        bool isWinActive = nextBtn.gameObject.activeSelf;

        if (isWinActive)
        {
            ToggleFailUI();
        }
        else
        {
            ToggleWinUI();
        }
    }
}