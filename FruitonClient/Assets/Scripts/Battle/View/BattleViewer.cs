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
using haxe.root;

public class BattleViewer : MonoBehaviour {

    public Button EndTurnButton;
    public Text TimeCounter;
    public GameObject Panel_LoadingGame;

    /// <summary> Client fruitons stored at their position. </summary>
    public GameObject[,] Grid { get; set; }
    /// <summary> For handling grid tiles. </summary>
    private GridLayoutManager gridLayoutManager;
    private GameManager gameManager;

    private static readonly string OPPONENTS_TURN = "Opponent's turn";
    private static readonly string END_TURN = "End turn";

    private Battle battle;

    private bool gameStarted;

    private void Start()
    {
        gridLayoutManager = GridLayoutManager.Instance;
        gameManager = GameManager.Instance;
        Grid = new GameObject[gridLayoutManager.WidthCount, gridLayoutManager.HeighCount];
        
        SetPositionsOfFruitonTeam(GameManager.Instance.CurrentFruitonTeam);

        bool online = Scenes.GetParam(Scenes.ONLINE) == bool.TrueString;
        Debug.Log("playing online = " + online);
        if (online)
        {
            battle = new OnlineBattle(this);
            Panel_LoadingGame.SetActive(true);
        }
        else
        {
            battle = new OfflineBattle(this);
            gameStarted = true;
        }
    }

    private void Update()
    {
        if (!gameStarted)
        {
            return;
        }
        UpdateTimer();
        if (Input.GetMouseButtonUp(0))
        {
            LeftButtonUpLogic();
        }
    }

    private void LeftButtonUpLogic()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            battle.LeftButtonUpEvent(hit);
        }
    }

    public void StartGame(bool isFirstPlayer)
    {
        if (!isFirstPlayer)
        {
            foreach (var fruiton in Grid)
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
        Panel_LoadingGame.SetActive(false);
        gameStarted = true;
    }

    public void InitializeTeam(IEnumerable<GameObject> currentTeam, ClientPlayerBase player, RepeatedField<Position> fruitonsPositions = null)
    {
        int counter = 0;
        int i = 0, j = 0;
        foreach (GameObject clientFruiton in currentTeam)
        {
            var kernelFruiton = clientFruiton.GetComponent<ClientFruiton>().KernelFruiton;
            kernelFruiton.owner = player.KernelPlayer;
            clientFruiton.gameObject.AddComponent<BoxCollider>();
            if (fruitonsPositions != null)
            {
                var currentPosition = fruitonsPositions[counter];
                i = currentPosition.X;
                j = currentPosition.Y;
                counter++;
            }
            Grid[i, j] = clientFruiton;
            kernelFruiton.position = new KVector2(i, j);
            Vector3 cellPosition = gridLayoutManager.GetCellPosition(i, j);
            clientFruiton.transform.position = cellPosition + new Vector3(0, clientFruiton.transform.lossyScale.y, 0);
        }
    }

    public void SetPositionsOfFruitonTeam(FruitonTeam fruitonTeam)
    {
        int i, j;
        int majorRow = 0;
        int minorRow = 1;
        int majorCounter = 2;
        int minorCounter = 2;
        FruitonDatabase fruitonDatabase = GameManager.Instance.FruitonDatabase;
        foreach (var id in fruitonTeam.FruitonIDs)
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


    private void UpdateTimer()
    {
        int timeLeft = battle.ComputeRemainingTime();
        TimeCounter.text = (timeLeft).ToString();
    }

    

    //private void PerformAction(EndTurnAction performedAction)
    //{
    //    EndTurn(performedAction);
    //    PerformActionLocally(performedAction);
    //}

    private void PerformAction(TargetableAction performedAction)
    {
        var castedToAction = (Action)performedAction;
        var context = performedAction.getContext();

        var actionMessage = new ProtoAction { From = context.source.ToPosition(), To = context.target.ToPosition(), Id = castedToAction.getId() };
        var wrapperMessage = new WrapperMessage { Action = actionMessage };
        ConnectionHandler.Instance.SendWebsocketMessage(wrapperMessage);
    }

    public List<T> VisualizeActionsOfType<T>(KVector2 indices) where T:Action
    {
        List<Action> allActions = battle.GetAllValidActionFrom(indices);
        IEnumerable<T> result = allActions.OfType<T>();
        var kernelFruiton = battle.GetFruiton(indices);
        foreach (T action in result)
        {
            VisualizeAction(action, kernelFruiton);
        }
        return result.ToList()  ;
    }

    public void ProcessEvent(KEvent kEvent)
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
        var killed = Grid[killedPos.x, killedPos.y];
        Destroy(killed);
        Grid[killedPos.x, killedPos.y] = null;
    }

    private void ProcessAttackEvent(AttackEvent kEvent)
    {
        var damagedPosition = kEvent.target;
        var damaged = Grid[damagedPosition.x, damagedPosition.y];
        damaged.GetComponent<ClientFruiton>().TakeDamage(kEvent.damage);
    }

    private void ProcessMoveEvent(MoveEvent moveEvent)
    {
        var from = moveEvent.from;
        var to = moveEvent.to;
        var movedObject = Grid[from.x, from.y];
        Grid[to.x, to.y] = movedObject;
        Grid[from.x, from.y] = null;
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
        List<KVector2> potentialTargets = battle.ComputePossibleAttacks(potentialPosition, kernelFruiton);
        foreach(KVector2 potentialTarget in potentialTargets)
        {
            gridLayoutManager.HighlightCell(potentialTarget.x, potentialTarget.y, Color.yellow);
        }
        
    }

    public void EndTurn()
    {
        DisableEndTurnButton();
        gridLayoutManager.ResetHighlights();
        battle.EndTurnEvent();
    }

    public void EnableEndTurnButton()
    {
        EndTurnButton.enabled = true;
        EndTurnButton.GetComponentInChildren<Text>().text = END_TURN;
    }

    public void DisableEndTurnButton()
    {
        EndTurnButton.enabled = false;
        EndTurnButton.GetComponentInChildren<Text>().text = OPPONENTS_TURN;
    }

    
}
