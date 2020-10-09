using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

public class Minesweeper : MonoBehaviour, IGame
{
    [SerializeField] private GameObject grid;
    [SerializeField] private MineCell gridCell;
    [SerializeField] private double gameHardity = 0.8;
    [SerializeField] private TextMeshProUGUI bombsRemainText;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(10, 10);

    public Sprite bombSprite;
    public Sprite flagSprite;
    private int cellsNum;
    private int bombsNum;
    private bool[,] minefield;
    private MineCell[,] cells;
    private int openedCellsNum;
    private GameManager gameManager;
    private const string scoreFormat = "{0:00}:{1:00}";
    private const string bombsRemainFormat = "{0} bombs remain";
    private readonly PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
    {
        button = PointerEventData.InputButton.Left
    };
    private int bombsRemain;
    public int BombsRemain
    {
        get => bombsRemain;
        set
        {
            bombsRemain = value;
            bombsRemainText.text = string.Format(bombsRemainFormat, BombsRemain);
        }
    }

    [SerializeField] private string gameTitle = "Minesweeper";
    public string GameTitle => gameTitle;

    public float RawScore => gameManager.timeFromStart;

    public bool SortScoresByDescending => false;

    public string FormatScore(float score)
    {
        (int minutes, int rem_seconds) = ExtensionMethods.GetMinSec(score);
        return string.Format(scoreFormat, minutes, rem_seconds);
    }

    [ContextMenu("Show field")]
    public void ShowField()
    {
        if (gameManager.gameState != GameState.Running) return;

        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                if (minefield[i, j])
                {
                    cells[i, j].image.gameObject.SetActive(true);
                    cells[i, j].SetSprite(bombSprite, 0.5f);
                }
            }
        }
    }

    [ContextMenu("Win game")]
    public void WinGame()
    {
        gameManager.GameOver(GameState.Win);
    }

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        bombsRemainText = gameManager.ScoreText;
        bombsRemainText.gameObject.SetActive(false);
    }

    void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        ResetValues();

        cellsNum = gridSize.x * gridSize.y;
        cells = new MineCell[gridSize.x, gridSize.y];

        grid.DestroyChildren();
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = gridSize.x;
        for (int j = 0; j < gridSize.y; j++)
        {
            for (int i = 0; i < gridSize.x; i++)
            {
                var cell = Instantiate(gridCell, grid.transform);
                cells[i, j] = cell;
            }
        }
    }

    public void CloseGame()
    {
        grid.DestroyChildren();
        bombsRemainText.gameObject.SetActive(false);
    }

    internal void CheckForWin()
    {
        if (WinCondition())
        {
            gameManager.GameOver(GameState.Win);
        }
    }

    internal int GetNumOfBombsAround(MineCell mineCell)
    {
        int num = 0;
        var pos = cells.CoordinatesOf(mineCell);

        foreach (var position in pos.GetNextCellPos(gridSize))
        {
            if (minefield.GetElemAt(position)) num++;
        }
        return num;
    }

    private void ResetValues()
    {
        bombsRemainText.gameObject.SetActive(false);
        openedCellsNum = 0;
        BombsRemain = 0;
        bombsNum = 0;
    }

    internal void TryQuickOpen(MineCell mineCell)
    {
        var pos = cells.CoordinatesOf(mineCell);
        var cellsAround = pos.GetNextCellPos(gridSize);
        var isAnyFlag = cellsAround.Any(cellPos => cells.GetElemAt(cellPos).flag);
        if (isAnyFlag) TriggerCellsAround(mineCell);
    }

    internal void TriggerCellsAround(MineCell mineCell)
    {
        var pos = cells.CoordinatesOf(mineCell);
        foreach (var nextCellPos in pos.GetNextCellPos(gridSize))
        {
            ExecuteEvents.Execute(cells.GetElemAt(nextCellPos).gameObject, pointerEventData, ExecuteEvents.pointerClickHandler);
        }
    }

    internal void ChangeFlagNum(int value)
    {
        BombsRemain += value;
    }

    internal bool IsThereBomb(MineCell cell)
    {
        openedCellsNum++;
        var pos = cells.CoordinatesOf(cell);

        if (gameManager.gameState == GameState.None)
        {
            minefield = GenerateMinefield(pos);
            bombsRemainText.gameObject.SetActive(true);
            ChangeFlagNum(bombsNum);
            gameManager.StartTimer();
        }

        var value = minefield[pos.x, pos.y];

        return value;
    }

    private bool[,] GenerateMinefield(Vector2Int initialPos)
    {
        bombsNum = 0;

        int x = gridSize.x,
            y = gridSize.y;

        var nextCellsPositions = initialPos.GetNextCellPos(gridSize).ToList();
        nextCellsPositions.Add(initialPos);
        var field = new bool[x, y];

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                if (nextCellsPositions.Contains(new Vector2Int(i, j)))
                {
                    field[i, j] = false;
                    continue;
                }
                var value = UnityRandom.value > gameHardity;
                field[i, j] = value;
                if (value) bombsNum++;
            }
        }
        return field;
    }

    // Each game will have its own method.
    internal bool WinCondition() => cellsNum - openedCellsNum == bombsNum;
}
