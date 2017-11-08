﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.fruitDb.factories;
using fruiton.kernel;
using fruiton.kernel.actions;
using fruiton.kernel.events;
using Google.Protobuf.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using KEvent = fruiton.kernel.events.Event;
using KFruiton = fruiton.kernel.Fruiton;
using KAction = fruiton.kernel.actions.Action;
using KVector2 = fruiton.dataStructures.Point;

public class BattleViewer : MonoBehaviour
{
    private static readonly string OPPONENTS_TURN = "Opponent's turn";
    private static readonly string END_TURN = "End turn";

    private Battle battle;
    private bool isGameStarted;
    private bool isInputEnabled = true;

    /// <summary> For handling grid tiles. </summary>
    private GridLayoutManager gridLayoutManager;

    public Button EndTurnButton;
    public GameObject PanelLoadingGame;
    public Text TimeCounter;

    /// <summary> Client fruitons stored at their position. </summary>
    public GameObject[,] Grid { get; set; }

    private void Start()
    {
        gridLayoutManager = GridLayoutManager.Instance;
        Grid = new GameObject[gridLayoutManager.WidthCount, gridLayoutManager.HeighCount];

        var online = Scenes.GetParam(Scenes.ONLINE) == bool.TrueString;
        Debug.Log("playing online = " + online);
        if (online)
        {
            battle = new OnlineBattle(this);
            PanelLoadingGame.SetActive(true);
        }
        else
        {
            battle = new OfflineBattle(this);
            isGameStarted = true;
        }
    }

    private void Update()
    {
        if (!isGameStarted)
            return;
        UpdateTimer();
        if (Input.GetMouseButtonUp(0) && isInputEnabled)
            LeftButtonUpLogic();
    }

    private void LeftButtonUpLogic()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
            battle.LeftButtonUpEvent(hit);
    }

    /// <summary>
    ///     Starts the online game. In case the local player is the second one, rotates the view so that he could see his
    ///     fruitons
    ///     at the bottom side of his screen.
    /// </summary>
    public void StartOnlineGame(bool isLocalPlayerFirst)
    {
        if (!isLocalPlayerFirst)
        {
            foreach (var fruiton in Grid)
                if (fruiton != null)
                    fruiton.transform.Rotate(0, 180, 0);
            var oldPosition = Camera.main.transform.position;
            Camera.main.transform.position = new Vector3(oldPosition.x, oldPosition.y, -oldPosition.z);
            var oldEulerAngles = Camera.main.transform.eulerAngles;
            Camera.main.transform.eulerAngles = new Vector3(oldEulerAngles.x, oldEulerAngles.y + 180, oldEulerAngles.z);
        }
        PanelLoadingGame.SetActive(false);
        isGameStarted = true;
    }

    public void InitializeTeam(IEnumerable<GameObject> currentTeam, Player player,
        RepeatedField<Position> fruitonsPositions = null)
    {
        var counter = 0;
        int i = 0, j = 0;
        foreach (var clientFruiton in currentTeam)
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
            Grid[i, j] = clientFruiton;
            kernelFruiton.position = new KVector2(i, j);
            var cellPosition = gridLayoutManager.GetCellPosition(i, j);
            clientFruiton.transform.position = cellPosition + new Vector3(0, clientFruiton.transform.lossyScale.y, 0);
        }
    }



    private void UpdateTimer()
    {
        var timeLeft = battle.ComputeRemainingTime();
        TimeCounter.text = timeLeft.ToString();
    }

    public List<T> VisualizeActionsOfType<T>(KVector2 indices) where T : KAction
    {
        var allActions = battle.GetAllValidActionFrom(indices);
        var result = allActions.OfType<T>();
        var kernelFruiton = battle.GetFruiton(indices);
        foreach (var action in result)
            VisualizeAction(action, kernelFruiton);
        return result.ToList();
    }

    public void ProcessEvent(KEvent kEvent)
    {
        var eventType = kEvent.GetType();
        if (eventType == typeof(MoveEvent))
            ProcessMoveEvent((MoveEvent) kEvent);
        else if (eventType == typeof(AttackEvent))
            ProcessAttackEvent((AttackEvent) kEvent);
        else if (eventType == typeof(DeathEvent))
            ProcessDeathEvent((DeathEvent) kEvent);
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
        isInputEnabled = false;
        KVector2 from = moveEvent.from;
        KVector2 to = moveEvent.to;
        GameObject movedObject = Grid[from.x, from.y];
        Grid[to.x, to.y] = movedObject;
        Grid[from.x, from.y] = null;
        Vector3 toPosition = gridLayoutManager.GetCellPosition(to.x, to.y);
        StartCoroutine(MoveCoroutine(movedObject.transform.position, toPosition, movedObject));
        gridLayoutManager.ResetHighlights();
    }

    private IEnumerator MoveCoroutine(Vector3 from, Vector3 to, GameObject movedObject)
    {
        var anim = movedObject.GetComponent<SkeletonAnimation>();
        float time = 1.0f;
        float currentTime = 0.0f;
        Vector3 direction = to - from;
        if (anim != null)
            anim.AnimationState.SetAnimation(5, "walk", true);
        while (Vector3.Distance(movedObject.transform.position, to) > 0.05)
        {
            currentTime += Time.deltaTime;
            movedObject.transform.position = from + (direction * currentTime);
            yield return null;
        }
        movedObject.transform.position = to; // Always make sure we made it exactly there
        if (anim != null)
            anim.AnimationState.ClearTrack(5);
        isInputEnabled = true;
    }

    private void VisualizeAction(KAction action, KFruiton kernelFruiton)
    {
        var type = action.GetType();
        if (type == typeof(MoveAction))
        {
            var moveAction = (MoveAction) action;
            var target = ((MoveActionContext) moveAction.actionContext).target;
            Debug.Log("Highlight x=" + target.x + " y=" + target.y);
            gridLayoutManager.HighlightCell(target.x, target.y, Color.blue);
            VisualizePossibleAttacks(target, kernelFruiton);
        }
        else if (type == typeof(AttackAction))
        {
            var attackAction = (AttackAction) action;
            var target = ((AttackActionContext) attackAction.actionContext).target;
            gridLayoutManager.HighlightCell(target.x, target.y, Color.red);
        }
    }

    private void VisualizePossibleAttacks(KVector2 potentialPosition, KFruiton kernelFruiton)
    {
        var potentialTargets = battle.ComputePossibleAttacks(potentialPosition, kernelFruiton);
        foreach (var potentialTarget in potentialTargets)
            gridLayoutManager.HighlightCell(potentialTarget.x, potentialTarget.y, Color.yellow);
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