using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("Level Design")]
    [SerializeField] LevelsDesign levelDesign;
    [Header("Events")]
    [SerializeField] UnityEvent onLevelStart = new(); // Called each day
    [SerializeField] UnityEvent<bool> onLevelEnd = new();
    [SerializeField] UnityEvent onGameStart = new(); //  Called only on first day
    [SerializeField] UnityEvent onGameEnd = new();
    [SerializeField] UnityEvent onEarnedMoneyChanged = new();
    [SerializeField] UnityEvent onCollectedMoneyChanged = new();

    private static GameManager instance = null;
    private int levelNumber; // Store Level Number
    private bool gameStarted = false;
    [SerializeField] private bool levelStarted = false;
    private float levelStartTime = 0;
    [SerializeField] private int earnedMoney = 0; // Total money earned throughout the game
    [SerializeField] private int collectedMoney = 0; // Money collected in a day

    public UnityEvent OnLevelStart { get => onLevelStart; set => onLevelStart = value; }
    public UnityEvent<bool> OnLevelEnd { get => onLevelEnd; set => onLevelEnd = value; }
    public UnityEvent OnGameStart { get => onGameStart; set => onGameStart = value; }
    public UnityEvent OnGameEnd { get => onGameEnd; set => onGameEnd = value; }
    public UnityEvent OnEarnedMoneyChanged { get => onEarnedMoneyChanged; set => onEarnedMoneyChanged = value; }
    public UnityEvent OnCollectedMoneyChanged { get => onCollectedMoneyChanged; set => onCollectedMoneyChanged = value; }

    public static GameManager Instance { get => instance; }
    public int EarnedMoney { get => earnedMoney; set { earnedMoney = value; onEarnedMoneyChanged.Invoke(); } } 
    public int CollectedMoney { get => collectedMoney; set { collectedMoney = value; onCollectedMoneyChanged.Invoke(); } }
    public float LevelStartTime { get => levelStartTime; }
    public bool LevelStarted { get => levelStarted; }
    public bool GameStarted { get => gameStarted; }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;
    }

    private void Start()
    {
        //levelNumber = -1; -> Load from save file
        if (levelDesign == null) { Debug.LogWarning("Game can't start without a level design assigned!");  return; }
        if (!levelDesign.CheckValidLevel(levelNumber)) { Debug.LogWarning("Cannot load current level"); return; }

        gameStarted = true;
        onGameStart.Invoke();
    }

    private void OnValidate()
    {
        if (levelDesign == null) Debug.LogWarning("Please assign a level design!");
    }

    // Called when open the game or by the "Let's cook" button
    [ContextMenu("Start current level")]
    public void StartCurrentLevel()
    {
        if (!gameStarted) { Debug.LogWarning("Can't start level because the game hasn't started!"); return; }
        if (levelStarted) { Debug.LogWarning("Level has already started!"); return; }

        collectedMoney = 0;
        levelStartTime = Time.time; // Save the starting time of this level -> Customer spawner will later use this value
        levelStarted = true;
        onLevelStart.Invoke();
    }

    // Called by "Next" Button
    [ContextMenu("Next level")]
    public void NextLevel()
    {
        if (levelStarted) { Debug.LogWarning("Level has already started!"); return; }
        levelNumber++;
    }
    public void EndLevel()
    {
        Debug.Log("Endlevel");
        if (!levelStarted || !gameStarted) return;

        earnedMoney += collectedMoney;

        levelStarted = false;

        bool win = earnedMoney >= GetCurrentLevelDetail().TargetMoney;

        Debug.Log($"End level, win: {win}");

        if (win) // If win -> Check if there are any levels left
        {
            if (!levelDesign.CheckValidLevel(levelNumber + 1))
            {
                // End game
                Debug.Log("EndGame");
                onGameEnd.Invoke();
                gameStarted = false;
                levelStarted = false;
                return;
            }
            onLevelEnd.Invoke(true);
        }
        else
        {
            onLevelEnd.Invoke(false);
        }
    }

    public LevelDetail GetCurrentLevelDetail()
    {
        if (levelDesign == null) return null;
        return levelDesign.GetLevelDetail(levelNumber);
    }
}
