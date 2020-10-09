using TMPro;
using UnityEngine;

public enum GameState { Running, Win, Lose, Paused, Over, None };

public class GameManager : MonoBehaviour
{    
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TMP_Dropdown chooseGameDropdown;

    public IGame previousGame = null;
    public IGame currentGame;
    public float timeFromStart = 0;
    public delegate void GameEventHandler();
    public event GameEventHandler Pause;
    public event GameEventHandler Resume;
    public TextMeshProUGUI ScoreText => scoreText;
    public GameState gameState = GameState.None;
    public GameState previousGameState = GameState.None;

    private ScoreManager scoreManager;
    private WindowsManager windowsManager;
    private const string defaultTimerText = "00:00";
    private const string timerFormat = "{0:00}:{1:00}";    

    private void Awake()
    {
        windowsManager = FindObjectOfType<WindowsManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        chooseGameDropdown.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(int arg0)
    {
        if (previousGame == null) previousGame = currentGame;

        var chosenGameTitle = chooseGameDropdown.options[arg0].text;
        IGame newGame = null;
        switch (chosenGameTitle)
        {
            case "Minesweeper":
                newGame = GetComponent<Minesweeper>();
                break;
            case "Snake":
                newGame = GetComponent<Snake>();
                break;
            default:
                break;
        }
        currentGame = newGame;
    }

    private void Start()
    {
        StartNewGame();
    }

    private void Update()
    {
        if (gameState == GameState.Running)
        {
            timeFromStart += Time.deltaTime;
            (int minutes, int seconds) = ExtensionMethods.GetMinSec(timeFromStart);
            timerText.text = string.Format(timerFormat, minutes, seconds);
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (gameState == GameState.Paused)
            {
                Resume?.Invoke();
                gameState = previousGameState;
                windowsManager.HideMainWindow();
            }
            else if (gameState == GameState.Running || gameState == GameState.None)
            {
                Pause?.Invoke();
                previousGameState = gameState;
                gameState = GameState.Paused;
                windowsManager.ShowMainWindow(gameState);
            }
        }
    }    

    public void StartNewGame()
    {
        if (currentGame == null) OnValueChanged(chooseGameDropdown.value);

        if (previousGame != null)
        {
            previousGame.CloseGame();
            previousGame = null; 
        }
        currentGame.StartNewGame();
        ResetValues();
    }

    private void ResetValues()
    {
        windowsManager.HideMainWindow();
        timeFromStart = 0;
        gameState = GameState.None;
        timerText.text = defaultTimerText;
    }

    public void GameOver(GameState state)
    {
        gameState = state;

        if (state == GameState.Win)
        {
            var score = currentGame.RawScore;
            scoreManager.SaveScore(score);
            windowsManager.ShowMainWindow(state, currentGame.FormatScore(score));
        }
        else if (state == GameState.Lose)
        {
            windowsManager.ShowMainWindow(state);
        }
        else if (state == GameState.Over)
        {
            var score = currentGame.RawScore;
            scoreManager.SaveScore(score);
            windowsManager.ShowMainWindow(state, currentGame.FormatScore(score));
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void StartTimer()
    {
        gameState = GameState.Running;
    }
}