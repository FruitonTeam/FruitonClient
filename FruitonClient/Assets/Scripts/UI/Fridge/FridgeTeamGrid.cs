using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using KFruiton = fruiton.kernel.Fruiton;
using fruiton.fruitDb.factories;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

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
    public bool AllowEdit;
    public bool IsMirrored;
    public FridgeGridSquare GridSquareTemplate;
    public UnityEvent<KFruiton, Position> OnBeginDragFromTeam { get; private set; }
    public UnityEvent<FridgeGridSquare> OnMouseEnterSquare { get; private set; }
    public UnityEvent<FridgeGridSquare> OnMouseExitSquare { get; private set; }

    private List<Position> availablePositions;

    public List<Position> AvailablePositions
    {
        get { return availablePositions; }
        set
        {
            availablePositions = value;

            for (var x = 0; x < gridSquares.GetLength(0); x++)
            {
                for (var y = 0; y < gridSquares.GetLength(1); y++)
                {
                    FridgeGridSquare square = gridSquares[x, y];
                    if (availablePositions != null 
                        && availablePositions.Contains(GetBattlePositionFromGridPosition(x, y)))
                    {
                        square.SecondaryBgColor = Color.blue;
                        square.SwitchDefaultBgColor();
                    }
                    else
                    {
                        square.ResetDefaultBgColor();
                    }
                }
            }
        }
    }


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

    public void LoadTeam(FruitonTeam team, Dictionary<int, FridgeFruiton> dbFridgeMapping)
    {
        ClearFruitons();
        if (dbFridgeMapping != null)
        {
            List<int> availableFruitons = GameManager.Instance.AvailableFruitons;
            foreach (var fruiton in GameManager.Instance.AllPlayableFruitons)
            {
                var dbId = fruiton.dbId;
                dbFridgeMapping[dbId].Count = availableFruitons.Count(id => id == dbId);
            }
        }
        for (int i = 0; i < team.FruitonIDs.Count; i++)
        {
            var fruitonId = team.FruitonIDs[i];
            var pos = team.Positions[i];
            var kernelFruiton = FruitonFactory.makeFruiton(fruitonId, GameManager.Instance.FruitonDatabase);
            var x = IsMirrored ? 1 - pos.Y : pos.Y;
            var y = pos.X - 2;
            if (dbFridgeMapping != null)
            {
                if (--dbFridgeMapping[fruitonId].Count < 0)
                {
                    gridSquares[x, y].SetSecondaryBgColorAsDefault();
                }
                else
                {
                    gridSquares[x,y].ResetDefaultBgColor();
                }
            }
            gridSquares[x, y].CancelHighlight();
            gridSquares[x, y].SetFruiton(kernelFruiton);
        }
    }

    public void ResetTeam()
    {
        ClearFruitons();
    }

    public void HighlightAvailable()
    {
        if (AvailablePositions != null)
        {
            foreach (Position pos in AvailablePositions)
            {
                Position gridPos = GetGridPositionFromBattlePosition(pos.X, pos.Y);
                gridSquares[gridPos.X, gridPos.Y].Highlight(Color.blue);
            }
        }
    }

    public bool HighlightAvailableSquares(int fruitonType, bool swapping = false)
    {
        bool isAnySquareAvailable = false;

        for (var x = 0; x < gridSquares.GetLength(0); x++)
        {
            for (var y = 0; y < gridSquares.GetLength(1); y++)
            {
                var square = gridSquares[x, y];
                if (square.FruitonType == fruitonType
                    && (AvailablePositions == null || AvailablePositions.Contains(GetBattlePositionFromGridPosition(x, y))))
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
        }

        return isAnySquareAvailable;
    }

    public List<Position> GetAvailableSquares(KFruiton fruiton)
    {
        var result = new List<Position>();
        for (int x = 0; x < gridSquares.GetLength(0); x++)
        {
            for (int y = 0; y < gridSquares.GetLength(1); y++)
            {
                if (gridSquares[x, y].IsEmpty 
                    && gridSquares[x, y].FruitonType == fruiton.type
                    && (AvailablePositions == null || AvailablePositions.Contains(GetBattlePositionFromGridPosition(x, y))))
                {
                    result.Add(GetBattlePositionFromGridPosition(x, y));
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
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                var gridFruitonObject = Instantiate(GridSquareTemplate);
                gridFruitonObject.transform.SetParent(GridSquareTemplate.transform.parent);
                gridFruitonObject.transform.localScale = GridSquareTemplate.transform.localScale;
                gridFruitonObject.GetComponent<RectTransform>().localPosition =
                    new Vector3(x * squareSize, -y * squareSize, 0);

                var square = gridFruitonObject.GetComponent<FridgeGridSquare>();
                if (IsMirrored)
                    square.SetFruitonType(x == 1 ? (y == 2 ? 1 : 2) : 3);
                else
                    square.SetFruitonType(x == 0 ? (y == 2 ? 1 : 2) : 3);
                var xLoc = x;
                var yLoc = y;
                square.OnBeginDrag.AddListener(() =>
                {
                    if (AllowEdit)
                    {
                        OnBeginDragFromTeam.Invoke(square.KernelFruiton, GetBattlePositionFromGridPosition(xLoc, yLoc));
                        square.ResetDefaultBgColor();
                        square.ClearFruiton();
                    }
                });
                square.OnMouseEnter.AddListener(() => OnMouseEnterSquare.Invoke(square));
                square.OnMouseExit.AddListener(() => OnMouseExitSquare.Invoke(square));
                square.SecondaryBgColor = Color.red;

                gridSquares[x, y] = square;
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

    private static Position GetGridPositionFromBattlePosition(int x, int y)
    {
        return new Position
        {
            X = y,
            Y = x - 2
        };
    }
}