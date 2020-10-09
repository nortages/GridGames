using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowsManager : MonoBehaviour
{
    [SerializeField] private Button backToMainWin;
    [SerializeField] private GameObject mainWindow;
    [SerializeField] private GameObject scoresPanel;
    [SerializeField] private GameObject scoresWindow;
    [SerializeField] private GameObject menuBackground;
    [SerializeField] private TextMeshProUGUI pauseScore;
    [SerializeField] private TextMeshProUGUI scorePrefab;
    [SerializeField] private TextMeshProUGUI mainWinTitle;
    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private string gameWinStatement = "You win!";
    [SerializeField] private string gameOverStatement = "Game Over";
    [SerializeField] private string gameLoseStatement = "You lose :(";
    [SerializeField] private string gamePausedStatement = "Game paused";
    [SerializeField] private Color winColor = Color.green;
    [SerializeField] private Color loseColor = Color.red;

    private bool isScoreWinOpen = false;

    public Func<string> formatFunc;

    private void Awake()
    {
        mainWindow.SetActive(false);
        menuBackground.SetActive(false);
        backToMainWin.onClick.AddListener(OnBackToMain);
    }

    private void OnBackToMain()
    {
        isScoreWinOpen = false;
    }

    public void HideMainWindow()
    {
        gameStateText.gameObject.SetActive(false);
        pauseScore.gameObject.SetActive(false);
        menuBackground.SetActive(false);
        if (isScoreWinOpen) backToMainWin.onClick.Invoke();
        mainWindow.SetActive(false);
    }

    internal void ShowMainWindow(GameState state, string formattedScore = default)
    {
        mainWindow.SetActive(true);
        menuBackground.SetActive(true);
        gameStateText.gameObject.SetActive(true);

        if (state == GameState.Win)
        {
            mainWinTitle.text = gameOverStatement;
            gameStateText.text = gameWinStatement;
            gameStateText.color = winColor;
            pauseScore.gameObject.SetActive(true);

            pauseScore.text = $"Your score is {formattedScore}";
        }
        else if (state == GameState.Lose)
        {
            mainWinTitle.text = gameOverStatement;
            gameStateText.text = gameLoseStatement;
            gameStateText.color = loseColor;
        }
        else if (state == GameState.Paused)
        {
            mainWinTitle.text = gamePausedStatement;
            gameStateText.gameObject.SetActive(false);
        }
        else if (state == GameState.Over)
        {
            mainWinTitle.text = gameOverStatement;
            pauseScore.text = $"Your score is {formattedScore}";
            pauseScore.gameObject.SetActive(true);
            gameStateText.gameObject.SetActive(false);
        }
    }

    internal void ShowScoresWindow(IEnumerable<string> scores)
    {
        isScoreWinOpen = true;
        menuBackground.SetActive(true);
        scoresWindow.SetActive(true);
        scoresPanel.DestroyChildren();

        foreach (var score in scores)
        {
            var scoreObj = Instantiate(scorePrefab, scoresPanel.transform);
            scoreObj.text = score;
        }
    }
}
