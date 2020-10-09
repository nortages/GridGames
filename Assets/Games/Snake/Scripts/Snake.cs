using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Snake : MonoBehaviour, IGame
{
    [SerializeField] private bool useTeleport;
    [SerializeField] private Image cellPrefab;
    [SerializeField] private GameObject gridObj;
    [SerializeField] private float snakeSpeed = 5;
    [SerializeField] private float acceleration = 0.02f;
    [SerializeField] private Color pointColor = Color.red;
    [SerializeField] private Color cellColor = Color.white;
    [SerializeField] private Color snakeBodyColor = Color.black;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private new ParticleSystem particleSystem;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(16, 16);
    [SerializeField] private Vector2Int currentDirection = new Vector2Int(1, 0);
    [SerializeField] private List<Vector2Int> snakeStartPos = new List<Vector2Int> { new Vector2Int(1, 8),
                                                                                     new Vector2Int(2, 8),
                                                                                     new Vector2Int(3, 8)};

    private bool isThisGameRunning = false;
    private Image[,] cells;
    private float currentSnakeSpeed;
    private List<Vector2Int> snakePos;
    private IEnumerator snakeCycle;
    private GameManager gameManager;
    private Vector2Int currentPoint;
    private Vector2Int previousDirection;
    private int obtainedPoints;
    private TextMeshProUGUI obtainedPointsText;
    private readonly System.Random rand = new System.Random();
    private readonly List<Vector2Int> allPos = new List<Vector2Int>();

    [SerializeField] private string gameTitle = "Snake";
    public string GameTitle => gameTitle;

    private int ObtainedPoints
    {
        set
        {
            obtainedPoints = value;
            obtainedPointsText.text = value.ToString();
        }
        get => obtainedPoints;
    }

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        obtainedPointsText = gameManager.ScoreText;
    }

    public void StartNewGame()
    {
        ResetValues();
        gameManager.Pause += OnPause;
        gameManager.Resume += OnResume;
        isThisGameRunning = true;
        gridObj.DestroyChildren();
        int x = gridSize.x,
            y = gridSize.y;
        cells = new Image[x, y];
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = x;
        particleSystem.startColor = pointColor;

        for (int j = 0; j < y; j++)
        {
            for (int i = 0; i < x; i++)
            {
                allPos.Add(new Vector2Int(i, j));
                var cell = Instantiate(cellPrefab, gridObj.transform);
                cell.name = $"({i}, {j})";
                cell.color = cellColor;
                cells[i, j] = cell;
            }
        }

        foreach (var item in snakePos)
        {
            cells.GetElemAt(item).color = snakeBodyColor;
        }
    }

    private void ResetValues()
    {
        previousDirection = new Vector2Int(1, 0);
        snakeCycle = null;
        gameManager.Pause -= OnPause;
        gameManager.Resume -= OnResume;
        currentSnakeSpeed = snakeSpeed;
        snakePos = snakeStartPos.ToList();
        obtainedPointsText.gameObject.SetActive(false);
    }

    private void OnPause()
    {
        if (snakeCycle != null) StopCoroutine(snakeCycle);
    }

    private void OnResume()
    {
        if (snakeCycle != null) StartCoroutine(snakeCycle);
    }

    public void CloseGame()
    {
        gridObj.DestroyChildren();
        isThisGameRunning = false;
        gameManager.Pause -= OnPause;
        gameManager.Resume -= OnResume;
    }

    private void GenerateNewPoint()
    {
        var emptyPos = allPos.Except(snakePos).ToList();
        var ind = rand.Next(0, emptyPos.Count() - 1);
        currentPoint = emptyPos[ind];
        cells[currentPoint.x, currentPoint.y].color = pointColor;
    }

    void Update()
    {
        if (!isThisGameRunning || (gameManager.gameState != GameState.None &&
                                   gameManager.gameState != GameState.Running)) return;

        var x = Input.GetAxisRaw("Horizontal");
        var xRound = Math.Sign(x);
        Vector2Int newDir = Vector2Int.zero;
        if (xRound != 0)
        {
            newDir = new Vector2Int(xRound, 0);
        }

        var y = Input.GetAxisRaw("Vertical");
        var yRound = Math.Sign(y);
        if (yRound != 0)
        {
            newDir = new Vector2Int(0, yRound);
        }

        if (newDir == Vector2Int.zero || newDir * -1 == previousDirection) return;

        currentDirection = newDir;
        if (gameManager.gameState == GameState.None)
        {
            GenerateNewPoint();
            snakeCycle = SnakeCycle();
            StartCoroutine(snakeCycle);
            gameManager.StartTimer();
            obtainedPointsText.gameObject.SetActive(true);
            ObtainedPoints = 0;
        }
    }

    public float RawScore => ObtainedPoints;

    public bool SortScoresByDescending => true;

    public string FormatScore(float score) => score.ToString();

    IEnumerator SnakeCycle()
    {
        while (true)
        {
            // Removes a tail cell.
            var tailPos = snakePos.First();
            snakePos.Remove(tailPos);
            cells[tailPos.x, tailPos.y].color = cellColor;

            var headPos = snakePos.Last();

            headPos += currentDirection;
            if (useTeleport) headPos = headPos.Mod(gridSize);

            try
            {
                if (snakePos.Contains(headPos))
                {
                    var innerImage = cells[headPos.x, headPos.y].transform.GetChild(0).GetComponent<Image>();
                    innerImage.gameObject.SetActive(true);
                    throw new IndexOutOfRangeException();
                }

                cells[headPos.x, headPos.y].color = snakeBodyColor;
            }
            catch (IndexOutOfRangeException)
            {
                gameManager.GameOver(GameState.Over);
                break;
            }
            snakePos.Add(headPos);

            if (headPos == currentPoint)
            {
                ObtainedPoints++;
                var cellPos = cells[currentPoint.x, currentPoint.y].transform.position;
                var emitParams = new ParticleSystem.EmitParams
                {
                    position = new Vector3(cellPos.x, cellPos.y, -1),
                    //applyShapeToPosition = true
                };
                particleSystem.Emit(emitParams, 1);
                snakePos.Insert(0, tailPos);
                currentSnakeSpeed += acceleration;
                GenerateNewPoint();
            }
            previousDirection = currentDirection;
            yield return new WaitForSecondsRealtime(1 / currentSnakeSpeed);
        }
    }
}
