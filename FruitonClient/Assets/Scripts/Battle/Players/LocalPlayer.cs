using System.Collections;
using System.Collections.Generic;
using fruiton.kernel;
using UnityEngine;
using fruiton.kernel.actions;
using UnityEngine.UI;
using KFruiton = fruiton.kernel.Fruiton;
using KVector2 = fruiton.dataStructures.Point;
using KAction = fruiton.kernel.actions.Action;
using System.Linq;
using System;

public class LocalPlayer : ClientPlayerBase
{    
    private GridLayoutManager gridLayoutManager;
    private List<MoveAction> availableMoveActions;
    private List<AttackAction> availableAttackActions;
    private BattleViewer battleViewer;
    

    public LocalPlayer(BattleViewer battleViewer, Player kernelPlayer, Battle battle) : base(kernelPlayer, battle)
    {
        this.battleViewer = battleViewer;
        this.gridLayoutManager = GridLayoutManager.Instance;
    }

    public void LeftButtonUpLogic(RaycastHit hit)
    {

        GameObject HitObject = hit.transform.gameObject;
        // Player clicked on a fruiton.
        if (battleViewer.Grid.Contains(HitObject))
        {
            gridLayoutManager.ResetHighlights();
            var indices = battleViewer.Grid.GetIndices(HitObject);
            if (availableAttackActions != null)
            {
                // Find the action where the target is the clicked fruiton and perform it (if such action exists).
                List<AttackAction> performedActions = availableAttackActions.FindAll(x => ((AttackActionContext)x.actionContext).target.equalsTo(indices));
                if (performedActions.Count != 0)
                {
                    AttackAction performedAction = performedActions[0];
                    battle.PerformAction(performedAction.getContext().source, performedAction.getContext().target, performedAction.getId());
                }
            }
            availableMoveActions = battleViewer.VisualizeActionsOfType<MoveAction>(indices);
            availableAttackActions = battleViewer.VisualizeActionsOfType<AttackAction>(indices);


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
                TargetableActionContext context = performedAction.getContext();
                KAction castAction = (KAction)performedAction;
                battle.PerformAction(context.source, context.target, castAction.getId());
            }
        }
    }
    

    public void EndTurn()
    {
        availableAttackActions.Clear();
        availableMoveActions.Clear();
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
