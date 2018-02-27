using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UI.Chat;
using UnityEngine;
using UnityEngine.UI;
using KFruiton = fruiton.kernel.Fruiton;

public class DraftManager : TeamManagerBase
{
    public FridgeTeamGrid EnemyTeamGrid;
    public Button ButtonSurrender;
    public Text EnemyName;
    public Text MyName;
    public GameObject LoadingPanel;
    public Text TimeToPickText;
    public Text TimeToPickLabel;
    public Text OpponetTurnToPickText;
    public GameResultsPanel GameResultsPanel;

    private readonly float pickTimeFrame = 2.0f;
    private FruitonTeam myTeam;
    private FruitonTeam enemyTeam;
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

    protected override void Start()
    {
        base.Start();
        myTeam = new FruitonTeam();
        enemyTeam = new FruitonTeam();
        InitializeAllFruitons();
        MyTeamGrid.LoadTeam(myTeam, dbFridgeMapping);
        EnemyTeamGrid.LoadTeam(enemyTeam, null);

        SetupView();


        InitializeTeamGridListeners();
        InitializeFruitonDetailListeners();
        DragAndDropFruiton.gameObject.SetActive(false);

        // HACK for the scroll view to work - without it it is just empty and somehow broken
        ScrollRect.gameObject.SetActive(false);
        ScrollRect.gameObject.SetActive(true);

        TurnOffDrafting();
        if (!ChallengeController.Instance.IsChallengeActive)
        {
            FindGame();
        }
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
        connectionHandler.SendWebsocketMessage(wrapperMessage);
    }

    public void StartDraft(DraftReady readyMessage)
    {
        EnemyName.text = readyMessage.Opponent.Login;
        LoadingPanel.SetActive(false);
        ButtonSurrender.onClick.AddListener(Surrender);
        ButtonSurrender.GetComponentInChildren<Text>().text = "Surrender";
    }

    protected override void Update()
    {
        double pickTimeLeft = Math.Max(0, (pickTimeEnd - DateTime.Now).TotalSeconds);
        TimeToPickText.text = Math.Floor(pickTimeLeft).ToString(CultureInfo.InvariantCulture);
        
        base.Update();
    }

    protected override Position ProcessDropGridPosition(Position dropGridPosition)
    {
        if (dropGridPosition != null && !dropGridPosition.Equals(currentDraftPosition))
        {
            return null;
        }
        return dropGridPosition;
    }

    protected override void ProcessStopDrag(Position dropGridPosition)
    {
        if (dropGridPosition != null)
            AddFruitonToTeam(draggedFruiton, dropGridPosition, myTeam);
        MyTeamGrid.LoadTeam(myTeam, dbFridgeMapping);
    }

    protected override bool ShouldBeginDrag(FridgeFruiton fruiton)
    {
        return fruiton.IsOwned && isMyTurnToDraft && fruiton.Count > 0;
    }

    public void UpdateFruitonInMyTeam(DraftResult result)
    {
        var pos = myTeam.Positions.IndexOf(result.Position);
        if (pos == -1)
        {
            // Just add it
            myTeam.FruitonIDs.Add(result.FruitonId);
            myTeam.Positions.Add(result.Position);
            MyTeamGrid.LoadTeam(myTeam, dbFridgeMapping);
        }
        else if (myTeam.FruitonIDs[pos] != result.FruitonId)
        {
            // Update it
            myTeam.FruitonIDs[pos] = result.FruitonId;
            MyTeamGrid.LoadTeam(myTeam, dbFridgeMapping);
        }
    }

    public void AddEnemyFruiton(DraftResult result)
    {
        enemyTeam.Positions.Add(result.Position);
        enemyTeam.FruitonIDs.Add(result.FruitonId);
        EnemyTeamGrid.LoadTeam(enemyTeam, null);
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

    public void LoadBattle(GameReady gameReady)
    {
        GameManager.Instance.CurrentFruitonTeam = myTeam;
        var param = new Dictionary<string, string>
        {
            {Scenes.BATTLE_TYPE, BattleType.OnlineBattle.ToString()},
            {Scenes.GAME_MODE, GameMode.Standard.ToString()}
        };
        var objParam = new Dictionary<string, object>
        {
            {Scenes.GAME_READY_MSG, gameReady}
        };
        Scenes.Load(Scenes.BATTLE_SCENE, param, objParam);
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

    protected override void AddToTeamButtonListener()
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
    }

    protected override void OnBeginDragFromTeamListener(KFruiton fruiton, Position position)
    {
        // Dragging from team is disabled in draft mode
    }

    protected override void InitializeTeamGridListeners()
    {
        base.InitializeTeamGridListeners();

        EnemyTeamGrid.OnMouseEnterSquare.AddListener(HighlightSquare);
        EnemyTeamGrid.OnMouseExitSquare.AddListener(CancelHighlightSquare);
    }

    private void AddFruitonToTeam(KFruiton fruiton, Position position, FruitonTeam team)
    {
        if (isMyTurnToDraft)
        {
            FridgeFruiton fridgeFruiton = dbFridgeMapping[fruiton.dbId];
            fridgeFruiton.Count--;
            team.FruitonIDs.Add(fruiton.dbId);
            team.Positions.Add(position);
            MyTeamGrid.AddFruitonAt(fruiton, position);
            draftHandler.SendDraftResponse(fruiton.dbId);
        }
    }

    private void SetupView()
    {
        WrapperFruitons.SetActive(true);
        Filters.SetActive(true);
        GameResultsPanel.gameObject.SetActive(false);
        MyTeamGrid.AllowEdit = false;
        ResizeScrollContent(GameManager.Instance.AllPlayableFruitons.Count());
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

        GameResultsPanel.ShowResult(gameOver);
    }
}