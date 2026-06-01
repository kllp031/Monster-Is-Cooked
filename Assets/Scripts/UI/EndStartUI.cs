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
    [SerializeField] RectTransform menuBtn;
    [Tooltip("EndWinPanel (or equivalent): all TMP under this gets theme text color.")]
    [SerializeField] RectTransform endWinPanel;

    [Header("Buttons (auto-wired in Start)")]
    [Tooltip("Nút Start trong Start panel. Gán Button component vào đây; " +
             "script sẽ tự AddListener(OnStartClicked).")]
    [SerializeField] Button startBtn;

    [Header("Multiplayer")]
    [Tooltip("(Optional) Text hiện 'Chờ host...' cho client không phải MasterClient.")]
    [SerializeField] TMP_Text waitingForHostText;

    [SerializeField] TMP_Text startUICustomersText;
    [SerializeField] TMP_Text startUIGoalText;
    [SerializeField] TMP_Text startUIDayText;

    [SerializeField] TMP_Text endUITargetText;
    [SerializeField] TMP_Text endUICoinTxt;

    [SerializeField] Image BGImage;

    [Header("End panel theme")]
    [SerializeField] Image endPanelBackgroundImage;
    [SerializeField] Image endTitleBackgroundImage;

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

    private static readonly Color32 EndThemeWoodText = new Color32(0x84, 0x34, 0x05, 255);
    private static readonly Color32 EndThemeStoneText = new Color32(0x25, 0x2B, 0x34, 255);
    /// <summary>Level / total coin lines — lighter than body text (same accent for wood and stone themes).</summary>
    private static readonly Color32 EndThemeCoinText = new Color32(0xC5, 0xCD, 0xD4, 255);

    private void Start()
    {
        // Luôn cache origin + ẩn panel ban đầu, ngay cả khi thiếu manager.
        // Điều này đảm bảo ToggleStartScreen(true) phía dưới vẫn hoạt động
        // nếu manager chưa sẵn sàng.
        if (startUI != null) _startUiOrigin = startUI.anchoredPosition;
        if (endUI != null) _endUiOrigin = endUI.anchoredPosition;

        if (endUI != null) endUI.anchoredPosition = _endUiOrigin + offScreenOffset;
        if (startUI != null) startUI.anchoredPosition = _startUiOrigin + offScreenOffset;

        // Subscribe sự kiện nếu manager có; nếu không, UI vẫn show được
        // (nhưng một số thông tin text có thể trống).
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelEnd.AddListener(OnLevelEnd);
        }
        else
        {
            Debug.LogWarning("EndStartUI: GameManager.Instance chưa có khi Start(). Level-end events sẽ không hoạt động.");
        }

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.onMoneyChanged.AddListener(OnUpdateCoinChange);
        }
        else
        {
            Debug.LogWarning("EndStartUI: PlayerDataManager.Instance chưa có khi Start(). Coin events sẽ không hoạt động.");
        }

        WireButtons();

        // Nhận broadcast "level started" từ MasterClient để ẩn start screen đồng bộ.
        GameManagerOnline.OnLevelStarted += HandleLevelStartedRemote;

        ToggleStartScreen(true);
    }

    private void Update()
    {
        // Trong multiplayer, chỉ MasterClient mới được bấm Start.
        // Poll interactable mỗi frame (rẻ, và bắt kịp khi host migrate).
        if (startBtn == null) return;

        if (IsMultiplayerActive())
        {
            bool isMaster = IsLocalMasterClient();
            startBtn.interactable = isMaster;
            if (waitingForHostText != null)
            {
                waitingForHostText.gameObject.SetActive(!isMaster);
            }
        }
        else
        {
            startBtn.interactable = true;
            if (waitingForHostText != null)
            {
                waitingForHostText.gameObject.SetActive(false);
            }
        }
    }

    private static bool IsMultiplayerActive()
    {
        return NetworkManager.Instance != null
            && NetworkManager.Instance.NetworkRunner != null
            && NetworkManager.Instance.NetworkRunner.IsRunning;
    }

    private static bool IsLocalMasterClient()
    {
        return NetworkManager.Instance != null
            && NetworkManager.Instance.NetworkRunner != null
            && NetworkManager.Instance.NetworkRunner.IsSharedModeMasterClient;
    }

    private void HandleLevelStartedRemote()
    {
        // Mọi client đều nhận: ẩn start screen.
        ToggleStartScreen(false);
    }

    // Auto-wire button clicks bằng code (không phụ thuộc Inspector OnClick).
    // - startBtn: serialize trực tiếp vào field -> AddListener(OnStartClicked).
    // - retryBtn / nextBtn: đang là RectTransform, thử GetComponent<Button>()
    //   trên cùng GameObject; nếu có thì tự nối.
    private void WireButtons()
    {
        if (startBtn != null)
        {
            startBtn.onClick.RemoveListener(OnStartClicked);
            startBtn.onClick.AddListener(OnStartClicked);
        }
        else
        {
            Debug.LogWarning("[EndStartUI] startBtn chưa được gán trong Inspector. " +
                             "Kéo Button 'Start' (trong startUI) vào field 'startBtn' để script tự wire OnStartClicked.");
        }

        if (retryBtn != null)
        {
            var btn = retryBtn.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(OnRetryClicked);
                btn.onClick.AddListener(OnRetryClicked);
            }
        }

        if (nextBtn != null)
        {
            var btn = nextBtn.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(OnNextClicked);
                btn.onClick.AddListener(OnNextClicked);
            }
        }
    }

    private void OnDisable()
    {
        // Cleanup event.
        GameManagerOnline.OnLevelStarted -= HandleLevelStartedRemote;

        // Cleanup button listeners.
        if (startBtn != null) startBtn.onClick.RemoveListener(OnStartClicked);
        if (retryBtn != null)
        {
            var btn = retryBtn.GetComponent<Button>();
            if (btn != null) btn.onClick.RemoveListener(OnRetryClicked);
        }
        if (nextBtn != null)
        {
            var btn = nextBtn.GetComponent<Button>();
            if (btn != null) btn.onClick.RemoveListener(OnNextClicked);
        }

        if (GameManager.Instance == null) return;

        GameManager.Instance.OnLevelEnd.RemoveListener(OnLevelEnd);
        GameManager.Instance.OnCollectedMoneyChanged.RemoveListener(OnUpdateCoinChange);
    }

    private void SetUpStartUI()
    {
        //if (GameManager.Instance == null)
        //{
        //    Debug.LogError("EndStartUI: GameManager instance not found!");
        //    return;
        //}

        //if (startUI == null)
        //{
        //    Debug.LogError("EndStartUI: Start UI reference is missing!");
        //    return;
        //}

        //LevelDetail currentLevel = GameManager.Instance.GetCurrentLevelDetail();

        //if (currentLevel == null)
        //{
        //    Debug.LogError("EndStartUI: Current level detail is missing!");
        //    return;
        //}

        //int targetMoney = currentLevel.TargetMoney;
        //int customerCount = currentLevel != null && currentLevel.CustomerDetails != null
        //    ? currentLevel.CustomerDetails.Count
        //    : 0;
        //int currentDay = GameManager.Instance.LevelNumber + 1;

        //startUICustomersText.text = customerCount.ToString();
        //startUIGoalText.text = "0/" + targetMoney.ToString();
        //startUIDayText.text = "Day " + currentDay.ToString();
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

        int collectedMoney = GameManager.Instance.CollectedMoney;
        int targetMoney = currentLevel.TargetMoney;
        int totalMoney = PlayerDataManager.Instance.TotalMoney;

        //endUICoinTxt.text = collectedMoney.ToString();
        endUITargetText.text = collectedMoney.ToString() + "/" + targetMoney.ToString();
        endUICoinTxt.text = totalMoney.ToString();
    }

    private void OnUpdateCoinChange()
    {
        UpdateEndUICoins(PlayerDataManager.Instance.TotalMoney);
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

        ApplyEndPanelTheme(isWin);

        // 2. Show the end screen
        ToggleEndScreen(true);

        // 3. Update the coin text (Logic preserved from your old method)
        OnUpdateCoinChange();
    }
    // --- BUTTON EVENTS ---

    public void OnStartClicked()
    {
        Debug.Log("[EndStartUI] OnStartClicked");

        // --- Multiplayer path ---
        if (IsMultiplayerActive())
        {
            if (!IsLocalMasterClient())
            {
                // Client thường không được start — chỉ MasterClient.
                Debug.Log("[EndStartUI] Chỉ MasterClient được start. Đang chờ host...");
                return;
            }

            if (GameManagerOnline.Instance == null)
            {
                Debug.LogError("[EndStartUI] Multiplayer: thiếu GameManagerOnline trong scene. " +
                               "Đảm bảo MultiplayerGameplay có 1 NetworkObject chứa GameManagerOnline.");
                return;
            }

            // Master: broadcast tới mọi client. RPC targets All -> master cũng nhận
            // và tự ẩn UI + call StartCurrentLevel qua handler.
            GameManagerOnline.Instance.RPC_StartLevel();
            return;
        }

        // --- Single-player path ---
        ToggleStartScreen(false);

        if (GameManager.Instance == null)
        {
            Debug.LogError("[EndStartUI] Không thể bắt đầu level: GameManager.Instance null. " +
                           "Thêm một GameObject có component GameManager vào scene.");
            return;
        }

        if (!GameManager.Instance.GameStarted)
        {
            Debug.LogError("[EndStartUI] Không thể bắt đầu level: GameManager.GameStarted == false. " +
                           "Kiểm tra GameManager: (1) levelDesign đã gán chưa, " +
                           "(2) PlayerDataManager có trong scene chưa, " +
                           "(3) saved level number có hợp lệ trong level design không.");
            return;
        }

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
        SetUpStartUI();
        if (BGImage != null)
        {
            BGImage.raycastTarget = show;
        }
    }

    public void ToggleEndScreen(bool show)
    {
        if (_endRoutine != null) StopCoroutine(_endRoutine);
        // Only animate BG if showing End Screen. 
        // If hiding End Screen (going back to start), the Start Screen logic will pick up the BG later.
        // However, if we simply want the BG to fade out with the End Screen, pass true.
        _endRoutine = StartCoroutine(AnimateUI(endUI, _endUiOrigin, show));
        SetUpEndUI();
        if (BGImage != null)
        {
            BGImage.raycastTarget = show;
        }
    }

    private IEnumerator AnimateUI(RectTransform targetUI, Vector2 originalPos, bool show, bool animateBG = true)
    {
        if (targetUI == null) yield break;

        float time = 0;
        Vector2 posStart = targetUI.anchoredPosition;
        Vector2 posEnd = show ? originalPos : originalPos + offScreenOffset;

        // Determine BG target alpha (BGImage có thể null)
        Color bgStartColor = BGImage != null ? BGImage.color : Color.clear;
        Color bgEndColor = bgStartColor;
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

    private void ApplyEndPanelTheme(bool isWin)
    {
        Color32 primaryText = isWin ? EndThemeWoodText : EndThemeStoneText;
        Color32 coinText = EndThemeCoinText;

        if (endWinPanel != null)
        {
            foreach (TMP_Text tmp in endWinPanel.GetComponentsInChildren<TMP_Text>(true))
            {
                if (tmp == endUITargetText || tmp == endUICoinTxt)
                    tmp.color = coinText;
                else
                    tmp.color = primaryText;
            }
        }
        else
        {
            if (endUITargetText != null) endUITargetText.color = coinText;
            if (endUICoinTxt != null) endUICoinTxt.color = coinText;
            SetButtonLabelColor(nextBtn, primaryText);
            SetButtonLabelColor(retryBtn, primaryText);
            SetButtonLabelColor(menuBtn, primaryText);
        }

        if (GameAssets.Instance == null)
            return;

        GameAssets ga = GameAssets.Instance;
        Sprite panelSprite = isWin ? ga.PanelWoodBG : ga.PanelStoneBG;
        Sprite titleSprite = isWin ? ga.TitleWoodBG : ga.TitleStoneBG;
        Sprite buttonSprite = isWin ? ga.ButtonWoodBG : ga.ButtonStoneBG;

        if (endPanelBackgroundImage != null && panelSprite != null)
            endPanelBackgroundImage.sprite = panelSprite;
        if (endTitleBackgroundImage != null && titleSprite != null)
            endTitleBackgroundImage.sprite = titleSprite;

        SetButtonBackgroundSprite(nextBtn, buttonSprite);
        SetButtonBackgroundSprite(retryBtn, buttonSprite);
        SetButtonBackgroundSprite(menuBtn, buttonSprite);
    }

    private static void SetButtonLabelColor(RectTransform buttonRoot, Color32 color)
    {
        if (buttonRoot == null) return;
        TMP_Text tmp = buttonRoot.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
            tmp.color = color;
    }

    private static void SetButtonBackgroundSprite(RectTransform buttonRoot, Sprite sprite)
    {
        if (buttonRoot == null || sprite == null)
            return;
        Image img = buttonRoot.GetComponent<Image>();
        if (img != null)
            img.sprite = sprite;
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