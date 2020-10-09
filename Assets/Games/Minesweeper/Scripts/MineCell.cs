using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MineCell : MonoBehaviour, IPointerClickHandler
{
    public Image image;
    [SerializeField] private Button button;
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

    public void SetSprite(Sprite sprite, float alpha = 1)
    {
        image.gameObject.SetActive(sprite != null ? true : false);
        image.sprite = sprite;
        var tempColor = image.color;
        tempColor.a = alpha;
        image.color = tempColor;
    }

    private void CheckForBomb()
    {
        if (flag || isOpen) return;

        isOpen = true;
        button.interactable = false;

        if (minesweeper.IsThereBomb(this))
        {
            SetSprite(minesweeper.bombSprite);
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
        SetSprite(flag ? minesweeper.flagSprite : null);
    }
}
    