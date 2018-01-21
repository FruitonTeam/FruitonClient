using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using KFruiton = fruiton.kernel.Fruiton;
using fruiton.fruitDb.factories;
using UnityEngine;
using UnityEngine.Events;

class OnDragFromTeamEvent : UnityEvent<KFruiton, Position>
{
}

class OnMouseEnterSquare : UnityEvent<FridgeGridSquare>
{
}

class OnMouseExitSquare : UnityEvent<FridgeGridSquare>
{
}

public class FridgeTeamGrid : MonoBehaviour
{
    public bool AllowEdit = false;
    public FridgeGridSquare GridSquareTemplate;
    public UnityEvent<KFruiton, Position> OnBeginDragFromTeam { get; private set; }
    public UnityEvent<FridgeGridSquare> OnMouseEnterSquare { get; private set; }
    public UnityEvent<FridgeGridSquare> OnMouseExitSquare { get; private set; }

    private int squareSize;
    private RectTransform rectTransform;
    private FridgeGridSquare[,] gridSquares;


    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        squareSize = (int) rectTransform.rect.width / 2;
        gridSquares = new FridgeGridSquare[2, 5];
        OnBeginDragFromTeam = new OnDragFromTeamEvent();
        OnMouseEnterSquare = new OnMouseEnterSquare();
        OnMouseExitSquare = new OnMouseExitSquare();
        InitGridFruitons();
    }

    public void LoadTeam(FruitonTeam team)
    {
        ClearFruitons();
        for (int i = 0; i < team.FruitonIDs.Count; i++)
        {
            var fruitonId = team.FruitonIDs[i];
            var pos = team.Positions[i];
            var kernelFruiton = FruitonFactory.makeFruiton(fruitonId, GameManager.Instance.FruitonDatabase);
            var x = pos.Y;
            var y = pos.X - 2;
            gridSquares[x, y].SetFruiton(kernelFruiton);
        }
    }

    public void ResetTeam()
    {
        ClearFruitons();
    }

    public bool HighlightAvailableSquares(int fruitonType, bool swapping = false)
    {
        bool isAnySquareAvailable = false;

        foreach (var square in gridSquares)
        {
            if (square.FruitonType == fruitonType)
            {
                if (square.IsEmpty || swapping)
                {
                    square.Highlight(Color.green);
                    isAnySquareAvailable = true;
                }
                else
                {
                    square.Highlight(Color.red);
                }
            }
        }

        return isAnySquareAvailable;
    }

    public List<Position> GetAvailableSquares(KFruiton fruiton)
    {
        List<Position> result = new List<Position>();
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (gridSquares[i, j].IsEmpty && gridSquares[i, j].FruitonType == fruiton.type)
                {
                    result.Add(GetBattlePositionFromGridPosition(i, j));
                }
            }
        }
        return result;
    }

    public void CancelHighlights()
    {
        foreach (var square in gridSquares)
        {
            square.CancelHighlight();
        }
    }

    public Position SuggestFruitonAtMousePosition(Vector3 pointerPosition, KFruiton fruiton,
        Position swapPosition = null)
    {
        // pointer position is in non-scaled world coordinates
        // we need to calculate square size in world coordinates
        Vector3[] corners = new Vector3[4];
        gridSquares[0, 0].RectTransform.GetWorldCorners(corners);
        var scaledSquareSize = corners[2].x;

        var x = (int) (pointerPosition.x / scaledSquareSize);
        var y = 4 - (int) (pointerPosition.y / scaledSquareSize);

        ClearSuggestions();

        if (x >= 0 && x < 2 && y >= 0 && y < 5
            && (gridSquares[x, y].IsEmpty || swapPosition != null)
            && gridSquares[x, y].FruitonType == fruiton.type)
        {
            gridSquares[x, y].SuggestFruiton(fruiton);
            if (swapPosition != null && gridSquares[x, y].KernelFruiton != null)
            {
                gridSquares[swapPosition.Y, swapPosition.X - 2].SuggestFruiton(gridSquares[x, y].KernelFruiton);
            }
            return GetBattlePositionFromGridPosition(x, y);
        }
        return null;
    }

    public void AddFruitonAt(KFruiton fruiton, Position position)
    {
        gridSquares[position.Y, position.X - 2].SetFruiton(fruiton);
    }

    private void InitGridFruitons()
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                var gridFruitonObject = Instantiate(GridSquareTemplate);
                gridFruitonObject.transform.SetParent(GridSquareTemplate.transform.parent);
                gridFruitonObject.transform.localScale = GridSquareTemplate.transform.localScale;
                gridFruitonObject.GetComponent<RectTransform>().localPosition =
                    new Vector3(i * squareSize, -j * squareSize, 0);

                var square = gridFruitonObject.GetComponent<FridgeGridSquare>();
                square.SetFruitonType(i == 0 ? (j == 2 ? 1 : 2) : 3); // sorry :(
                var x = i;
                var y = j;
                square.OnBeginDrag.AddListener(() =>
                {
                    if (AllowEdit)
                    {
                        OnBeginDragFromTeam.Invoke(square.KernelFruiton, GetBattlePositionFromGridPosition(x, y));
                        square.ClearFruiton();
                    }
                });
                square.OnMouseEnter.AddListener(() => OnMouseEnterSquare.Invoke(square));
                square.OnMouseExit.AddListener(() => OnMouseExitSquare.Invoke(square));

                gridSquares[i, j] = square;
            }
        }
        GridSquareTemplate.gameObject.SetActive(false);
    }

    private void ClearFruitons()
    {
        foreach (var square in gridSquares)
        {
            square.ClearFruiton();
        }
    }

    private void ClearSuggestions()
    {
        foreach (var square in gridSquares)
        {
            square.ClearSuggestion();
        }
    }

    private static Position GetBattlePositionFromGridPosition(int x, int y)
    {
        return new Position
        {
            X = y + 2,
            Y = x
        };
    }
}