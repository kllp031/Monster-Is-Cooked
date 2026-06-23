using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using TMPro;

/// <summary>
/// Online variant of EndStartUI. Replaces PlayerDataManager references with LocalPlayerData.
/// All multiplayer / animation logic identical to EndStartUI.
/// </summary>
public class EndStartUIOnline : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] RectTransform startUI;
    [SerializeField] RectTransform endUI;
    [SerializeField] RectTransform retryBtn;
    [SerializeField] RectTransform nextBtn;
    [SerializeField] RectTransform menuBtn;
    [Tooltip("EndWinPanel: all TMP under this gets theme text color.")]
    [SerializeField] RectTransform endWinPanel;

    [Header("Buttons (auto-wired in Start)")]
    [SerializeField] Button startBtn;

    [Header("Multiplayer")]
    [SerializeField] TMP_Text waitingForHostText;
    [SerializeField] TMP_Text waitingForNextText;

    private bool _hasClickedRetry;

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
    [SerializeField] Vector2 offScreenOffset = new Vector2(0, -1000);
    [Range(0, 1)][SerializeField] float bgMaxAlpha = 0.8f;

    private Vector2 _startUiOrigin;
    private Vector2 _endUiOrigin;
    private Coroutine _startRoutine;
    private Coroutine _endRoutine;
    private Coroutine _sequenceRoutine;

    private PlayerDataManagerOnline PlayerData => LocalPlayerData.Instance?.Data;

    private static readonly Color32 EndThemeWoodText  = new Color32(0x84, 0x34, 0x05, 255);
    private static readonly Color32 EndThemeStoneText = new Color32(0x25, 0x2B, 0x34, 255);
    private static readonly Color32 EndThemeCoinText  = new Color32(0xC5, 0xCD, 0xD4, 255);

    private void Start()
    {
        if (startUI != null) _startUiOrigin = startUI.anchoredPosition;
        if (endUI != null)   _endUiOrigin   = endUI.anchoredPosition;

        if (endUI != null)   endUI.anchoredPosition   = _endUiOrigin   + offScreenOffset;
        if (startUI != null) startUI.anchoredPosition = _startUiOrigin + offScreenOffset;

        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelEnd.AddListener(OnLevelEnd);
        else
            //Debug.LogWarning("EndStartUIOnline: GameManager.Instance not found. Level-end events won't fire.");

        GameManagerOnline.OnLevelEnd += OnLevelEnd;

        // Coin events: subscribe when LocalPlayerData is ready, or poll in Update
        TrySubscribeCoinEvent();

        WireButtons();
        GameManagerOnline.OnLevelStarted += HandleLevelStartedRemote;
        GameManagerOnline.OnNextLevelReady += HandleNextLevelReady;
        // Refresh start panel values once GameManagerOnline spawns (level detail not ready at scene open).
        GameManagerOnline.OnReady += SetUpStartUI;
        ToggleStartScreen(true);
        // GameManagerOnline may have spawned before this UI's Start() ran — populate now if so.
        if (GameManagerOnline.Instance != null && GameManagerOnline.Instance.IsReady)
            SetUpStartUI();
    }

    private void Update()
    {
        // Retry subscribing if LocalPlayerData wasn't ready at Start
        if (PlayerData != null)
            TrySubscribeCoinEvent();

        if (startBtn == null) return;
        if (IsMultiplayerActive())
        {
            bool isMaster = IsLocalMasterClient();
            bool allRetried = AllPlayersRetried();
            startBtn.interactable = isMaster && allRetried;
            if (waitingForHostText != null)
            {
                if (!isMaster)
                {
                    waitingForHostText.gameObject.SetActive(true);
                    waitingForHostText.text = "Chờ host bắt đầu...";
                }
                else if (!allRetried)
                {
                    waitingForHostText.gameObject.SetActive(true);
                    waitingForHostText.text = "Chờ người chơi khác...";
                }
                else
                {
                    waitingForHostText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            startBtn.interactable = true;
            if (waitingForHostText != null)
                waitingForHostText.gameObject.SetActive(false);
        }
    }

    private bool _coinEventSubscribed = false;
    private void TrySubscribeCoinEvent()
    {
        if (_coinEventSubscribed || PlayerData == null) return;
        PlayerData.onMoneyChanged.AddListener(OnUpdateCoinChange);
        _coinEventSubscribed = true;
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

    private static bool AllPlayersRetried()
    {
        var gmo = GameManagerOnline.Instance;
        var runner = NetworkManager.Instance?.NetworkRunner;
        if (gmo == null || runner == null || gmo.Object == null || !gmo.Object.IsValid) return true;
        if (!gmo.IsWaitingForRetry) return true;
        return gmo.RetryReadyCount >= runner.ActivePlayers.Count();
    }

    private void HandleLevelStartedRemote()
    {
        // A client may be on the end screen when the next level starts (they didn't click Next yet).
        ToggleEndScreen(false);
        ToggleStartScreen(false);
    }

    private void WireButtons()
    {
        if (startBtn != null)
        {
            startBtn.onClick.RemoveListener(OnStartClicked);
            startBtn.onClick.AddListener(OnStartClicked);
        }
        if (retryBtn != null)
        {
            var btn = retryBtn.GetComponent<Button>();
            if (btn != null) { btn.onClick.RemoveListener(OnRetryClicked); btn.onClick.AddListener(OnRetryClicked); }
        }
        if (nextBtn != null)
        {
            var btn = nextBtn.GetComponent<Button>();
            if (btn != null) { btn.onClick.RemoveListener(OnNextClicked); btn.onClick.AddListener(OnNextClicked); }
        }
    }

    private void OnDisable()
    {
        GameManagerOnline.OnLevelStarted -= HandleLevelStartedRemote;
        GameManagerOnline.OnLevelEnd -= OnLevelEnd;
        GameManagerOnline.OnNextLevelReady -= HandleNextLevelReady;
        GameManagerOnline.OnReady -= SetUpStartUI;
        if (startBtn != null) startBtn.onClick.RemoveListener(OnStartClicked);
        if (retryBtn != null) { var btn = retryBtn.GetComponent<Button>(); if (btn != null) btn.onClick.RemoveListener(OnRetryClicked); }
        if (nextBtn != null)  { var btn = nextBtn.GetComponent<Button>();  if (btn != null) btn.onClick.RemoveListener(OnNextClicked); }
        if (GameManager.Instance != null) GameManager.Instance.OnLevelEnd.RemoveListener(OnLevelEnd);
        if (_coinEventSubscribed && PlayerData != null) PlayerData.onMoneyChanged.RemoveListener(OnUpdateCoinChange);
    }

    private void SetUpStartUI()
    {
        if (IsMultiplayerActive())
        {
            if (GameManagerOnline.Instance == null || GameManagerOnline.Instance.Runner == null) return;
            LevelDetailOnline currentLevel = GameManagerOnline.Instance.GetCurrentLevelDetail();
            if (currentLevel == null) return;
            int targetMoney   = currentLevel.TargetMoney;
            int customerCount = currentLevel.CustomerDetails?.Count ?? 0;
            int currentDay    = GameManagerOnline.Instance.LevelNumber + 1;
            if (startUICustomersText != null) startUICustomersText.text = customerCount.ToString();
            if (startUIGoalText != null)      startUIGoalText.text      = "0/" + targetMoney;
            if (startUIDayText != null)       startUIDayText.text       = "Day " + currentDay;
            return;
        }

        if (GameManager.Instance == null) return;
        LevelDetail currentLevelOffline = GameManager.Instance.GetCurrentLevelDetail();
        if (currentLevelOffline == null) return;
        int targetMoneyOffline   = currentLevelOffline.TargetMoney;
        int customerCountOffline = currentLevelOffline.CustomerDetails != null ? currentLevelOffline.CustomerDetails.Count : 0;
        int currentDayOffline    = GameManager.Instance.LevelNumber + 1;
        if (startUICustomersText != null) startUICustomersText.text = customerCountOffline.ToString();
        if (startUIGoalText != null)      startUIGoalText.text      = "0/" + targetMoneyOffline;
        if (startUIDayText != null)       startUIDayText.text       = "Day " + currentDayOffline;
    }

    private void SetUpEndUI()
    {
        if (IsMultiplayerActive())
        {
            if (GameManagerOnline.Instance == null) return;
            LevelDetailOnline currentLevel = GameManagerOnline.Instance.GetCurrentLevelDetail();
            if (currentLevel == null) return;
            int collectedMoney = GameManagerOnline.Instance.CollectedMoney;
            int targetMoney    = currentLevel.TargetMoney;
            int totalMoney     = PlayerData?.TotalMoney ?? 0;
            if (endUITargetText != null) endUITargetText.text = collectedMoney + "/" + targetMoney;
            if (endUICoinTxt != null)    endUICoinTxt.text    = totalMoney.ToString();
            return;
        }

        if (GameManager.Instance == null) return;
        LevelDetail currentLevelOffline = GameManager.Instance.GetCurrentLevelDetail();
        if (currentLevelOffline == null) return;
        int collectedMoneyOffline = GameManager.Instance.CollectedMoney;
        int targetMoneyOffline    = currentLevelOffline.TargetMoney;
        int totalMoneyOffline     = PlayerData != null ? PlayerData.TotalMoney : 0;
        if (endUITargetText != null) endUITargetText.text = collectedMoneyOffline + "/" + targetMoneyOffline;
        if (endUICoinTxt != null)    endUICoinTxt.text    = totalMoneyOffline.ToString();
    }

    private void OnUpdateCoinChange()
    {
        int coins = PlayerData != null ? PlayerData.TotalMoney : 0;
        UpdateEndUICoins(coins);
    }

    private void OnLevelEnd(bool isWin)
    {
        _hasClickedRetry = false;
        SetEndPanelWaiting(false);
        if (isWin) ToggleWinUI(); else ToggleFailUI();
        ApplyEndPanelTheme(isWin);
        ToggleEndScreen(true);
        OnUpdateCoinChange();
    }

    // --- BUTTON EVENTS ---

    public void OnStartClicked()
    {
        if (IsMultiplayerActive())
        {
            if (!IsLocalMasterClient()) return;
            if (GameManagerOnline.Instance == null) { /*Debug.LogError("[EndStartUIOnline] Missing GameManagerOnline.");*/ return; }
            GameManagerOnline.Instance.RPC_StartLevel();
            return;
        }

        ToggleStartScreen(false);
        if (GameManager.Instance == null) { /*Debug.LogError("[EndStartUIOnline] GameManager.Instance null.");*/ return; }
        if (!GameManager.Instance.GameStarted) { /*Debug.LogError("[EndStartUIOnline] GameManager.GameStarted == false.");*/ return; }
        GameManager.Instance.StartCurrentLevel();
    }

    public void OnRetryClicked()
    {
        if (!_hasClickedRetry && IsMultiplayerActive() && GameManagerOnline.Instance != null)
        {
            _hasClickedRetry = true;
            GameManagerOnline.Instance.RPC_PlayerPressedRetry();
        }
        if (_sequenceRoutine != null) StopCoroutine(_sequenceRoutine);
        _sequenceRoutine = StartCoroutine(ReturnToStartSequence());
    }

    public void OnNextClicked()
    {
        if (IsMultiplayerActive())
        {
            if (IsLocalMasterClient() && GameManagerOnline.Instance != null)
                GameManagerOnline.Instance.NextLevel(); // RPC sẽ trigger HandleNextLevelReady cho tất cả
            else
                SetEndPanelWaiting(true); // client: chờ host
            return;
        }

        if (_sequenceRoutine != null) StopCoroutine(_sequenceRoutine);
        _sequenceRoutine = StartCoroutine(ReturnToStartSequence());
        if (GameManager.Instance != null) GameManager.Instance.NextLevel();
    }

    private void HandleNextLevelReady()
    {
        SetEndPanelWaiting(false);
        if (_sequenceRoutine != null) StopCoroutine(_sequenceRoutine);
        _sequenceRoutine = StartCoroutine(ReturnToStartSequence());
    }

    private void SetEndPanelWaiting(bool waiting)
    {
        if (nextBtn != null)
        {
            var btn = nextBtn.GetComponent<Button>();
            if (btn != null) btn.interactable = !waiting;
        }
        if (waitingForNextText != null)
            waitingForNextText.gameObject.SetActive(waiting);
    }

    private IEnumerator ReturnToStartSequence()
    {
        ToggleEndScreen(false);
        yield return new WaitForSeconds(animationDuration);
        if (nextBtn)  nextBtn.gameObject.SetActive(false);
        if (retryBtn) retryBtn.gameObject.SetActive(false);
        ToggleStartScreen(true);
    }

    // --- ANIMATION ---

    public void ToggleStartScreen(bool show)
    {
        if (_startRoutine != null) StopCoroutine(_startRoutine);
        _startRoutine = StartCoroutine(AnimateUI(startUI, _startUiOrigin, show));
        SetUpStartUI();
        if (BGImage != null) BGImage.raycastTarget = show;
    }

    public void ToggleEndScreen(bool show)
    {
        if (_endRoutine != null) StopCoroutine(_endRoutine);
        _endRoutine = StartCoroutine(AnimateUI(endUI, _endUiOrigin, show));
        SetUpEndUI();
        if (BGImage != null) BGImage.raycastTarget = show;
    }

    private IEnumerator AnimateUI(RectTransform targetUI, Vector2 originalPos, bool show, bool animateBG = true)
    {
        if (targetUI == null) yield break;
        float time = 0;
        Vector2 posStart = targetUI.anchoredPosition;
        Vector2 posEnd   = show ? originalPos : originalPos + offScreenOffset;
        Color bgStartColor = BGImage != null ? BGImage.color : Color.clear;
        Color bgEndColor   = bgStartColor;
        bgEndColor.a = show ? bgMaxAlpha : 0f;
        while (time < animationDuration)
        {
            float t = time / animationDuration;
            t = t * t * (3f - 2f * t);
            targetUI.anchoredPosition = Vector2.Lerp(posStart, posEnd, t);
            if (BGImage != null && animateBG) BGImage.color = Color.Lerp(bgStartColor, bgEndColor, t);
            time += Time.deltaTime;
            yield return null;
        }
        targetUI.anchoredPosition = posEnd;
        if (BGImage != null && animateBG) BGImage.color = bgEndColor;
    }

    public void ToggleWinUI()  { if (retryBtn) retryBtn.gameObject.SetActive(false); if (nextBtn) nextBtn.gameObject.SetActive(true); }
    public void ToggleFailUI() { if (nextBtn)  nextBtn.gameObject.SetActive(false);  if (retryBtn) retryBtn.gameObject.SetActive(true); }

    private void ApplyEndPanelTheme(bool isWin)
    {
        Color32 primaryText = isWin ? EndThemeWoodText : EndThemeStoneText;
        Color32 coinText    = EndThemeCoinText;

        if (endWinPanel != null)
        {
            foreach (TMP_Text tmp in endWinPanel.GetComponentsInChildren<TMP_Text>(true))
            {
                if (tmp == waitingForNextText) continue;
                tmp.color = (tmp == endUITargetText || tmp == endUICoinTxt) ? coinText : primaryText;
            }
        }
        else
        {
            if (endUITargetText != null) endUITargetText.color = coinText;
            if (endUICoinTxt != null)    endUICoinTxt.color    = coinText;
            SetButtonLabelColor(nextBtn, primaryText);
            SetButtonLabelColor(retryBtn, primaryText);
            SetButtonLabelColor(menuBtn, primaryText);
        }

        if (GameAssets.Instance == null) return;
        GameAssets ga = GameAssets.Instance;
        if (endPanelBackgroundImage != null && ga.PanelWoodBG != null)
            endPanelBackgroundImage.sprite = isWin ? ga.PanelWoodBG : ga.PanelStoneBG;
        if (endTitleBackgroundImage != null && ga.TitleWoodBG != null)
            endTitleBackgroundImage.sprite = isWin ? ga.TitleWoodBG : ga.TitleStoneBG;
        Sprite buttonSprite = isWin ? ga.ButtonWoodBG : ga.ButtonStoneBG;
        SetButtonBackgroundSprite(nextBtn,  buttonSprite);
        SetButtonBackgroundSprite(retryBtn, buttonSprite);
        SetButtonBackgroundSprite(menuBtn,  buttonSprite);
    }

    private static void SetButtonLabelColor(RectTransform buttonRoot, Color32 color)
    {
        if (buttonRoot == null) return;
        TMP_Text tmp = buttonRoot.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null) tmp.color = color;
    }

    private static void SetButtonBackgroundSprite(RectTransform buttonRoot, Sprite sprite)
    {
        if (buttonRoot == null || sprite == null) return;
        Image img = buttonRoot.GetComponent<Image>();
        if (img != null) img.sprite = sprite;
    }

    public void UpdateEndUICoins(int coinCount)
    {
        if (endUICoinTxt != null) endUICoinTxt.text = coinCount.ToString();
    }
}
