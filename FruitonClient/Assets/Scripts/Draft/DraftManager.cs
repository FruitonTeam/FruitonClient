using System;
using Cz.Cuni.Mff.Fruiton.Dto;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using KFruiton = fruiton.kernel.Fruiton;
using System.Linq;
using Networking;

public class DraftManager : MonoBehaviour
{
    public ScrollRect ScrollRect;
    public FridgeTeamGrid MyTeamGrid;
    public FridgeTeamGrid EnemyTeamGrid;
    public Image DragAndDropBarrier;
    public GameObject FridgeFruitonTemplate;
    public FridgeDndFruiton DragAndDropFruiton;
    public GameObject WrapperFruitons;
    public GameObject PanelTooltip;
    public GameObject Filters;
    public FridgeFruitonDetail FruitonDetail;
    public RectTransform ScrollContentRectTransform;
    public RectTransform[] TooltipPanelPositions;
    public Button ButtonSurrender;
    public Text WarningText;
    public FridgeFilterManager FilterManager;
    public Text EnemyName;
    public Text MyName;
    public GameObject LoadingPanel;
    public Text TimeToPickText;
    public Text TimeToPickLabel;
    public Text OpponetTurnToPickText;
    public MessagePanel GameOverPanel;


    private readonly float pickTimeFrame = 2.0f;
    private FruitonTeam myTeam;
    private FruitonTeam enemyTeam;
    private bool isDragging;
    private bool isDraggingFromTeam;
    private bool isAddingFromDetail;
    private Position teamDragGridPosition;
    private KFruiton draggedFruiton;
    private List<FridgeFruiton> fridgeFruitons;
    private DraftHandler draftHandler;
    private bool isMyTurnToDraft;
    private Position currentDraftPosition;
    private DateTime pickTimeEnd;

    void Awake()
    {
        draftHandler = new DraftHandler(this);
        MyName.text = GameManager.Instance.UserName;
        EnemyName.text = "Waiting for opponent";
    }

    void Start()
    {
        myTeam = new FruitonTeam();
        enemyTeam = new FruitonTeam();
        MyTeamGrid.LoadTeam(myTeam);
        EnemyTeamGrid.LoadTeam(enemyTeam);
        
        SetupView();
        InitializeAllFruitons();

        InitializeTeamGridListeners();
        InitializeFruitonDetailListeners();
        DragAndDropFruiton.gameObject.SetActive(false);

        // HACK for the scroll view to work - without it it is just empty and somehow broken
        ScrollRect.gameObject.SetActive(false);
        ScrollRect.gameObject.SetActive(true);

        TurnOffDrafting();
        FindGame();
    }

    private void FindGame()
    {
        var connectionHandler = ConnectionHandler.Instance;
        var findGameMessage = new FindGame
        {
            Team = GameManager.Instance.CurrentFruitonTeam,
            GameMode = GameMode.Standard,
            PickMode = PickMode.Draft
        };
        var wrapperMessage = new WrapperMessage
        {
            FindGame = findGameMessage
        };
        Debug.Log(GameManager.Instance.CurrentFruitonTeam);
        connectionHandler.SendWebsocketMessage(wrapperMessage);
    }

    public void StartDraft(DraftReady readyMessage)
    {
        EnemyName.text = readyMessage.Opponent.Login;
        LoadingPanel.SetActive(false);
        ButtonSurrender.onClick.AddListener(Surrender);
        ButtonSurrender.GetComponentInChildren<Text>().text = "Surrender";
    }

    void Update()
    {
        double pickTimeLeft = Math.Max(0, (pickTimeEnd - DateTime.Now).TotalSeconds);
        TimeToPickText.text = Math.Floor(pickTimeLeft).ToString(CultureInfo.InvariantCulture);
        
        // drag and drop (or adding from fruiton detail window) logic
        if (isDragging)
        {
            Vector3 dndPosition = Vector3.left * 1000;
            Vector3 pointerPosition = Vector3.left * 1000;
#if UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                var touchPos = Input.GetTouch(0).position;
                pointerPosition = new Vector3(touchPos.x, touchPos.y, 0) + new Vector3(-1, 1, 0) * 50;
                dndPosition = pointerPosition + new Vector3(-1, 1, 0) * 100;
            }
#endif
#if UNITY_STANDALONE || UNITY_EDITOR
            pointerPosition = Input.mousePosition;
            dndPosition = Input.mousePosition + Vector3.down * 50;
#endif
            DragAndDropFruiton.transform.position = dndPosition;

            var dropGridPosition =
                MyTeamGrid.SuggestFruitonAtMousePosition(pointerPosition, draggedFruiton, teamDragGridPosition);

            if (dropGridPosition != null)
                Debug.Log(dropGridPosition.X + " " + dropGridPosition.Y);
            if (dropGridPosition != null && !dropGridPosition.Equals(currentDraftPosition))
            {
                dropGridPosition = null;
            }

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
                //MyTeamGrid.HighlightAvailable();
                ScrollRect.horizontal = true;

                if (dropGridPosition != null)
                    AddFruitonToTeam(draggedFruiton, dropGridPosition, myTeam);
                MyTeamGrid.LoadTeam(myTeam);
                WarningText.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateFruitonInMyTeam(DraftResult result)
    {
        var pos = myTeam.Positions.IndexOf(result.Position);
        if (pos == -1)
        {
            // Just add it
            myTeam.FruitonIDs.Add(result.FruitonId);
            myTeam.Positions.Add(result.Position);
            MyTeamGrid.LoadTeam(myTeam);
        }
        else if (myTeam.FruitonIDs[pos] != result.FruitonId)
        {
            // Update it
            myTeam.FruitonIDs[pos] = result.FruitonId;
            MyTeamGrid.LoadTeam(myTeam);
        }
    }

    public void AddEnemyFruiton(DraftResult result)
    {
        enemyTeam.Positions.Add(result.Position);
        enemyTeam.FruitonIDs.Add(result.FruitonId);
        EnemyTeamGrid.LoadTeam(enemyTeam);
    }

    public void TurnOnDrafting(DraftRequest request)
    {
        isMyTurnToDraft = true;
        currentDraftPosition = request.Position;
        MyTeamGrid.AvailablePositions = new List<Position> {currentDraftPosition};
        MyTeamGrid.CancelHighlights();
        MyTeamGrid.AllowEdit = true;
        MyName.color = Color.red;
        float pickTimeLeft = request.SecondsToPick - pickTimeFrame;
        pickTimeEnd = DateTime.Now.AddSeconds(pickTimeLeft);
        OpponetTurnToPickText.gameObject.SetActive(false);
        TimeToPickText.gameObject.SetActive(true);
        TimeToPickLabel.gameObject.SetActive(true);
    }

    public void TurnOffDrafting()
    {
        isMyTurnToDraft = false;
        MyTeamGrid.AvailablePositions = null;
        MyTeamGrid.CancelHighlights();
        MyTeamGrid.AllowEdit = false;
        MyName.color = Color.black;
        OpponetTurnToPickText.gameObject.SetActive(true);
        TimeToPickText.gameObject.SetActive(false);
        TimeToPickLabel.gameObject.SetActive(false);
    }
    

    public void HideDetail()
    {
        FruitonDetail.gameObject.SetActive(false);
        FruitonDetail.Barrier.gameObject.SetActive(false);
        HideTooltip();
    }

    public void LoadBattle()
    {
        if (GameManager.Instance.CurrentFruitonTeam != null)
        {
            var param = new Dictionary<string, string>
            {
                {Scenes.BATTLE_TYPE, Scenes.GetParam(BattleType.OnlineBattle.ToString())}
            };

            Scenes.Load(Scenes.BATTLE_SCENE, param);
        }
    }

    public void CancelSearch()
    {
        draftHandler.CancelSearch();
        Scenes.Load(Scenes.MAIN_MENU_SCENE);
    }

    public void Surrender()
    {
        draftHandler.SendSurrender();
        Scenes.Load(Scenes.MAIN_MENU_SCENE);
    }

    private void InitializeFruitonDetailListeners()
    {
        FruitonDetail.CloseButton.onClick.AddListener(HideDetail);
        FruitonDetail.Barrier.onClick.AddListener(HideDetail);
        FruitonDetail.AddToTeamButton.onClick.AddListener(() =>
        {
            HideDetail();
            if (isMyTurnToDraft)
            {
                var availablePositions = MyTeamGrid.GetAvailableSquares(FruitonDetail.CurrentFruiton);
                if (availablePositions.Count == 1)
                {
                    AddFruitonToTeam(FruitonDetail.CurrentFruiton, availablePositions[0], myTeam);
                }
                else
                {
                    isAddingFromDetail = true;
                    BeginFruitonDrag(FruitonDetail.CurrentFruiton);
                }
            }
        });
    }

    private void InitializeTeamGridListeners()
    {
        MyTeamGrid.OnBeginDragFromTeam.AddListener((a, b) => { });
        MyTeamGrid.OnMouseEnterSquare.AddListener(square =>
        {
            if (!isDragging)
            {
                square.Highlight(new Color(1, 1, 0.8f));
                if (square.KernelFruiton != null)
                {
                    ShowTooltip(square.KernelFruiton, 1);
                }
            }
        });
        MyTeamGrid.OnMouseExitSquare.AddListener(square =>
        {
            if (!isDragging)
            {
                square.CancelHighlight();
                HideTooltip();
            }
        });

        EnemyTeamGrid.OnMouseEnterSquare.AddListener(square =>
        {
            if (!isDragging)
            {
                square.Highlight(new Color(1, 1, 0.8f));
                if (square.KernelFruiton != null)
                {
                    ShowTooltip(square.KernelFruiton, 1);
                }
            }
        });
        EnemyTeamGrid.OnMouseExitSquare.AddListener(square =>
        {
            if (!isDragging)
            {
                square.CancelHighlight();
                HideTooltip();
            }
        });
    }

    private void InitializeAllFruitons()
    {
        GameManager gameManager = GameManager.Instance;
        IEnumerable<KFruiton> allFruitons = gameManager.AllFruitons;
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

    private void AddFruitonToTeam(KFruiton fruiton, Position position, FruitonTeam team)
    {
        if (isMyTurnToDraft)
        {
            team.FruitonIDs.Add(fruiton.dbId);
            team.Positions.Add(position);
            MyTeamGrid.AddFruitonAt(fruiton, position);
            draftHandler.SendDraftResponse(fruiton.dbId);
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

    /// <summary>
    /// Calculates position of an object (fruiton or team) on the scroll view grid
    /// </summary>
    /// <param name="index">index of the object</param>
    /// <returns>position of an object on the scroll view grid</returns>
    private static Vector3 GetPositionOnScrollViewGrid(int index)
    {
        return new Vector3(
            27.5f + (index / 2) * 240,
            -233 - (index % 2) * 231,
            0
        );
    }

    private void BeginFruitonDrag(FridgeFruiton fruiton)
    {
        if (fruiton.IsOwned && isMyTurnToDraft)
        {
            BeginFruitonDrag(fruiton.KernelFruiton);
        }
    }

    private void BeginFruitonDrag(KFruiton fruiton, Position teamPosition = null)
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

    private void ShowDetail(FridgeFruiton fruiton)
    {
        FruitonDetail.SetFruiton(fruiton, MyTeamGrid.GetAvailableSquares(fruiton.KernelFruiton).Count != 0);
        FruitonDetail.TooltipText.text = TooltipUtil.GenerateTooltip(fruiton.KernelFruiton);
        FruitonDetail.gameObject.SetActive(true);
    }

    private void ShowTooltip(KFruiton fruiton, int positionIndex = 0)
    {
        var targetTransform = TooltipPanelPositions[positionIndex];
        var tooltipTransform = PanelTooltip.GetComponent<RectTransform>();
        PanelTooltip.SetActive(true);
        PanelTooltip.transform.SetParent(targetTransform.parent);
        tooltipTransform.pivot = targetTransform.pivot;
        tooltipTransform.anchorMin = targetTransform.anchorMin;
        tooltipTransform.anchorMax = targetTransform.anchorMax;
        tooltipTransform.anchoredPosition = targetTransform.anchoredPosition;
        PanelTooltip.GetComponentInChildren<Text>().text = TooltipUtil.GenerateTooltip(fruiton);
    }

    private void HideTooltip()
    {
        PanelTooltip.SetActive(false);
    }

    private void SetupView()
    {
        WrapperFruitons.SetActive(true);
        Filters.SetActive(true);
        MyTeamGrid.AllowEdit = false;
        ResizeScrollContent(GameManager.Instance.AllFruitons.Count());
    }

    private void ResizeScrollContent(int objectCount)
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

    void OnEnable()
    {
        draftHandler.StartListening();
    }

    void OnDisable()
    {
        draftHandler.StopListening();
    }

    public void GameOver(GameOver gameOver)
    {
        ButtonSurrender.interactable = false;

        BattleUIUtil.ShowResults(GameOverPanel, gameOver);
    }
}