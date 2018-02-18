using System.Collections.Generic;
using Networking;
using UnityEngine;
using Cz.Cuni.Mff.Fruiton.Dto;
using UnityEngine.UI;
using KFruiton = fruiton.kernel.Fruiton;

public abstract class TeamsManagerBase : MonoBehaviour
{
    public ScrollRect ScrollRect;
    public GameObject PanelTooltip;
    public FridgeTeamGrid MyTeamGrid;
    public FridgeFruitonDetail FruitonDetail;
    public GameObject FridgeFruitonTemplate;
    public GameObject WrapperFruitons;
    public RectTransform[] TooltipPanelPositions;
    public FridgeFilterManager FilterManager;
    public RectTransform ScrollContentRectTransform;
    public Image DragAndDropBarrier;
    public FridgeDndFruiton DragAndDropFruiton;
    public Text WarningText;
    public GameObject Filters;

    protected List<FridgeFruiton> fridgeFruitons;
    protected KFruiton draggedFruiton;
    protected Position teamDragGridPosition;
    protected bool isDragging;
    protected bool isDraggingFromTeam;
    protected bool isAddingFromDetail;

    protected abstract bool ShouldBeginDrag(FridgeFruiton fruiton);
    protected abstract void AddToTeamButtonListener();
    protected abstract void OnBeginDragFromTeamListener(KFruiton fruiton, Position position);
    protected abstract void ProcessStopDrag(Position dropGridPosition);

    protected virtual Position ProcessDropGridPosition(Position dropGridPosition)
    {
        return dropGridPosition;
    }

    protected  void InitializeFruitonDetailListeners()
    {
        FruitonDetail.CloseButton.onClick.AddListener(HideDetail);
        FruitonDetail.Barrier.onClick.AddListener(HideDetail);
        FruitonDetail.AddToTeamButton.onClick.AddListener(AddToTeamButtonListener);
    }

    protected virtual void InitializeTeamGridListeners()
    {
        MyTeamGrid.OnBeginDragFromTeam.AddListener(OnBeginDragFromTeamListener);
        MyTeamGrid.OnMouseEnterSquare.AddListener(HighlightSquare);
        MyTeamGrid.OnMouseExitSquare.AddListener(CancelHighlightSquare);
    }

    protected virtual void Update()
    {
        // drag and drop (or adding from fruiton detail window) logic
        if (isDragging)
        {
#if UNITY_ANDROID
            Vector3 dndPosition;
            Vector3 pointerPosition;
            if (Input.touchCount > 0)
            {
                var touchPos = Input.GetTouch(0).position;
                pointerPosition = new Vector3(touchPos.x, touchPos.y, 0) + new Vector3(-1, 1, 0) * 50;
                dndPosition = pointerPosition + new Vector3(-1, 1, 0) * 100;
            }
#endif
#if UNITY_STANDALONE || UNITY_EDITOR
            Vector3 pointerPosition = Input.mousePosition;
            Vector3 dndPosition = Input.mousePosition + Vector3.down * 50;
#endif
            DragAndDropFruiton.transform.position = dndPosition;

            Position dropGridPosition =
                MyTeamGrid.SuggestFruitonAtMousePosition(pointerPosition, draggedFruiton, teamDragGridPosition);

            dropGridPosition = ProcessDropGridPosition(dropGridPosition);

            if (dropGridPosition == null)
            {
                DragAndDropFruiton.SetDropStatus(
                    isDraggingFromTeam
                        ? FridgeDndFruiton.DropStatus.Delete
                        : FridgeDndFruiton.DropStatus.Nothing
                );
            }
            else
            {
                DragAndDropFruiton.SetDropStatus(
                    isDraggingFromTeam
                        ? FridgeDndFruiton.DropStatus.Swap
                        : FridgeDndFruiton.DropStatus.Ok
                );
            }

            // stop drag and drop
            if ((!isAddingFromDetail && !Input.GetMouseButton(0))
                || (isAddingFromDetail && Input.GetMouseButton(0)))
            {
                HideTooltip();
                isDragging = false;
                isAddingFromDetail = false;
                DragAndDropFruiton.gameObject.SetActive(false);
                DragAndDropBarrier.gameObject.SetActive(false);
                MyTeamGrid.CancelHighlights();
                ScrollRect.horizontal = true;

                ProcessStopDrag(dropGridPosition);
                WarningText.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    protected void HighlightSquare(FridgeGridSquare square)
    {
        if (!isDragging)
        {
            square.Highlight(new Color(1, 1, 0.8f));
            if (square.KernelFruiton != null)
            {
                ShowTooltip(square.KernelFruiton, 1);
            }
        }
    }

    protected void CancelHighlightSquare(FridgeGridSquare square)
    {
        if (!isDragging)
        {
            square.CancelHighlight();
            HideTooltip();
        }
    }

    protected void InitializeAllFruitons()
    {
        GameManager gameManager = GameManager.Instance;
        IEnumerable<KFruiton> allFruitons = gameManager.AllPlayableFruitons;
        fridgeFruitons = new List<FridgeFruiton>();
        var i = 0;
        var templateRectTransform = FridgeFruitonTemplate.gameObject.GetComponent<RectTransform>();
        foreach (KFruiton fruiton in allFruitons)
        {
            var fridgeFruiton = Instantiate(FridgeFruitonTemplate);
            fridgeFruiton.transform.SetParent(WrapperFruitons.transform);
            fridgeFruiton.transform.localScale = templateRectTransform.localScale;
            fridgeFruiton.transform.localPosition = GetPositionOnScrollViewGrid(i);

            var kFruiton = fruiton;
            var fFruiton = fridgeFruiton.GetComponent<FridgeFruiton>();
            fFruiton.OnBeginDrag.AddListener(() => BeginFruitonDrag(fFruiton));
#if UNITY_STANDALONE || UNITY_EDITOR
            fFruiton.OnMouseEnter.AddListener(() => ShowTooltip(kFruiton));
            fFruiton.OnMouseExit.AddListener(HideTooltip);
            fFruiton.OnRightClick.AddListener(() => ShowDetail(fFruiton));
#endif
            fFruiton.OnTap.AddListener(() => ShowDetail(fFruiton));
            fFruiton.SetKernelFruiton(kFruiton);
            fFruiton.FridgeIndex = i;
            fridgeFruitons.Add(fFruiton);
            i++;
        }
        FridgeFruitonTemplate.SetActive(false);
        FilterManager.AllFruitons = fridgeFruitons;
        FilterManager.OnFilterUpdated.AddListener(ReindexFruitons);
        FilterManager.UpdateAvailableFruitons(gameManager.AvailableFruitons);
        PlayerHelper.GetAvailableFruitons(FilterManager.UpdateAvailableFruitons, Debug.Log);
    }

    private void BeginFruitonDrag(FridgeFruiton fruiton)
    {
        if (ShouldBeginDrag(fruiton))
        {
            BeginFruitonDrag(fruiton.KernelFruiton);
        }
    }

    protected void BeginFruitonDrag(KFruiton fruiton, Position teamPosition = null)
    {
        draggedFruiton = fruiton;
        DragAndDropBarrier.gameObject.SetActive(true);
        DragAndDropFruiton.gameObject.SetActive(true);
        DragAndDropFruiton.SetSkin(fruiton.model);
        ScrollRect.horizontal = false;
        teamDragGridPosition = teamPosition;
        isDragging = true;
        isDraggingFromTeam = teamPosition != null;
#if UNITY_ANDROID && !UNITY_EDITOR
        ShowTooltip(fruiton, 2);
# else
        HideTooltip();
#endif
        var isAnySquareAvailable = MyTeamGrid.HighlightAvailableSquares(fruiton.type, isDraggingFromTeam);
        if (isAnySquareAvailable)
        {
            if (isDraggingFromTeam)
            {
                WarningText.text =
                    "<color=#5555ff>Move</color> fruiton to other <color=#00ff00>available square</color> or <color=#ff0000>remove</color> it from the team";
            }
            else if (isAddingFromDetail)
            {
                WarningText.text =
                    "Choose an <color=#00ff00>available square</color> or click anywhere else to cancel";
            }
            else
            {
                WarningText.text =
                    "Drag the fruiton to any of the <color=#00ff00>available squares</color> to add it to the team";
            }
            WarningText.color = Color.white;
        }
        else
        {
            WarningText.text = "You don't have any empty square for fruiton of this type!";
            WarningText.color = Color.red;
        }
        WarningText.transform.parent.gameObject.SetActive(true);
    }

    protected void ResizeScrollContent(int objectCount)
    {
        var contentSize = ScrollContentRectTransform.sizeDelta;
        var helperIndex = objectCount + objectCount % 2;
        var newWidth = GetPositionOnScrollViewGrid(helperIndex).x;
        ScrollContentRectTransform.sizeDelta = new Vector2(newWidth, contentSize.y);
        var scrollViewWidth = ScrollContentRectTransform.parent.parent.GetComponent<RectTransform>().rect.width;
        if (newWidth < scrollViewWidth)
        {
            ScrollContentRectTransform.localPosition = Vector3.zero;
            return;
        }
        var contentWidth = newWidth + ScrollContentRectTransform.localPosition.x;
        if (contentWidth < scrollViewWidth)
        {
            ScrollContentRectTransform.localPosition = new Vector3(scrollViewWidth - newWidth, 0, 0);
        }
    }

    private void ReindexFruitons()
    {
        int newIndex = 0;
        foreach (var fruiton in fridgeFruitons)
        {
            var oldIndex = fruiton.FridgeIndex;
            if (!fruiton.gameObject.activeSelf)
            {
                fruiton.FridgeIndex = -1;
                continue;
            }
            fruiton.FridgeIndex = newIndex;
            if (newIndex != oldIndex)
            {
                if (oldIndex < 0)
                {
                    fruiton.gameObject.transform.localPosition = GetPositionOnScrollViewGrid(newIndex);
                }
                else
                {
                    iTween.Stop(fruiton.gameObject);
                    iTween.MoveTo(fruiton.gameObject, iTween.Hash(
                            "position", GetPositionOnScrollViewGrid(newIndex),
                            "islocal", true,
                            "time", 1,
                            "easetype", iTween.EaseType.easeOutExpo
                        )
                    );
                }
            }
            newIndex++;
        }
        ResizeScrollContent(newIndex + 1);
    }

    private void ShowDetail(FridgeFruiton fruiton)
    {
        FruitonDetail.SetFruiton(fruiton, MyTeamGrid.GetAvailableSquares(fruiton.KernelFruiton).Count != 0);
        FruitonDetail.TooltipText.text = TooltipUtil.GenerateTooltip(fruiton.KernelFruiton);
        FruitonDetail.gameObject.SetActive(true);
    }

    public void HideDetail()
    {
        FruitonDetail.gameObject.SetActive(false);
        FruitonDetail.Barrier.gameObject.SetActive(false);
        HideTooltip();
    }

    protected void ShowTooltip(KFruiton fruiton, int positionIndex = 0)
    {
        RectTransform targetTransform = TooltipPanelPositions[positionIndex];
        var tooltipTransform = PanelTooltip.GetComponent<RectTransform>();
        PanelTooltip.SetActive(true);
        PanelTooltip.transform.SetParent(targetTransform.parent);
        tooltipTransform.pivot = targetTransform.pivot;
        tooltipTransform.anchorMin = targetTransform.anchorMin;
        tooltipTransform.anchorMax = targetTransform.anchorMax;
        tooltipTransform.anchoredPosition = targetTransform.anchoredPosition;
        PanelTooltip.GetComponentInChildren<Text>().text = TooltipUtil.GenerateTooltip(fruiton);
    }

    protected void HideTooltip()
    {
        PanelTooltip.SetActive(false);
    }

    /// <summary>
    /// Calculates position of an object (fruiton or team) on the scroll view grid
    /// </summary>
    /// <param name="index">index of the object</param>
    /// <returns>position of an object on the scroll view grid</returns>
    protected static Vector3 GetPositionOnScrollViewGrid(int index)
    {
        return new Vector3(
            27.5f + (index / 2) * 240,
            -233 - (index % 2) * 231,
            0
        );
    }
}
