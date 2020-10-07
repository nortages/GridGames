using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MineCell : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private Sprite bombSprite;
    [SerializeField] private Sprite flagSprite;
    [SerializeField] private TextMeshProUGUI text;

    private GameManager gameManager;
    private Minesweeper minesweeper;
    private bool isOpen = false;
    
    [HideInInspector] public bool flag = false;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        minesweeper = FindObjectOfType<Minesweeper>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            CheckForBomb();

            if (!(eventData.clickCount == 2 && isOpen)) return;

            minesweeper.TryQuickOpen(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            ToggleFlag();
        }        
    }   

    private void CheckForBomb()
    {
        if (flag || isOpen) return;

        minesweeper.openedCellsNum++;
        isOpen = true;
        button.interactable = false;

        if (minesweeper.IsThereBomb(this))
        {
            image.enabled = true;
            image.sprite = bombSprite;

            gameManager.GameOver(GameState.Lose);
        }
        else
        {
            var num = minesweeper.GetNumOfBombsAround(this);

            if (num > 0) text.text = num.ToString();
            else minesweeper.TriggerCellsAround(this);

            minesweeper.CheckForWin();
        }
    }

    private void ToggleFlag()
    {
        if (isOpen) return;

        flag = !flag;
        var mult = flag ? -1 : 1;
        minesweeper.ChangeFlagNum(mult);
        image.enabled = flag;
    }
}
    