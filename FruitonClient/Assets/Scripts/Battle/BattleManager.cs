using fruiton.kernel;
using fruiton.kernel.actions;
using fruiton.kernel.targetPatterns;
using System.Collections.Generic;
using UnityEngine;

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

public class BattleManager : MonoBehaviour {

    public Button EndTurnButton;
    public Text TimeCounter;

    private GameObject[,] grid;
    private GridLayoutManager gridLayoutManager;
    private GameManager gameManager;
    private Player me, opponent;
    private GameState gameState;
    private Kernel kernel;

    private List<MoveAction> availableMoveActions;
    private List<AttackAction> availableAttackActions;

    private DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private void Start()
    {
        me = new Player(0);
        opponent = new Player(1);
        gridLayoutManager = GridLayoutManager.Instance;
        gameManager = GameManager.Instance;
        grid = new GameObject[gridLayoutManager.WidthCount, gridLayoutManager.HeighCount];
        IEnumerable<GameObject> currentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs);
        // TODO: This is just temporary offline solution. It is needed to obtain opponent team from server.
        IEnumerable<GameObject> opponentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs);
        var fruitons = new Array<object>();
        InitializeTeam(currentTeam, me);
        InitializeTeam(opponentTeam, opponent);

        foreach (var fruiton in currentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        foreach (var fruiton in opponentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        kernel = new Kernel(me, opponent, fruitons);
    }

    private void InitializeTeam(IEnumerable<GameObject> currentTeam, Player player)
    {
        int majorRow = player.id == 0 ? 0 : gridLayoutManager.HeighCount - 1;
        int minorRow = player.id == 0 ? 1 : majorRow - 1;
        int majorCounter = 2;
        int minorCounter = 2;
        int i = 0, j = 0;
        foreach (GameObject clientFruiton in currentTeam)
        {
            var kernelFruiton = clientFruiton.GetComponent<ClientFruiton>().KernelFruiton;
            kernelFruiton.owner = player;
            
            clientFruiton.gameObject.AddComponent<BoxCollider>();

            switch (kernelFruiton.type)
            {
                case 1:
                    {
                        i = gridLayoutManager.WidthCount / 2;
                        j = majorRow;
                    }
                    break;
                case 2:
                    {
                        i = gridLayoutManager.WidthCount / 2 - majorCounter;
                        j = majorRow;
                        if (--majorCounter == 0) --majorCounter;
                    }
                    break;
                case 3:
                    {
                        i = gridLayoutManager.WidthCount / 2 - minorCounter;
                        j = minorRow;
                        --minorCounter;
                    }
                    break;
            }
            grid[i, j] = clientFruiton;
            kernelFruiton.position = new fruiton.dataStructures.Point(i, j);
            Vector3 cellPosition = gridLayoutManager.GetCellPosition(i, j);
            clientFruiton.transform.position = cellPosition + new Vector3(0, clientFruiton.transform.lossyScale.y, 0);
        }
    }

    private void Update()
    {
        
        int currentEpochTime = (int)(DateTime.UtcNow - epochStart).TotalSeconds;
        int timeLeft = (int)(kernel.currentState.turnState.endTime - currentEpochTime);
        if (timeLeft <= 0)
        {
            EndTurn();
            return;
        }
        TimeCounter.text = (timeLeft).ToString();
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
            GameObject HitObject = hit.transform.gameObject;
            if (grid.Contains(HitObject)) {
                var indices = grid.GetIndices(HitObject);
                var allActions = kernel.getAllValidActionsFrom(indices).CastToList<Action>();
                var moveActionsUncasted = allActions.FindAll(x => (x.GetType() == typeof(MoveAction)));
                availableMoveActions = moveActionsUncasted.ConvertAll<MoveAction>(x => (MoveAction)x);
                Debug.Log("actions: " + availableMoveActions);
                gridLayoutManager.ResetHighlights();
                var kernelFruiton = kernel.currentState.field.get(indices).fruiton;
                foreach (Action action in availableMoveActions)
                {
                    if (action != null)
                    {
                        VisualizeAction(action, kernelFruiton);
                    }
                }
            } else if (gridLayoutManager.ContainsTile(HitObject))
            {
                KVector2 tileIndices = gridLayoutManager.GetIndicesOfTile(HitObject);
                Debug.Log(tileIndices);
                if (availableMoveActions == null)
                {
                    return;
                }
                var performedActions = availableMoveActions.FindAll(x => ((MoveActionContext)x.actionContext).target.equalsTo(tileIndices));
                if (performedActions == null || performedActions.Count == 0)
                {
                    return;
                }
                var performedAction = performedActions[0];
                var events = kernel.performAction(performedAction).CastToList<KEvent>();
                foreach (var item in events)
                {
                    ProcessEvent(item);
                }
            }
        }
    }

    private void ProcessEvent(KEvent kEvent)
    {
        System.Type eventType = kEvent.GetType();
        if (eventType == typeof(MoveEvent))
        {
            ProcessMoveEvent((MoveEvent) kEvent);
        }
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
        if (action is MoveAction)
        {
            var moveAction = (MoveAction)action;
            var target = ((MoveActionContext)(moveAction.actionContext)).target;
            Debug.Log("Highlight x=" + target.x + " y=" + target.y);
            gridLayoutManager.HighlightCell(target.x, target.y, Color.blue);
            VisualizePossibleAttacks(target, kernelFruiton);
        }
    }

    private void VisualizePossibleAttacks(KVector2 potentialPosition, KFruiton kernelFruiton)
    {
        var attackGenerators = kernelFruiton.attackGenerators.CastToList<AttackGenerator>();
        foreach (var attackGenerator in attackGenerators)
        {
            var attacks = attackGenerator.getAttacks(potentialPosition).CastToList<AttackAction>();
            foreach (var attack in attacks)
            {
                var target = ((AttackActionContext)attack.actionContext).target;
                if (kernel.currentState.field.exists(target))
                {
                    var potentialTarget = kernel.currentState.field.get(target).fruiton;
                    if (potentialTarget != null && potentialTarget.owner.id != kernel.currentState.activePlayerIdx)
                    {
                        gridLayoutManager.HighlightCell(target.x, target.y, Color.red);
                    }
                }
            }
        }
    }

    public void EndTurn()
    {
        gridLayoutManager.ResetHighlights();
        EndTurnAction endTurnAction = new EndTurnAction(new EndTurnActionContext());
        kernel.performAction(endTurnAction);
        Debug.Log("End turn.");
        var oldPos = EndTurnButton.transform.localPosition;
        EndTurnButton.transform.localPosition = new Vector3(oldPos.x, -oldPos.y, oldPos.z);
    }
}
