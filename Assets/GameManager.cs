using TMPro;
using UnityEngine;

public enum GameState { Running, Win, Lose, Paused, None };

public class GameManager : MonoBehaviour
{    
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private TMP_Dropdown chooseGameDropdown;

    private const string defaultTimerText = "00:00";
    private const string timerFormat = "{0:00}:{1:00}";
    
    private float timeFromStart = 0;
    private ScoreManager scoreManager;
    private WindowsManager windowsManager;
    internal GameState gameState = GameState.None;
    internal GameState previousGameState = GameState.None;
    
    private void Awake()
    {
        windowsManager = FindObjectOfType<WindowsManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
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
            timer.text = string.Format(timerFormat, minutes, seconds);
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (gameState == GameState.Paused)
            {
                gameState = previousGameState;
                windowsManager.HideMainWindow();
            }
            else if (gameState == GameState.Running || gameState == GameState.None)
            {
                previousGameState = gameState;
                gameState = GameState.Paused;
                windowsManager.ShowMainWindow(gameState);
            }
        }
    }    

    public void StartNewGame()
    {
        var chosenGameTitle = chooseGameDropdown.options[chooseGameDropdown.value].text;

        if (chosenGameTitle == "Minesweeper")
        {
            GetComponent<Minesweeper>().StartNewGame();
        }
    }

    internal void ResetValues()
    {
        timeFromStart = 0;                
        gameState = GameState.None;
        timer.text = defaultTimerText;
        windowsManager.HideMainWindow();                
    }

    internal void GameOver(GameState state)
    {
        gameState = state;

        if (state == GameState.Win)
        {
            scoreManager.SaveScore(timeFromStart);
            windowsManager.ShowMainWindow(state, timeFromStart);
        }
        else if (state == GameState.Lose)
        {
            windowsManager.ShowMainWindow(state);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}