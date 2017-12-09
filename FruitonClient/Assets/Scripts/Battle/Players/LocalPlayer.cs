using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.kernel;
using fruiton.kernel.actions;
using UnityEngine;
using KAction = fruiton.kernel.actions.Action;

public class LocalPlayer : ClientPlayerBase
{
    private List<AttackAction> availableAttackActions;
    private List<MoveAction> availableMoveActions;
    private List<HealAction> availableHealActions;
    private readonly BattleViewer battleViewer;
    private readonly GridLayoutManager gridLayoutManager;

    public LocalPlayer(BattleViewer battleViewer, Player kernelPlayer, Battle battle, string login) 
        : base(kernelPlayer, battle, login)
    {
        this.battleViewer = battleViewer;
        gridLayoutManager = GridLayoutManager.Instance;
        availableAttackActions = new List<AttackAction>();
        availableMoveActions = new List<MoveAction>();
        availableHealActions = new List<HealAction>();
    }

    public void LeftButtonUpLogic(RaycastHit hit)
    {
        var hitObject = hit.transform.gameObject;
        // Player clicked on a fruiton.
        if (battleViewer.Grid.Contains(hitObject))
        {
            gridLayoutManager.ResetHighlights();
            var indices = battleViewer.Grid.GetIndices(hitObject);
            if (availableAttackActions != null)
            {
                // Find the action where the target is the clicked fruiton and perform it (if such action exists).
                var performedActions = availableAttackActions.FindAll(x =>
                    ((AttackActionContext) x.actionContext).target.equalsTo(indices));
                if (performedActions.Count != 0)
                {
                    var performedAction = performedActions[0];
                    battle.PerformAction(performedAction.getContext().source, performedAction.getContext().target,
                        performedAction.getId());
                }
            }
            if (availableHealActions != null)
            {
                var performedActions = availableHealActions.FindAll(x =>
                    ((HealActionContext)x.actionContext).target.equalsTo(indices));
                if (performedActions.Count != 0)
                {
                    var performedAction = performedActions[0];
                    battle.PerformAction(performedAction.getContext().source, performedAction.getContext().target,
                        performedAction.getId());
                }
            }
            availableMoveActions = battleViewer.VisualizeActionsOfType<MoveAction>(indices);
            availableAttackActions = battleViewer.VisualizeActionsOfType<AttackAction>(indices);
            availableHealActions = battleViewer.VisualizeActionsOfType<HealAction>(indices);

        }
        // A tile was clicked.
        else if (gridLayoutManager.ContainsTile(hitObject))
        {
            var tileIndices = gridLayoutManager.GetIndicesOfTile(hitObject);
            Debug.Log(tileIndices);
            TargetableAction performedAction = null;
            if (availableMoveActions != null)
            {
                var performedActions = availableMoveActions.FindAll(x =>
                    ((MoveActionContext) x.actionContext).target.equalsTo(tileIndices));
                if (performedActions.Count != 0)
                    performedAction = performedActions[0];
            }
            if (availableAttackActions != null && performedAction == null)
            {
                var performedActions = availableAttackActions.FindAll(x =>
                    ((AttackActionContext) x.actionContext).target.equalsTo(tileIndices));
                if (performedActions.Count != 0)
                    performedAction = performedActions[0];
            }
            if (performedAction != null)
            {
                var context = performedAction.getContext();
                var castAction = (KAction) performedAction;
                battle.PerformAction(context.source, context.target, castAction.getId());
            }
        }
    }

    public void EndTurn()
    {
        availableAttackActions.Clear();
        availableMoveActions.Clear();
        availableHealActions.Clear();
        gridLayoutManager.ResetHighlights();
        battle.PerformAction(null, null, EndTurnAction.ID);
    }

    public override void ProcessOpponentAction(EndTurnAction action)
    {
        battleViewer.EnableEndTurnButton();
    }

    public override void ProcessOpponentAction(TargetableAction action)
    {
    }
}