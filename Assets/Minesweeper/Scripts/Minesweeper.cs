using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

[Serializable]
public class MyArray
{
    public bool[] array;
}

public class Minesweeper : MonoBehaviour
{
    [SerializeField] private GameObject grid;
    [SerializeField] private MineCell gridCell;
    [SerializeField] private double gameHardity = 0.8;
    [SerializeField] private TextMeshProUGUI bombsRemain;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(10, 10);

    private int cellsNum;
    private int bombsNum = 0;
    private bool[,] minefield;
    private MineCell[,] cells;
    private int bombsFlagged = 0;
    private GameManager gameManager;
    private const string bombsRemainFormat = "{0} bombs remain";
    private readonly PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
    {
        button = PointerEventData.InputButton.Left
    };

    public MyArray[] minefieldForDebug;
    [HideInInspector] public int openedCellsNum = 0;

    [ContextMenu("Show field")]
    public void ShowField()
    {
        minefieldForDebug = new MyArray[minefield.GetUpperBound(0) + 1];
        for (int i = 0; i < minefield.GetUpperBound(0) + 1; i++)
        {
            minefieldForDebug[i] = new MyArray
            {
                array = new bool[minefield.GetUpperBound(1) + 1]
            };
            for (int j = 0; j < minefield.GetUpperBound(1) + 1; j++)
            {
                minefieldForDebug[i].array[j] = minefield[i, j];
            }
        }
    }

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        bombsRemain.gameObject.SetActive(false);
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

        foreach (var item in pos.GetNextCellPos(gridSize))
        {
            if (minefield[item.x, item.y]) num++;
        }
        return num;
    }

    private void ResetValues()
    {
        gameManager.ResetValues();
        bombsRemain.gameObject.SetActive(false);
        openedCellsNum = 0;
        bombsFlagged = 0;
    }

    internal void TryQuickOpen(MineCell mineCell)
    {
        var pos = cells.CoordinatesOf(mineCell);
        var cellsAround = pos.GetNextCellPos(gridSize);
        var isAnyFlag = cellsAround.Any(cellPos => GetCellAt(cellPos).flag);
        if (isAnyFlag) TriggerCellsAround(mineCell);
    }

    internal void TriggerCellsAround(MineCell mineCell)
    {
        var pos = cells.CoordinatesOf(mineCell);
        foreach (var nextCellPos in pos.GetNextCellPos(gridSize))
        {
            ExecuteEvents.Execute(GetCellAt(nextCellPos).gameObject, pointerEventData, ExecuteEvents.pointerClickHandler);
        }
    }

    internal MineCell GetCellAt(Vector2Int cellPos) => cells[cellPos.x, cellPos.y];

    internal void ChangeFlagNum(int value)
    {
        bombsFlagged += value;
        bombsRemain.text = string.Format(bombsRemainFormat, bombsFlagged);
    }

    internal bool IsThereBomb(MineCell cell)
    {
        var pos = cells.CoordinatesOf(cell);

        if (gameManager.gameState == GameState.None)
        {
            minefield = GenerateMinefield(pos);
            bombsRemain.gameObject.SetActive(true);
            bombsFlagged = bombsNum;
            ChangeFlagNum(bombsNum);
            gameManager.gameState = GameState.Running;
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
