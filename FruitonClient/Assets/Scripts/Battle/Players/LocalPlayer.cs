using System.Collections;
using System.Collections.Generic;
using fruiton.kernel;
using UnityEngine;
using fruiton.kernel.actions;
using UnityEngine.UI;
using KFruiton = fruiton.kernel.Fruiton;
using KVector2 = fruiton.dataStructures.Point;
using System.Linq;
using System;

public class LocalPlayer : ClientPlayerBase
{
    public Button EndTurnButton;

    private GameObject[,] grid;
    private GridLayoutManager gridLayoutManager;
    private List<MoveAction> availableMoveActions;
    private List<AttackAction> availableAttackActions;
    

    public LocalPlayer(GameObject[,] grid, BattleViewer battleManager) : base(battleManager)
    {
        this.grid = grid;
        this.gridLayoutManager = GridLayoutManager.Instance;
    }

    private void Update()
    {
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
            // Player clicked on a fruiton.
            if (grid.Contains(HitObject))
            {
                gridLayoutManager.ResetHighlights();
                var indices = grid.GetIndices(HitObject);
                if (availableAttackActions != null)
                {
                    // Find the action where the target is the clicked fruiton and perform it (if such action exists).
                    var performedActions = availableAttackActions.FindAll(x => ((AttackActionContext)x.actionContext).target.equalsTo(indices));
                    if (performedActions.Count != 0)
                    {
                        NextAction = performedActions[0];
                        //PerformAction(performedAction);
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
                    NextAction = (Action) performedAction;
                }
            }
        }
    }

    private List<T> VisualizeActionsOfType<T>(KVector2 indices) where T : Action
    {
        var allActions = kernel.getAllValidActionsFrom(indices).CastToList<Action>();
        var result = allActions.OfType<T>();
        //Debug.Log("actions: " + availableMoveActions);
        var kernelFruiton = kernel.currentState.field.get(indices).fruiton;
        foreach (T action in result)
        {
            VisualizeAction(action, kernelFruiton);
        }
        return result.ToList();
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

    public void EndTurn(EndTurnAction endTurnAction)
    {
        availableAttackActions.Clear();
        availableMoveActions.Clear();
        gridLayoutManager.ResetHighlights();
        var oldPos = EndTurnButton.transform.localPosition;
        EndTurnButton.transform.localPosition = new Vector3(oldPos.x, -oldPos.y, oldPos.z);
    }

    public override void ProcessOpponentAction(fruiton.kernel.actions.Action action)
    {
        throw new NotImplementedException();
    }
}
