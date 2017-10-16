using fruiton.kernel;
using fruiton.kernel.actions;
using fruiton.kernel.targetPatterns;
using System.Collections.Generic;
using UnityEngine;

using ProtoAction = Cz.Cuni.Mff.Fruiton.Dto.Action;
using Action = fruiton.kernel.actions.Action;
using Event = fruiton.kernel.events.Event;
using KEvent = fruiton.kernel.events.Event;
using Field = fruiton.kernel.Field;
using KFruiton = fruiton.kernel.Fruiton;
using KAction = fruiton.kernel.actions.Action;
using KVector2 = fruiton.dataStructures.Point;
using fruiton.kernel.events;
using UnityEngine.UI;
using System;
using System.Linq;
using Networking;
using Cz.Cuni.Mff.Fruiton.Dto;
using Google.Protobuf.Collections;
using fruiton.fruitDb.factories;
using fruiton.fruitDb;

public class BattleManager : MonoBehaviour, IOnMessageListener {

    public Button EndTurnButton;
    public Text TimeCounter;
    public GameObject Panel_LoadingGame;

    /// <summary> Client fruitons stored at their position. </summary>
    private GameObject[,] grid;
    /// <summary> For handling grid tiles. </summary>
    private GridLayoutManager gridLayoutManager;
    private GameManager gameManager;
    private Player me, opponent;
    private Kernel kernel;

    private List<MoveAction> availableMoveActions;
    private List<AttackAction> availableAttackActions;

    private readonly DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly string OPPONENTS_TURN = "Opponent's turn";
    private static readonly string END_TURN = "End turn";

    private bool isFirstPlayer;

    private void Start()
    {
        Panel_LoadingGame.SetActive(true);
        gridLayoutManager = GridLayoutManager.Instance;
        gameManager = GameManager.Instance;
        grid = new GameObject[gridLayoutManager.WidthCount, gridLayoutManager.HeighCount];
        
        SetPositionsOfCurrentFruitonTeam();

        FindGame();
    }

    private void FindGame()
    {
        var connectionHandler = ConnectionHandler.Instance;
        FindGame findGameMessage = new FindGame
        {
            Team = gameManager.CurrentFruitonTeam
        };
        var wrapperMessage = new WrapperMessage
        {
            FindGame = findGameMessage
        };
        connectionHandler.SendWebsocketMessage(wrapperMessage);
    }

    private void SendReadyMessage()
    {
        var connectionHandler = ConnectionHandler.Instance;
        var playerReadyMessage = new PlayerReady();
        var wrapperMessage = new WrapperMessage
        {
            PlayerReady = playerReadyMessage
        };
        connectionHandler.SendWebsocketMessage(wrapperMessage);
    }

    private void InitializeTeam(IEnumerable<GameObject> currentTeam, Player player, RepeatedField<Position> fruitonsPositions = null)
    {
        int counter = 0;
        int i = 0, j = 0;
        foreach (GameObject clientFruiton in currentTeam)
        {
            var kernelFruiton = clientFruiton.GetComponent<ClientFruiton>().KernelFruiton;
            kernelFruiton.owner = player;
            clientFruiton.gameObject.AddComponent<BoxCollider>();
            if (fruitonsPositions != null)
            {
                var currentPosition = fruitonsPositions[counter];
                i = currentPosition.X;
                j = currentPosition.Y;
                counter++;
            }
            grid[i, j] = clientFruiton;
            kernelFruiton.position = new KVector2(i, j);
            Vector3 cellPosition = gridLayoutManager.GetCellPosition(i, j);
            clientFruiton.transform.position = cellPosition + new Vector3(0, clientFruiton.transform.lossyScale.y, 0);
        }
    }

    private void SetPositionsOfCurrentFruitonTeam()
    {
        int i, j;
        int majorRow = 0;
        int minorRow = 1;
        int majorCounter = 2;
        int minorCounter = 2;
        FruitonDatabase fruitonDatabase = GameManager.Instance.FruitonDatabase;
        foreach (var id in GameManager.Instance.CurrentFruitonTeam.FruitonIDs)
        {
            var kernelFruiton = FruitonFactory.makeFruiton(id, fruitonDatabase);
            switch ((FruitonType)kernelFruiton.type)
            {
                case FruitonType.KING:
                    {
                        i = gridLayoutManager.WidthCount / 2;
                        j = majorRow;
                    }
                    break;
                case FruitonType.MAJOR:
                    {
                        i = gridLayoutManager.WidthCount / 2 - majorCounter;
                        j = majorRow;
                        if (--majorCounter == 0) --majorCounter;
                    }
                    break;
                case FruitonType.MINOR:
                    {
                        i = gridLayoutManager.WidthCount / 2 - minorCounter;
                        j = minorRow;
                        --minorCounter;
                    }
                    break;
                default:
                    {
                        throw new UndefinedFruitonTypeException();
                    }
            }
            GameManager.Instance.CurrentFruitonTeam.Positions.Add(new Position { X = i, Y = j });
        }
        
    }

    private void Update()
    {
        if (kernel == null)
        {
            return;
        }
        UpdateTimer();
        if (kernel.currentState.get_activePlayer().id == me.id)
        {
            if (Input.GetMouseButtonUp(0))
            {
                LeftButtonUpLogic();
            }
        }

    }

    private void UpdateTimer()
    {
        int currentEpochTime = (int)(DateTime.UtcNow - epochStart).TotalSeconds;
        int timeLeft = (int)(kernel.currentState.turnState.endTime - currentEpochTime);
        if (timeLeft <= 0)
        {
            PerformAction(new EndTurnAction(new EndTurnActionContext()));
            return;
        }
        TimeCounter.text = (timeLeft).ToString();
    }

    private void LeftButtonUpLogic()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject HitObject = hit.transform.gameObject;
            // Player clicked on a fruiton.
            if (grid.Contains(HitObject)) {
                gridLayoutManager.ResetHighlights();
                var indices = grid.GetIndices(HitObject);
                if (availableAttackActions != null)
                {
                    // Find the action where the target is the clicked fruiton and perform it (if such action exists).
                    var performedActions = availableAttackActions.FindAll(x => ((AttackActionContext)x.actionContext).target.equalsTo(indices));
                    if (performedActions.Count != 0)
                    {
                        var performedAction = performedActions[0];
                        PerformAction(performedAction);
                    } 
                }
                availableMoveActions = VisualizeActionsOfType<MoveAction>(indices);
                availableAttackActions = VisualizeActionsOfType<AttackAction>(indices);

            }
            // A tile was clicked.
            else if (gridLayoutManager.ContainsTile(HitObject))
            {
                KVector2 tileIndices = gridLayoutManager.GetIndicesOfTile(HitObject);
                Debug.Log(tileIndices);
                TargetableAction performedAction = null;
                if (availableMoveActions != null)
                {
                    var performedActions = availableMoveActions.FindAll(x => ((MoveActionContext)x.actionContext).target.equalsTo(tileIndices));
                    if (performedActions.Count != 0)
                        performedAction = performedActions[0];
                }
                if (availableAttackActions != null && performedAction == null)
                {
                    var performedActions = availableAttackActions.FindAll(x => ((AttackActionContext)x.actionContext).target.equalsTo(tileIndices));
                    if (performedActions.Count != 0)
                        performedAction = performedActions[0];
                }
                if (performedAction != null)
                {
                    PerformAction(performedAction);
                }
            }
        }
    }

    private void PerformAction(EndTurnAction performedAction)
    {
        var actionMessage = new ProtoAction { From = null, To = null, Id = performedAction.getId() };
        var wrapperMessage = new WrapperMessage { Action = actionMessage };
        ConnectionHandler.Instance.SendWebsocketMessage(wrapperMessage);
        EndTurn(performedAction);
        PerformActionLocally(performedAction);
    }

    private void PerformAction(TargetableAction performedAction)
    {
        var castedToAction = (Action)performedAction;
        var context = performedAction.getContext();

        var actionMessage = new ProtoAction { From = context.source.ToPosition(), To = context.target.ToPosition(), Id = castedToAction.getId() };
        var wrapperMessage = new WrapperMessage { Action = actionMessage };
        ConnectionHandler.Instance.SendWebsocketMessage(wrapperMessage);

        PerformActionLocally(castedToAction);
    }

    private void PerformActionLocally(Action performedAction)
    {
        var events = kernel.performAction(performedAction).CastToList<KEvent>();
        foreach (var item in events)
        {
            ProcessEvent(item);
        }
    }

    private List<T> VisualizeActionsOfType<T>(KVector2 indices) where T:Action
    {
        var allActions = kernel.getAllValidActionsFrom(indices).CastToList<Action>();
        var result = allActions.OfType<T>();
        Debug.Log("actions: " + availableMoveActions);
        var kernelFruiton = kernel.currentState.field.get(indices).fruiton;
        foreach (T action in result)
        {
            VisualizeAction(action, kernelFruiton);
        }
        return result.ToList()  ;
    }

    private void ProcessEvent(KEvent kEvent)
    {
        System.Type eventType = kEvent.GetType();
        if (eventType == typeof(MoveEvent))
        {
            ProcessMoveEvent((MoveEvent)kEvent);
        } 
        else if (eventType == typeof(AttackEvent))
        {
            ProcessAttackEvent((AttackEvent)kEvent);
        } 
        else if (eventType == typeof(DeathEvent))
        {
            ProcessDeathEvent((DeathEvent)kEvent);
        }
    }

    private void ProcessDeathEvent(DeathEvent kEvent)
    {
        var killedPos = kEvent.target;
        var killed = grid[killedPos.x, killedPos.y];
        Destroy(killed);
        grid[killedPos.x, killedPos.y] = null;
    }

    private void ProcessAttackEvent(AttackEvent kEvent)
    {
        var damagedPosition = kEvent.target;
        var damaged = grid[damagedPosition.x, damagedPosition.y];
        damaged.GetComponent<ClientFruiton>().TakeDamage(kEvent.damage);
    }

    private void ProcessMoveEvent(MoveEvent moveEvent)
    {
        var from = moveEvent.from;
        var to = moveEvent.to;
        var movedObject = grid[from.x, from.y];
        grid[to.x, to.y] = movedObject;
        grid[from.x, from.y] = null;
        var toPosition = gridLayoutManager.GetCellPosition(to.x, to.y);
        movedObject.transform.position = toPosition + new Vector3(0, movedObject.transform.lossyScale.y, 0);
        gridLayoutManager.ResetHighlights();
    }

    private void VisualizeAction(Action action, KFruiton kernelFruiton)
    {
        var type = action.GetType();
        if (type == typeof(MoveAction))
        {
            var moveAction = (MoveAction)action;
            var target = ((MoveActionContext)(moveAction.actionContext)).target;
            Debug.Log("Highlight x=" + target.x + " y=" + target.y);
            gridLayoutManager.HighlightCell(target.x, target.y, Color.blue);
            VisualizePossibleAttacks(target, kernelFruiton);
        }
        else if (type == typeof(AttackAction))
        {
            var attackAction = (AttackAction)action;
            var target = ((AttackActionContext)(attackAction.actionContext)).target;
            gridLayoutManager.HighlightCell(target.x, target.y, Color.red);
        }
    }

    private void VisualizePossibleAttacks(KVector2 potentialPosition, KFruiton kernelFruiton)
    {
        var attackGenerators = kernelFruiton.attackGenerators.CastToList<AttackGenerator>();
        foreach (var attackGenerator in attackGenerators)
        {
            var attacks = attackGenerator.getAttacks(potentialPosition, kernelFruiton.damage).CastToList<AttackAction>();
            foreach (var attack in attacks)
            {
                var target = ((AttackActionContext)attack.actionContext).target;
                if (kernel.currentState.field.exists(target))
                {
                    var potentialTarget = kernel.currentState.field.get(target).fruiton;
                    if (potentialTarget != null && potentialTarget.owner.id != kernel.currentState.get_activePlayer().id)
                    {
                        gridLayoutManager.HighlightCell(target.x, target.y, Color.yellow);
                    }
                }
            }
        }
    }

    public void EndTurn()
    {
        DisableEndTurnButton();
        var action = new EndTurnAction(new EndTurnActionContext());
        PerformAction(action);
        EndTurn(action);
    }

    public void EndTurn(EndTurnAction endTurnAction)
    { 
        gridLayoutManager.ResetHighlights();
        kernel.performAction(endTurnAction);
        var oldPos = EndTurnButton.transform.localPosition;
        EndTurnButton.transform.localPosition = new Vector3(oldPos.x, -oldPos.y, oldPos.z);
    }

    public void OnMessage(WrapperMessage message)
    {
        switch (message.MessageCase)
        {
            case WrapperMessage.MessageOneofCase.GameReady:
                {
                    ProcessMessage(message.GameReady);
                } break;
            case WrapperMessage.MessageOneofCase.GameStarts:
                {
                    ProcessMessage(message.GameStarts);
                } break;
            case WrapperMessage.MessageOneofCase.Action:
                {
                    ProcessMessage(message.Action);
                } break;
        }
    }

    private void ProcessMessage(ProtoAction protoAction)
    {
        if (protoAction.Id == EndTurnAction.ID)
        {
            EnableEndTurnButton();
            PerformActionLocally(new EndTurnAction(new EndTurnActionContext()));
        }
        else if (protoAction.Id == MoveAction.ID)
        {
            PerformOpponentAction<MoveAction>(protoAction);
        } 
        else if (protoAction.Id == AttackAction.ID)
        {
            PerformOpponentAction<AttackAction>(protoAction);
        }
    }

    private void PerformOpponentAction<TTargetableAction>(ProtoAction protoAction) where TTargetableAction : Action, TargetableAction
    {
        KVector2 from = protoAction.From.ToKernelPosition();
        KVector2 to = protoAction.To.ToKernelPosition();
        IEnumerable<TTargetableAction> allValidActionsFrom = kernel.getAllValidActionsFrom(from).CastToList<Action>().OfType<TTargetableAction>();
        TTargetableAction performedAction = allValidActionsFrom.SingleOrDefault(x => (x.getContext()).target.equalsTo(to));
        if (performedAction == null)
        {
            Debug.Log("Server-Client desync detected.");
        }
        PerformActionLocally(performedAction);
    }

    private void ProcessMessage(GameReady gameReadyMessage)
    {
        me = new Player(0);
        opponent = new Player(1);
        Debug.Log("RECEIVED WEBSOCKET MSG: gameReadyMessage");
        Debug.Log("Opponent positions: " + gameReadyMessage.OpponentTeam.FruitonIDs);
        IEnumerable<GameObject> opponentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameReadyMessage.OpponentTeam.FruitonIDs);
        IEnumerable<GameObject> currentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs);
        InitializeTeam(opponentTeam, opponent, gameReadyMessage.OpponentTeam.Positions);

        var fruitons = new Array<object>();
        foreach (var fruiton in currentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        foreach (var fruiton in opponentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        isFirstPlayer = gameReadyMessage.StartsFirst;
        if (isFirstPlayer)
        {
            InitializeTeam(currentTeam, me, GameManager.Instance.CurrentFruitonTeam.Positions);
            kernel = new Kernel(me, opponent, fruitons);
        }
        else
        {
            var width = gridLayoutManager.WidthCount;
            var height = gridLayoutManager.HeighCount;
            var flippedPositions = BattleHelper.FlipCoordinates(GameManager.Instance.CurrentFruitonTeam.Positions, width, height);
            InitializeTeam(currentTeam, me, flippedPositions);
            kernel = new Kernel(opponent, me, fruitons);
            DisableEndTurnButton();
        }
        SendReadyMessage();
    }

    private void ProcessMessage(GameStarts gameStartsMessage)
    {
        if (!isFirstPlayer)
        {
            foreach (var fruiton in grid)
            {
                if (fruiton != null)
                {
                    fruiton.transform.Rotate(0, 180, 0);
                }
            }
            var oldPosition = Camera.main.transform.position;
            //Camera.main.transform.Rotate(0, 180, 0);
            Camera.main.transform.position = new Vector3(oldPosition.x, oldPosition.y, -oldPosition.z);
            var oldEulerAngles = Camera.main.transform.eulerAngles;
            Camera.main.transform.eulerAngles = new Vector3(oldEulerAngles.x, oldEulerAngles.y + 180, oldEulerAngles.z);
        }

        Debug.Log("RECEIVED WEBSOCKET MSG: gameStartsMessage");
        kernel.startGame();
        Panel_LoadingGame.SetActive(false);
    }

    private void EnableEndTurnButton()
    {
        EndTurnButton.enabled = true;
        EndTurnButton.GetComponentInChildren<Text>().text = END_TURN;
    }

    private void DisableEndTurnButton()
    {
        EndTurnButton.enabled = false;
        EndTurnButton.GetComponentInChildren<Text>().text = OPPONENTS_TURN;
    }

    void OnEnable()
    {
        if (ConnectionHandler.Instance.IsLogged())
        {
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.GameReady, this);
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.GameStarts, this);
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.Action, this);
        }
    }

    void OnDisable()
    {
        if (ConnectionHandler.Instance.IsLogged())
        {
            ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.GameReady, this);
            ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.GameStarts, this);
            ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.Action, this);
        }
    }
}
